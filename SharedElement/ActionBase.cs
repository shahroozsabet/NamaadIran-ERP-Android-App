/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
namespace NamaadDB
{
	public class ActionBase
	{
		public int CompanyCode { get; set; }
		public int ActionCode { get; set; }
		public int ActionType { get; set; }
		public string ActionName { get; set; }
		public string ActionSource { get; set; }
		public string ActionArgument { get; set; }
		public int ParentCode { get; set; }
		public string DbNameServer { get; set; }
		public string DbNameClient { get; set; }
	}
}