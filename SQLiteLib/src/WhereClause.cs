namespace CFG2.Utils.SQLiteLib;

public class WhereClause {
    private string _field;
    private string _whereCondition;
    private object _value;

    public WhereClause(string field, string whereCondition, object value) {
        this._field = field;
        this._whereCondition = whereCondition;
        this._value = value;
    }

    public string Field => _field;
    public string WhereCondition => _whereCondition;
    public object Value => _value;
}
