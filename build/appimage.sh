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

ICON_NAME=gw2-pve-desktop
ICON_ICO="${ICON_ICO:-Gw2PveDesktop/Assets/1128644.ico}"
ICON_PNG="${ICON_PNG:-Gw2PveDesktop.Avalonia/Assets/1128644.png}"

install_appimage_icons() {
  local root_icon="$APPDIR/${ICON_NAME}.png"

  if command -v magick >/dev/null 2>&1; then
    echo "Preparing icons from ${ICON_ICO}..."
    # Frame 5 is the largest embedded size (96x96) in this ICO.
    magick "${ICON_ICO}[5]" -filter Lanczos -resize 256x256 "$root_icon"
    for size in 48 64 96 128 256; do
      local themed_icon="$APPDIR/usr/share/icons/hicolor/${size}x${size}/apps/${ICON_NAME}.png"
      mkdir -p "$(dirname "$themed_icon")"
      magick "${ICON_ICO}[5]" -filter Lanczos -resize "${size}x${size}" "$themed_icon"
    done
  else
    echo "ImageMagick not found; using ${ICON_PNG} (install imagemagick for sharper icons)."
    cp "$ICON_PNG" "$root_icon"
    for size in 48 64 96 128 256; do
      local themed_icon="$APPDIR/usr/share/icons/hicolor/${size}x${size}/apps/${ICON_NAME}.png"
      mkdir -p "$(dirname "$themed_icon")"
      cp "$ICON_PNG" "$themed_icon"
    done
  fi
}

install_appimage_icons

mkdir -p "$APPDIR/usr/share/metainfo"
cp build/gw2-pve-desktop.appdata.xml "$APPDIR/usr/share/metainfo/io.github.a727891.gw2_pve_desktop.appdata.xml"

cat > "$APPDIR/gw2-pve-desktop.desktop" << EOF
[Desktop Entry]
Type=Application
Name=GW2 PvE Desktop
Comment=Guild Wars 2 daily raid bounties and fractals
Exec=Gw2PveDesktop
Icon=${ICON_NAME}
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

ICON_EXPORT="$ROOT/artifacts/${ICON_NAME}.png"
cp "$APPDIR/${ICON_NAME}.png" "$ICON_EXPORT"

ICON_URI="file://$(realpath "$ICON_EXPORT")"
echo "Setting Dolphin custom icon..."
if command -v gio >/dev/null 2>&1; then
  gio set "$OUTPUT" metadata::custom-icon "$ICON_URI"
  echo "Applied: gio set \"$OUTPUT\" metadata::custom-icon \"$ICON_URI\""
else
  echo "gio not found; run manually after build:"
  echo "  gio set \"$OUTPUT\" metadata::custom-icon \"$ICON_URI\""
fi

echo "Created $OUTPUT"
