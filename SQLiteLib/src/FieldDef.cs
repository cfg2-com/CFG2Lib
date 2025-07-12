namespace CFG2.Utils.SQLiteLib;

public class FieldDef {
    private string _table;
    private string _field;
    private DataType _type;

    public FieldDef(string table, string field, DataType type) {
        this._table = table;
        this._field = field;
        this._type = type;
    }

    public string Table => _table;
    public string Field => _field;
    public DataType Type => _type;
}
