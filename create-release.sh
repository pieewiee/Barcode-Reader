#!/bin/bash
set -e

echo "Creating release package..."

# Create release directory
mkdir -p release/BarcodeReader-v2.0.0
cd "QR Code Reader/bin/Release"

# Copy all necessary files
cp "Barcode Reader.exe" ../../../release/BarcodeReader-v2.0.0/
cp "Barcode Reader.exe.config" ../../../release/BarcodeReader-v2.0.0/
cp *.dll ../../../release/BarcodeReader-v2.0.0/ 2>/dev/null || true
cp -r Resources ../../../release/BarcodeReader-v2.0.0/ 2>/dev/null || true

cd ../../../release

# Create README
cat > BarcodeReader-v2.0.0/README.txt << 'EOREADME'
Barcode Reader v2.0.0
=====================

MAJOR STABILITY IMPROVEMENTS!
- Fixed crashes (no more Thread.Abort)
- Better memory management  
- Thread-safe operations
- Improved error handling

REQUIREMENTS:
- Windows 7 or later
- .NET Framework 4.5.2+
- Webcam

INSTALLATION:
1. Extract this ZIP
2. Run "Barcode Reader.exe"
3. Done!

CHANGES:
See CHANGELOG.md on GitHub:
https://github.com/pieewiee/Barcode-Reader

LICENSE: MIT
EOREADME

# Create ZIP
zip -r "BarcodeReader-v2.0.0.zip" "BarcodeReader-v2.0.0"

echo "✅ Release package created: release/BarcodeReader-v2.0.0.zip"
ls -lh BarcodeReader-v2.0.0.zip
