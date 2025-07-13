namespace CFG2.Utils.SQLiteLib;

using CFG2.Utils.LogLib;
using Microsoft.Data.Sqlite;

public class SQLiteUtil
{
    // ref: https://www.sqlite.org/datatype3.html

    private Metadata metadata;

    public SQLiteUtil(string file)
    {
        metadata = new Metadata(file);
    }
    
    public string File {
        get { return metadata.File; }
    }

    /// <summary>
    /// Registers a field for type safety and conversion
    /// </summary>
    /// <param name="fieldDef">Field Definition (includes table name)</param>
    public void RegisterField(FieldDef fieldDef)
    {
        if (!TableExists(fieldDef.Table))
        {
            CreateTable(fieldDef.Table, fieldDef.Field + " " + fieldDef.Type.ToString());
        }
        else if (!FieldExists(fieldDef.Table, fieldDef.Field))
        {
            AddField(fieldDef);
        }

        metadata.RegisterField(fieldDef);

        //metadata.Print();
    }

    public DataType GetFieldDataType(string table, string field) {
        return metadata.GetFieldDef(table, field).Type;
    }

    public bool TableExists(string table) {
        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "SELECT name FROM sqlite_master WHERE UPPER(name) = UPPER('"+table+"')";
                object name = command.ExecuteScalar();
                if ((name != null) && (name.ToString().Equals(table))) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }

    public void CreateTable(string table, string fieldsWithTypes) {
        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "CREATE TABLE "+table+" ("+fieldsWithTypes+")";
                object name = command.ExecuteNonQuery();
            }
        }
    }

    public void TruncateTable(string table) {
        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "DELETE FROM "+table;
                object name = command.ExecuteNonQuery();
            }
        }
    }

    // ref: https://stackoverflow.com/questions/18920136/check-if-a-column-exists-in-sqlite
    public bool FieldExists(string table, string field) {
        bool exists = false;
        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "SELECT COUNT(*) AS fieldcount FROM pragma_table_info('"+table+"') WHERE UPPER(name) = UPPER('"+field+"')";
                object fieldcount = command.ExecuteScalar();
                if ((fieldcount != null) && (Int32.Parse(fieldcount.ToString()) == 1)) {
                    exists = true;
                }
            }
        }
        return exists;
    }

    public void AddField(FieldDef fieldDef) {
        if (!FieldExists(fieldDef.Table, fieldDef.Field)) {
            string typeStr = fieldDef.Type.ToString();
            if (fieldDef.Type.Equals(DataType.EXT_DATE)) {
                typeStr = "DATE";
            } else if (fieldDef.Type.Equals(DataType.EXT_DATETIME)) {
                typeStr = "DATETIME";
            }
            using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand()) {
                    command.CommandText = "ALTER TABLE "+fieldDef.Table+" ADD COLUMN "+fieldDef.Field+" "+typeStr;
                    object name = command.ExecuteNonQuery();
                }
            }
        } else {
            //Logger.Trace("The field "+fieldDef.Field+" aleady exists on the table "+fieldDef.Table);
        }
    }

    public bool RecordExists(string table, List<WhereClause> whereClauses) {
        bool exists = false;

        List<FieldVal> binds = new List<FieldVal>();
        string whereFieldsAndVals = "";
        if (whereClauses != null) {
            whereFieldsAndVals = "WHERE ";
            foreach (WhereClause entry in whereClauses) {
                string field = entry.Field;
                string bindField = "@WHERE_"+field;
                binds.Add(new FieldVal(bindField, entry.Value));
                whereFieldsAndVals += field+" "+entry.WhereCondition+" "+bindField+" AND ";
            }
            whereFieldsAndVals = whereFieldsAndVals.Trim().Substring(0, whereFieldsAndVals.Length-4);
            //Logger.Trace(whereFieldsAndVals);
        }

        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "SELECT * FROM "+table+" "+whereFieldsAndVals;
                //Logger.Trace(command.CommandText);
                foreach (FieldVal entry in binds) {
                    string field = entry.Field;
                    string bindField = "@"+field;
                    object value = entry.Value;
                    if (field.StartsWith("@WHERE_")) {
                        bindField = field;
                    }
                    
                    //Logger.Trace("  "+bindField+" = "+value);
                    command.Parameters.AddWithValue(bindField, value);
                }
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        exists = true;
                    }
                }
            }
        }

        return exists;
    }

    public void InsertRecord(string table, Record record) {
        string fields = "";
        string binds = "";
        foreach (FieldVal entry in record.FieldVals) {
            string field = entry.Field;
            fields += field+", ";
            binds += "@"+field+", ";
        }
        fields = fields.Trim().Substring(0, fields.Length-2);
        binds = binds.Trim().Substring(0, binds.Length-2);
        //Logger.Trace(fields);
        //Logger.Trace(binds);
        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "INSERT INTO "+table+" ("+fields+") VALUES ("+binds+")";
                //Logger.Trace(command.CommandText);
                foreach (FieldVal entry in record.FieldVals) {
                    string field = entry.Field;
                    string bindField = "@"+field;
                    object value = entry.Value;
                    //Logger.Trace(bindField+"="+value);
                    command.Parameters.AddWithValue(bindField, value);
                }
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// This method performs an UPDATE for a single table
    /// </summary>
    /// <param name="table">The table to UPDATE</param>
    /// <param name="unboundSets">Fixed updates like FIELD_NB = FIELD_NB + 2</param>
    /// <param name="boundSets">Updates with dynamic values. ex: FIELD_NB = @FIELD_NB</param>
    /// <param name="whereClauses">The conditions for performing the update</param>
    public void UpdateRecords(string table, List<FieldVal> unboundSets, List<FieldVal> boundSets, List<WhereClause> whereClauses) {
        List<FieldVal> binds = new List<FieldVal>();

        string fieldsAndVals = "";
        if (unboundSets != null) {
            foreach (FieldVal entry in unboundSets) {
                fieldsAndVals += entry.Field+" = "+entry.Value+", ";
            }
        }
        if (boundSets != null) {
            foreach (FieldVal entry in boundSets) {
                binds.Add(entry);
                string field = entry.Field;
                fieldsAndVals += field+" = @"+field+", ";
            }
        }
        fieldsAndVals = fieldsAndVals.Trim().Substring(0, fieldsAndVals.Length-2);
        //Logger.Trace(fieldsAndVals);

        string whereFieldsAndVals = "";
        if (whereClauses != null) {
            whereFieldsAndVals = "WHERE ";
            foreach (WhereClause entry in whereClauses) {
                string field = entry.Field;
                string bindField = "@WHERE_"+field;
                binds.Add(new FieldVal(bindField, entry.Value));
                whereFieldsAndVals += field+" "+entry.WhereCondition+" "+bindField+" AND ";
            }
            whereFieldsAndVals = whereFieldsAndVals.Trim().Substring(0, whereFieldsAndVals.Length-4);
            //Logger.Trace(whereFieldsAndVals);
        }

        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "UPDATE "+table+" SET "+fieldsAndVals+" "+whereFieldsAndVals;
                //Logger.Trace(command.CommandText);
                foreach (FieldVal entry in binds) {
                    string field = entry.Field;
                    string bindField = "@"+field;
                    object value = entry.Value;
                    //DataType type;
                    if (field.StartsWith("@WHERE_")) {
                        bindField = field;
                        //type = metadata.GetFieldDef(table, field.Replace("@WHERE_", "")).Type;
                    //} else {
                        //type = metadata.GetFieldDef(table, field).Type;
                    }
                    //Logger.Trace("  "+bindField+" = "+value);
                    command.Parameters.AddWithValue(bindField, value);
                }
                command.ExecuteNonQuery();
            }
        }
    }

    public List<Record> SelectRecords(string sql)
    {
        List<Record> records = new List<Record>();

        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    int fields = reader.FieldCount;
                    while (reader.Read()) {
                        Record record = new Record();
                        for (int x = 0; x < fields; x++) {
                            record.AddField(reader.GetName(x), reader.GetValue(x));
                        }
                        records.Add(record);
                    }
                }
            }
        }

        //Logger.Trace("Returning "+records.Count+" records");
        return records;
    }

    /// <summary>
    /// This method performs an UPDATE for a single table
    /// </summary>
    /// <param name="table">The table to UPDATE</param>
    /// <param name="whereClauses">The conditions for performing the update</param>
    public void DeleteRecords(string table, List<WhereClause> whereClauses) {
        List<FieldVal> binds = new List<FieldVal>();

        string whereFieldsAndVals = "";
        if (whereClauses != null) {
            whereFieldsAndVals = "WHERE ";
            foreach (WhereClause entry in whereClauses) {
                string field = entry.Field;
                string bindField = "@WHERE_"+field;
                binds.Add(new FieldVal(bindField, entry.Value));
                whereFieldsAndVals += field+" "+entry.WhereCondition+" "+bindField+" AND ";
            }
            whereFieldsAndVals = whereFieldsAndVals.Trim().Substring(0, whereFieldsAndVals.Length-4);
        }

        using (var connection = new SqliteConnection("Data Source="+metadata.File)) 
        {
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()) {
                command.CommandText = "DELETE FROM "+table+" "+whereFieldsAndVals;
                //Logger.Trace(command.CommandText);
                foreach (FieldVal entry in binds) {
                    string field = entry.Field;
                    string bindField = "@"+field;
                    object value = entry.Value;
                    DataType type;
                    if (field.StartsWith("@WHERE_")) {
                        bindField = field;
                        type = metadata.GetFieldDef(table, field.Replace("@WHERE_", "")).Type;
                    } else {
                        type = metadata.GetFieldDef(table, field).Type;
                    }
                    //Logger.Trace("  "+bindField+" = "+value);
                    command.Parameters.AddWithValue(bindField, value);
                }
                command.ExecuteNonQuery();
            }
        }
    }

    public FieldVal GetFieldVal(string field, object value) {
        return new FieldVal(field, value);
    }

    public FieldVal GetFieldVal(string table, string field, object value) {
        DataType type = metadata.GetFieldDef(table, field).Type;
        return new FieldVal(field, value, type);
    }
}
