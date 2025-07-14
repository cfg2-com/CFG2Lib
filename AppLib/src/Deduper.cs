namespace CFG2.Utils.AppLib;

using CFG2.Utils.LogLib;
using CFG2.Utils.SQLiteLib;

public class Deduper
{
    private KVP kvp;

    /// <summary>
    /// Deduper constructor.
    /// </summary>
    /// <param name="app">The app to create this deduper for. Used to get various config info.</param>
    /// <param name="name">A unique name for the deduper. The value will be forced to UPPER_CASE and should NOT contain any special characters.</param>
    /// <param name="global">If true, this deduper will be global (i.e. uniqueness will be looked at across any "app" - not just this one).</param>
    /// <param name="useMDP">If true, will use the MDP.db SQLite db at the app SyncDir. If false, will write to file either in the AppDataDir or BackupDir depending on value of "global"</param>
    public Deduper(AppLib app, string name, bool global = false, bool useMDP = true)
    {        
        if (useMDP)
        {
            kvp = new KVPmdp(app, name, "DEDUPER");
        }
        else
        {
            string dir = app.AppDataDir;
            if (global)
            {
                name = "GLOBAL_" + name;
                dir = app.BackupDir;
            }
            string file = Path.Combine(dir, $"dedup-{name.ToUpper()}.txt");
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
}