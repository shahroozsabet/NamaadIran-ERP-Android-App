/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.Content;
using Android.Text;
using Android.Util;
using Mono.Data.Sqlite;
using NamaadMobile.entity;
using NamaadMobile.Entity;
using NamaadMobile.Function;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace NamaadMobile.Data
{
    public class NmdMobileDBAdapter : IDisposable
    {
        #region Define
        public string DirName { get; set; }
        public string DBNamaad { get; set; }
        public static string DbPath { get; set; }
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadMobile.Data.NmdMobileDBAdapter";
        private Context mCtx;
        private SqliteTransaction myTrans;
        internal SqliteConnection Connection { get; set; }
        private bool IsTransaction { get; set; }
        private const string Nl = "\n";
        #endregion
        #region Constructor
        public NmdMobileDBAdapter(Context ctx)
        {
            mCtx = ctx;
            DirName = mCtx.GetString(Resource.String.DirName);
            DBNamaad = mCtx.GetString(Resource.String.DBNamaad);
            DbPath = DbPathPrep();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NmdMobileDBAdapter"/> class.
        /// Note: This Constructor is not fully tested yet.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="con">The con.</param>
        public NmdMobileDBAdapter(Context ctx, SqliteConnection con)
        {
            mCtx = ctx;
            Connection = con;
        }
        #endregion
        #region Function
        public string DbPathPrep()
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
            //var output = "";
            //build connection string
            Mono.Data.Sqlite.SqliteConnectionStringBuilder connString = new SqliteConnectionStringBuilder();
            connString.DataSource = DbPath + dBName;
            connString.JournalMode = SQLiteJournalModeEnum.Persist;
            if (!System.IO.File.Exists(DbPath + dBName))
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
                return sqCommand.ExecuteNonQuery();
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
        public object ExecuteScalar(string sql)
        {
            using (SqliteCommand sqCommand = Connection.CreateCommand())
            {
                if (IsTransaction == true)
                    sqCommand.Transaction = myTrans;
                sqCommand.CommandText = sql;
                return sqCommand.ExecuteScalar();
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
        public DataTable ExecuteSQL(string sql, bool acceptChangesDuringFill = true)
        {
            using (DataTable dt = new DataTable("dt"))
            using (SqliteDataAdapter sqlDa = new SqliteDataAdapter(sql, Connection))
            {
                sqlDa.AcceptChangesDuringFill = acceptChangesDuringFill;
                sqlDa.Fill(dt);
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
        public bool Exist(string tableName)
        {
            return Convert.ToBoolean(ExecuteScalar("SELECT Count(name) FROM sqlite_master WHERE type='table' AND name='" + tableName + "'"));
        }
        /// <summary>
        /// Fills to table.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="tableName">Name of the table.</param>
        public void CopyToDB(DataTable dt, string tableName, string deleteTableName, string deleteKey)
        {
            string colName;
            string strWhereDelete;
            StringBuilder fieldSelected;
            bool flag = false;
            string valueSQL;
            if (deleteKey != "")
                deleteKey = "," + deleteKey + ",";
            using (dt)
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
                            fieldSelected = fieldSelected.Append(',');
                        else
                        {
                            fieldSelected = new StringBuilder();
                            fieldSelected.Append("Insert Into ");
                            fieldSelected.Append(tableName);
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
                        if (value is bool)
                        {
                            valueSQL = "'" + NConvert.Bool2Int((bool)value).ToString() + "'";
                            fieldSelected.Append(valueSQL);
                            fieldSelected.Append(" As ");
                            fieldSelected.Append(colName);
                            flag = true;
                        }
                        if (!flag)
                            throw new Exception("Field data type, " + value.GetType() + ", is not supported yet . Column Name= " + colName + " Table Name= " + tableName);
                        if (deleteKey != "")
                            if (deleteKey.Contains("," + colName + ","))
                            {
                                if (strWhereDelete != "")
                                    strWhereDelete = strWhereDelete + " And ";
                                strWhereDelete = strWhereDelete + colName + " = " + valueSQL;
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
        public void CopyToDB(DataTable dt, string tableName, bool doCreateTable = true)
        {
            if (doCreateTable) ExecuteNonQuery("CREATE TABLE " + tableName + "( " + DataFunction.GetTableDef(dt) + ")");
            foreach (DataRow dr in dt.Rows)
                Insert(tableName, dr);
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
        /// Return true i a transaction has not been commited / rolled back
        /// </summary>
        /// <returns></returns>
        public bool InTransaction()
        {
            return IsTransaction;
        }
        /// <summary>
        /// Commit updates back to last begin transaction / savepoint
        /// </summary>
        /// <returns>true if still in transaction</returns>
        public bool Commit()
        {
            EndTransaction();
            return InTransaction();
        }
        /// <summary>
        /// Creates the table from given Datatable
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dt">The dt.</param>
        public void CreateTable(string tableName, DataTable dt)
        {
            ExecuteNonQuery("CREATE TABLE " + tableName + "( " + DataFunction.GetTableDef(dt) + ")");
            CopyToDB(dt, tableName, false);
        }
        /// <summary>
        /// Inserts Datarow into the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dr">The dr to be inserted</param>
        public void Insert(string tableName, DataRow dr)
        {
            StringBuilder sbCmdText = new StringBuilder();
            int colCount = dr.ItemArray.Length;
            using (SqliteCommand command = Connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO " + tableName + " VALUES (";
                for (int i = 0; i < colCount; i++)
                {
                    sbCmdText.Append("@value" + i);
                    command.Parameters.Add(new SqliteParameter("@value" + i, dr[i]));
                    if (i < colCount - 1) sbCmdText.Append(",");
                }
                command.CommandText += sbCmdText.ToString() + ")";
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Deletes the specified Datarow from tableName.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dr">The dr to be deleted</param>
        public void Delete(string tableName, DataRow dr)
        {
            string[] deleteKeyArr = GetPrimaryKey(tableName);
            using (SqliteCommand command = Connection.CreateCommand())
            {
                command.CommandText = "Delete From " + tableName + " Where ";
                for (int i = 0; i < deleteKeyArr.Length; i++)
                {
                    command.CommandText += deleteKeyArr[i] + "=@" + deleteKeyArr[i];
                    command.Parameters.Add(new SqliteParameter("@" + deleteKeyArr[i], dr[deleteKeyArr[i]]));
                    if (i < deleteKeyArr.Length - 1) command.CommandText += " And ";
                }
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Updates the specified Datarow in the tableName.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dr">The dr to be updated</param>
        public void Update(string tableName, DataRow dr)
        {
            string[] colArr = GetColumn(tableName);
            string[] updateKeyArr = GetPrimaryKey(tableName);
            using (SqliteCommand command = Connection.CreateCommand())
            {
                command.CommandText = "UPDATE " + tableName + " Set ";
                for (int i = 0; i < colArr.Length; i++)
                {
                    command.CommandText += colArr[i] + "=@" + colArr[i];
                    command.Parameters.Add(new SqliteParameter("@" + colArr[i], dr[colArr[i]]));
                    if (i < colArr.Length - 1) command.CommandText += " , ";
                }
                command.CommandText += " Where ";
                for (int i = 0; i < updateKeyArr.Length; i++)
                {
                    command.CommandText += updateKeyArr[i] + "=@" + updateKeyArr[i];
                    if (i < updateKeyArr.Length - 1) command.CommandText += " And ";
                }
                command.ExecuteNonQuery();
            }
        }
        #endregion
        #region aSQLiteManager
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
                        catch (Exception)
                        {
                            fld.setFieldType(AField.FieldType.UNRESOLVED);
                        }
                        if (fld.getFieldType() == AField.FieldType.NULL)
                            fld.setFieldData("");
                        else if (fld.getFieldType() == AField.FieldType.UNRESOLVED)
                            fld.setFieldData("Unknown field");
                        else
                            fld.setFieldData(cursor.GetValue(j * 2 + 1).ToString());
                        fields[j] = fld;
                    }
                    //recs[i++].setFields(fields);
                    recs.Peek().setFields(fields);
                }
            }
            return recs.ToArray();
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
                    catch (Exception)
                    {
                        tf.setUpdateable(false);
                        tf.setValue("BLOB");

                    }
                    tfs[j] = tf;
                }
            }			// Get foreign keys
            sql = "PRAGMA foreign_key_list([" + tableName + "])";
            using (SqliteDataReader cursor = ExecuteReader(sql))
                while (cursor.Read())
                    //Go through all fields to see if the fields has FK
                    for (int i = 0; i < fields; i++)
                        if (tfs[i].getName().Equals(cursor.GetString(3)))//fkName
                        {
                            tfs[i].setForeignKey("Select [" + cursor.GetString(4) + "] From [" + cursor.GetString(2) + "]");
                            break;
                        }
            return tfs;
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
            //Boolean rawType = false;
            if (where.Trim().Equals(""))
                where = "";
            else
                where = " Where " + where + " ";
            // || sqlStatement.toLowerCase().startsWith("pragma")
            if (!sqlStatement.ToLower().StartsWith("pragma") && Regex.Matches(sqlStatement, "where|order|limit|offset", RegexOptions.IgnoreCase).Count > 0)
                sql = "Select * From ( " + sqlStatement + " ) As Tmp" + where + order + " limit " + limit + " offset " + offset;
            else if (sqlStatement.ToLower().StartsWith("pragma"))
                sql = sqlStatement;
            else
                sql = sqlStatement + where + order + " limit " + limit + " offset " + offset;
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
                        catch (Exception)
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
        #endregion
        #region Helper Function
        /// <summary>
        /// Convert a field type retrieved by a pragma table_info (tableName)
        /// to a RecordEditorBuilder editor type
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <returns></returns>
        private static int FieldType2Int(string fieldType)
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
        /// Translate a field type in text format to the field type as "enum"
        /// </summary>
        /// <param name="fldType">Type of the field.</param>
        /// <returns></returns>
        private static NamaadMobile.Entity.AField.FieldType getFieldType(string fldType)
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
        /// <summary>
        /// Return a String list with all field names of the table
        /// </summary>
        /// <param name="tableName">The table.</param>
        /// <returns>The table's Column Names</returns>
        public string[] GetColumn(string tableName)
        {
            List<string> fields = new List<string>();
            using (SqliteDataReader res = ExecuteReader("pragma table_info([" + tableName + "])"))
                while (res.Read())
                    fields.Add(res.GetString(1));
            return fields.ToArray();
        }
        public int getNoOfRecords(string tableName, string where)
        {
            int recs = 0;
            if (where.Trim().Equals(""))
                where = "";
            else
                where = " Where " + where;
            using (SqliteDataReader cursor = ExecuteReader("Select Count(*) From [" + tableName + "] " + where))
                while (cursor.Read())
                    recs += cursor.GetInt32(0);
            return recs;
        }
        public int getNoOfRecordsSQLStatment(string sqlStatement, string where)
        {
            int recs = 0;
            if (where.Trim().Equals(""))
                where = "";
            else
                where = " where " + where;
            using (SqliteDataReader cursor = ExecuteReader("Select Count(*) From (" + sqlStatement + ") As Tmp " + where))
                while (cursor.Read())
                    recs += cursor.GetInt32(0);
            return recs;
        }
        /// <summary>
        /// Retrieve a list of FieldDescr to describe all fields of a table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public FieldDescr[] getTableStructureDef(string tableName)
        {
            List<FieldDescr> flds;
            using (SqliteDataReader cursor = ExecuteReader("pragma table_info ([" + tableName + "])"))
            {
                //int rows = cursor.getCount();
                flds = new List<FieldDescr>();
                //int i = 0;
                while (cursor.Read())
                {
                    FieldDescr fld = new FieldDescr();
                    fld.setCid(cursor.GetInt32(0));
                    fld.setName(cursor.GetString(1));
                    fld.setType(FieldType2Int(cursor.GetString(2)));
                    fld.setNotNull(NConvert.Int2Bool(cursor.GetInt32(3)));
                    fld.setDefaultValue(cursor.GetValue(4).ToString());
                    fld.setPk(NConvert.Int2Bool(cursor.GetInt32(5)));
                    flds.Add(fld);
                    //i++;
                }
            } return flds.ToArray();
        }
        /// <summary>
        /// Gets the primary keys.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Primary keys of the tableName</returns>
        public string[] GetPrimaryKey(string tableName)
        {
            List<string> fields = new List<string>();
            using (SqliteDataReader cursor = ExecuteReader("pragma table_info ([" + tableName + "])"))
                while (cursor.Read())
                    if (NConvert.Int2Bool(cursor.GetInt32(5)))
                        fields.Add(cursor.GetString(1));
            return fields.ToArray();
        }
        #endregion
    }
}