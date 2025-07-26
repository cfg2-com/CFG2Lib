using CFG2.Utils.SecLib;

namespace CFG2.Utils.AppLib;

public class AppConfig
{
    private readonly App _app;
    private readonly string _configFile;
    private readonly string _mdpTable;
    private KVP _kvp;
    private KVPmdp _kvpMDP;
    private KVP _kvpFile;

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

        _app = app;
        _configFile = Path.Combine(app.Dir, cfgFile);
        _mdpTable = mdpTable;

        Reload();
    }

    public void Reload()
    {
        _app.Log("Loading AppConfig");
        _kvp = new KVPmemory(_app);

        _kvpMDP = new KVPmdp(_app, _app.Name, _mdpTable, "KEY_ID", "APP_C");
        foreach (string key in _kvpMDP.Keys)
        {
            _kvp.Add(key, _kvpMDP.Value(key));
        }

        if (File.Exists(_configFile))
        {
            _kvpFile = new KVPfile(_app, _app.Name, _configFile);
            foreach (string key in _kvpFile.Keys)
            {
                _kvp.Add(key, _kvpFile.Value(key));
            }
        }
    }

    public bool ContainsProperty(string property)
    {
        return _kvp.ContainsKey(property);
    }

    /// <summary>
    /// Adds a temporary property with the specified key and value to the collection if the property does NOT already exist.
    /// </summary>
    /// <param name="property">The key of the property to add. Cannot be null or empty.</param>
    /// <param name="value">The value associated with the property key.</param>
    /// <param name="debug">An optional debug string for additional context or logging purposes.</param>
    /// <returns><see langword="true"/> if the property was successfully added; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="property"/> is null or empty.</exception>
    public bool AddTempProperty(string property, string value, string debug = "")
    {
        if (string.IsNullOrEmpty(property))
        {
            throw new ArgumentException("property can not be null");
        }
        return _kvp.Add(property, value, debug);
    }

    /// <summary>
    /// Adds a property to the persisted key-value store if the property does NOT already exist and optionally encrypts its value.
    /// </summary>
    /// <remarks>This method adds the specified property to both the runtime key-value store and the persisted
    /// store. If <paramref name="secure"/> is <see langword="true"/>, the value is encrypted before being
    /// stored.</remarks>
    /// <param name="property">The name of the property to add. Cannot be null or empty.</param>
    /// <param name="value">The value of the property to add. If <paramref name="secure"/> is <see langword="true"/>, the value will be
    /// encrypted before being stored.</param>
    /// <param name="debug">An optional debug string for logging or tracking purposes. Can be null or empty.</param>
    /// <param name="secure">A boolean indicating whether the value should be encrypted before being stored. <see langword="true"/> to
    /// encrypt; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the property was successfully added to the persisted store; otherwise, <see
    /// langword="false"/>.</returns>
    public bool AddPersistedProperty(string property, string value, string debug = "", bool secure = false)
    {
        if (secure)
        {
            value = SecLib.SecLib.Encrypt(value);
        }
        AddTempProperty(property, value, debug); // Adds to runtime kvp
        return _kvpMDP.Add(property, value, debug); // Save for next time
    }

    public string GetProperty(string property, bool secure = false)
    {
        string? value = _kvp.Value(property);
        if (value == null)
        {
            return "";
        }
        else
        {
            if (secure)
            {
                return SecLib.SecLib.Decrypt(value);
            }
            else
            {
                return value;
            }
        }
    }

    /// <summary>
    /// Get the full path to the config file
    /// </summary>
    /// <returns></returns>
    public string GetFile()
    {
        return _configFile;
    }

    /// <summary>
    /// Get the full path to the MDP SQLite database holding the config values
    /// </summary>
    /// <returns></returns>
    public string GetDB()
    {
        return _kvpMDP.GetFile();
    }
}