using CFG2.Utils.SQLiteLib;

namespace CFG2.Utils.AppLib;

public class KVPmdp : KVP
{
    private readonly string _table;
    private readonly SQLiteUtil _sqliteUtil;
    private readonly string _keyField;
    private readonly string _groupField;

    public KVPmdp(App app, string group, string table = "KVP", string fieldKey = "KEY_ID", string fieldGroup = "GROUP_C") : base(app, group)
    {
        if (string.IsNullOrEmpty(fieldKey)) { fieldKey = "KEY_ID"; }
        if (string.IsNullOrEmpty(fieldGroup)) { fieldGroup = "GROUP_C"; }

        _keyField = fieldKey;
        _groupField = fieldGroup;

        if (string.IsNullOrEmpty(table))
        {
            App.Log("KVPmdp table name is empty, defaulting to 'KVP'.");
            _table = "KVP";
        }
        else
        {
            _table = table.ToUpper();
        }

        _sqliteUtil = App.GetMDP();
        _sqliteUtil.RegisterField(new FieldDef(table, _keyField, DataType.TEXT));
        _sqliteUtil.RegisterField(new FieldDef(table, _groupField, DataType.TEXT));
        _sqliteUtil.RegisterField(new FieldDef(table, "VALUE_X", DataType.TEXT));
        _sqliteUtil.RegisterField(new FieldDef(table, "DEBUG_X", DataType.TEXT));
        _sqliteUtil.RegisterField(new FieldDef(table, "CREATED_DT", DataType.EXT_DATETIME));

        List<Record> records = _sqliteUtil.SelectRecords("SELECT " + _keyField + ", VALUE_X FROM " + table + " WHERE " + _groupField + " = '" + Group + "'");
        foreach (Record record in records)
        {
            string key = record.FieldVal(_keyField);
            string value = record.FieldVal("VALUE_X");
            Add(key, value);
        }
    }

    public override bool Add(string key, string value, string? debug = "")
    {
        if (ShouldAdd(key, value))
        {
            if (string.IsNullOrEmpty(debug))
            {
                debug = "";
            }
            Record record = new Record();
            record.AddField(_keyField, key);
            record.AddField(_groupField, Group);
            record.AddField("VALUE_X", value);
            record.AddField("DEBUG_X", debug);
            record.AddField("CREATED_DT", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            _sqliteUtil.InsertRecord(_table, record);

            Add(key, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    public string GetFile()
    {
        return _sqliteUtil.File;
    }
}