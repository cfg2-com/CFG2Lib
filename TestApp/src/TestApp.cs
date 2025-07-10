namespace CFG2.Test;

using CFG2.Utils.AppLib;
using CFG2.Utils.HttpLib;

public class TestApp
{
    static void Main(string[] args)
    {
        // Initialize AppLib
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

        HttpRequest req = new("https://google.com");
        app.Trace(HttpLib.Get(req).Content);

        app.Trace("Goodbye");
    }
}