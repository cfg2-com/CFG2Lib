using CFG2.Utils.SQLiteLib;

namespace CFG2.Utils.AppLib;

public class KVPmemory(App app) : KVP(app, "MEMORY")
{
    public override bool Add(string key, string value, string? debug = "")
    {
        if (ShouldAdd(key, value))
        {
            //App.Trace($"Adding kvp: {key}={value}");
            if (!string.IsNullOrEmpty(debug)) { App.Log(debug); }

            base.Add(key, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Remove(string key)
    {
        base.Remove(key);
    }
}