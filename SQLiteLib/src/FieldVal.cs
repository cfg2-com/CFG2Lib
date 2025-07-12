namespace CFG2.Utils.SQLiteLib;

public class FieldVal {
    private string _field;
    private object _value;
    private DataType _type;

    public FieldVal(string field, object value) {
        this._field = field;
        if (value == null) {
            value = DBNull.Value;
        }
        this._value = value;
    }

    public FieldVal(string field, object value, DataType type) {
        this._field = field;
        this._type = type;

        if (value == null) {
            value = DBNull.Value;
        } else if (DataType.EXT_DATE.Equals(type)) {
            value = DateTime.Parse(value.ToString()).Date;
        } else if (DataType.EXT_DATETIME.Equals(type)) {
            value = DateTime.Parse(value.ToString());
        }
        this._value = value;
    }

    public string Field => _field;
    public object Value => _value;
}
