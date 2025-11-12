namespace CFG2.Utils.AppLib;

public class Deduper
{
    private App _app;
    private string _name;
    private bool _global;
    private bool _useMDP;
    private string _file;
    private string _mdpFile;
    private KVP _kvp;

    /// <summary>
    /// Deduper constructor.
    /// </summary>
    /// <param name="app">The App to create this Deduper for.</param>
    /// <param name="name">A unique name for the deduper. The value will be forced to UPPER_CASE and should NOT contain any special characters.</param>
    /// <param name="global">If true, this deduper will be global (i.e. uniqueness will be looked at across any "app" - not just this one).</param>
    /// <param name="useMDP">If true, will use the MDP.db SQLite db at the app SyncDir. If false, will write to file either in the AppDataDir or BackupDir depending on value of <paramref name="global"/></param>
    public Deduper(App app, string name, bool global = false, bool useMDP = true)
    {
        _app = app;
        _name = name;
        _global = global;
        _useMDP = useMDP;

        Reload();
    }

    public bool UseMDP => _useMDP;

    public void Reload()
    {
        string group = _app.Name+" - "+_name;
        if (_global)
        {
            group = "GLOBAL - "+_name;
        }

        if (_useMDP)
        {
            _kvp = new KVPmdp(_app, group, "DEDUPER");
            _mdpFile = _app.GetMDP().File;
        }
        else
        {
            string dir = _app.DataDir;
            if (_global)
            {
                dir = _app.BackupDir;
            }
            _file = Path.Combine(dir, $"dedup-{_name.ToUpper()}.txt");
            _kvp = new KVPfile(_app, group, _file);
        }
    }

    public bool ContainsKey(string key)
    {
        return _kvp.ContainsKey(key);
    }

    public void AddItem(string key, string? debug = "")
    {
        _kvp.Add(key, key, debug);
    }

    public void RemoveItem(string key)
    {
        _kvp.Remove(key);
    }
    
    /// <summary>
    /// Get the full path to the file (plain txt or MDP SQLite db)
    /// </summary>
    /// <returns></returns>
    public string GetFile()
    {
        if (_useMDP)
        {
            return _mdpFile;
        }
        else
        {
            return _file;
        }
    }
}