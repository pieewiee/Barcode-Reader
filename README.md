# Barcode Reader for Windows

### Basic AF Barcode Reader which I forked and copied + pasted together

Clean and easy Barcode Reader:

![alt text](https://github.com/pieewiee/QR-Code-Reader/blob/master/Examaple.png)

## Features

- detect barcodes (See: Supported Formats)
- draw detected barcodes
- loop through all cameras
- Open Weblink based on whitelist (Regex)
- CopyAndPaste function (Paste Barcode content)
- Aiming Help & Sound Feedback
- Save Image 2 Disk
- Adjust: Frames, Timeout, Detection Freeze Time
- define custom logo and color theme (config.xml)
- Debug level (0 - 4 --> config.xml)

## Usage

.NET Framework 4.5.2 required

```
run "Barcode Reader.exe"
```

Base configuration is stored in Resources\template.xml which is located in binary folder. User configuration is stored in %UserProfile%\Documents\Barcode-Reader\config.xml

```xml
<?xml version="1.0"?>
<Config>
  <Base Frames="0" Timeout="1" Freeze="20" Sound="1" Aim="1" DefaultCam="0" Debug="0" Color="FF1A347F" Logo="Resources\logo.png" />
  <Domain PrefixValue="" Value="https:\/\/.*" />
</Config>
```

## Supported Formats

| 1D product            | 1D industrial | 2D           |
| :-------------------- | :------------ | :----------- |
| UPC-A                 | Code 39       | QR Code      |
| UPC-E                 | Code 93       | Data Matrix  |
| EAN-8                 | Code 128      | Aztec        |
| EAN-13                | Codabar       | PDF 417      |
| UPC/EAN Extension 2/5 | ITF           | MaxiCode     |
|                       |               | RSS-14       |
|                       |               | RSS-Expanded |

## Credits

this project uses ZXing&#46;Net library for decoding barcodes in images and it uses AForge Framework for accessing webcam ressources.

| Library            | license                                                  |
| :----------------- | :------------------------------------------------------- |
| `ZXing.Net`        | https://github.com/micjahn/ZXing.Net/blob/master/COPYING |
| `AForge Framework` | http://www.aforgenet.com/framework/license.html          |
