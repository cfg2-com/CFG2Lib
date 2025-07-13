namespace CFG2.Test;

using CFG2.Utils.AppLib;
using CFG2.Utils.HttpLib;
using CFG2.Utils.SQLiteLib;

public class TestApp
{
    private static AppLib app;

    static void Main(string[] args)
    {
        // Initialize AppLib
        app = new();

        app.Log("Logging a message via the default logger provided by AppLib.");

        TestProperties();
        TestAppDeduper();
        TestGlobalDeduper();
        TestSoftDelete();
        //TestHttp();

        app.Trace("Goodbye");
    }

    private static void TestProperties()
    {
        app.Trace(app.AppName);
        app.Trace(app.AppDir);
        app.Trace(app.AppLogDir);
        app.Trace(app.AppDataDir);
        app.Trace(app.AppBackupDir);
        app.Trace(app.AppSoftDeleteDir);
        app.Trace(app.SyncDir);
        app.Trace(app.BackupDir);
        app.Trace(app.InboxDir);
        app.Trace(app.GetMDP().File);
    }

    private static void TestAppDeduper()
    {
        Deduper deduper = new(app, "Test");
        deduper.AddItem("test-key", "This is a test item");
        app.Trace("Deduper Key Exists: " + deduper.ContainsKey("test-key"));
        app.Trace("Deduper Key Never Exists: " + deduper.ContainsKey("test-key-never-exists"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("DEDUP_ID", "=", "TEST_test-key")
        };
        app.GetMDP().DeleteRecords("DEDUP", whereClauses);
    }

    private static void TestGlobalDeduper()
    {
        Deduper globalDeduper = new(app, "Test", true);
        globalDeduper.AddItem("test-key", "This is a global test item");
        app.Trace("Global Deduper Key Exists: " + globalDeduper.ContainsKey("test-key"));
        app.Trace("Global Deduper Key Never Exists: " + globalDeduper.ContainsKey("test-key-never-exists"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("DEDUP_ID", "=", "GLOBAL_TEST_test-key")
        };
        app.GetMDP().DeleteRecords("DEDUP", whereClauses);
    }

    private static void TestSoftDelete()
    {
        app.CleanupSoftDeleteDirectory(4);
        app.SoftDeleteFile(app.LogFile);
    }

    private static void TestHttp()
    {
        HttpRequest req = new("https://google.com");
        app.Trace(HttpLib.Get(req).Content);
    }
}