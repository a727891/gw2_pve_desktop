#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VERSION="${1:-dev}"
VERSION="${VERSION#v}"

PUBLISH_DIR="$ROOT/artifacts/linux-x64"
APPDIR="$ROOT/artifacts/Gw2PveDesktop.AppDir"
OUTPUT="$ROOT/artifacts/Gw2PveDesktop-${VERSION}-x86_64.AppImage"
APPIMAGETOOL="$ROOT/artifacts/appimagetool-x86_64.AppImage"

rm -rf "$PUBLISH_DIR" "$APPDIR" "$OUTPUT"
mkdir -p "$PUBLISH_DIR" "$ROOT/artifacts"

echo "Publishing self-contained linux-x64 build..."
dotnet publish Gw2PveDesktop.Avalonia/Gw2PveDesktop.Avalonia.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:DebugType=none \
  -p:PublishSingleFile=false \
  -o "$PUBLISH_DIR"

mkdir -p "$APPDIR/usr/bin"
cp -a "$PUBLISH_DIR"/. "$APPDIR/usr/bin/"

cp Gw2PveDesktop.Avalonia/Assets/1128644.png "$APPDIR/gw2-pve-desktop.png"

cat > "$APPDIR/gw2-pve-desktop.desktop" << 'EOF'
[Desktop Entry]
Type=Application
Name=GW2 PvE Desktop
Comment=Guild Wars 2 daily raid bounties and fractals
Exec=Gw2PveDesktop
Icon=gw2-pve-desktop
Terminal=false
Categories=Game;Utility;
StartupNotify=false
EOF

cat > "$APPDIR/AppRun" << 'EOF'
#!/bin/sh
HERE="$(dirname "$(readlink -f "${0}")")"
export LD_LIBRARY_PATH="${HERE}/usr/bin:${LD_LIBRARY_PATH:-}"
exec "${HERE}/usr/bin/Gw2PveDesktop" "$@"
EOF

chmod +x "$APPDIR/AppRun" "$APPDIR/usr/bin/Gw2PveDesktop"

if [ ! -x "$APPIMAGETOOL" ]; then
  echo "Downloading appimagetool..."
  curl -fsSL -o "$APPIMAGETOOL" \
    "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
  chmod +x "$APPIMAGETOOL"
fi

echo "Building AppImage..."
ARCH=x86_64 "$APPIMAGETOOL" --appimage-extract-and-run "$APPDIR" "$OUTPUT"

echo "Created $OUTPUT"
