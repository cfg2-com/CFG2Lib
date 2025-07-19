namespace CFG2.Utils.AppLib;

public class Deduper
{
    private App app;
    private string name;
    private bool global;
    private bool useMDP;
    private string file;
    private string mdpFile;
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
        this.app = app;
        this.name = name;
        this.global = global;
        this.useMDP = useMDP;

        Reload();
    }

    public bool UseMDP => useMDP;

    public void Reload()
    {
        string group = app.Name+" - "+name;
        if (global)
        {
            group = "GLOBAL - "+name;
        }

        if (useMDP)
        {
            kvp = new KVPmdp(app, group, "DEDUPER");
            mdpFile = app.GetMDP().File;
        }
        else
        {
            string dir = app.DataDir;
            if (global)
            {
                dir = app.BackupDir;
            }
            file = Path.Combine(dir, $"dedup-{name.ToUpper()}.txt");
            kvp = new KVPfile(app, group, file);
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
    /// Get the full path to the file (plain txt or MDP SQLite db)
    /// </summary>
    /// <returns></returns>
    public string GetFile()
    {
        if (useMDP)
        {
            return mdpFile;
        }
        else
        {
            return file;
        }
    }
}