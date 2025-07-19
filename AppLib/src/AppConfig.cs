using System.ComponentModel;

namespace CFG2.Utils.AppLib;

public class AppConfig
{
    private KVP kvp;
    private KVP kvpMDP;
    private KVP kvpFile;

    /// <summary>
    /// Deduper constructor.
    /// </summary>
    /// <param name="app">The app to for these configuration properties.</param>
    /// <param name="cfgFile">If a file with this name exists in the AppDir, it will be read.</param>
    /// <param name="mdpTable">This table will be read from MDP.db in the SyncDir.</param>
    public AppConfig(App app, string? cfgFile = "app.cfg", string? mdpTable = "APP_CONFIG")
    {
        if (string.IsNullOrEmpty(cfgFile)) { cfgFile = "app.cfg"; }
        if (string.IsNullOrEmpty(mdpTable)) { mdpTable = "APP_CONFIG"; }

        kvp = new KVPmemory(app);

        kvpMDP = new KVPmdp(app, app.Name, mdpTable, "KEY_ID", "APP_C");
        foreach (string key in kvpMDP.Keys)
        {
            kvp.Add(key, kvpMDP.Value(key));
        }

        string file = Path.Combine(app.Dir, cfgFile);
        if (File.Exists(file))
        {
            kvpFile = new KVPfile(app, app.Name, file);
            foreach (string key in kvpFile.Keys)
            {
                kvp.Add(key, kvpFile.Value(key));
            }
        }

        //app.Trace(kvp.)
    }

    public bool ContainsProperty(string property)
    {
        return kvp.ContainsKey(property);
    }

    public void AddTempProperty(string property, string value, string debug = "")
    {
        if (string.IsNullOrEmpty(property))
        {
            throw new ArgumentException("property can not be null");
        }
        kvp.Add(property, value, debug);
    }

    public void AddPersistedProperty(string property, string value, string debug = "")
    {
        AddTempProperty(property, value, debug);
        kvpMDP.Add(property, value, debug);
    }

    public string GetProperty(string property)
    {
        string? value = kvp.Value(property);
        if (value == null)
        {
            return "";
        }
        else
        {
            return value;
        }
    }
}