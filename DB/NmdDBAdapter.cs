/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using Android.Content;
using Android.Text;
using Android.Util;

using Mono.Data.Sqlite;

using NamaadDB.entity;
using NamaadDB.Entity;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NamaadDB
{
    public class NmdDBAdapter : IDisposable
    {
        public string DirName { get; set; }
        public string DBNamaad { get; set; }
        public static string DbPath { get; set; }

        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadDB.Login";
        private Context mCtx;

        private SqliteTransaction myTrans;
        internal SqliteConnection Connection { get; set; }
        private bool IsTransaction { get; set; }

        private const string Nl = "\n";

        public NmdDBAdapter(Context ctx)
        {
            mCtx = ctx;
            DirName = mCtx.GetString(Resource.String.DirName);
            DBNamaad = mCtx.GetString(Resource.String.DBNamaad);
            DbPath = dbPathPrep();
        }
        public string dbPathPrep()
        {
#if __ANDROID__
			// Just use whatever directory SpecialFolder.Personal returns
			//string dbPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path.ToString(), DirName);
			Java.IO.File pathToExternalStorage = Android.OS.Environment.ExternalStorageDirectory;
			Java.IO.File qualifiedDir = new Java.IO.File(pathToExternalStorage + "/" + DirName + "/");
			qualifiedDir.Mkdirs();
			//if (qualifiedDir.Mkdirs())
			//	System.Console.WriteLine("Directory created.");
			//else
			//	System.Console.WriteLine("Directory was created or \n failed in it creation!");
			DbPath = qualifiedDir.Path + "/";

#else

            // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
            // (they don't want non-user-generated data in Documents)
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            DbPath = Path.Combine(documentsPath, "../Library/"); // Library folder
#endif
            return DbPath;
        }
        /// <summary>
        /// Opens or creates the data base if it is not already exist and then opens it. 
        /// </summary>
        /// <param name="dBName">Name of the d b.</param>
        public void OpenOrCreateDatabase(string dBName)
        {
            bool exists = System.IO.File.Exists(DbPath + dBName);
            //var output = "";
            //build connection string
            Mono.Data.Sqlite.SqliteConnectionStringBuilder connString = new SqliteConnectionStringBuilder();
            connString.DataSource = DbPath + dBName;
            connString.JournalMode = SQLiteJournalModeEnum.Persist;
            if (!exists)
            {
                //output += "Creating database";
                // Need to create the database and seed it with some data.
                //Mono.Data.Sqlite.SqliteConnection.SetConfig(SQLiteConfig.Serialized);
                Mono.Data.Sqlite.SqliteConnection.CreateFile(connString.DataSource);
            }
            Connection = new SqliteConnection(connString.ToString());
            Connection.Open();
        }
        /// <summary>
        /// Allows the programmer to interact with the database for purposes other than a query.
        /// Typically used for table creation or data insertion.
        /// </summary>
        /// <param name="sql">The SQL to be run.</param>
        /// <returns>INSERT, UPDATE and DELETE statements will return the number of rows affected. 
        /// All other SQL statements will return 0.
        /// An Integer containing the number of rows updated. 
        /// The return value for some operations is the number of rows affected, otherwise it’s 0.</returns>
        public int ExecuteNonQuery(string sql)
        {
            using (SqliteCommand sqCommand = Connection.CreateCommand())
            {
                if (IsTransaction == true)
                    sqCommand.Transaction = myTrans;
                sqCommand.CommandText = sql;
                int rowsUpdated = sqCommand.ExecuteNonQuery();

                return rowsUpdated;
            }

        }

        /// <summary>
        /// Executescalars the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns>The ExecuteScalar method’s return type is object
        /// – you should cast the result depending on the database query. 
        /// The result could be an integer from a COUNT query or 
        /// a string from a single column SELECT query. 
        /// Note that this is different to other Execute methods 
        /// that return a reader object or a count of the number of rows affected.</returns>
        public object EXECUTESCALAR(string sql)
        {
            using (SqliteCommand sqCommand = Connection.CreateCommand())
            {
                if (IsTransaction == true)
                    sqCommand.Transaction = myTrans;
                sqCommand.CommandText = sql;
                object rowsUpdated = sqCommand.ExecuteScalar();
                return rowsUpdated;
            }

        }
        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns>The ExecuteReader method returns a SqliteDataReader object.</returns>
        public SqliteDataReader ExecuteReader(string sql)
        {
            using (SqliteCommand cmd = Connection.CreateCommand())
            {
                if (IsTransaction == true)
                    cmd.Transaction = myTrans;
                cmd.CommandText = sql;
                return cmd.ExecuteReader();
            }
        }

        /// <summary>
        /// Allows the programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DataTable GetDataTable(string sql)
        {
            using (DataTable dt = new DataTable())
            using (SqliteDataAdapter sqlDA = new SqliteDataAdapter(sql, Connection))
            {
                //sqlDA.AcceptChangesDuringFill = false;
                sqlDA.Fill(dt);
                return dt;
            }
        }
        /// <summary>
        /// Starts a transaction. Transaction can be nested as the SQLite savepoints
        /// </summary>
        /// <returns>
        /// true if transaction started
        /// </returns>
        /// <exception cref="Mono.Data.Sqlite.SqliteException">Error in Setting</exception>
        public bool BeginTransaction()
        {
            if (IsTransaction == true)
                throw new SqliteException("Error in Setting");
            myTrans = Connection.BeginTransaction();
            IsTransaction = true;
            return IsTransaction;
        }
        public void EndTransaction()
        {
            if (IsTransaction == false)
                throw new SqliteException("Error in Setting");
            myTrans.Commit();
            IsTransaction = false;
        }
        public void RollbackTransaction(string exceptionMessage)
        {
            if (IsTransaction == false)
                throw new SqliteException("Error in Setting");

            myTrans.Rollback();
            if (!TextUtils.IsEmpty(exceptionMessage.Trim()))
                Log.Debug(TAG, exceptionMessage);
            IsTransaction = false;
        }
        /// <summary>
        /// Namaad drop table if exist. Is using ExecuteNonQuery.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void DropTable(string tableName)
        {
            ExecuteNonQuery(" Drop Table IF EXISTS " + tableName);
        }
        /// <summary>
        /// If the tableName exists. Is using EXECUTESCALAR.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The return value for some operations is the number of rows affected, otherwise it’s -1.</returns>
        public bool IfExists(string tableName)
        {
            return Convert.ToBoolean(EXECUTESCALAR("SELECT Count(name) FROM sqlite_master WHERE type='table' AND name='" + tableName + "'"));
        }
        /// <summary>
        /// Fills to table.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="destTableName">Name of the table.</param>
        public void FillToTable(DataTable dt, string destTableName, string deleteTableName, string deleteKey)
        {
            string colName;
            string strWhereDelete;
            StringBuilder fieldSelected;
            bool flag = false;
            string valueSQL;
            if (deleteKey != "")
                deleteKey = "," + deleteKey + ",";
            using (dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    fieldSelected = null;
                    colName = "";
                    strWhereDelete = "";

                    foreach (DataColumn col in dt.Columns)
                    {
                        flag = false;
                        valueSQL = "";

                        if (fieldSelected != null)
                        {
                            fieldSelected = fieldSelected.Append(',');
                        }
                        else
                        {
                            fieldSelected = new StringBuilder();
                            fieldSelected.Append("Insert Into ");
                            fieldSelected.Append(destTableName);
                            fieldSelected.Append(" Select ");
                        }
                        var value = row[col];
                        //var valueType1 = row[col].GetType();
                        colName = col.ColumnName;
                        if (value is int || value is Int32 || value is Int16 || value is Int64 || value is Byte || value is double || value is float)
                        {
                            valueSQL = value.ToString();
                            fieldSelected.Append(valueSQL);
                            fieldSelected.Append(" As ");
                            fieldSelected.Append(colName);
                            flag = true;
                        }
                        if (value is char || value is string)
                        {
                            valueSQL = "'" + value.ToString() + "'";
                            fieldSelected.Append(valueSQL);
                            fieldSelected.Append(" As ");
                            fieldSelected.Append(colName);
                            flag = true;
                        }
                        if (!flag)
                            throw new Exception("Field data type, " + value.GetType() + ", is not supported yet . Column Name= " + colName + " Table Name= " + destTableName);
                        if (deleteKey != "")
                        {
                            if (deleteKey.Contains("," + colName + ","))
                            {
                                if (strWhereDelete != "")
                                    strWhereDelete = strWhereDelete + " And ";
                                strWhereDelete = strWhereDelete + colName + " = " + valueSQL;
                            }
                        }
                    }
                    if (strWhereDelete != "")
                    {
                        strWhereDelete = "Delete From " + deleteTableName + " Where " + strWhereDelete;
                        ExecuteNonQuery(strWhereDelete);
                    }
                    ExecuteNonQuery(fieldSelected.ToString());
                }
            }

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
            mCtx = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmdDBAdapter"/> class.
        /// Note: This Constructor is not fully tested yet.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="con">The con.</param>
        public NmdDBAdapter(Context ctx, SqliteConnection con)
        {
            mCtx = ctx;
            Connection = con;
        }
        /// <summary>
        /// Retrieve data for table viewer
        /// </summary>
        /// <param name="cont">The cont.Context on which to display errors</param>
        /// <param name="table">table Name of table</param>
        /// <param name="where">The where.Where clause for filter</param>
        /// <param name="order">The order.Order if data sorted (by clicking on title)</param>
        /// <param name="offset">The offset.Offset if paging</param>
        /// <param name="limit">limit Page size</param>
        /// <param name="view">indication of view (with out on update trigger) </param>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>a list of Records</returns>
        public Record[] getTableDataWithWhere(Context cont, string table, string where, string order, int offset, int limit, bool view, string[] fieldNames)
        {
            string sql = "";
            if (view)
                sql = "Select ";
            else
                sql = "Select typeof(rowid), rowid As rowid, ";
            if (where.Trim().Equals(""))
                where = "";
            else
                where = " Where " + where + " ";
            //String[] fieldNames = getFieldsNames(table);
            for (int i = 0; i < fieldNames.Length; i++)
            {
                sql += "typeof([" + fieldNames[i] + "]), [" + fieldNames[i] + "]";
                if (i < fieldNames.Length - 1)
                    sql += ", ";
            }
            sql += " From [" + table + "] " + where + order + " limit " + limit + " offset " + offset;
            //Record[] recs = null;
            Stack<Record> recs = new Stack<Record>();
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {
                int columns = cursor.FieldCount / 2;
                //Utils.logD("Columns: " + columns, logging);
                //int rows = cursor.Depth;
                //Utils.logD("Rows = " + rows, logging);
                //recs = new Record[rows];
                //int i = 0;
                while (cursor.Read())
                {
                    //recs[i] = new Record();
                    recs.Push(new Record());
                    AField[] fields = new AField[columns];
                    for (int j = 0; j < columns; j++)
                    {
                        AField fld = new AField();
                        //Get the field type due to SQLites flexible handling of field types the type from 
                        //the table definition can't be used
                        try
                        {
                            string fldType = cursor.GetString(j * 2);   //TODO still problems here with BLOB fields!?!?!?!
                            fld.setFieldType(getFieldType(fldType));
                        }
                        catch (Exception e)
                        {
                            fld.setFieldType(AField.FieldType.UNRESOLVED);
                        }
                        if (fld.getFieldType() == AField.FieldType.NULL)
                        {
                            fld.setFieldData("");
                        }
                        else if (fld.getFieldType() == AField.FieldType.UNRESOLVED)
                        {
                            fld.setFieldData("Unknown field");
                        }
                        else
                        {
                            fld.setFieldData(cursor.GetValue(j * 2 + 1).ToString());
                        }
                        fields[j] = fld;
                    }
                    //recs[i++].setFields(fields);
                    recs.Peek().setFields(fields);
                }
            }
            return recs.ToArray();
        }
        /// <summary>
        /// Return a String list with all field names of the table
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public string[] getFieldsNames(string table)
        {
            string sql = "pragma table_info([" + table + "])";
            //String[] fields = null;
            List<string> fields = new List<string>();

            using (SqliteDataReader res = ExecuteReader(sql))
            {
                //int cols = res.Depth;
                //fields = new String[cols];
                //int i = 0;
                // getting field names
                while (res.Read())
                {
                    //fields[i] = res.GetString(1);
                    fields.Add(res.GetString(1));
                    //i++;
                }
            }
            return fields.ToArray();
        }
        /// <summary>
        /// Translate a field type in text format to the field type as "enum"
        /// </summary>
        /// <param name="fldType">Type of the field.</param>
        /// <returns></returns>
        private NamaadDB.Entity.AField.FieldType getFieldType(string fldType)
        {
            if (fldType.ToUpper().Equals("TEXT"))
                return AField.FieldType.TEXT;
            else if (fldType.ToUpper().Equals("INTEGER"))
                return AField.FieldType.INTEGER;
            else if (fldType.ToUpper().Equals("REAL"))
                return AField.FieldType.REAL;
            else if (fldType.ToUpper().Equals("BLOB"))
                return AField.FieldType.BLOB;
            else if (fldType.ToUpper().Equals("NULL"))
                return AField.FieldType.NULL;
            return AField.FieldType.UNRESOLVED;
        }
        public int getNoOfRecords(string tableName, string where)
        {
            int recs = 0;
            if (where.Trim().Equals(""))
            {
                where = "";
            }
            else
            {
                where = " Where " + where;
            }
            string sql = "Select Count(*) From [" + tableName + "] " + where;
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {
                while (cursor.Read())
                {
                    recs += cursor.GetInt32(0);
                }
            }
            return recs;
        }
        public int getNoOfRecordsSQLStatment(string sqlStatement, string where)
        {
            int recs = 0;
            if (where.Trim().Equals(""))
            {
                where = "";
            }
            else
            {
                where = " where " + where;
            }
            string sql = "Select Count(*) From (" + sqlStatement + ") As Tmp " + where;
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {
                while (cursor.Read())
                {
                    recs += cursor.GetInt32(0);
                }
            }
            return recs;
        }
        /// <summary>
        /// Retrieve a record based on table name and rowid
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="rowId">The row identifier.</param>
        /// <returns>a list of TableFields one for each field the first contains
        /// the rowid for the record</returns>
        public TableField[] getRecord(string tableName, long rowId)
        {
            string sql = "Select rowid As rowid, * From '" + tableName + "' Where rowid = " + rowId;
            // retrieves field types, pk, ... from database
            FieldDescr[] tabledef = getTableStructureDef(tableName);
            TableField[] tfs;
            int fields;
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {
                fields = cursor.FieldCount;//cursor.getColumnCount();
                tfs = new TableField[fields];//cursor.getColumnCount()				
                cursor.Read();
                for (int j = 0; j < fields; j++)
                {
                    TableField tf = new TableField();
                    tf.setName(cursor.GetName(j));//getColumnName
                    tf.setDisplayName(cursor.GetName(j));//getColumnName
                    // The extra field rowid
                    if (j == 0)
                    {
                        // Don't allow updating of rowid
                        tf.setUpdateable(false);
                        tf.setType(TableField.TYPE_INTEGER);
                    }
                    else
                    {
                        tf.setUpdateable(true);
                        tf.setType(tabledef[j - 1].getType());
                        tf.setNotNull(tabledef[j - 1].isNotNull());
                        tf.setPrimaryKey(tabledef[j - 1].isPk());
                        tf.setDefaultValue(tabledef[j - 1].getDefaultValue());
                        //TODO need to retrieve the foreign key
                    }
                    //TODO Implement BLOB edit
                    //is it a BLOB field turn edit off
                    try
                    {
                        //tf.setValue(cursor.GetString(j));
                        tf.setValue(cursor.GetValue(j).ToString());
                    }
                    catch (Exception e)
                    {
                        tf.setUpdateable(false);
                        tf.setValue("BLOB");

                    }
                    tfs[j] = tf;
                }
            }			// Get foreign keys
            sql = "PRAGMA foreign_key_list([" + tableName + "])";
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {
                while (cursor.Read())
                {
                    //Go through all fields to see if the fields has FK
                    for (int i = 0; i < fields; i++)
                    {
                        string fkName = cursor.GetString(3);
                        if (tfs[i].getName().Equals(fkName))
                        {
                            tfs[i].setForeignKey("Select [" + cursor.GetString(4) + "] From [" + cursor.GetString(2) + "]");
                            break;
                        }
                    }
                }
            }
            return tfs;
        }
        /// <summary>
        /// Retrieve a list of FieldDescr to describe all fields of a table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public FieldDescr[] getTableStructureDef(string tableName)
        {
            string sql = "pragma table_info ([" + tableName + "])";
            List<FieldDescr> flds;
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {
                //int rows = cursor.getCount();
                flds = new List<FieldDescr>();
                //int i = 0;
                while (cursor.Read())
                {
                    FieldDescr fld = new FieldDescr();
                    fld.setCid(cursor.GetInt32(0));
                    fld.setName(cursor.GetString(1));
                    fld.setType(fieldType2Int(cursor.GetString(2)));
                    fld.setNotNull(int2boolean(cursor.GetInt32(3)));
                    fld.setDefaultValue(cursor.GetValue(4).ToString());
                    fld.setPk(int2boolean(cursor.GetInt32(5)));
                    flds.Add(fld);
                    //i++;
                }
            } return flds.ToArray();
        }
        /// <summary>
        /// Convert a field type retrieved by a pragma table_info (tableName)
        /// to a RecordEditorBuilder editor type
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <returns></returns>
        private int fieldType2Int(string fieldType)
        {
            if (fieldType.ToUpper().Equals("STRING")
                    || fieldType.ToUpper().Equals("TEXT"))
                return TableField.TYPE_STRING;
            else if (fieldType.ToUpper().Equals("INTEGER")
                || fieldType.ToUpper().Equals("INT"))
                return TableField.TYPE_INTEGER;
            else if (fieldType.ToUpper().Equals("REAL")
                    || fieldType.ToUpper().Equals("FLOAT")
                    || fieldType.ToUpper().Equals("DOUBLE"))
                return TableField.TYPE_FLOAT;
            else if (fieldType.ToUpper().Equals("BOOLEAN")
                    || fieldType.ToUpper().Equals("BOOL"))
                return TableField.TYPE_BOOLEAN;
            else if (fieldType.ToUpper().Equals("DATE"))
                return TableField.TYPE_DATE;
            else if (fieldType.ToUpper().Equals("TIME"))
                return TableField.TYPE_TIME;
            else if (fieldType.ToUpper().Equals("DATETIME"))
                return TableField.TYPE_DATETIME;
            else if (fieldType.ToUpper().Equals("PHONENO"))
                return TableField.TYPE_PHONENO;
            else
                return TableField.TYPE_STRING;
        }
        /// <summary>
        /// Convert the SQLite 0 / 1 boolean to Java boolean 
        /// </summary>
        /// <param name="intBool">The int bool.</param>
        /// <returns></returns>
        private bool int2boolean(int intBool)
        {
            bool res = false;
            if (intBool == 1)
                res = true;
            return res;
        }

        /// <summary>
        /// Retrieve a number of rows based on a sql query.
        /// This method is slow when the sqlStatment has this "where|order|limit|offset" pattern,
        /// Since it will execute Select * From(sqlStatment) in order to have paging and filtering of Data later in this case.
        /// This method also accept pragma sql statement.
        /// Note: Excecution of sql statement without "where|order|limit|offset" pattern is faster.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="offset">The offset. number of rows to skip</param>
        /// <param name="limit">The limit. max number of rows to retrieve</param>
        /// <param name="_cont">The _cont.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <param name="where">The where.Where clause for filter</param>
        /// <param name="order">The order.Order if data sorted (by clicking on title)</param>
        /// <returns>
        /// a QueryResult object
        /// </returns>
        public QueryResult getSQLQueryPage(string sqlStatement, int offset, int limit, Context _cont, string[] fieldNames, string where, string order)
        {
            string sql;
            string pattern = "where|order|limit|offset";
            //Boolean rawType = false;
            if (where.Trim().Equals(""))
                where = "";
            else
                where = " Where " + where + " ";
            // || sqlStatement.toLowerCase().startsWith("pragma")
            if (!sqlStatement.ToLower().StartsWith("pragma") && Regex.Matches(sqlStatement, pattern, RegexOptions.IgnoreCase).Count > 0)
            {
                sql = "Select * From ( " + sqlStatement + " ) As Tmp" + where + order + " limit " + limit + " offset " + offset;
            }
            else if (sqlStatement.ToLower().StartsWith("pragma"))
            {
                sql = sqlStatement;
            }
            else
            {
                sql = sqlStatement + where + order + " limit " + limit + " offset " + offset;
            }
            //rawType = true;
            QueryResult nres = null;
            // Find out which for of query to use
            //if (sql.Trim().ToLower().StartsWith("select"))
            //	rawType = true;
            //else if (sql.Trim().ToLower().StartsWith("pragma"))
            //	rawType = true;
            //else
            //{
            //	rawType = true;
            //}
            // Use execSQL where no result is expected
            //if (!rawType)
            //{
            //	try
            //	{
            //		ExecuteNonQuery(sql);
            //		nres = new QueryResult();
            //		nres.Data = new List<String[]>();
            //		nres.ColumnNames = new String[] { "" };
            //		nres.Data[0][0] = _cont.GetText(Resource.String.NotAnArror);
            //	}
            //	catch (Exception e)
            //	{
            //		nres = new QueryResult();
            //		nres.ColumnNames = new String[] { _cont.GetText(Resource.String.Error) };
            //		nres.Data = new List<String[]>();
            //		nres.Data[0][0] = e.ToString();
            //	}
            //}
            //else
            //{
            //try
            //{
            nres = new QueryResult();
            using (SqliteDataReader cursor = ExecuteReader(sql))
            {				// TOD get column names
                nres.ColumnNames = fieldNames;
                //nres.ColumnDisplayNames = fieldDisplayNames;
                //int rows = 0;//cursor.getCount();
                int cols = cursor.VisibleFieldCount;//getColumnCount();
                nres.Data = new List<string[]>();
                //int i = 0;
                //Boolean result = false;
                while (cursor.Read())
                {
                    //result = true;
                    string[] colList = new string[cols];
                    for (int k = 0; k < cols; k++)
                    {
                        // Fails if it is a BLOB field
                        try
                        {
                            colList[k] = cursor.GetValue(k).ToString();
                        }
                        catch (Exception e)
                        {
                            //nres.Data[i][k] = "BLOB (size: " + cursor.GetBlob(k).length + ")";
                            colList[k] = "BLOB";
                        }
                    }
                    nres.Data.Add(colList);
                    //i++;
                }
                //if (!result)
                //{
                //	nres = new QueryResult();
                //	nres.Data = new List<String[]>();
                //	nres.ColumnNames = new String[] { "" };
                //	String[] colList = new String[1] { "" };//_cont.GetText(Resource.String.NoResult);
                //	nres.Data.Add(colList);
                //}
            }
            //}
            //catch (Exception e)
            //{
            //	nres.ColumnNames = new String[] { _cont.GetText(Resource.String.Error) };
            //	nres.Data = new List<String[]>();
            //	nres.Data[0][0] = e.ToString();
            //	//if (cursor != null)
            //	//	cursor.close();
            //	//}
            //}
            return nres;
        }
        /// <summary>
        /// Return true i a transaction has not been commitet / rolled back
        /// </summary>
        /// <returns></returns>
        public bool inTransaction()
        {
            return IsTransaction;
        }

        /// <summary>
        /// Commit updates back to last begin transaction / savepoint
        /// </summary>
        /// <returns>true if still in transaction</returns>
        public bool commit()
        {
            EndTransaction();
            return inTransaction();
        }
    }
}

///// <summary>
///// Fills to table.
///// </summary>
///// <param name="dt">The dt.</param>
///// <param name="tableName">Name of the table.</param>
//public void FillToTable(DataTable dt, String tableName)
//{
//    String sql = "select * from " + tableName + " where 1=0";
//    using (SqliteDataAdapter sqlDA = new SqliteDataAdapter(sql, connection))
//    {
//        sqlDA.Update(dt);
//    }
//}

///// <summary>
///// Retrieve a list of TableFields to match a empty record for the database, 
///// used for Record Editor, 
///// Currently is not used in NamaadDB, Comment by Shahrooz 20140223
///// </summary>
///// <param name="tableName">Name of the table.</param>
///// <returns></returns>
//public TableField[] getEmptyRecord(String tableName)
//{
//	FieldDescr[] fd = getTableStructureDef(tableName);
//	TableField[] tfs = new TableField[fd.Length];
//	for (int i = 0; i < fd.Length; i++)
//	{
//		TableField tf = new TableField();
//		tf.setName(fd[i].getName());
//		tf.setType(fd[i].getType());
//		tf.setPrimaryKey(fd[i].isPk());
//		tf.setUpdateable(true);
//		tf.setValue(null);
//		tf.setNotNull(fd[i].isNotNull());
//		tf.setDefaultValue(fd[i].getDefaultValue());
//		tfs[i] = tf;
//	}
//	// Get the FK's
//	// Get foreign keys
//	String sql = "PRAGMA foreign_key_list([" + tableName + "])";
//	using (SqliteDataReader cursor = ExecuteReader(sql))
//	{
//		while (cursor.Read())
//		{
//			// Go through all fields to see if the fields has FK
//			for (int i = 0; i < fd.Length; i++)
//			{
//				String fkName = cursor.GetString(3);
//				// Utils.logD("NameMH: " + tfs[i].getName());
//				if (tfs[i].getName().Equals(fkName))
//				{
//					tfs[i].setForeignKey("select [" + cursor.GetString(4) + "] from ["
//							+ cursor.GetString(2) + "]");
//					break;
//				}
//			}
//		}
//	} return tfs;
//}

///// <summary>
///// Retrieve a list of lookup values for selection lists, 
///// used for Record Editor,
///// Currently is not used in NamaadDB, Comment by Shahrooz 20140223
///// </summary>
///// <param name="foreignKey">The foreign key.</param>
///// <returns></returns>
//public ForeignKeyHolder getFKList2(String foreignKey)
//{
//	// TODO must be changed to handle lookup tables with code - values
//	// must the return both the foreign key "code" and describing text
//	ForeignKeyHolder lists = new ForeignKeyHolder();
//	List<String> ids = new List<String>();
//	using (SqliteDataReader cursor = ExecuteReader(foreignKey))
//	{
//		while (cursor.Read())
//		{
//			ids.Add(cursor.GetString(0));
//		}
//	}
//	// select [id] from [foreign]
//	// TODO replace field name with *
//	String sql = foreignKey.Substring(0, foreignKey.IndexOf('[')) + "*"
//			+ foreignKey.Substring(foreignKey.IndexOf(']') + 1);
//	List<String> texts = new List<String>();
//	using (SqliteDataReader cursor = ExecuteReader(foreignKey))
//	{
//		int cols = cursor.FieldCount;
//		while (cursor.Read())
//		{
//			int j = 0;
//			String rowText = "";
//			for (j = 0; j < cols; j++)
//			{
//				rowText += cursor.GetString(j);
//				if (j < cols - 1)
//					rowText += " | ";
//			}
//			texts.Add(rowText);
//		}
//	}
//	lists.setId(ids.ToArray());
//	lists.setText(texts.ToArray());
//	return lists;
//}
///// <summary>
///// Export the current query to a file named after the database with the
///// extension .export
///// </summary>
///// <param name="sql">The SQL to query</param>
//public void exportQueryResult(string sql)
//{
//	using (SqliteDataReader data = ExecuteReader(sql))
//	{
//		string backupName = dbPath + ".export";
//		Java.IO.File backupFile = new Java.IO.File(backupName);
//		using (FileWriter f = new FileWriter(backupFile))
//		{
//			using (BufferedWriter outBuf = new BufferedWriter(f))
//			{
//				while (data.Read())
//				{
//					// write export
//					string fields = "";
//					for (int i = 0; i < data.VisibleFieldCount; i++)
//					{
//						string val = data.GetString(i);
//						// tabInf.moveToPosition(i);
//						// String type = tabInf.getString(2);
//						if (val == null)
//						{
//							fields += "null";
//							if (i != data.VisibleFieldCount - 1)
//								fields += "; ";
//							// } else if (type.equals("INTEGER") || type.equals("REAL")) {
//							// fields += val;
//							// if (i != data.getColumnCount()-1)
//							// fields += ", ";
//						}
//						else
//						{ // it must be string or blob(?) so quote it
//							fields += "\"" + val + "\"";
//							if (i != data.VisibleFieldCount - 1)
//								fields += "; ";
//						}
//					}
//					outBuf.Write(fields + nl);
//				}
//			}
//		}
//	}
//}