/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
namespace NamaadDB.Entity
{
	public class RefreshDataEntity
	{
		public int CompanyCode { get; set; }
		public int TableCode { get; set; }
		public string TableName { get; set; }
		public int TableDataVersion { get; set; }
		public string TableDesc { get; set; }
		public override string ToString()
		{
			base.ToString();
			return TableDesc;
		}
	}
}