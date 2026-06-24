# GW2 PvE Desktop

[![CI](https://github.com/a727891/gw2_pve_desktop/actions/workflows/ci.yml/badge.svg)](https://github.com/a727891/gw2_pve_desktop/actions/workflows/ci.yml)

A system-tray app for **Guild Wars 2** that shows daily **Raid Bounties** and **Fractals** (T4 dailies and Challenge Motes) with a Guild Wars–style popup.

Available on **Windows** (WPF) and **Linux** (Avalonia).

## Example

![Popup screenshot](ref/example.png)

## Features

- **Raid Bounties** - Today and tomorrow’s bounty bosses with icons
- **Fractals** - Toggle between:
  - **Dailies** - Tier 4 fractals for today and tomorrow with instabilities
  - **CMs** - Challenge Mote fractals (scales 95–100) with current instabilities in two columns
- **Reset countdown** - Time until daily reset at the bottom of the popup
- **System tray** - Runs in the tray; left-click or “Show” to open the popup (opens automatically on startup after data loads)
- **Guild Wars–style UI** - Wispy transparent-edged background (asset 1909321), Menomonia font, and cream/gold palette

## Requirements

### Windows

- **Windows** (x64)
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (if not using a self-contained build)

### Linux

- **Linux** (x64)
- For the portable **AppImage** release: no .NET runtime required
- For framework-dependent builds: [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Typical Avalonia dependencies: `fontconfig`, `libX11`, `libICE`, `libSM`, `libXcursor`
- **System tray**: works on KDE and most X11 sessions; on GNOME Wayland you may need a tray extension (AppIndicator / KStatusNotifierItem support)

## Download

Pre-built releases are available on the [Releases](https://github.com/a727891/gw2_pve_desktop/releases) page:

1. Open the [Releases](https://github.com/a727891/gw2_pve_desktop/releases) page.
2. Choose the latest release (or a specific version).
3. Under **Assets**, download:
   - **Windows**: `Gw2PveDesktop-win-x64.zip` (extract and run `Gw2PveDesktop.exe`)
   - **Linux**: `Gw2PveDesktop-<version>-x86_64.AppImage` (make executable and run)
4. Run the executable (unzip first if you downloaded an archive).

**Linux AppImage:**

```bash
chmod +x Gw2PveDesktop-*-x86_64.AppImage
./Gw2PveDesktop-*-x86_64.AppImage
```

## Build

### Windows (WPF)

```bash
cd Gw2PveDesktop
dotnet build
```

**Release (single-file exe):**

```bash
dotnet publish -c Release
```

Output: `Gw2PveDesktop/bin/Release/net8.0-windows/win-x64/Gw2PveDesktop.exe`

### Linux (Avalonia)

```bash
dotnet build Gw2PveDesktop.Avalonia/Gw2PveDesktop.Avalonia.csproj
```

**Release publish:**

```bash
./publish.sh
```

Output: `publish/linux-x64/Gw2PveDesktop`

**AppImage (self-contained, portable):**

```bash
chmod +x build/appimage.sh
./build/appimage.sh          # creates artifacts/Gw2PveDesktop-dev-x86_64.AppImage
./build/appimage.sh 1.0.0    # optional version label in filename
```

The build also writes `artifacts/gw2-pve-desktop.png` and applies a Dolphin custom icon to the new AppImage when possible.

**Dolphin file icon:** The icon is embedded in the AppImage, but Dolphin needs system support to display it (the generic gear/arrow is shown otherwise). On Nobara/Fedora:

```bash
sudo dnf install libappimage
```

Then in Dolphin: **Settings → Configure Dolphin → General → Previews** → enable **AppImage application bundle**. Clear cached thumbnails if needed:

```bash
rm -rf ~/.cache/thumbnails/*
kbuildsycoca6 --noincremental
```

For a one-off fix on an already-built AppImage:

```bash
gio set ./artifacts/Gw2PveDesktop-*.AppImage metadata::custom-icon file://$(pwd)/artifacts/gw2-pve-desktop.png
```

Optional: install the desktop entry and icon:

```bash
cp Gw2PveDesktop.Avalonia/gw2-pve-desktop.desktop ~/.local/share/applications/
cp Gw2PveDesktop.Avalonia/Assets/1128644.png ~/.local/share/icons/gw2-pve-desktop.png
```

Ensure `publish/linux-x64` is on your `PATH`, or edit the `Exec=` line in the `.desktop` file to the full path.

## Run

- Run the app; it will appear in the system tray (if your desktop supports it).
- The popup opens automatically after the first successful data load.
- **Tray left-click** or **Show** in the context menu - open or focus the popup.
- **Refresh** - reload schedule data from the API.
- **Exit** - quit the app.

On Linux without a working system tray, the popup still opens automatically after data loads.

## Data source

Schedule data (fractal maps, instabilities, daily bounties, raid/strike info) is loaded from a static JSON API (same source as [BlishHud Raid Clears](https://github.com/a727891/BlishHud-Raid-Clears)). Icons are fetched from [gw2dat.com](https://assets.gw2dat.com/) and cached locally (`%LOCALAPPDATA%/Gw2PveDesktop/cache` on Windows, `~/.cache/gw2-pve-desktop` on Linux).

## Project structure

```
Gw2PveDesktop.Core/           # Shared models, services, view models
Gw2PveDesktop/                # Windows WPF app (tray + popup UI)
Gw2PveDesktop.Avalonia/       # Linux Avalonia app (tray + popup UI)
build/appimage.sh             # Linux AppImage packaging script
.github/workflows/            # CI and release automation
```

## CI/CD

GitHub Actions builds the solution on every push and pull request:

- **Linux**: builds `Gw2PveDesktop.Core` and `Gw2PveDesktop.Avalonia`
- **Windows**: builds `Gw2PveDesktop.Core` and the WPF app

Pushing a version tag (`v*`, e.g. `v1.0.0`) or running the **Release** workflow manually uploads:

- `Gw2PveDesktop-<version>-x86_64.AppImage` (Linux)
- `Gw2PveDesktop-win-x64.zip` (Windows)

## License

This project is licensed under the [MIT License](LICENSE): free to use, modify, and distribute, with no warranty and no liability.
