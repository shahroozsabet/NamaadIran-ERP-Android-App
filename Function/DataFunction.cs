/*
 * Author: Shahrooz Sabet
 * Date: 20150401
 * */
#region using
using System.Data;
using System.Text;
#endregion
namespace NamaadMobile.Function
{
    class DataFunction
    {
        #region Function
        public static string GetTableDef(DataTable dt)
        {
            StringBuilder strBuilder = new StringBuilder();
            int colCount = dt.Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                strBuilder.Append(dt.Columns[i] + " ");
                strBuilder.Append("[" + dt.Columns[i].DataType + "] ");
                if (!dt.Columns[i].AllowDBNull) strBuilder.Append("NOT NULL");
                if (i < colCount - 1) strBuilder.Append(",");
            }
            if (dt.PrimaryKey.Length > 0)
            {
                strBuilder.Append(",");
                strBuilder.Append("PRIMARY KEY (");
                strBuilder.Append(GetColumnName(dt.PrimaryKey));
                strBuilder.Append(")");
            }
            return strBuilder.ToString();
        }
        public static string GetColumnName(DataTable dt)
        {
            StringBuilder strBuilder = new StringBuilder();
            int colCount = dt.Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                if (i > 0 && i < colCount - 1) strBuilder.Append(",");
                strBuilder.Append(dt.Columns[i]);
            }
            return strBuilder.ToString();
        }
        public static string GetColumnName(DataColumn[] dtColArray)
        {
            StringBuilder strBuilder = new StringBuilder();
            int colCount = dtColArray.Length;
            for (int i = 0; i < colCount; i++)
            {
                if (i > 0 && i < colCount - 1) strBuilder.Append(",");
                strBuilder.Append(dtColArray[i]);
            }
            return strBuilder.ToString();
        }
        public static bool ExistDB(string dBName)
        {

            return false;
        }
        #endregion
    }
}



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