namespace CFG2.Utils.SQLiteLib;

public class Metadata
{
    private string _file;
    private Dictionary<string, List<FieldDef>> tablesAndFields = new Dictionary<string, List<FieldDef>>();

    public Metadata(string file) {
        this._file = file;
    }

    public string File => _file;

    public void RegisterTable(string table) {
        if (!tablesAndFields.ContainsKey(table)) {
            tablesAndFields[table] = new List<FieldDef>();
        }
    }

    public void RegisterField(FieldDef fieldDef) {
        RegisterTable(fieldDef.Table);

        tablesAndFields[fieldDef.Table].Add(fieldDef);
    }

    public FieldDef GetFieldDef(string table, string field) {
        FieldDef ret = null;
        List<FieldDef> fieldDefList = tablesAndFields[table];
        foreach (FieldDef fieldDef in fieldDefList) {
            if (fieldDef.Field.Equals(field)) {
                ret = fieldDef;
                break;
            }
        }
        return ret;
    }

    public void Print() {
        Console.WriteLine("file: "+this._file);
        foreach (KeyValuePair<string, List<FieldDef>> entry in tablesAndFields) {
            Console.WriteLine(entry.Key);
            foreach (FieldDef fieldDef in entry.Value) {
                Console.WriteLine("  - "+fieldDef.Field+" "+fieldDef.Type);
            }
        }
    }
}
