# Instructions - NetdiskEncryptUploader

Ver 1.0.0

## Arguments in config.json

### string: inputList
Path of a plain text file, which includes paths of files and directories to encrypt.

### string: outputDir
Path of the folder where encrypted files are stored.

### string: passwd
Password for encryption.

### bool: isHashEnabled
`true`: the program will check whether a file is new or modified. If a certain file has no modification, it will be skipped to save time.

*This argument can be set in the program by clicking the checkbox, which is valid until the program exits. The value in Config only determines the initial value.*

### bool: isEncryptEnabled
`true`: the program will generate encrypted files

*This argument can be set in the program by clicking the checkbox, which is valid until the program exits. The value in Config only determines the initial value.*

### string: replacementForColonInPath
The name of encrypted file (and hash) originates from the path of its original file, but `:` and `\` need to be replaced since they cannot be a part of a filename. This argument specifies one or multiple character(s) to **replace `:`**. ATTENTION: characters like `:`, `\`, `/`, `*`, etc. are unacceptable, which cannot be used in filenames.

### string: replacementForBackslashInPath
The name of encrypted file (and hash) originates from the path of its original file, but `:` and `\` need to be replaced since they cannot be a part of a filename. This argument specifies one or multiple character(s) to **replace `\`**. ATTENTION: characters like `:`, `\`, `/`, `*`, etc. are unacceptable, which cannot be used in filenames.

### bool: isForceAbsolutePath
`true`: The filename of the encrypted file (and hash) will use the absolute path of its original file.

### bool: isForceRelativePath
`true`: When the program processes a **directory**, the filename of the encrypted file (and hash) will use the relative path of its original file.

### int: compressionLevel
7-zip compression level.
|Level|Name|Generated File Size|Speed|
|-|-|-|-|
|0|None|Largest|Fastest|
|1|Fast|↑|↓|
|2|Low|↑|↓|
|3|Normal|↑|↓|
|4|High|↑|↓|
|5|Ultra|Smallest|Slowest|

*This argument can be set in the program by slide the slider, which is valid until the program exits. The value in Config only determines the initial value.*

### bool: isUploadShown
`true`: Show the Upload button so as to execute uploading process specified in Config.

### string: uploadProgram
The program that the uploading process needs.

### string: uploadArg
The arguments that the uploading program needs.

### string: logPath
Path of the log file. Change it to a empty string (`""`) to disable log.

### int: minLogLevel
The minimal level of the log written to log file. Change it to 5 to disable writing log to log file.
|Level|Name|
|-|-|
|0|DEBUG|
|1|INFO|
|2|WARN|
|3|ERROR|
|4|FATAL|