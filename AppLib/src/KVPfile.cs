using CFG2.Utils.SQLiteLib;

namespace CFG2.Utils.AppLib;

public class KVPfile : KVP
{
    private readonly string _file;

    public KVPfile(App app, string group, string? file = null) : base(app, group)
    {
        if (string.IsNullOrEmpty(file))
        {
            _file = Path.Combine(App.DataDir, "kvp-"+Group+".txt");
            App.Trace("KVPfile file param is empty, defaulting to: "+_file);
        }
        else
        {
            _file = file;
        }

        if (!File.Exists(_file))
        {
            App.Log("Creating " + Group + " kvp file: " + _file);
            File.Create(_file).Close();
        }

        foreach (string line in File.ReadLines(_file))
        {
            if (!string.IsNullOrEmpty(line) && !line.StartsWith('#') && !line.StartsWith("--") && !line.StartsWith(@"//"))
            {
                string key = line;
                string value = "";

                if (line.Contains('='))
                {
                    string[] parts = line.Split("=");
                    key = parts[0];
                    if (parts.Length > 1)
                    {
                        for (int x = 1; x < parts.Length; x++)
                        {
                            if (x > 1) { value += "="; }
                            value += parts[x];
                        }
                    }
                }
                Add(key, value);
            }
        }
    }

    public override bool Add(string key, string value, string? debug = "")
    {
        if (ShouldAdd(key, value))
        {
            //App.Trace($"Adding kvp: {key}={value}");
            if (!string.IsNullOrEmpty(debug)) { App.Log(debug); }
            File.AppendAllText(_file, key+"="+value+"\n");

            Add(key, value);
            return true;
        }
        else
        {
            return false;
        }
    }
}