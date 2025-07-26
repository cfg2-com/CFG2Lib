using System.Collections.ObjectModel;

namespace CFG2.Utils.AppLib;

public abstract class KVP
{
    private readonly App _app;
    private readonly string _group;
    protected Dictionary<string, string> _kvp = [];

    public KVP(App app, string group)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app), "AppLib instance cannot be null.");
        }
        if (string.IsNullOrEmpty(group))
        {
            throw new ArgumentException("Group cannot be null or empty.", nameof(group));
        }
        if (
            group.Contains("/") ||
            group.Contains("\\") ||
            group.Contains(":") ||
            group.Contains("*") ||
            group.Contains("?") ||
            group.Contains("&") ||
            group.Contains("\"") ||
            group.Contains("<") ||
            group.Contains(">") ||
            group.Contains("|")
            )
        {
            throw new ArgumentException("Group cannot contain special characters: " + group);
        }

        _app = app;
        _group = group.ToUpper();
    }

    public App App => _app;
    public string Group => _group;
    public Dictionary<string, string>.KeyCollection Keys => _kvp.Keys;
    /// <summary>
    /// Adds a key-value pair to the collection if the key does not already exist.
    /// </summary>
    /// <param name="key">The key associated with the value to add. Cannot be null or empty.</param>
    /// <param name="value">The value to associate with the specified key. Cannot be null.</param>
    /// <param name="debug">An optional debug string for logging or diagnostic purposes. Can be empty.</param>
    /// <returns><see langword="true"/> if the key-value pair was successfully added; otherwise, <see langword="false"/> if the
    /// key already exists or the operation fails.</returns>
    public abstract bool Add(string key, string value, string debug = "");

    protected bool Add(string key, string value)
    {
        if (!ContainsKey(key))
        {
            _kvp[key] = value;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ContainsKey(string key)
    {
        if (_kvp == null || string.IsNullOrEmpty(key))
        {
            return false;
        }
        return _kvp.ContainsKey(key);
    }

    public string? Value(string key)
    {
        if (ContainsKey(key))
        {
            return _kvp[key];
        }
        else
        {
            return null;
        }
    }

    public bool ShouldAdd(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        if (!ContainsKey(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}