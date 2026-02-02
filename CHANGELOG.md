# Changelog

## Version 2.0.0 (2026-02-02)

### 🎯 Major Stability Improvements

#### Fixed Critical Issues:
- ❌ **Removed `Thread.Abort()`** - Replaced dangerous thread termination with proper `CancellationToken` pattern
- ✅ **Added comprehensive exception handling** - All major code paths now wrapped in try-catch blocks
- ✅ **Fixed memory leaks** - Proper disposal of Bitmap objects and MemoryStreams using IDisposable pattern
- ✅ **Implemented thread safety** - Added lock statements to prevent race conditions on shared resources
- ✅ **Proper resource cleanup** - Clean shutdown of camera and threads on application exit

#### Technical Improvements:
- Background threads now use `IsBackground = true` for clean process termination
- Added `CacheOption.OnLoad` for BitmapImage to prevent file locking issues
- Improved camera switching with proper device cleanup
- Better error messages and user feedback
- Debug output for troubleshooting without crashes

#### What This Means:
- **No more random crashes** 🎉
- **Stable camera switching**
- **Better memory usage**
- **Cleaner application shutdown**
- **More reliable barcode detection**

### 📦 New Distribution

- **MSI Installer** - Professional Windows installer package
- Automatic installation to Program Files
- Start Menu shortcuts
- Clean uninstallation support

---

## How to Get It

### Download MSI Installer
Go to [Releases](https://github.com/pieewiee/Barcode-Reader/releases) and download the latest `BarcodeReader-Setup.msi`

### Install
1. Double-click the MSI file
2. Follow the installation wizard
3. Launch from Start Menu

### Requirements
- Windows 7 or later
- .NET Framework 4.5.2 or higher
- Webcam

---

## Previous Versions

See commit history for earlier changes.
