using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;

namespace BrowserSelect
{
    public class MyItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string DirName { get; set; } = string.Empty;
        public string ToolTip { get; set; } = string.Empty;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _edgeExe;
        private string? _heliumExe;
        private string? _firefoxExe;
        private string? _waterfoxExe;
        private string? _chromiumExe;
        private string? _chromeExe;
        private string? _braveExe;

        public MainWindow()
        {
            InitializeComponent();
        }

        private static bool TryGetTargetUrl(string? raw, out string? url)
        {
            url = null;
            if (string.IsNullOrWhiteSpace(raw))
                return true;
            raw = raw.Trim();
            if (Uri.TryCreate(raw, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                url = uri.AbsoluteUri;
                return true;
            }
            return false;
        }

        private static void StartBrowser(string exePath, IEnumerable<string> args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
            };
            foreach (var a in args)
                psi.ArgumentList.Add(a);
            Process.Start(psi);
        }

        private void SelectButton_Click(object sender, RoutedEventArgs? e)
        {
            if (!TryGetTargetUrl(Url.Text, out var targetUrl))
            {
                MessageBox.Show("The supplied URL is not a valid http(s) URL and was not opened.", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool privateMode = IncognitoCheckBox.IsChecked == true;

            static bool IsValidProfileValue(string? v) =>
                !string.IsNullOrWhiteSpace(v) && !v.Contains('"');

            void LaunchChromium(string? exePath, MyItem item, bool useFlagSwitches, string? privateFlag)
            {
                if (!IsValidProfileValue(item.DirName))
                {
                    MessageBox.Show("The selected profile name is invalid and was not opened.", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (exePath == null || !File.Exists(exePath))
                {
                    MessageBox.Show($"Browser executable not found:{Environment.NewLine}{exePath}", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var args = new List<string>();
                if (useFlagSwitches)
                    args.Add("--flag-switches-begin");
                args.Add($"--profile-directory={item.DirName}");
                args.Add("--no-first-run");
                args.Add("--no-default-browser-check");
                args.Add("--ash-no-nudges");
                if (privateFlag != null)
                    args.Add(privateFlag);
                if (useFlagSwitches)
                    args.Add("--flag-switches-end");
                if (targetUrl != null)
                    args.Add(targetUrl);
                StartBrowser(exePath, args);
                Application.Current.Shutdown();
            }

            void LaunchFirefox(string? exePath, MyItem item, bool privateMode)
            {
                if (!IsValidProfileValue(item.DisplayName))
                {
                    MessageBox.Show("The selected profile name is invalid and was not opened.", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (exePath == null || !File.Exists(exePath))
                {
                    MessageBox.Show($"Browser executable not found:{Environment.NewLine}{exePath}", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var args = new List<string> { "-p", item.DisplayName };
                if (privateMode)
                    args.Add("--private-window");
                if (targetUrl != null)
                    args.Add(targetUrl);
                StartBrowser(exePath, args);
                Application.Current.Shutdown();
            }

            var i = EdgeListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchChromium(_edgeExe, i, useFlagSwitches: false, privateFlag: privateMode ? "--inprivate" : null);
                return;
            }

            i = HeliumListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchChromium(_heliumExe, i, useFlagSwitches: true, privateFlag: privateMode ? "--incognito" : null);
                return;
            }

            i = FirefoxListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchFirefox(_firefoxExe, i, privateMode);
                return;
            }

            i = WaterfoxListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchFirefox(_waterfoxExe, i, privateMode);
                return;
            }

            i = ChromiumListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchChromium(_chromiumExe, i, useFlagSwitches: true, privateFlag: privateMode ? "--incognito" : null);
                return;

            }

            i = ChromeListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchChromium(_chromeExe, i, useFlagSwitches: true, privateFlag: privateMode ? "--incognito" : null);
                return;

            }

            i = BraveListBox.SelectedItem as MyItem;
            if (i != null)
            {
                LaunchChromium(_braveExe, i, useFlagSwitches: true, privateFlag: privateMode ? "--incognito" : null);
                return;

            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            SelectButton.IsEnabled = false;

            // Parse command line arguments  
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                Url.Text = args[1];
            }
            else
            {
                Url.Text = "";
                Url.Visibility = Visibility.Hidden;
                this.Grid.RowDefinitions[0].Height = new GridLength(0);
            }

            _edgeExe = BrowserLocator.Resolve("msedge.exe", new[]
            {
                @"%ProgramFiles(x86)%\Microsoft\Edge\Application\msedge.exe",
                @"%ProgramFiles%\Microsoft\Edge\Application\msedge.exe",
                @"%LocalAppData%\Microsoft\Edge\Application\msedge.exe",
            });
            _heliumExe = BrowserLocator.Resolve("chrome.exe", new[]
            {
                @"%LocalAppData%\imput\Helium\Application\chrome.exe",
                @"%ProgramFiles%\imput\Helium\Application\chrome.exe",
                @"%ProgramFiles(x86)%\imput\Helium\Application\chrome.exe",
            });
            _firefoxExe = BrowserLocator.Resolve("firefox.exe", new[]
            {
                @"%ProgramFiles%\Mozilla Firefox\firefox.exe",
                @"%ProgramFiles(x86)%\Mozilla Firefox\firefox.exe",
                @"%LocalAppData%\Programs\Mozilla Firefox\firefox.exe",
            });
            _waterfoxExe = BrowserLocator.Resolve("waterfox.exe", new[]
            {
                @"%ProgramFiles%\Waterfox\waterfox.exe",
                @"%ProgramFiles(x86)%\Waterfox\waterfox.exe",
                @"%LocalAppData%\Programs\Waterfox\waterfox.exe",
            });
            _chromiumExe = BrowserLocator.Resolve("chrome.exe", new[]
            {
                @"%ProgramFiles%\Chromium\Application\chrome.exe",
                @"%ProgramFiles(x86)%\Chromium\Application\chrome.exe",
                @"%LocalAppData%\Chromium\Application\chrome.exe",
            });
            _chromeExe = BrowserLocator.Resolve("chrome.exe", new[]
            {
                @"%ProgramFiles%\Google\Chrome\Application\chrome.exe",
                @"%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe",
                @"%LocalAppData%\Google\Chrome\Application\chrome.exe",
            });
            _braveExe = BrowserLocator.Resolve("brave.exe", new[]
            {
                @"%ProgramFiles%\BraveSoftware\Brave-Browser\Application\brave.exe",
                @"%ProgramFiles(x86)%\BraveSoftware\Brave-Browser\Application\brave.exe",
                @"%LocalAppData%\BraveSoftware\Brave-Browser\Application\brave.exe",
            });

            MarkUnavailable(EdgeListBox, EdgeHeader, _edgeExe);
            MarkUnavailable(HeliumListBox, HeliumHeader, _heliumExe);
            MarkUnavailable(FirefoxListBox, FirefoxHeader, _firefoxExe);
            MarkUnavailable(WaterfoxListBox, WaterfoxHeader, _waterfoxExe);
            MarkUnavailable(ChromiumListBox, ChromiumHeader, _chromiumExe);
            MarkUnavailable(ChromeListBox, ChromeHeader, _chromeExe);
            MarkUnavailable(BraveListBox, BraveHeader, _braveExe);

            var edge = LoadChromiumProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Microsoft\\Edge\\User Data"), EdgeListBox);
            var helium = LoadChromiumProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\imput\\Helium\\User Data"), HeliumListBox);
            var chromium = LoadChromiumProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Chromium\\User Data"), ChromiumListBox);
            var firefox = LoadFirefoxFamilyProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Roaming\\Mozilla\\Firefox"), FirefoxListBox, "\\mozilla firefox");
            var waterfox = LoadFirefoxFamilyProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Roaming\\Waterfox"), WaterfoxListBox, "\\waterfox");
            var chrome = LoadChromiumProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Google\\Chrome\\User Data"), ChromeListBox);
            var brave = LoadChromiumProfiles(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\BraveSoftware\\Brave-Browser\\User Data"), BraveListBox);

            int loaded = edge.Loaded + helium.Loaded + chromium.Loaded + firefox.Loaded + waterfox.Loaded + chrome.Loaded + brave.Loaded;
            int skipped = edge.Skipped + helium.Skipped + chromium.Skipped + firefox.Skipped + waterfox.Skipped + chrome.Skipped + brave.Skipped;

            var status = loaded == 1 ? "1 profile loaded" : $"{loaded} profiles loaded";
            if (skipped > 0)
                status += $" · {skipped} skipped";

            var missing = new List<string>();
            if (_edgeExe == null) missing.Add("Edge");
            if (_heliumExe == null) missing.Add("Helium");
            if (_firefoxExe == null) missing.Add("Firefox");
            if (_waterfoxExe == null) missing.Add("Waterfox");
            if (_chromiumExe == null) missing.Add("Chromium");
            if (_chromeExe == null) missing.Add("Chrome");
            if (_braveExe == null) missing.Add("Brave");
            if (missing.Count > 0)
                status += $" · not found: {string.Join(", ", missing)}";

            StatusText.Text = status;
        }

        private static void MarkUnavailable(ListBox list, FrameworkElement header, string? exePath)
        {
            if (exePath == null)
            {
                list.IsEnabled = false;
                header.IsEnabled = false;
                header.ToolTip = "Browser executable was not found on this system.";
            }
        }

        private static string? TryReadChromiumProfileName(string prefFile)
        {
            try
            {
                string pref;
                using (var fs = new FileStream(prefFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var sr = new StreamReader(fs))
                {
                    pref = sr.ReadToEnd();
                }
                var deser = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(pref);
                if (deser?.TryGetPropertyValue("profile", out var profile) == true &&
                    profile?.AsObject().TryGetPropertyValue("name", out var name) == true &&
                    name?.AsValue().TryGetValue<string>(out var profileName) == true &&
                    !string.IsNullOrWhiteSpace(profileName))
                {
                    return profileName;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Text.Json.JsonException || ex is InvalidOperationException)
            {
            }
            return null;
        }

        private static (int Loaded, int Skipped) LoadChromiumProfiles(string userDataDir, ListBox target)
        {
            string[] dirs;
            try
            {
                if (!Directory.Exists(userDataDir))
                {
                    target.Visibility = Visibility.Hidden;
                    return (0, 0);
                }
                dirs = Directory.GetDirectories(userDataDir);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                target.Visibility = Visibility.Hidden;
                return (0, 0);
            }
            var localState = ProfileNameResolver.ReadLocalStateProfiles(userDataDir);
            var items = new List<MyItem>();
            foreach (var s in dirs)
            {
                try
                {
                    var prefFn = Path.Combine(s, "Preferences");
                    if (!File.Exists(prefFn))
                        continue;
                    var dirName = Path.GetFileName(s);
                    var displayName = ProfileNameResolver.ResolveDisplayName(
                        s, dirName, TryReadChromiumProfileName(prefFn), localState);
                    items.Add(new MyItem
                    {
                        DisplayName = displayName,
                        Path = s,
                        DirName = dirName,
                        ToolTip = s,
                    });
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                }
            }

            DisambiguateCollidingNames(items);

            target.Items.Clear();
            foreach (var item in items)
                target.Items.Add(item);

            return (items.Count, 0);
        }

        private static void DisambiguateCollidingNames(List<MyItem> items)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
                counts[item.DisplayName] = counts.TryGetValue(item.DisplayName, out var c) ? c + 1 : 1;

            foreach (var item in items)
            {
                if (counts.TryGetValue(item.DisplayName, out var c) && c > 1)
                    item.DisplayName += $" ({item.DirName})";
            }
        }

        private static (int Loaded, int Skipped) LoadFirefoxFamilyProfiles(string appDataDir, ListBox target, string platformSuffix)
        {
            int loaded = 0, skipped = 0;
            try
            {
                target.Items.Clear();
                var iniPath = Path.Combine(appDataDir, "Profiles.ini");
                if (!Directory.Exists(appDataDir) || !File.Exists(iniPath))
                {
                    target.Visibility = Visibility.Hidden;
                    return (0, 0);
                }
                var ini = new IniFileReader(iniPath);
                for (int i = 0; ; i++)
                {
                    try
                    {
                        if (ini.GetSection($"Profile{i}") == null)
                            break;
                        var name = ini.GetValue($"Profile{i}", "Name");
                        var rawPath = ini.GetValue($"Profile{i}", "Path");
                        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(rawPath))
                        {
                            skipped++;
                            continue;
                        }
                        var path = rawPath.Replace("/", "\\");
                        if (ini.GetValue($"Profile{i}", "IsRelative") == "1")
                            path = Path.Combine(appDataDir, path);

                        var compatFn = Path.Combine(path, "compatibility.ini");
                        string? lastPlatformDir = null;
                        if (File.Exists(compatFn))
                        {
                            try
                            {
                                lastPlatformDir = new IniFileReader(compatFn).GetValue("Compatibility", "LastPlatformDir");
                            }
                            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                            {
                            }
                        }
                        if (lastPlatformDir == null || !lastPlatformDir.EndsWith(platformSuffix, StringComparison.OrdinalIgnoreCase))
                        {
                            // Profile cannot be classified (no compatibility.ini, or
                            // unreadable file) or belongs to another product (a Firefox
                            // Beta/ESR profile won't match "\mozilla firefox", for example).
                            skipped++;
                            continue;
                        }
                        target.Items.Add(new MyItem { DisplayName = name, Path = path, DirName = Path.GetFileName(path), ToolTip = path });
                        loaded++;
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
                    {
                        skipped++;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
            }
            return (loaded, skipped);
        }

        private void UpdateButtonState()
        {
            SelectButton.IsEnabled =
                EdgeListBox.SelectedItem != null ||
                FirefoxListBox.SelectedItem != null ||
                HeliumListBox.SelectedItem != null ||
                WaterfoxListBox.SelectedItem != null ||
                ChromiumListBox.SelectedItem != null ||
                BraveListBox.SelectedItem != null ||
                ChromeListBox.SelectedItem != null;
        }

        private void EdgeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EdgeListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                //EdgeListBox.SelectedItem = null;
                HeliumListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                WaterfoxListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void EdgeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void FirefoxListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirefoxListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                HeliumListBox.SelectedItem = null;
                //FirefoxListBox.SelectedItem = null;
                WaterfoxListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void ChromiumListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChromiumListBox.SelectedItem is MyItem i)
            {
                //ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                HeliumListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                WaterfoxListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void ChromiumListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void FirefoxListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void ChromeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void ChromeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChromeListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                HeliumListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                WaterfoxListBox.SelectedItem = null;
                //ChromeListBox.SelectedItem = null;
                BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();

        }

        private void BraveListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BraveListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                HeliumListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                WaterfoxListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                //BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void BraveListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void WaterfoxListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WaterfoxListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                HeliumListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                //WaterfoxListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void WaterfoxListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void HeliumListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeliumListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                //HeliumListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                WaterfoxListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                BraveListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void HeliumListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void EdgeListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(EdgeListBox.SelectedItem);
        private void HeliumListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(HeliumListBox.SelectedItem);
        private void ChromeListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(ChromeListBox.SelectedItem);
        private void BraveListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(BraveListBox.SelectedItem);
        private void ChromiumListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(ChromiumListBox.SelectedItem);
        private void FirefoxListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(FirefoxListBox.SelectedItem);
        private void WaterfoxListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e) => OpenProfileFolder(WaterfoxListBox.SelectedItem);

        private static void OpenProfileFolder(object? selectedItem)
        {
            if (selectedItem is not MyItem item || string.IsNullOrEmpty(item.Path))
                return;
            if (!Directory.Exists(item.Path))
            {
                MessageBox.Show("The profile folder no longer exists.", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var psi = new ProcessStartInfo { FileName = "explorer.exe", UseShellExecute = false };
                psi.ArgumentList.Add(item.Path);
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the profile folder: {ex.Message}", "BrowserSelect", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Listbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SelectButton_Click(sender, null);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }
    }
}