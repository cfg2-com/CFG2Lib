# CFG2.Utils.AppLib

An opinionated application configuration library. Those opinions include, but are not limited to:

- An app config lib should be *simple* to configure (just ```dotnet add package CFG2.AppLib```)
- An app config lib should be *simple* to use (see example)
- An app config lib should default as much as possible (see opinions above)

### Usage Examples

```
namespace CFG2.Test;

using CFG2.Utils.AppLib;

public class TestApp
{
    static void Main(string[] args)
    {
        AppLib app = new();

        app.Log("Logging a message via the default logger provided by AppLib.");

        app.Trace(app.AppName);
        app.Trace(app.AppDir);
        app.Trace(app.AppLogDir);
        app.Trace(app.AppDataDir);
        app.Trace(app.AppBackupDir);
        app.Trace(app.AppSoftDeleteDir);
        app.Trace(app.SyncDir);
        app.Trace(app.BackupDir);
        app.Trace(app.InboxDir);

        app.CleanupSoftDeleteDirectory(4);
        app.SoftDeleteFile(app.LogFile);

        app.Trace("Goodbye");
    }
}
```

## Release Notes

### 1.0.0
- Initial Release