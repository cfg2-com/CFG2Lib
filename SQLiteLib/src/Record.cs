namespace CFG2.Utils.SQLiteLib;

public class Record {
    List<FieldVal> _fieldVals;

    public Record() {
        this._fieldVals = new List<FieldVal>();
    }

    public void AddField(string field, object value, DataType type) {
        AddField(new FieldVal(field, value, type));
    }

    public void AddField(string field, object value) {
        AddField(new FieldVal(field, value));
    }

    public void AddField(FieldVal fieldVal) {
        _fieldVals.Add(fieldVal);
    }

    public List<FieldVal> FieldVals => _fieldVals;

    public string FieldVal(string field) {
        string ret = null;
        foreach (FieldVal fieldVal in _fieldVals) {
            if (fieldVal.Field.Equals(field)) {
                ret = fieldVal.Value.ToString();
                break;
            }
        }
        return ret;
    }
}
