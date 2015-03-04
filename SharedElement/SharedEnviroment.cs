/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using Android.App;
using System;

namespace NamaadDB
{
	[Application]
	public class SharedEnviroment : Application
	{
		public int CompanyCode { get; set; }
		public int ActionCode { get; set; }
		public string ActionName { get; set; }
		public string ActionSource { get; set; }
		public string UserName { get; set; }
		public int UserCode { get; set; }
		public bool IsAdmin { get; set; }
		public string DbNameServer { get; set; }
		public string DbNameClient { get; set; }

		public SharedEnviroment(IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

	}
}