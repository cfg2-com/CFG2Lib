# CFG2.Utils.SQLiteLib

A library of SQLite functions.

Nothing in here is rocket science, but IMO it make it easier to work with SQLite for things like dynamically creating/updating tables 
and running common CRUD type functions.

## Usage Examples

```
using CFG2.Utils.SQLiteLib;

public class Program
{
    private SQLiteUtil sqliteUtil;

    static void Main(string[] args)
    {
        sqliteUtil = new SQLiteUtil("c:\path\to\sqlite.db");

        sqliteUtil.RegisterField(new FieldDef("MY_TABLE", "KEY_ID", DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef("MY_TABLE", "VALUE_X", DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef("MY_TABLE", "DEBUG_X", DataType.TEXT));
        sqliteUtil.RegisterField(new FieldDef("MY_TABLE", "CREATED_DT", DataType.EXT_DATETIME));

        Record record = new Record();
        record.AddField("KEY_ID", "key");
        record.AddField("VALUE_X", "value");
        record.AddField("CREATED_DT", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sqliteUtil.InsertRecord(table, record);
    }
}
```

## Release Notes

### 1.0.0
- Initial Release
