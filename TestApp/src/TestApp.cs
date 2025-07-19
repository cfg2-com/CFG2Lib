namespace CFG2.Test;

using CFG2.Utils.AppLib;
using CFG2.Utils.HttpLib;
using CFG2.Utils.SQLiteLib;

public class TestApp
{
    private static App app;

    static void Main(string[] args)
    {
        // Initialize App
        app = new();

        app.Log("Logging a message via the default logger provided by AppLib.");

        //TestAppConfig();
        //TestMigrateAppConfig();
        //TestMigrateFile();
        TestMigrateDeduper();
        //TestProperties();
        //TestAppDeduper();
        //TestGlobalMdpDeduper();
        //TestGlobalFileDeduper();
        //TestSoftDelete();
        //TestKVPfile();
        //TestKVPmdp();
        //TestHttp();

        app.Trace("Goodbye");
    }

    private static void TestAppConfig()
    {
        AppConfig appConfig = new AppConfig(app);
        appConfig.AddPersistedProperty("appBackupDir", app.BackupDir);
        app.Trace("prop val: " + appConfig.GetProperty("appBackupDir"));

        string tempProp = "temp-prop";
        app.Trace(tempProp+" (before): " + appConfig.GetProperty(tempProp));
        appConfig.AddTempProperty(tempProp, "some value");
        app.Trace(tempProp+" (after): " + appConfig.GetProperty(tempProp));
    }

    private static void TestMigrateAppConfig()
    {
        string testProp = "something";
        AppConfig appConfig = new AppConfig(app);
        string legacyFile = Path.Combine(app.Dir, "legacy.properties");
        app.Trace("Migration success (should be false)=" + MigrationUtils.MigrateAppConfigFile(legacyFile, appConfig));
        app.Trace("prop that doesn't exist: " + appConfig.GetProperty(testProp));

        File.WriteAllText(legacyFile, testProp+"=avalue");
        app.Trace("Migration success (should be true)=" + MigrationUtils.MigrateAppConfigFile(legacyFile, appConfig));
        app.Trace("prop that should exist: " + appConfig.GetProperty(testProp));

        File.Delete(appConfig.GetFile());
    }

    private static void TestMigrateFile()
    {
        string legacyFile = Path.Combine(app.Dir, "temp.txt");
        string newFile = Path.Combine(app.Dir, "new.txt");
        app.Trace("Migration success (should be false)=" + MigrationUtils.MigrateFile(legacyFile, newFile));

        File.WriteAllText(legacyFile, "Junk");
        app.Trace("Migration success (should be true)=" + MigrationUtils.MigrateFile(legacyFile, newFile));

        File.Delete(newFile);
    }

    private static void TestProperties()
    {
        app.Trace(app.Name);
        app.Trace(app.Dir);
        app.Trace(app.LogDir);
        app.Trace(app.DataDir);
        app.Trace(app.BackupDir);
        app.Trace(app.SoftDeleteDir);
        app.Trace(app.SyncDir);
        app.Trace(app.BackupBaseDir);
        app.Trace(app.BackupRootDir);
        app.Trace(app.InboxDir);
        app.Trace(app.GetMDP().File);
    }

    private static void TestKVPfile()
    {
        KVP kvpFile = new KVPfile(app, "TEST");
        kvpFile.Add("key1", "value1");
        kvpFile.Add("key2", "value2=5");
        app.Trace(kvpFile.Value("key1"));
        app.Trace(kvpFile.Value("key2"));
        app.Trace(kvpFile.ContainsKey("key1")+"");
    }

    private static void TestKVPmdp()
    {
        KVP kvpMDP = new KVPmdp(app, "test");
        kvpMDP.Add("key1", "value1");
        kvpMDP.Add("key2", "value2=5");
        app.Trace(kvpMDP.Value("key1"));
        app.Trace(kvpMDP.Value("key2"));
        app.Trace(kvpMDP.ContainsKey("key1")+"");
    }

    private static void TestAppDeduper()
    {
        Deduper deduper = new(app, "Test");
        app.Trace("Deduper File: "+deduper.GetFile());
        deduper.AddItem("test-key", "This is a test item");
        app.Trace("Deduper Key Exists: " + deduper.ContainsKey("test-key"));
        app.Trace("Deduper Key Never Exists: " + deduper.ContainsKey("test-key-never-exists"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("KEY_ID", "=", "test-key"),
            new WhereClause("GROUP_C", "=", "TEST")
        };
        app.GetMDP().DeleteRecords("DEDUPER", whereClauses);
    }

    private static void TestMigrateDeduper()
    {
        string legacyFile = Path.Combine(app.Dir, "temp.txt");
        File.WriteAllText(legacyFile, @"c:\some\file.txt");

        Deduper deduper = new(app, "Test");

        MigrationUtils.MigrateDeduper(legacyFile, deduper);

        app.Trace("Deduper Key Exists: " + deduper.ContainsKey(@"c:\some\file.txt"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("KEY_ID", "=", @"c:\some\file.txt"),
            new WhereClause("GROUP_C", "=", "TEST")
        };
        app.GetMDP().DeleteRecords("DEDUPER", whereClauses);
    }

    private static void TestGlobalMdpDeduper()
    {
        Deduper globalDeduper = new(app, "Test", true);
        app.Trace("Deduper File: "+globalDeduper.GetFile());
        globalDeduper.AddItem("test-key", "This is a global test item");
        app.Trace("Global Deduper Key Exists: " + globalDeduper.ContainsKey("test-key"));
        app.Trace("Global Deduper Key Never Exists: " + globalDeduper.ContainsKey("test-key-never-exists"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("KEY_ID", "=", "test-key"),
            new WhereClause("GROUP_C", "=", "GLOBAL_TEST")
        };
        app.GetMDP().DeleteRecords("DEDUPER", whereClauses);
    }

    private static void TestGlobalFileDeduper()
    {
        Deduper globalDeduper = new(app, "Test", true, false);
        app.Trace("Deduper File: "+globalDeduper.GetFile());
        globalDeduper.AddItem("test-key", "This is a global test item");
        app.Trace("Global Deduper Key Exists: " + globalDeduper.ContainsKey("test-key"));
        app.Trace("Global Deduper Key Never Exists: " + globalDeduper.ContainsKey("test-key-never-exists"));
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