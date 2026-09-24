using System;
using System.IO;
using Microsoft.Win32;

namespace BrowserSelect;

class Program
{
    private const string RegisteredAppId = "kenaro.BrowserSelect";
    private const string CapabilitiesPath = @"Software\kenaro\kenaro.BrowserSelect\Capabilities";

    [STAThread]
    public static void Main2(string[] args)
    {
        if (args.Length == 1 && string.Equals(args[0], "--install", StringComparison.Ordinal))
        {
            RunInstallerAction(Install, "Registration complete." + Environment.NewLine + Environment.NewLine +
                "Set \"kenaro BrowserSelect\" as your default browser in Windows Settings > Apps > Default apps.");
            return;
        }

        if (args.Length == 1 && string.Equals(args[0], "--uninstall", StringComparison.Ordinal))
        {
            RunInstallerAction(Uninstall, "Registration removed.");
            return;
        }

        var application = new App();
        application.Run();
    }

    private static void RunInstallerAction(Action action, string successMessage)
    {
        try
        {
            action();
            System.Windows.MessageBox.Show(successMessage, "BrowserSelect",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (UnauthorizedAccessException)
        {
            System.Windows.MessageBox.Show("Registration failed: access to the registry was denied.",
                "BrowserSelect", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Registration failed: {ex.Message}",
                "BrowserSelect", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private static void Install()
    {
        var assemblyPath = typeof(Program).Assembly.Location;
        var exePath = Path.ChangeExtension(assemblyPath, ".exe");
        var quotedExe = $"\"{exePath}\"";

        // Per-user registration: HKCU needs no elevation and is what the
        // "Default apps" UI discovers for the current user.
        using (var registeredApplications = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications", true))
        {
            registeredApplications?.SetValue(RegisteredAppId, CapabilitiesPath);
        }

        using (var kenaroKey = Registry.CurrentUser.CreateSubKey(@"Software\kenaro", true))
        using (var appKey = kenaroKey?.CreateSubKey("kenaro.BrowserSelect", true))
        using (var capabilities = appKey?.CreateSubKey("Capabilities", true))
        {
            capabilities?.SetValue("ApplicationName", "kenaro BrowserSelect");
            capabilities?.SetValue("ApplicationDescription", "Select your preferred browser and browser profile for opening links or just starting the browser with the required profile.");

            using (var startMenu = capabilities?.CreateSubKey("Startmenu", true))
            {
                startMenu?.SetValue("StartmenuInternet", quotedExe);
            }

            using (var urlAssociations = capabilities?.CreateSubKey("UrlAssociations", true))
            {
                urlAssociations?.SetValue("http", RegisteredAppId);
                urlAssociations?.SetValue("https", RegisteredAppId);
            }

            using (var shell = appKey?.CreateSubKey("shell", true))
            using (var open = shell?.CreateSubKey("Open", true))
            using (var command = open?.CreateSubKey("Command", true))
            {
                command?.SetValue("", quotedExe);
            }
        }

        using (var classes = Registry.CurrentUser.CreateSubKey(@"Software\Classes", true))
        using (var progIdKey = classes?.CreateSubKey(RegisteredAppId, true))
        {
            using (var application = progIdKey?.CreateSubKey("Application", true))
            {
                application?.SetValue("ApplicationCompany", "kenaro");
                application?.SetValue("ApplicationDescription", "Select your preferred browser and browser profile for opening links or just starting the browser with the required profile.");
                application?.SetValue("ApplicationName", "kenaro BrowserSelect");
                application?.SetValue("ApplicationIcon", $"{exePath},0");
            }

            using (var defaultIcon = progIdKey?.CreateSubKey("DefaultIcon", true))
            {
                defaultIcon?.SetValue("", $"{assemblyPath},0");
            }

            using (var shell = progIdKey?.CreateSubKey("shell", true))
            using (var open = shell?.CreateSubKey("Open", true))
            using (var command = open?.CreateSubKey("Command", true))
            {
                command?.SetValue("", $"{quotedExe} \"%1\"");
            }
        }
    }

    private static void Uninstall()
    {
        using (var registeredApplications = Registry.CurrentUser.OpenSubKey(@"Software\RegisteredApplications", true))
        {
            registeredApplications?.DeleteValue(RegisteredAppId, throwOnMissingValue: false);
        }

        DeleteSubKeyTreeIfExists(@"Software\kenaro\kenaro.BrowserSelect");
        DeleteSubKeyTreeIfExists(@"Software\Classes\kenaro.BrowserSelect");

        // Remove the now-empty parent key, but leave it alone if another
        // kenaro tool registered something under it.
        var parent = Registry.CurrentUser.OpenSubKey(@"Software\kenaro", true);
        if (parent != null)
        {
            var isEmpty = parent.SubKeyCount == 0;
            parent.Close();
            if (isEmpty)
            {
                Registry.CurrentUser.DeleteSubKey(@"Software\kenaro", throwOnMissingSubKey: false);
            }
        }
    }

    private static void DeleteSubKeyTreeIfExists(string path)
    {
        if (Registry.CurrentUser.OpenSubKey(path) is RegistryKey existing)
        {
            existing.Close();
            Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
        }
    }
}