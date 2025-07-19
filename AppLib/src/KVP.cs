using System.Collections.ObjectModel;

namespace CFG2.Utils.AppLib;

public abstract class KVP
{
    private readonly App app;
    private readonly string group;
    protected Dictionary<string, string> kvp = [];

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

        this.app = app;
        this.group = group.ToUpper();
    }

    public App App => app;
    public string Group => group;
    public Dictionary<string, string>.KeyCollection Keys => kvp.Keys;
    public abstract void Add(string key, string value, string debug = "");

    protected void Add(string key, string value)
    {
        kvp[key] = value;
    }

    public bool ContainsKey(string key)
    {
        if (kvp == null || string.IsNullOrEmpty(key))
        {
            return false;
        }
        return kvp.ContainsKey(key);
    }

    public string? Value(string key)
    {
        if (ContainsKey(key))
        {
            return kvp[key];
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