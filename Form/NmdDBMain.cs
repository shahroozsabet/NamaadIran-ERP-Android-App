/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Mono.Data.Sqlite;

using NamaadDB.Adapter;
using NamaadDB.Util;

using System;
using System.Collections.Generic;

namespace NamaadDB
{
	/// <summary>
	/// TODO: Can use Navigation Drawer?
	/// </summary>
	[Activity(Label = "@string/NmdSys")]
	public class NmdDBMain : Activity
	{
		private List<ActionBase> name2class = new List<ActionBase>();

		public static NmdDBAdapter dBHelper;

		/// <summary>
		/// The tag For Debugging purpose
		/// </summary>
		private const string TAG = "NamaadDB.NmdDBMain";
		private string userName;
		private int userCode;
		private bool isAdmin;

		// UI references.
		private TextView mUserNameView;
		private ListView listView;

		private bool _logging;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			_logging = Prefs.getLogging(this);
			SetActionBar();
			SetActionBarTitle(GetString(Resource.String.NmdSys));
			SetContentView(Resource.Layout.home_screen);
			//AddActionBarFlag(ActionBarDisplayOptions.HomeAsUp);

			listView = FindViewById<ListView>(Resource.Id.listView);
			mUserNameView = (TextView)FindViewById(Resource.Id.UserNameLable);

			try
			{
				if (Intent.Extras != null)
				{
					userName = Intent.Extras.GetString("UserName");
					mUserNameView.Text = GetString(Resource.String.Welcome) + " " + userName;
					userCode = Intent.Extras.GetInt("UserCode");
					isAdmin = Intent.Extras.GetBoolean("IsAdmin");

					var companyCode = Intent.Extras.GetInt("CompanyCode");
					var actionCode = Intent.Extras.GetInt("ActionCode");

					using (dBHelper = new NmdDBAdapter(this))
					{
						dBHelper.OpenOrCreateDatabase(dBHelper.DBNamaad);
						SetMenu(companyCode, actionCode, userCode, isAdmin);
					}

				}
				else
				{
					ExceptionHandler.toastMsg(this, GetString(Resource.String.NoPermision));
				}
			}
			catch (Exception e)
			{
				ExceptionHandler.toastMsg(this, e.Message);
				ExceptionHandler.logDActivity(e.ToString(), _logging, TAG);
			}
		}
		protected override void OnResume()
		{
			base.OnResume();
			listView.ItemClick += OnListItemClick;

		}
		protected override void OnPause()
		{
			base.OnPause();
			listView.ItemClick -= OnListItemClick;
		}
		private void SetActionBar()
		{
			Window.RequestFeature(WindowFeatures.ActionBar);
			AddActionBarFlag(ActionBarDisplayOptions.ShowCustom);
			ActionBar.SetCustomView(Resource.Layout.actionbar);
			//ActionBar.SetDisplayHomeAsUpEnabled(true);
			//AddActionBarFlag(ActionBarDisplayOptions.HomeAsUp);
		}
		private void AddActionBarFlag(ActionBarDisplayOptions flag)
		{
			var bar = ActionBar;
			var change = bar.DisplayOptions ^ flag;
			ActionBar.SetDisplayOptions(change, flag);
		}
		public void SetActionBarTitle(string title)
		{
			((TextView)FindViewById(Resource.Id.ActionBarTitle)).Text = title;
		}
		private void SetMenu(int companyCode, int actionCode, int userCode, bool isAdmin)
		{
			name2class.Clear();
			/* Add Activities to list */
			bool hasRec = false;
			string strSQL;
			if (isAdmin)
			{
				strSQL = "Select Distinct * \n" +
				"From WebActions \n" +
				"Where ActionStatus=1";
				if (companyCode > 0)
					strSQL = strSQL + " And CompanyCode=" + companyCode + " And ParentCode = " + actionCode;
				else
					strSQL = strSQL + " And ActionCode=1 ";
			}
			else
			{
				strSQL = "Select Distinct A.* \n" +
				"From WebActions as A Inner Join \n" +
				"     (Select Distinct CompanyCode From WebUserActions Where UserCode=" + userCode + ") As U \n" +
				"On A.CompanyCode= U.CompanyCode \n" +
				"Where A.ActionCode=1 And A.ActionStatus=1";
			}

			using (SqliteDataReader reader = dBHelper.ExecuteReader(strSQL))
				while (reader.Read())
				{
					hasRec = true;

					ActionBase actBase = new ActionBase();
					actBase.CompanyCode = (int)reader["CompanyCode"];
					actBase.ActionCode = (int)reader["ActionCode"];
					actBase.ActionType = (int)reader["ActionType"];
					actBase.ActionName = reader["ActionName"].ToString();
					actBase.ParentCode = (int)reader["ParentCode"];
					actBase.ActionSource = reader["ActionSource"].ToString();
					actBase.ActionArgument = reader["ActionArgument"].ToString();
					actBase.DbNameServer = reader["DbNameServer"].ToString();
					actBase.DbNameClient = reader["DbNameClient"].ToString();

					addActivity(actBase);

				}
			if (!hasRec)
			{
				throw new Exception(GetString(Resource.String.NoPermision));
			}
			listView.Adapter = new HomeScreenAdapter(this, name2class);// Using cursor is not feasible since we use Mono SQlite library.
		}
		private void addActivity(ActionBase ab)
		{
			name2class.Add(ab);
		}
		protected void OnListItemClick(object sender, Android.Widget.AdapterView.ItemClickEventArgs e)
		{
			try
			{
				/* Find selected activity */
				ActionBase activity_class = name2class[e.Position];
				if (activity_class.ActionType == 2)
				{
					//dBHelper.Dispose();
					var type = Type.GetType(activity_class.ActionSource);
					ActionBaseForm form = (ActionBaseForm)Activator.CreateInstance(type); //new ActionBaseForm();
					((SharedEnviroment)ApplicationContext).ActionCode = activity_class.ActionCode;
					((SharedEnviroment)ApplicationContext).ActionName = activity_class.ActionName;
					((SharedEnviroment)ApplicationContext).CompanyCode = activity_class.CompanyCode;
					((SharedEnviroment)ApplicationContext).ActionSource = activity_class.ActionSource;
					((SharedEnviroment)ApplicationContext).DbNameClient = activity_class.DbNameClient;
					((SharedEnviroment)ApplicationContext).DbNameServer = activity_class.DbNameServer;
					((SharedEnviroment)ApplicationContext).UserCode = userCode;
					((SharedEnviroment)ApplicationContext).UserName = userName;
					((SharedEnviroment)ApplicationContext).IsAdmin = isAdmin;
					form.ShowForm(this);
				}
				if (activity_class.ActionType == 1)
				{
					var i = new Intent(this, typeof(NmdDBMain));
					i.PutExtra("UserCode", userCode);
					i.PutExtra("UserName", userName);
					i.PutExtra("IsAdmin", isAdmin);
					i.PutExtra("CompanyCode", activity_class.CompanyCode);
					i.PutExtra("ActionCode", activity_class.ActionCode);
					StartActivity(i);
				}
			}
			catch (Exception ex)
			{
				ExceptionHandler.toastMsg(this, ex.Message);
				ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
			}
		}
	}
}


