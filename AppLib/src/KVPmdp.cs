using CFG2.Utils.SQLiteLib;

namespace CFG2.Utils.AppLib;

public class KVPmdp : KVP
{
    private readonly string table;
    private readonly SQLiteUtil sqliteUtil;
    private readonly string keyField;
    private readonly string groupField;

    public KVPmdp(App app, string group, string table = "KVP", string fieldKey = "KEY_ID", string fieldGroup = "GROUP_C") : base(app, group)
    {
        if (string.IsNullOrEmpty(fieldKey)) { fieldKey = "KEY_ID"; }
        if (string.IsNullOrEmpty(fieldGroup)) { fieldGroup = "GROUP_C"; }

        keyField = fieldKey;
        groupField = fieldGroup;

        if (string.IsNullOrEmpty(table))
        {
            App.Log("KVPmdp table name is empty, defaulting to 'KVP'.");
            this.table = "KVP";
        }
        else
        {
            this.table = table.ToUpper();
        }

        sqliteUtil = App.GetMDP();
        sqliteUtil.RegisterField(new FieldDef(table, keyField, DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef(table, groupField, DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef(table, "VALUE_X", DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef(table, "DEBUG_X", DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef(table, "CREATED_DT", DataType.EXT_DATETIME));

        List<Record> records = sqliteUtil.SelectRecords("SELECT " + keyField + ", VALUE_X FROM " + table + " WHERE " + groupField + " = '" + Group + "'");
        foreach (Record record in records)
        {
            string key = record.FieldVal(keyField);
            string value = record.FieldVal("VALUE_X");
            Add(key, value);
        }
    }

    public override void Add(string key, string value, string debug = "")
    {
        if (ShouldAdd(key, value))
        {
            Record record = new Record();
            record.AddField(keyField, key);
            record.AddField(groupField, Group);
            record.AddField("VALUE_X", value);
            record.AddField("DEBUG_X", debug);
            record.AddField("CREATED_DT", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sqliteUtil.InsertRecord(table, record);

            Add(key, value);
        }
    }

    public string GetFile()
    {
        return sqliteUtil.File;
    }
}