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
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public string DirName { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var i = EdgeListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe", $"--profile-directory=\"{i.DirName}\" --no-first-run --no-default-browser-check --ash-no-nudges {Url.Text}");
                Application.Current.Shutdown();
                return;
            }

            i = EdgeDevListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files (x86)\\Microsoft\\Edge Dev\\Application\\msedge.exe", $"--profile-directory=\"{i.DirName}\" --no-first-run --no-default-browser-check --ash-no-nudges {Url.Text}");
                Application.Current.Shutdown();
                return;
            }

            i = FirefoxListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Mozilla Firefox\\firefox.exe", $"-p \"{i.DisplayName}\" {Url.Text}");
                Application.Current.Shutdown();
                return;
            }

            i = FirefoxDevListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Firefox Developer Edition\\firefox.exe", $"-p \"{i.DisplayName}\" {Url.Text}");
                Application.Current.Shutdown();
                return;
            }

            i = ChromiumListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Chromium\\Application\\chrome.exe", $"--flag-switches-begin --profile-directory=\"{i.DirName}\" --no-first-run --no-default-browser-check --ash-no-nudges --flag-switches-end {Url.Text}");
                Application.Current.Shutdown();
                return;

            }

            i = ChromeListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", $"--flag-switches-begin --profile-directory=\"{i.DirName}\" --no-first-run --no-default-browser-check --ash-no-nudges --flag-switches-end {Url.Text}");
                Application.Current.Shutdown();
                return;

            }

            i = ChromeDevListBox.SelectedItem as MyItem;
            if (i != null)
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Google\\Chrome Dev\\Application\\chrome.exe", $"--flag-switches-begin --profile-directory=\"{i.DirName}\" --no-first-run --no-default-browser-check --ash-no-nudges --flag-switches-end {Url.Text}");
                Application.Current.Shutdown();
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

            var edgePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Microsoft\\Edge\\User Data");
            if (Directory.Exists(edgePath))
            {
                EdgeListBox.Items.Clear();

                foreach (var s in Directory.GetDirectories(edgePath))
                {
                    var prefFn = Path.Combine(s, "Preferences");
                    if (File.Exists(prefFn))
                    {
                        var pref = File.ReadAllText(prefFn);
                        var deser = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(pref);
                        if (deser?.TryGetPropertyValue("profile", out var profile) == true)
                        {
                            if (profile?.AsObject().TryGetPropertyValue("name", out var name) == true)
                            {
                                if (name?.AsValue().TryGetValue<string>(out var profileName) == true)
                                {
                                    MyItem m = new MyItem { DisplayName = profileName, Path = s, DirName = Path.GetFileName(s) };
                                    EdgeListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                EdgeListBox.Visibility = Visibility.Hidden;
            }

            var edgeDevPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Microsoft\\Edge Dev\\User Data");
            if (Directory.Exists(edgeDevPath))
            {
                EdgeDevListBox.Items.Clear();

                foreach (var s in Directory.GetDirectories(edgeDevPath))
                {
                    var prefFn = Path.Combine(s, "Preferences");
                    if (File.Exists(prefFn))
                    {
                        var pref = File.ReadAllText(prefFn);
                        var deser = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(pref);
                        if (deser?.TryGetPropertyValue("profile", out var profile) == true)
                        {
                            if (profile?.AsObject().TryGetPropertyValue("name", out var name) == true)
                            {
                                if (name?.AsValue().TryGetValue<string>(out var profileName) == true)
                                {
                                    MyItem m = new MyItem { DisplayName = profileName, Path = s, DirName = Path.GetFileName(s) };
                                    EdgeDevListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                EdgeDevListBox.Visibility = Visibility.Hidden;
            }

            var chromiumPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Chromium\\User Data");
            if (Directory.Exists(chromiumPath))
            {
                ChromiumListBox.Items.Clear();

                foreach (var s in Directory.GetDirectories(chromiumPath))
                {
                    var prefFn = Path.Combine(s, "Preferences");
                    if (File.Exists(prefFn))
                    {
                        var pref = File.ReadAllText(prefFn);
                        var deser = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(pref);
                        if (deser?.TryGetPropertyValue("profile", out var profile) == true)
                        {
                            if (profile?.AsObject().TryGetPropertyValue("name", out var name) == true)
                            {
                                if (name?.AsValue().TryGetValue<string>(out var profileName) == true)
                                {
                                    MyItem m = new MyItem { DisplayName = profileName, Path = s, DirName = Path.GetFileName(s) };
                                    ChromiumListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChromiumListBox.Visibility = Visibility.Hidden;
            }

            var ffProfilesPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Roaming\\Mozilla\\Firefox");
            if (Directory.Exists(ffProfilesPath))
            {
                FirefoxListBox.Items.Clear();

                var ffProfiles = Path.Combine(ffProfilesPath, "Profiles.ini");
                if (File.Exists(ffProfiles))
                {
                    var ini = new IniFileReader(ffProfiles);
                    for (int i = 0; ; i++)
                    {
                        var profile = ini.GetSection($"Profile{i}");
                        if (profile == null)
                            break;
                        var name = ini.GetValue($"Profile{i}", "Name");
                        var path = ini.GetValue($"Profile{i}", "Path").Replace("/", "\\");

                        if (ini.GetValue($"Profile{i}", "IsRelative") == "1")
                        {
                            path = Path.Combine(ffProfilesPath, path);
                        }

                        var compat = new IniFileReader(Path.Combine(path, "compatibility.ini"));

                        var sec = compat.GetSection("Compatibility");
                        if (sec != null)
                        {
                            var p2 = compat.GetValue("Compatibility", "LastPlatformDir");

                            if (p2 != null)
                            {
                                if (p2.EndsWith("\\mozilla firefox", StringComparison.OrdinalIgnoreCase))
                                {
                                    MyItem m = new MyItem { DisplayName = name, Path = path, DirName = Path.GetFileName(path) };
                                    FirefoxListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
                else
                {
                    FirefoxListBox.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                FirefoxListBox.Visibility = Visibility.Hidden;
            }

            var ffProfiles2Path = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Roaming\\Mozilla\\Firefox");
            if (Directory.Exists(ffProfiles2Path))
            {
                FirefoxDevListBox.Items.Clear();

                var ffProfiles2 = Path.Combine(ffProfiles2Path, "Profiles.ini");
                if (File.Exists(ffProfiles2))
                {
                    var ini = new IniFileReader(ffProfiles2);
                    for (int i = 0; ; i++)
                    {
                        var profile = ini.GetSection($"Profile{i}");
                        if (profile == null)
                            break;
                        var name = ini.GetValue($"Profile{i}", "Name");
                        var path = ini.GetValue($"Profile{i}", "Path").Replace("/", "\\");

                        if (ini.GetValue($"Profile{i}", "IsRelative") == "1")
                        {
                            path = Path.Combine(ffProfiles2Path, path);
                        }

                        var compat = new IniFileReader(Path.Combine(path, "compatibility.ini"));

                        var sec = compat.GetSection("Compatibility");
                        if (sec != null)
                        {
                            var p2 = compat.GetValue("Compatibility", "LastPlatformDir");
                            if (p2 != null)
                            {
                                if (p2.EndsWith("\\firefox developer edition", StringComparison.OrdinalIgnoreCase))
                                {
                                    MyItem m = new MyItem { DisplayName = name, Path = path, DirName = Path.GetFileName(path) };
                                    FirefoxDevListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
                else
                {
                    FirefoxDevListBox.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                FirefoxDevListBox.Visibility = Visibility.Hidden;
            }

            var chromePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Google\\Chrome\\User Data");
            if (Directory.Exists(chromePath))
            {
                ChromeListBox.Items.Clear();
                foreach (var s in Directory.GetDirectories(chromePath))
                {
                    var prefFn = Path.Combine(s, "Preferences");
                    if (File.Exists(prefFn))
                    {
                        var pref = File.ReadAllText(prefFn);
                        var deser = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(pref);
                        if (deser?.TryGetPropertyValue("profile", out var profile) == true)
                        {
                            if (profile?.AsObject().TryGetPropertyValue("name", out var name) == true)
                            {
                                if (name?.AsValue().TryGetValue<string>(out var profileName) == true)
                                {
                                    MyItem m = new MyItem { DisplayName = profileName, Path = s, DirName = Path.GetFileName(s) };
                                    ChromeListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChromeListBox.Visibility = Visibility.Hidden;
            }

            var chromeDevPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Google\\Chrome Dev\\User Data");
            if (Directory.Exists(chromeDevPath))
            {
                ChromeDevListBox.Items.Clear();

                foreach (var s in Directory.GetDirectories(chromeDevPath))
                {
                    var prefFn = Path.Combine(s, "Preferences");
                    if (File.Exists(prefFn))
                    {
                        var pref = File.ReadAllText(prefFn);
                        var deser = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(pref);
                        if (deser?.TryGetPropertyValue("profile", out var profile) == true)
                        {
                            if (profile?.AsObject().TryGetPropertyValue("name", out var name) == true)
                            {
                                if (name?.AsValue().TryGetValue<string>(out var profileName) == true)
                                {
                                    MyItem m = new MyItem { DisplayName = profileName, Path = s, DirName = Path.GetFileName(s) };
                                    ChromeDevListBox.Items.Add(m);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ChromeDevListBox.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateButtonState()
        {
            SelectButton.IsEnabled =
                EdgeListBox.SelectedItem != null ||
                FirefoxListBox.SelectedItem != null ||
                EdgeDevListBox.SelectedItem != null ||
                FirefoxDevListBox.SelectedItem != null ||
                ChromiumListBox.SelectedItem != null ||
                ChromeDevListBox.SelectedItem != null ||
                ChromeListBox.SelectedItem != null;
        }

        private void EdgeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EdgeListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                //EdgeListBox.SelectedItem = null;
                EdgeDevListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                FirefoxDevListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                ChromeDevListBox.SelectedItem = null;
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
                EdgeDevListBox.SelectedItem = null;
                //FirefoxListBox.SelectedItem = null;
                FirefoxDevListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                ChromeDevListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void ChromiumListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChromiumListBox.SelectedItem is MyItem i)
            {
                //ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                EdgeDevListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                FirefoxDevListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                ChromeDevListBox.SelectedItem = null;
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
                EdgeDevListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                FirefoxDevListBox.SelectedItem = null;
                //ChromeListBox.SelectedItem = null;
                ChromeDevListBox.SelectedItem = null;
            }
            UpdateButtonState();

        }

        private void ChromeDevListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChromeDevListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                EdgeDevListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                FirefoxDevListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                //ChromeDevListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void ChromeDevListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void FirefoxDevListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FirefoxDevListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                EdgeDevListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                //FirefoxDevListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                ChromeDevListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void FirefoxDevListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void EdgeDevListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EdgeDevListBox.SelectedItem is MyItem i)
            {
                ChromiumListBox.SelectedItem = null;
                EdgeListBox.SelectedItem = null;
                //EdgeDevListBox.SelectedItem = null;
                FirefoxListBox.SelectedItem = null;
                FirefoxDevListBox.SelectedItem = null;
                ChromeListBox.SelectedItem = null;
                ChromeDevListBox.SelectedItem = null;
            }
            UpdateButtonState();
        }

        private void EdgeDevListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, null);
        }

        private void EdgeListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = EdgeListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
            }
        }
        private void EdgeDevListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = EdgeDevListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
            }
        }
        private void ChromeListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = ChromeListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
            }
        }
        private void ChromeDevListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = ChromeDevListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
            }
        }
        private void ChromiumListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = ChromiumListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
            }
        }
        private void FirefoxListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = FirefoxListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
            }
        }
        private void FirefoxDevListBox_ContextMenu_Open_Click(object sender, RoutedEventArgs e)
        {
            var e2 = FirefoxDevListBox.SelectedItem as MyItem;
            if (e2 != null && !string.IsNullOrEmpty(e2.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", e2.Path);
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