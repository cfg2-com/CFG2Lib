namespace CFG2.Utils.AppLib;

public class Deduper
{
    private readonly string file;
    private readonly string mdpFile;
    private KVP kvp;

    /// <summary>
    /// Deduper constructor.
    /// </summary>
    /// <param name="app">The app to create this deduper for. Used to get various config info.</param>
    /// <param name="name">A unique name for the deduper. The value will be forced to UPPER_CASE and should NOT contain any special characters.</param>
    /// <param name="global">If true, this deduper will be global (i.e. uniqueness will be looked at across any "app" - not just this one).</param>
    /// <param name="useMDP">If true, will use the MDP.db SQLite db at the app SyncDir. If false, will write to file either in the AppDataDir or BackupDir depending on value of "global"</param>
    public Deduper(App app, string name, bool global = false, bool useMDP = true)
    {
        if (useMDP)
        {
            kvp = new KVPmdp(app, name, "DEDUPER");
            mdpFile = app.GetMDP().File;
        }
        else
        {
            string dir = app.DataDir;
            if (global)
            {
                name = "GLOBAL_" + name;
                dir = app.BackupDir;
            }
            file = Path.Combine(dir, $"dedup-{name.ToUpper()}.txt");
            kvp = new KVPfile(app, name, file);
        }
    }

    public bool ContainsKey(string key)
    {
        return kvp.ContainsKey(key);
    }

    public void AddItem(string key, string debug)
    {
        kvp.Add(key, key, debug);
    }
    
    /// <summary>
    /// Get the full path to the file
    /// </summary>
    /// <returns></returns>
    public string GetFile()
    {
        return file;
    }

    /// <summary>
    /// Get the full path to the MDP SQLite database holding the values
    /// </summary>
    /// <returns></returns>
    public string GetDB()
    {
        return mdpFile;
    }
}