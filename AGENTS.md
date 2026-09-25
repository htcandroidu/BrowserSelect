# AGENTS.md

Single-project WPF app, Windows-only. No tests, lint, CI, or multi-package structure.

## Stack

- `BrowserSelect.csproj`: `Microsoft.NET.Sdk`, `net9.0-windows`, `UseWPF`, `OutputType WinExe`. Build on Windows with .NET 9 SDK + WPF workload (`dotnet build BrowserSelect.sln`).
- `Program.cs:Main2` — dispatches `--install` / `--uninstall` registry work; the trailing `new App().Run()` is effectively dead for normal launch.
- `App.xaml.cs:OnStartup` intercepts `--install` / `--uninstall` (exact single-arg match only), delegates to `Program.Main2`, then shuts down.
- `BrowserLocator.cs` — exe resolution: fixed candidate paths (`%ProgramFiles%`, `%ProgramFiles(x86)%`, `%LocalAppData%`) then `PATH`. No registry App-Paths lookup.
- `Properties/launchSettings.json` defines the two run modes: URL arg (e.g. `http://google.com`) and `--install`.

## Commands

- Build: `dotnet build BrowserSelect.sln -c Release`
- Run picker with no URL: `dotnet run --project BrowserSelect.csproj` (hides the URL row in the layout)
- Run with URL: `dotnet run --project BrowserSelect.csproj -- "https://example.com"`
- Register as default browser — **per-user, no elevation** (writes `HKCU\Software\RegisteredApplications` + `HKCU\Software\Classes`): `BrowserSelect.exe --install`, then set it in Windows Settings > Apps > Default apps. Remove with `BrowserSelect.exe --uninstall`.

## Where logic lives

- `MainWindow.xaml(.cs)` — everything else: profile discovery in `OnInitialized`, launching in `SelectButton_Click`, mutually-exclusive selection across 7 ListBoxes.
- `IniFileReader.cs` — minimal INI parser for Firefox `Profiles.ini` / `compatibility.ini`. Case-insensitive, no write support.

## Quirks agents miss

- Chromium-family profiles (Edge, Helium, Chrome, Brave, Chromium): discovered by scanning `%USERPROFILE%\...\User Data\<dir>\Preferences` for `profile.name`, then enriched via `ProfileNameResolver` — account display name from the DPAPI-decrypted `Secure Preferences` (`account_info.full_name`), signed-in email from `Local State` `profile.info_cache[dir].user_name`, and a ` (Folder)` suffix when display names collide. DPAPI works because discovery runs as the same logged-in user; any decrypt failure falls back to the plain `profile.name`. Launch uses folder name (`DirName`) as `--profile-directory`, not the display name. Note Helium's unusual layout: per-user install under `%LocalAppData%\imput\Helium\` with an internally-renamed `chrome.exe`, so its `User Data` dir is `%LocalAppData%\imput\Helium\User Data`.
- Firefox and Waterfox each load from their own `Profiles.ini` (`%APPDATA%\Mozilla\Firefox` and `%APPDATA%\Waterfox`) via `LoadFirefoxFamilyProfiles`, routed by `compatibility.ini:Compatibility/LastPlatformDir` suffix match (`\mozilla firefox` vs `\waterfox`); launch uses `-p "DisplayName"`.
- Browser exe paths are **not** hardcoded: `BrowserLocator.Resolve(...)` checks well-known install dirs then `PATH` at startup (`OnInitialized`). Unresolved browsers have their ListBox + header disabled and are listed in the `StatusText` status bar, which also shows profile counts ("N profiles loaded · M skipped"). Adding a browser means: new `ListBox` + named header in XAML, fields/candidates in `OnInitialized`, a launch branch, disabling it in every other `SelectionChanged` handler, plus `MarkUnavailable` + status entries.
- Firefox/Waterfox discovery is one shared pass (`LoadFirefoxFamilyProfiles`) parameterized by app-data dir + `LastPlatformDir` suffix; one list per browser, profiles whose suffix doesn't match (other Mozilla-family products such as Beta/ESR) or with no readable `compatibility.ini` count as skipped.
- Selection is exclusive by manually nulling the other six ListBoxes; `UpdateButtonState` gates the Select button. Double-click / Enter launches, Esc quits.
- The "Private window" checkbox (top-right, only visible when a URL is present) adds the browser's private-mode flag to the launch args: Edge `--inprivate`, other Chromium-family (Helium, Chrome, Brave, Chromium) `--incognito`, Firefox/Waterfox `--private-window`. It keeps the profile flags — the private window is scoped to the selected browser+profile.
- Mouse-wheel scrolling over any profile list is deliberately slowed 4x. `EnablePixelScrolling` (fired on `Loaded`) sets `CanContentScroll = false` on each ListBox's internal `ScrollViewer` so offsets become pixel-based — WPF floors item-based offsets to whole items, which made slow touchpad gestures dead-stop. `Listbox_PreviewMouseWheel` on every ListBox then swallows the wheel event (`Handled = true`) and scrolls directly with `ScrollToVerticalOffset` by a quarter of WPF's fixed `WheelScrollLines * 16px` per-notch amount. WPF ignores `e.Delta` magnitude, so scaling/re-raising the delta does nothing and causes double-processing. Don't remove it thinking it's a bug.
- No test suite or CI to run; verify by building on Windows. `.gitignore` excludes `bin/`, `obj/`, `*.exe`.
