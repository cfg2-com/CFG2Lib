using System.ComponentModel;

namespace CFG2.Utils.AppLib;

public class ConfigProperties
{
    private KVP kvp;

    /// <summary>
    /// Deduper constructor.
    /// </summary>
    /// <param name="app">The app to for these configuration properties.</param>
    /// <param name="cfgFile">If a file with this name exists in the AppDir, it will be read.</param>
    /// <param name="mdpTable">If not null, this table will be read from MDP.db in the SyncDir.</param>
    public ConfigProperties(AppLib app, string? cfgFile = "app.cfg", string? mdpTable = null)
    {
        kvp = new KVPmemory(app);

        if (!string.IsNullOrEmpty(mdpTable))
        {
            KVP kvpMDP = new KVPmdp(app, app.AppName, mdpTable);
            foreach (string key in kvpMDP.Keys)
            {
                kvp.Add(key, kvpMDP.Value(key));
            }
        }

        if (!string.IsNullOrEmpty(cfgFile))
        {
            string file = Path.Combine(app.AppDir, cfgFile);
            if (File.Exists(file))
            {
                KVP kvpFile = new KVPfile(app, app.AppName, file);
                foreach (string key in kvpFile.Keys)
                {
                    kvp.Add(key, kvpFile.Value(key));
                }
            }
        }
    }

    public bool ContainsKey(string key)
    {
        return kvp.ContainsKey(key);
    }
}