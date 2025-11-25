## CFG2.Utils.SysLib

A library of system (FileSystem, Env Vars, etc) related functions.

Nothing in here is rocket science, but if it saves you from writing any boiler plate code over and over... you're welcome.

## Release Notes

### 1.0.12
- Added EmailAddress domain object
- Added PhoneNumber domain object

### 1.0.11
- Added MiscUtils with correlation id functions
- Deprecated SysLib in favor of SysUtils

### 1.0.10
- Added SanitizeFolderPath
- Keep returned value under 150 in GetFileNameSafeString
- Fix IsFileDifferentThanString and IsFileDifferent

### 1.0.9
- Output info about file diffs

### 1.0.8
- Cleanup

### 1.0.7
- Added SpecialFolder.Temp
- Added IsFileDifferent(file1, file2)
- Added IsFileDifferentThanString(file, content)

### 1.0.6
- Added GetFileNameSafeString

### 1.0.1
- Initial release