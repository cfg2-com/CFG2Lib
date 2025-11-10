# CFG2.Utils.AppLib

An opinionated application configuration library. Those opinions include, but are not limited to:

- An app config lib should be *simple* to configure (just ```dotnet add package CFG2.AppLib```)
- An app config lib should be *simple* to use (see example, but simply ```App app = new();```)
- An app config lib should default as much as possible (see opinions above)

Some ways AppLib attempts to be easy to use include:

- Providing a consistent filesystem structure for any of your applications.
- Providing a way to get application configuration properties.
- Providing utility functionality like key value pair (KVP) management, and deduplication control.

## Usage Examples

```
namespace CFG2.Test;

using CFG2.Utils.AppLib;

public class TestApp
{
    static void Main(string[] args)
    {
        App app = new();
        AppConfig appConfig = new AppConfig(app); // Optional, but extremely useful if you have configuration you want to load/save
        Deduper deduper = new Deduper(app, "MyAppDeduper"); // Optional, but useful if you want to track things you've already processed.

        app.Log("Logging a message via the default logger provided by App instance.");

        app.Trace(app.Name);
        app.Trace(app.Dir);
        app.Trace(app.LogDir);
        app.Trace(app.DataDir);
        app.Trace(app.BackupDir);
        app.Trace(app.SoftDeleteDir);
        app.Trace(app.BackupBaseDir);
        app.Trace(app.BackupRootDir);
        app.Trace(app.SyncDir);
        app.Trace(app.InboxDir);

        app.CleanupSoftDeleteDirectory(4);
        app.SoftDeleteFile(app.LogFile);

        app.Trace("Goodbye");
    }
}
```

## Release Notes

### 1.0.20
- Improved null value handling
- Updated documentation

### 1.0.18
- Using retentionDays for log cleanup
- Better protection against retentionDays <= 0 (i.e. don't delete anything)

### 1.0.17
- Added TempFile

### 1.0.16
- Added TempLocalDir
- Added TempDir

### 1.0.15
- SoftDeleteFile logging improvements

### 1.0.14
- Improved support for secure/encrypted app config properties.

### 1.0.13
- Added support for secure/encrypted app config properties.

### 1.0.10
- Bug fix for named app
- Bug fix for MDP Deduper

### 1.0.7
- Deduper Improvements

### 1.0.6
- Additional MigrationUtils

### 1.0.5
- Introduced MigrationUtils

### 1.0.4
- Moved ConfigProperties to AppConfig

### 1.0.2
- ConfigProperties added

### 1.0.1
- KVP added
- Deduper added

### 1.0.0
- Initial Release