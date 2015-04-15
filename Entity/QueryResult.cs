/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using System.Collections.Generic;

namespace NamaadMobile.entity
{
	public class QueryResult
	{
		public string[] ColumnNames { get; set; }
		//public String[] ColumnDisplayNames { get; set; }
		public List<string[]> Data { get; set; }
	}
}