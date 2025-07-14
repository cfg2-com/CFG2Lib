using CFG2.Utils.SQLiteLib;

namespace CFG2.Utils.AppLib;

public class KVPfile : KVP
{
    private readonly string file;

    public KVPfile(AppLib app, string group, string? file = null) : base(app, group)
    {
        if (string.IsNullOrEmpty(file))
        {
            this.file = Path.Combine(App.AppDataDir, "kvp-"+Group+".txt");
            App.Trace("KVPfile file param is empty, defaulting to: "+this.file);
        }
        else
        {
            this.file = file;
        }

        if (!File.Exists(this.file))
        {
            App.Log("Creating " + Group + " kvp file: " + this.file);
            File.Create(this.file).Close();
        }

        foreach (string line in File.ReadLines(this.file))
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

    public override void Add(string key, string value, string debug = "")
    {
        if (ShouldAdd(key, value))
        {
            //App.Trace($"Adding kvp: {key}={value}");
            if (!string.IsNullOrEmpty(debug)) { App.Log(debug); }
            File.AppendAllText(file, key+"="+value+"\n");

            Add(key, value);
        }
    }
}