#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
APP_NAME="Guitar TUI"
BUNDLE_ID="com.idontbyte.guitartui"
RUNTIME="${1:-osx-arm64}"
CONFIGURATION="Release"
ARTIFACT_DIR="$ROOT_DIR/artifacts/macos"
PUBLISH_DIR="$ARTIFACT_DIR/publish-$RUNTIME"
APP_DIR="$ARTIFACT_DIR/$APP_NAME.app"
DMG_ROOT="$ARTIFACT_DIR/dmg-root"
DMG_PATH="$ARTIFACT_DIR/$APP_NAME-$RUNTIME.dmg"
ICON_PNG="$ARTIFACT_DIR/GuitarTUIIcon.png"
ICONSET="$ARTIFACT_DIR/GuitarTUI.iconset"

rm -rf "$ARTIFACT_DIR"
mkdir -p "$ARTIFACT_DIR"

dotnet publish "$ROOT_DIR/guitar-resources-tui.csproj" \
  -c "$CONFIGURATION" \
  -r "$RUNTIME" \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:DebugType=None \
  -p:DebugSymbols=false \
  -o "$PUBLISH_DIR"

mkdir -p "$APP_DIR/Contents/MacOS" "$APP_DIR/Contents/Resources/app"
cp -R "$PUBLISH_DIR/." "$APP_DIR/Contents/Resources/app/"

cat > "$APP_DIR/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>en</string>
  <key>CFBundleDisplayName</key>
  <string>$APP_NAME</string>
  <key>CFBundleExecutable</key>
  <string>GuitarTUI</string>
  <key>CFBundleIconFile</key>
  <string>GuitarTUI</string>
  <key>CFBundleIdentifier</key>
  <string>$BUNDLE_ID</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>$APP_NAME</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>1.0.0</string>
  <key>CFBundleVersion</key>
  <string>1</string>
  <key>LSMinimumSystemVersion</key>
  <string>13.0</string>
</dict>
</plist>
PLIST

cat > "$APP_DIR/Contents/MacOS/GuitarTUI" <<'LAUNCHER'
#!/usr/bin/env bash
set -euo pipefail

APP_CONTENTS="$(cd "$(dirname "$0")/.." && pwd)"
CLI="$APP_CONTENTS/Resources/app/guitar-resources-tui"

osascript - "$CLI" <<'APPLESCRIPT'
on run argv
  set cliPath to item 1 of argv
  tell application "Terminal"
    activate
    do script quoted form of cliPath
  end tell
end run
APPLESCRIPT
LAUNCHER
chmod +x "$APP_DIR/Contents/MacOS/GuitarTUI"

clang -fobjc-arc -framework AppKit "$ROOT_DIR/packaging/macos/GuitarTUIIcon.m" -o "$ARTIFACT_DIR/GuitarTUIIconGen"
"$ARTIFACT_DIR/GuitarTUIIconGen" "$ICON_PNG"
mkdir -p "$ICONSET"
sips -z 16 16 "$ICON_PNG" --out "$ICONSET/icon_16x16.png" >/dev/null
sips -z 32 32 "$ICON_PNG" --out "$ICONSET/icon_16x16@2x.png" >/dev/null
sips -z 32 32 "$ICON_PNG" --out "$ICONSET/icon_32x32.png" >/dev/null
sips -z 64 64 "$ICON_PNG" --out "$ICONSET/icon_32x32@2x.png" >/dev/null
sips -z 128 128 "$ICON_PNG" --out "$ICONSET/icon_128x128.png" >/dev/null
sips -z 256 256 "$ICON_PNG" --out "$ICONSET/icon_128x128@2x.png" >/dev/null
sips -z 256 256 "$ICON_PNG" --out "$ICONSET/icon_256x256.png" >/dev/null
sips -z 512 512 "$ICON_PNG" --out "$ICONSET/icon_256x256@2x.png" >/dev/null
sips -z 512 512 "$ICON_PNG" --out "$ICONSET/icon_512x512.png" >/dev/null
cp "$ICON_PNG" "$ICONSET/icon_512x512@2x.png"
iconutil -c icns "$ICONSET" -o "$APP_DIR/Contents/Resources/GuitarTUI.icns"

if command -v codesign >/dev/null 2>&1; then
  codesign --force --deep --sign - "$APP_DIR" >/dev/null
fi

mkdir -p "$DMG_ROOT"
cp -R "$APP_DIR" "$DMG_ROOT/"
ln -s /Applications "$DMG_ROOT/Applications"

hdiutil create \
  -volname "$APP_NAME" \
  -srcfolder "$DMG_ROOT" \
  -ov \
  -format UDZO \
  "$DMG_PATH"

echo "$DMG_PATH"
