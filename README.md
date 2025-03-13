# NetdiskEncryptUploader
A program to encrypt file so as to upload files to netdisks without privacy concern

by GitHub @[Keqing-Yuheng](https://github.com/Keqing-Yuheng)

## Overview

The project is designed to help utilize netdisks as a convenient off-site backup and avoid trouble including privacy leak and file banning by encrypting files.

![NetdiskEncryptUploader Example](https://raw.githubusercontent.com/Keqing-Yuheng/NetdiskEncryptUploader/main/Docs/NetdiskEncryptUploader-Example.PNG)

## Features

**Ver 1.1.0**

Implemented:

- [x] Encrypting files
- [x] Skipping automatically when the file has no modification
- [x] Multi-thread
- [x] One-click uploading encrypted files (adding uploading operation to config is needed).

In Progress:

- [ ] ~~One-click decrypting files~~ (Manual decryption is needed at present)
- [ ] ~~Command-Line Interface~~
- [ ] ~~GUI Configuration Editor~~ (Only support editing config.json directly at present)
- [ ] ~~Language: Simplified Chinese~~

## Quick Start

1. Let the program create `config.json` and then edit it, especially the password in it. As for other settings, **see `Docs/Instructions.md`**.
2. Create a plain text file `input.txt` (or the name you have written in Config `inputList`). Write the path of files or directories (Both absolute path and relative path are acceptable) in it, one item per line.
3. Run the program and then click `START/STOP`. Wait until the icon of a green check appears, which indicates the completion of the task.
4. Encrypted files are in the folder `output` (or the name you have written in Config `outputDir`). If the upload operation is defined in Config (For example, run a certain program to upload output folder), click `Upload`. After uploading, use `Clear Output ` to delete encrypted files to save disk space (Original files would not be deleted).
5. When the program is run next time, it will check whether files are new or modified (by hash). Only new or modified files will be processed. Old files will be skipped to save time.

## Runtime

.NET 8.0

## Building

Visual Studio 2022

NuGet Budge:
-  SevenZipSharp ([NuGet](https://www.nuget.org/packages/Squid-Box.SevenZipSharp/) | [GitHub](https://github.com/squid-box/SevenZipSharp))
-  WpfAnimatedGif ([NuGet](https://www.nuget.org/packages/WpfAnimatedGif) | [GitHub](https://github.com/XamlAnimatedGif/WpfAnimatedGif))

DLL:
- 7z.dll ([7-Zip Website](https://www.7-zip.org/)) (Attention: 7z.dll in this repository might not be latest)

## Update

### Ver 1.0.1
- Multi-thread compression
- Update relative filename format
- Bug fix

## License

[![LGPL-v3](https://www.gnu.org/graphics/lgplv3-with-text-154x68.png)](https://www.gnu.org/licenses/lgpl-3.0.txt)

LGPL-v3