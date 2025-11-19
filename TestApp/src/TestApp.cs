namespace CFG2.Test;

using CFG2.Utils.AppLib;
using CFG2.Utils.HttpLib;
using CFG2.Utils.SecLib;
using CFG2.Utils.SQLiteLib;
using CFG2.Utils.SysLib;

public class TestApp
{
    private static App _app;
    private static App _app2;

    static void Main(string[] args)
    {
        // Initialize App
        _app = new();
        _app2 = new("Named Test App", false);

        AppConfig appConfig = new AppConfig(_app); // Optional, but extremely useful if you have configuration you want to load/save
        Deduper deduper = new Deduper(_app, "TestAppDeduper"); // Optional, but useful if you want to track things you've already processed.

        _app.Log("Logging a message via the default logger provided by AppLib.");

        //TestSecUtil();
        //TestSecurProp();
        //TestAppConfig();
        //TestMigrateAppConfig();
        //TestMigrateFile();
        //TestMigrateDeduper();
        //TestProperties();
        //TestAppMdpDeduper();
        //TestGlobalMdpDeduper();
        //TestGlobalFileDeduper();
        //TestSoftDelete();
        //TestKVPfile();
        //TestKVPmdp();
        TestHttp();
        //TestFileDiff();

        //_app = new(null, false); // Test out creating an App using the default name, but not in the sync directory.

        _app.Trace("Goodbye");
    }

    private static void TestSecUtil()
    {
        string password = "This is a test password";
        string originalValue = "This is a test value";
        string encryptedValue = SecUtil.Encrypt(password, "This is a test value");
        _app.Trace("Encrypted Value: " + encryptedValue);
        string decryptedValue = SecUtil.Decrypt(password, encryptedValue);
        _app.Trace("Decrypted Value: " + decryptedValue);
        _app.Trace($"Success: {originalValue == decryptedValue}");
    }

    private static void TestFileDiff()
    {
        string file1 = SysLib.GetTempFile();
        string file2 = SysLib.GetTempFile();

        string file1Content = "This is a test file.\nWith multiple lines.\nLine 3.\nLine 4.";
        File.WriteAllText(file1, file1Content);
        File.WriteAllText(file2, "This is a test file.\nWith multiple lines.\nLine 3 modified.\nLine 4.");

        bool differ = SysLib.IsFileDifferent(file1, file2);
        _app.Trace("Files differ (should return true): " + differ);

        differ = SysLib.IsFileDifferentThanString(file1, file1Content);
        _app.Trace("File differs from string (should return false): " + differ);

        File.Delete(file1);
        File.Delete(file2);
    }

    private static void TestSecurProp()
    {
        AppConfig appConfig = new AppConfig(_app, null, null, "thisIsMyTestPassword");
        bool success = appConfig.AddPersistedProperty("testSecureProp", "This is a secure property", "Test secure property", true);
        if (!success)
        {
            _app.Trace("WARN: Did not add property, but likely because it already exists.");
        }
        _app.Trace("Secure prop value: " + appConfig.GetProperty("testSecureProp", true));
    }

    private static void TestAppConfig()
    {
        AppConfig appConfig = new AppConfig(_app);
        appConfig.AddPersistedProperty("appBackupDir", _app.BackupDir);
        _app.Trace("prop val: " + appConfig.GetProperty("appBackupDir"));

        string tempProp = "temp-prop";
        _app.Trace(tempProp+" (before): " + appConfig.GetProperty(tempProp));
        appConfig.AddTempProperty(tempProp, "some value");
        _app.Trace(tempProp + " (after): " + appConfig.GetProperty(tempProp));

        appConfig.RemoveProperty(tempProp);
        _app.Trace("tempProp exists (should be false): "+appConfig.ContainsProperty(tempProp));
    }

    private static void TestMigrateAppConfig()
    {
        string testProp = "something";
        AppConfig appConfig = new AppConfig(_app);
        string legacyFile = Path.Combine(_app.Dir, "legacy.properties");
        _app.Trace("Migration success (should be false)=" + MigrationUtils.MigrateAppConfigFile(legacyFile, appConfig));
        _app.Trace("prop that doesn't exist: " + appConfig.GetProperty(testProp));

        File.WriteAllText(legacyFile, testProp+"=avalue");
        _app.Trace("Migration success (should be true)=" + MigrationUtils.MigrateAppConfigFile(legacyFile, appConfig));
        _app.Trace("prop that should exist: " + appConfig.GetProperty(testProp));

        File.Delete(appConfig.GetFile());
    }

    private static void TestMigrateFile()
    {
        string legacyFile = Path.Combine(_app.Dir, "temp.txt");
        string newFile = Path.Combine(_app.Dir, "new.txt");
        _app.Trace("Migration success (should be false)=" + MigrationUtils.MigrateFile(legacyFile, newFile));

        File.WriteAllText(legacyFile, "Junk");
        _app.Trace("Migration success (should be true)=" + MigrationUtils.MigrateFile(legacyFile, newFile));

        File.Delete(newFile);
    }

    private static void TestProperties()
    {
        _app.Trace(_app.Name);
        _app.Trace(_app.Dir);
        _app.Trace(_app.LogDir);
        _app.Trace(_app.DataDir);
        _app.Trace(_app.BackupDir);
        _app.Trace(_app.SoftDeleteDir);
        _app.Trace(_app.SyncDir);
        _app.Trace(_app.BackupBaseDir);
        _app.Trace(_app.BackupRootDir);
        _app.Trace(_app.InboxDir);
        _app.Trace(_app.GetMDP().File);
    }

    private static void TestKVPfile()
    {
        KVP kvpFile = new KVPfile(_app, "TEST");
        kvpFile.Add("key1", "value1");
        kvpFile.Add("key2", "value2=5");
        _app.Trace(kvpFile.Value("key1"));
        _app.Trace(kvpFile.Value("key2"));
        _app.Trace(kvpFile.ContainsKey("key1") + "");
        
        kvpFile.Remove("key1");
        _app.Trace("After removal, key1 exists (should be false): " + kvpFile.ContainsKey("key1"));
        _app.Trace("After removal, key2 exists (should be true): " + kvpFile.ContainsKey("key2"));
    }

    private static void TestKVPmdp()
    {
        KVP kvpMDP = new KVPmdp(_app, "test");
        kvpMDP.Add("key1", "value1");
        kvpMDP.Add("key2", "value2=5");
        _app.Trace(kvpMDP.Value("key1"));
        _app.Trace(kvpMDP.Value("key2"));
        _app.Trace(kvpMDP.ContainsKey("key1") + "");
        
        kvpMDP.Remove("key1");
        _app.Trace("After removal, key1 exists (should be false): " + kvpMDP.ContainsKey("key1"));
        _app.Trace("After removal, key2 exists (should be true): " + kvpMDP.ContainsKey("key2"));
    }

    private static void TestAppMdpDeduper()
    {
        Deduper deduper = new(_app, "Test Deduper");
        _app.Trace("Deduper File: "+deduper.GetFile());
        deduper.AddItem("test-key", "This is a test item");
        _app.Trace("Deduper Key Exists: " + deduper.ContainsKey("test-key"));
        _app.Trace("Deduper Key Never Exists: " + deduper.ContainsKey("test-key-never-exists"));

        Deduper deduper2 = new(_app2, "Test Deduper");
        deduper2.AddItem("test-key", "This is a test item");
        deduper2.AddItem("test-key-app2", "This is a test item only in app2");

        _app.Trace("Deduper Key Exists (true): " + deduper2.ContainsKey("test-key-app2"));
        _app.Trace("Deduper Key Exists (false): " + deduper.ContainsKey("test-key-app2"));

        List<WhereClause> whereClauses = new()
        {
            new WhereClause("KEY_ID", "LIKE", "test-key%"),
            new WhereClause("GROUP_C", "LIKE", "%TEST APP%")
        };
        _app.GetMDP().DeleteRecords("DEDUPER", whereClauses);
    }

    private static void TestMigrateDeduper()
    {
        string legacyFile = Path.Combine(_app.Dir, "temp.txt");
        File.WriteAllText(legacyFile, @"c:\some\file.txt");

        Deduper deduper = new(_app, "Test");

        MigrationUtils.MigrateDeduper(legacyFile, deduper);

        _app.Trace("Deduper Key Exists: " + deduper.ContainsKey(@"c:\some\file.txt"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("KEY_ID", "=", @"c:\some\file.txt"),
            new WhereClause("GROUP_C", "=", "TEST")
        };
        _app.GetMDP().DeleteRecords("DEDUPER", whereClauses);
    }

    private static void TestGlobalMdpDeduper()
    {
        Deduper globalDeduper = new(_app, "Test", true);
        _app.Trace("Deduper File: "+globalDeduper.GetFile());
        globalDeduper.AddItem("test-key", "This is a global test item");
        _app.Trace("Global Deduper Key Exists: " + globalDeduper.ContainsKey("test-key"));
        _app.Trace("Global Deduper Key Never Exists: " + globalDeduper.ContainsKey("test-key-never-exists"));
        List<WhereClause> whereClauses = new()
        {
            new WhereClause("KEY_ID", "=", "test-key"),
            new WhereClause("GROUP_C", "=", "GLOBAL - TEST")
        };
        _app.GetMDP().DeleteRecords("DEDUPER", whereClauses);
    }

    private static void TestGlobalFileDeduper()
    {
        Deduper globalDeduper = new(_app, "Test", true, false);
        _app.Trace("Deduper File: "+globalDeduper.GetFile());
        globalDeduper.AddItem("test-key", "This is a global test item");
        _app.Trace("Global Deduper Key Exists: " + globalDeduper.ContainsKey("test-key"));
        _app.Trace("Global Deduper Key Never Exists: " + globalDeduper.ContainsKey("test-key-never-exists"));
    }

    private static void TestSoftDelete()
    {
        _app.CleanupSoftDeleteDirectory(4);
        _app.SoftDeleteFile(_app.LogFile);
    }

    private static void TestHttp()
    {
        HttpRequest req = new("https://google.com");
        _app.Trace(HttpLib.Get(req).Content);
    }
}