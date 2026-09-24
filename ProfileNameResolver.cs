using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BrowserSelect;

/// <summary>
/// Resolves richer display names for Chromium-family profiles.
///
/// Chromium stores the visible profile name in <c>Preferences</c> ("Person 1"
/// unless renamed), but the account display name a signed-in user actually sees
/// in the browser lives in the DPAPI-encrypted <c>Secure Preferences</c> file,
/// and the signed-in email is listed per folder in <c>Local State</c>.
///
/// <see cref="ResolveDisplayName"/> picks the most specific name available:
/// account full name, then email, then the Preferences name, then the profile
/// folder name. Every source degrades gracefully (DPAPI only works for the
/// same logged-in user, which discovery always is).
/// </summary>
static class ProfileNameResolver
{
    /// Entropy strings Chromium-family browsers use when DPAPI-encrypting Secure Preferences.
    private static readonly string[] SecurePreferencesEntropies =
    {
        "Chrome Secure Preferences",
        "Edge Secure Preferences",
        string.Empty,
    };

    /// Per-profile metadata from Local State's <c>profile.info_cache</c>.
    public sealed record LocalStateProfile(string Name, string UserName);

    /// <summary>Reads Local State and returns info_cache entries keyed by profile folder name (case-insensitive).</summary>
    public static Dictionary<string, LocalStateProfile> ReadLocalStateProfiles(string userDataDir)
    {
        var result = new Dictionary<string, LocalStateProfile>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var localStateFile = Path.Combine(userDataDir, "Local State");
            if (!File.Exists(localStateFile))
                return result;

            string json;
            using (var fs = new FileStream(localStateFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var sr = new StreamReader(fs))
            {
                json = sr.ReadToEnd();
            }

            var root = JsonSerializer.Deserialize<JsonObject>(json);
            var cache = root?["profile"]?["info_cache"]?.AsObject();
            if (cache == null)
                return result;

            foreach (var entry in cache)
            {
                var info = entry.Value?.AsObject();
                if (info == null)
                    continue;
                result[entry.Key] = new LocalStateProfile(
                    GetString(info["name"]),
                    GetString(info["user_name"]));
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is InvalidOperationException)
        {
        }
        return result;
    }

    /// <summary>
    /// Resolves the display name for one profile directory using the signed-in
    /// account name when available, appending the email only when it adds
    /// information the name itself does not already convey.
    /// </summary>
    public static string ResolveDisplayName(
        string profileDir,
        string dirName,
        string? preferencesName,
        IReadOnlyDictionary<string, LocalStateProfile> localStateProfiles)
    {
        var account = TryGetAccountInfo(profileDir);

        var email = string.Empty;
        if (!string.IsNullOrEmpty(account.Email))
            email = account.Email;
        else if (localStateProfiles.TryGetValue(dirName, out var ls) && !string.IsNullOrEmpty(ls.UserName))
            email = ls.UserName;

        string name;
        if (!string.IsNullOrEmpty(account.FullName))
            name = account.FullName;
        else if (!string.IsNullOrEmpty(email))
            name = email;
        else if (!string.IsNullOrEmpty(preferencesName))
            name = preferencesName;
        else
            name = dirName;

        if (!string.IsNullOrEmpty(email) && !string.Equals(email, name, StringComparison.OrdinalIgnoreCase))
            name += " · " + email;

        return name;
    }

    private static (string FullName, string Email) TryGetAccountInfo(string profileDir)
    {
        var raw = TryReadSecurePreferences(profileDir);
        if (raw == null)
            return (string.Empty, string.Empty);

        try
        {
            var root = JsonSerializer.Deserialize<JsonObject>(raw);
            var accounts = root?["account_info"]?.AsArray();
            if (accounts == null || accounts.Count == 0)
                return (string.Empty, string.Empty);

            foreach (var entry in accounts)
            {
                var obj = entry?.AsObject();
                if (obj == null)
                    continue;
                var fullName = DecodeMaybeBase64(GetString(obj["full_name"]));
                var email = DecodeMaybeBase64(GetString(obj["email"]));
                if (!string.IsNullOrEmpty(fullName) || !string.IsNullOrEmpty(email))
                    return (fullName, email);
            }
        }
        catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException)
        {
        }
        return (string.Empty, string.Empty);
    }

    private static string? TryReadSecurePreferences(string profileDir)
    {
        var securePrefsFile = Path.Combine(profileDir, "Secure Preferences");
        byte[] encrypted;
        try
        {
            using var fs = new FileStream(securePrefsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new BinaryReader(fs);
            encrypted = reader.ReadBytes((int)fs.Length);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return null;
        }

        // Some Chromium builds store Secure Preferences unencrypted.
        try
        {
            return new UTF8Encoding(false, true).GetString(encrypted);
        }
        catch (DecoderFallbackException)
        {
        }

        foreach (var entropy in SecurePreferencesEntropies)
        {
            var text = TryUnprotect(encrypted, entropy);
            if (text != null)
                return text;
        }
        return null;
    }

    private static string? TryUnprotect(byte[] data, string entropy)
    {
        var dataIn = ToBlob(data);
        var entropyBytes = string.IsNullOrEmpty(entropy) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(entropy);
        var entropyIn = ToBlob(entropyBytes);
        var dataOut = new DataBlob();
        IntPtr description = IntPtr.Zero;
        try
        {
            if (!CryptUnprotectData(ref dataIn, out description, ref entropyIn, IntPtr.Zero, IntPtr.Zero, 0, ref dataOut))
                return null;

            if (dataOut.cbData <= 0)
                return null;

            var bytes = new byte[dataOut.cbData];
            Marshal.Copy(dataOut.pbData, bytes, 0, dataOut.cbData);
            return Encoding.UTF8.GetString(bytes);
        }
        finally
        {
            if (dataIn.pbData != IntPtr.Zero)
                Marshal.FreeHGlobal(dataIn.pbData);
            if (entropyIn.pbData != IntPtr.Zero)
                Marshal.FreeHGlobal(entropyIn.pbData);
            if (dataOut.pbData != IntPtr.Zero)
                LocalFree(dataOut.pbData);
            if (description != IntPtr.Zero)
                LocalFree(description);
        }
    }

    private static DataBlob ToBlob(byte[] data)
    {
        if (data.Length == 0)
            return default;
        var ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        return new DataBlob { cbData = data.Length, pbData = ptr };
    }

    private static string GetString(JsonNode? node)
    {
        try
        {
            return node?.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException || ex is FormatException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Chromium base64-encodes some Secure Preferences string fields. Decodes
    /// only when the value is valid base64 that decodes to control-free text,
    /// so plain-text values are never mangled.
    /// </summary>
    private static string DecodeMaybeBase64(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0)
            return value;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return value;
        }

        if (bytes.Length == 0)
            return value;

        string decoded;
        try
        {
            decoded = new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return value;
        }

        return decoded.Any(c => char.IsControl(c)) ? value : decoded;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int cbData;
        public IntPtr pbData;
    }

    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CryptUnprotectData(
        ref DataBlob pDataIn,
        out IntPtr ppszDataDescr,
        ref DataBlob pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        uint dwFlags,
        ref DataBlob pDataOut);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr hMem);
}