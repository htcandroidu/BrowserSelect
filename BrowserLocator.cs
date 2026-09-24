using System.IO;

namespace BrowserSelect;

/// <summary>
/// Resolves browser executables instead of relying on hardcoded absolute paths.
/// Checks well-known install locations first (machine-wide and per-user),
/// then falls back to a PATH lookup.
/// </summary>
static class BrowserLocator
{
    public static string? Resolve(string exeName, IEnumerable<string> candidatePaths)
    {
        foreach (var candidate in candidatePaths)
        {
            try
            {
                var expanded = Environment.ExpandEnvironmentVariables(candidate);
                if (File.Exists(expanded))
                    return expanded;
            }
            catch (ArgumentException)
            {
            }
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var entry in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var full = Path.Combine(entry.Trim(), exeName);
                if (File.Exists(full))
                    return full;
            }
            catch (ArgumentException)
            {
            }
        }

        return null;
    }
}