using System.Data;
using CFG2.Utils.SecLib;

namespace CFG2.Utils.AppLib;

public class AppConfig
{
    private readonly App _app;
    private readonly string _configFile;
    private readonly string _mdpTable;
    private readonly string _encryptionPassword;
    private KVP _kvp;
    private KVPmdp _kvpMDP;
    private KVP _kvpFile;

    /// <summary>
    /// AppConfig constructor.
    /// </summary>
    /// <param name="app">The App for these configuration properties.</param>
    /// <param name="cfgFile">If a file with this name exists in the AppDir, it will be read. Default to app.cfg if empty or null.</param>
    /// <param name="mdpTable">This table will be read from MDP.db in the SyncDir. Default to APP_CONFIG if empty or null.</param>
    /// <param name="encryptionPassword">If provided, this password will be used to encrypt/decrypt secure properties.</param>
    public AppConfig(App app, string? cfgFile = "app.cfg", string? mdpTable = "APP_CONFIG", string encryptionPassword = "")
    {
        if (string.IsNullOrEmpty(cfgFile)) { cfgFile = "app.cfg"; }
        if (string.IsNullOrEmpty(mdpTable)) { mdpTable = "APP_CONFIG"; }

        _app = app;
        _configFile = Path.Combine(app.Dir, cfgFile);
        _mdpTable = mdpTable;
        _encryptionPassword = encryptionPassword;

        Reload();
    }

    /// <summary>
    /// Forces a reload the configuration properties.
    /// </summary>
    public void Reload()
    {
        _app.Trace("Loading AppConfig");
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

    /// <summary>
    /// Checks if the specified <paramref name="property"/> exists in the configuration.
    /// </summary>
    /// <param name="property">The property to check for existence.</param>
    /// <returns><see langword="true"/> if <paramref name="property"/> exists, otherwise <see langword="false"/>./returns>
    public bool ContainsProperty(string property)
    {
        return _kvp.ContainsKey(property);
    }

    /// <summary>
    /// Adds a temporary <paramref name="property"/> with the specified <paramref name="value"/> to the collection if the property does NOT already exist. 
    /// If the <paramref name="property"/> exists, no action is taken.
    /// </summary>
    /// <param name="property">The key of the property to add. Cannot be null or empty.</param>
    /// <param name="value">The value associated with the <paramref name="property"/> key.</param>
    /// <param name="debug">An optional debug string for additional context or logging purposes.</param>
    /// <returns><see langword="true"/> if the property was successfully added; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="property"/> is null or empty.</exception>
    public bool AddTempProperty(string property, string value, string? debug = "")
    {
        if (string.IsNullOrEmpty(property))
        {
            throw new ArgumentException("Property can not be empty or null", nameof(property));
        }
        return _kvp.Add(property, value, debug);
    }

    /// <summary>
    /// Adds a <paramref name="property"/> to the persisted key-value store if the property does NOT already exist and optionally encrypts the <paramref name="value"/>. 
    /// If the <paramref name="property"/> exists, no action is taken.
    /// </summary>
    /// <remarks>This method adds the specified <paramref name="property"/> to both the runtime key-value store and the persisted
    /// store. If <paramref name="secure"/> is <see langword="true"/>, the value is encrypted using the <paramref name="encryptionPassword"/> 
    /// specified in the constructor before being stored.</remarks>
    /// <param name="property">The name of the property to add. Cannot be null or empty.</param>
    /// <param name="value">The value of the <paramref name="property"/> to add. If <paramref name="secure"/> is <see langword="true"/>, the value will be
    /// encrypted before being stored.</param>
    /// <param name="debug">An optional debug string for logging or tracking purposes. Can be null or empty.</param>
    /// <param name="secure">A boolean indicating whether the value should be encrypted before being stored. <see langword="true"/> to
    /// encrypt; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the property was successfully added to the persisted store; otherwise, <see
    /// langword="false"/>.</returns>
    /// <exception cref="ArgumentException">If <paramref name="encryptionPassword"/> or <paramref name="property"/> is null or empty.</exception>
    public bool AddPersistedProperty(string property, string value, string? debug = "", bool secure = false)
    {
        if (secure)
        {
            if (string.IsNullOrEmpty(_encryptionPassword))
            {
                throw new DataException("encryptionPassword (set in the constructor) cannot be empty or null when secure is true");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be null or empty when secure is true", nameof(value));
            }
            value = SecUtil.Encrypt(_encryptionPassword, value);
        }

        AddTempProperty(property, value, debug); // Adds to runtime kvp
        return _kvpMDP.Add(property, value, debug); // Save for next time
    }

    /// <summary>
    /// Removes the specified <paramref name="property"/> from any underlying storage mechanisms.
    /// </summary>
    /// <param name="property">Config property to remove.</param>
    public void RemoveProperty(string property)
    {
        _kvpMDP?.Remove(property);
        _kvpFile?.Remove(property);
        _kvp?.Remove(property);
    }

    /// <summary>
    /// Gets the value of the specified <paramref name="property"/>. If <paramref name="secure"/> is true, the value will be decrypted 
    /// using the <paramref name="encryptionPassword"/> specified in the constructor before being returned.
    /// </summary>
    /// <param name="property">The property to retrieve the value for.</param>
    /// <param name="secure">A boolean indicating whether the value should be decrypted before being returned. <see langword="true"/> to
    /// decrypt; otherwise, <see langword="false"/>./param>
    /// <returns>The value of the specified <paramref name="property"/>./returns>
    /// <exception cref="ArgumentException">If <paramref name="encryptionPassword"/> is null or empty./exception>
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
                if (string.IsNullOrEmpty(_encryptionPassword))
                {
                    throw new DataException("encryptionPassword (set in the constructor) cannot be null when secure is true");
                }
                value = SecUtil.Decrypt(_encryptionPassword, value);
            }

            return value;
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