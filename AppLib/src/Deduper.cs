namespace CFG2.Utils.AppLib;

using CFG2.Utils.LogLib;
using CFG2.Utils.SQLiteLib;

public class Deduper
{
    private readonly string name;
    private readonly string file;
    private Dictionary<string, string> map = [];
    private SQLiteUtil? sqliteUtil;
    private readonly HashSet<string> seenItems = new HashSet<string>();
    private readonly AppLib app;
    private readonly bool useMDP;

    /// <summary>
    /// Deduper constructor.
    /// </summary>
    /// <param name="app">The app to create this deduper for. Used to get various config info.</param>
    /// <param name="name">A unique name for the deduper. The value will be forced to UPPER_CASE and should NOT contain any special characters.</param>
    /// <param name="global">If true, this deduper will be global (i.e. uniqueness will be looked at across any "app" - not just this one).</param>
    /// <param name="useMDP">If true, will use the MDP.db SQLite db at the app SyncDir. If false, will write to file either in the AppDataDir or BackupDir depending on value of "global"</param>
    public Deduper(AppLib app, string name, bool global = false, bool useMDP = true)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Deduper name cannot be null or empty.");
        }
        else if (name.Contains("/") || name.Contains("\\") || name.Contains(":") || name.Contains("*") || name.Contains("?") || name.Contains("&") || name.Contains("\"") || name.Contains("<") || name.Contains(">") || name.Contains("|"))
        {
            throw new ArgumentException("Deduper name cannot contain special characters: " + name);
        }
        this.app = app;
        this.name = name.ToUpper();
        this.useMDP = useMDP;
        string dir = app.AppDataDir;
        if (global)
        {
            this.name = "GLOBAL_" + this.name;
            dir = app.BackupDir;
        }
        file = Path.Combine(dir, $"dedup-{this.name}.txt");

        if (useMDP)
        {
            ConfigMDP();
            LoadKeysFromMDP();
        }

        LoadKeysFromFile();
    }

    private void LoadKeysFromFile()
    {
        if (!File.Exists(file))
        {
            app.Log("Creating " + name + " dedup file: " + file);
            File.Create(file).Close();
        }
        else
        {
            foreach (string line in File.ReadLines(file))
            {
                map.Add(line, line);
            }
            Logger.Trace("Loaded " + map.Count + " entries into " + name + " map from " + file);
        }
    }

    private void LoadKeysFromMDP()
    {
        List<Record> records = sqliteUtil.SelectRecords("SELECT DEDUP_ID FROM DEDUP WHERE TYPE_C = '" + name + "'");
        foreach (Record record in records)
        {
            string dedupId = record.FieldVal("DEDUP_ID");
            map.Add(dedupId, dedupId);
        }
    }

    public bool ContainsKey(string key)
    {
        if (map == null || string.IsNullOrEmpty(key))
        {
            return false;
        }
        return map.ContainsKey(key);
    }

    public void AddItem(string key, string debug)
    {
        try
        {
            File.AppendAllText(this.file, key+"\n");
            map.Add(key, key);
            if (useMDP)
            {
                InsertMDP(key, debug);
            }            
        }
        catch (Exception e)
        {
            app.Error("Adding item to " + name + " Deduper: "+ e.Message);
        }
    }
    
    private string GetDedupId(string key)
    {
        return $"{name}_{key}";
    }

    private bool InsertMDP(String key, String debug)
    {
        try
        {
            if (!DedupExists(key))
            {
                Record record = new Record();
                record.AddField("DEDUP_ID", GetDedupId(key));
                record.AddField("TYPE_C", name.ToUpper());
                record.AddField("VALUE_X", key);
                record.AddField("DEBUG_X", debug);
                record.AddField("CREATED_DT", "datetime(current_timestamp, 'localtime')");
                sqliteUtil.InsertRecord("DEDUP", record);
                return true;
            }
        }
        catch (Exception e)
        {
            app.Error("Error inserting into MDP: " + e.Message);
        }

        return false;
    }

    private bool DedupExists(string key)
    {
        string dedupId = GetDedupId(key);
        List<WhereClause> whereClauses = new();
        whereClauses.Add(new WhereClause("DEDUP_ID", "=", dedupId));
        return sqliteUtil.RecordExists("DEDUP", whereClauses);
    }

    private void ConfigMDP()
    {
        if (sqliteUtil == null)
        {
            app.Trace("Configuring MDP for Deduper: " + name);
            sqliteUtil = app.GetMDP();
            sqliteUtil.RegisterField(new FieldDef("DEDUP", "DEDUP_ID", DataType.TEXT));
            sqliteUtil.RegisterField(new FieldDef("DEDUP", "TYPE_C", DataType.TEXT));
            sqliteUtil.RegisterField(new FieldDef("DEDUP", "VALUE_X", DataType.TEXT));
            sqliteUtil.RegisterField(new FieldDef("DEDUP", "DEBUG_X", DataType.TEXT));
            sqliteUtil.RegisterField(new FieldDef("DEDUP", "CREATED_DT", DataType.EXT_DATETIME));
        }
    }
}