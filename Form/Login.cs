/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
// Decompiled by dotPeek after system crashing and Login.cs file lost on 2013/11/25 by Shahrooz Sabet
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using System;
using System.Data;
using System.Threading;

using Mono.Data.Sqlite;
using NamaadDB.Util;
using NamaadDB.WebService;

namespace NamaadDB
{
	/// <summary>
	/// Activity which displays a login screen to the user "Namaad Mobile"
	/// TODO: Using Cryptography to Store Credentials Safely
	/// TODO: Should check for Username and Password length and should store and compare
	/// Hashcode of password instead of clear text.
	/// TODO: Admin user should be online?
	/// </summary>
	[Activity(MainLauncher = true, Label = "@string/app_name")]
	public class Login : ActionBaseForm
	{
		/// <summary>
		/// The tag For Debugging purpose
		/// </summary>
		private const string TAG = "NamaadDB.Login";
		/// <summary>
		/// Keep track of the login task to ensure we can cancel it if requested.
		/// </summary>
		private bool mAuthTask = false;

		// UI references.
		private EditText mUserNameView;
		private EditText mPasswordView;
		private TextView mLoginStatusMessageView;
		private View mStatusView;
		private View mFormView;
		private View mRefbtn;
		private View sign_in_button;

		// Values for email and password at the time of the login attempt.
		private string mUserName;
		private string mPassword;

		private NmdDBAdapter dBHelper;
		private string strSQL;
		private bool _logging;

		//EventHandler
		private EventHandler<TextView.EditorActionEventArgs> mPasswordView_EditorAction;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			_logging = Prefs.getLogging(this);

			LoginActionBar();

			SetContentView(Resource.Layout.login);

			mUserNameView = (EditText)FindViewById(Resource.Id.userName);
			mPasswordView = (EditText)FindViewById(Resource.Id.password);
			mStatusView = FindViewById(Resource.Id.status);
			mFormView = FindViewById(Resource.Id.form);

			mLoginStatusMessageView = (TextView)FindViewById(Resource.Id.login_status_message);
			sign_in_button = FindViewById(Resource.Id.sign_in_button);
			mRefbtn = FindViewById(Resource.Id.Refbtn);

		}
		protected override void OnResume()
		{
			base.OnResume();
			mPasswordView_EditorAction = (object sender, TextView.EditorActionEventArgs e) =>
			{
				e.Handled = false;
				if (e.ActionId == ImeAction.Done || e.ActionId == ImeAction.ImeNull)
				{
					attemptLogin();
					e.Handled = true;
				}
				e.Handled = false;
			};
			mPasswordView.EditorAction += mPasswordView_EditorAction;
			mRefbtn.Click += Refresh_Click;
			sign_in_button.Click += Login_Click;
		}
		protected override void OnPause()
		{
			base.OnPause();
			mPasswordView.EditorAction -= mPasswordView_EditorAction;
			mRefbtn.Click -= Refresh_Click;
			sign_in_button.Click -= Login_Click;
		}
		private void Login_Click(object sender, EventArgs e)
		{
			attemptLogin();
		}
		private void Refresh_Click(object sender, EventArgs e)
		{
			showProgress(true);
			//Task<RefreshDataCT> task = null;
			new Thread((ThreadStart)(() =>
				{
					try
					{
						using (NmdWSAdapter nmdWS = new NmdWSAdapter(this))
						using (dBHelper = new NmdDBAdapter(this))
						{
							dBHelper.OpenOrCreateDatabase(dBHelper.DBNamaad);//ToDo: We should create a preference UI which will write to XML resource file to read DBNamaad name.
							((SharedEnviroment)ApplicationContext).DbNameServer = ((SharedEnviroment)ApplicationContext).DbNameClient = dBHelper.DBNamaad;
							DataTable dt = null;
							using (dt = dBHelper.GetDataTable("Select Cast(1 As int) As CompanyCode, Cast(1 As int) As TableCode ,'WebTableName' As TableName"))
							{
								nmdWS.RefreshWSCall(dt, 0).Wait();//task.Wait();//For Throwing exception, otherwise next line will be executed in case of exception.
								if (dBHelper.IfExists("WebTableCode"))
								{
									dt = dBHelper.GetDataTable("Select * From WebTableCode Where CompanyCode=1 And TableCode>1 And TableCode<1000 ");
									nmdWS.RefreshWSCall(dt, 0).Wait();//task.Wait();
								}
							}
						}
					}
					catch (AggregateException ae)
					{
						ExceptionHandler.logDActivity(ae.ToString(), _logging, TAG);
						RunOnUiThread(() =>
						{
							ExceptionHandler.toastAggregateException(this, ae);
						});
					}
					catch (Exception ex)
					{
						ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
						RunOnUiThread(() =>
						{
							ExceptionHandler.toastMsg(this, ex.Message);
						});
					}
					finally
					{
						RunOnUiThread(() =>
						{
							showProgress(false);
						});
					}
				}))
			{
				IsBackground = true
			}.Start();
		}

		private void LoginActionBar()
		{
			ActionBar.SetDisplayHomeAsUpEnabled(false);
			SetActionBarTitle(GetString(Resource.String.title_activity_login));
		}
		/// <summary>
		/// Attempts to sign in the account specified by the login form.
		/// If there are form errors (invalid User Name, missing fields, etc.), the
		/// errors are presented and no actual login attempt is made.
		/// </summary>
		public void attemptLogin()
		{
			if (mAuthTask)
				return;

			// Reset errors.
			mUserNameView.Error = null;
			mPasswordView.Error = null;

			// Store values at the time of the login attempt.
			mUserName = mUserNameView.Text;
			mPassword = mPasswordView.Text;

			bool cancel = false;
			View view = null;

			// Check for a valid password.
			if (TextUtils.IsEmpty(mPassword))
			{
				mPasswordView.Error = "*";
				view = mPasswordView;
				cancel = true;
			}

			// Check for a valid User Name.
			if (TextUtils.IsEmpty(mUserName))
			{
				mUserNameView.Error = "*";
				view = mUserNameView;
				cancel = true;
			}
			if (cancel)
			{
				// There was an error; don't attempt login and focus the first
				// form field with an error.
				view.RequestFocus();
			}
			else
			{
				// Show a progress spinner, and kick off a background task to
				// perform the user login attempt.
				//mLoginStatusMessageView.Text = GetString(Resource.String.login_progress_signing_in);
				showProgress(true);
				mAuthTask = true;
				UserLoginThread();
			}
		}



		// TODO: change UserLoginThread method to use a task like Google Login code template
		/// <summary>
		/// Users login thread.
		/// Represents an asynchronous login task used to authenticate
		/// the user.
		/// </summary>
		private void UserLoginThread()
		{
			new Thread((ThreadStart)(() =>
			{
				strSQL = "Select * From WebUsers";
				try
				{
					using (dBHelper = new NmdDBAdapter(this))
					{
						dBHelper.OpenOrCreateDatabase(dBHelper.DBNamaad);//ToDo: We should create a preference UI which will write to XML resource file to read DBNamaad name.
						((SharedEnviroment)ApplicationContext).DbNameServer = ((SharedEnviroment)ApplicationContext).DbNameClient = dBHelper.DBNamaad;
						using (SqliteDataReader reader = dBHelper.ExecuteReader(strSQL))
							while (reader.Read())
								if (reader.GetString(1).Equals(mUserName))
								{
									if (reader.GetString(2).Equals(mPassword))
									{
										// Account exists, return true if the password matches.
										/* Create new result intent */
										Intent intentMenu = new Intent(this, typeof(NmdDBMain));
										intentMenu.PutExtra("UserCode", reader.GetInt32(0));
										intentMenu.PutExtra("UserName", reader.GetString(4));
										intentMenu.PutExtra("IsAdmin", Convert.ToBoolean(reader.GetInt16(5)));
										StartActivity(intentMenu);
										Finish();
										break;
									}
									else
									{
										RunOnUiThread(() =>
										{   // simulate onPostExecute in task
											//showProgress(false);
											ExceptionHandler.showException(GetString(Resource.String.error_incorrect_password), this);
											mPasswordView.RequestFocus();
										});
									}
								}
						//else
						//{
						// TODO: register the new account here if needed.
						//}
					}
				}
				catch (Exception eDB)
				{
					RunOnUiThread(() =>
					{
						if (eDB.Message.Contains("SQLite error\r\nno such table:"))
							ExceptionHandler.toastMsg(this, GetString(Resource.String.error_reRefresh_login));
						else
							ExceptionHandler.toastMsg(this, eDB.Message);
					});
					ExceptionHandler.logDActivity(eDB.ToString(), _logging, TAG);
				}
				finally
				{
					RunOnUiThread(() =>
					{
						mAuthTask = false;
						showProgress(false);
					});

				}
			}))
			{
				IsBackground = true
			}.Start();
		}
		/// <summary>
		/// Shows the progress UI and hides the login form.
		/// </summary>
		/// <param name="show">if set to <c>true</c> [show].</param>
		public void showProgress(bool show)
		{
			// On Honeycomb MR2 we have the ViewPropertyAnimator APIs, which allow
			// for very easy animations. If available, use these APIs to fade-in
			// the progress spinner.
			if (Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr2)
			{
				int integer = Resources.GetInteger(Android.Resource.Integer.ConfigShortAnimTime);
				mStatusView.Visibility = ViewStates.Visible;
				mStatusView.Animate().SetDuration(integer).Alpha(show ? 1 : 0).SetListener(new ObjectAnimatorListenerAdapter(mStatusView, show));

				mFormView.Visibility = ViewStates.Visible;
				mFormView.Animate().SetDuration(integer).Alpha(show ? 0 : 1).SetListener(new ObjectAnimatorListenerAdapter2(mFormView, show));
			}
			else
			{
				// The ViewPropertyAnimator APIs are not available, so simply show
				// and hide the relevant UI components.
				mStatusView.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
				mFormView.Visibility = show ? ViewStates.Gone : ViewStates.Visible;
			}
		}
	}
}

//public void SetActionBarTitle(String title)
//{
//	TextView tv = (TextView)FindViewById(Resource.Id.ActionBarTitle);
//	tv.Text = title;

//}
//private void SetActionBar()
//{
//	Window.RequestFeature(WindowFeatures.ActionBar);
//	AddActionBarFlag(ActionBarDisplayOptions.ShowCustom);
//	ActionBar.SetCustomView(Resource.Layout.ActionBarLogin);
//	//ActionBar.SetDisplayHomeAsUpEnabled(true);
//	//AddActionBarFlag(ActionBarDisplayOptions.HomeAsUp);
//}
//private void AddActionBarFlag(ActionBarDisplayOptions flag)
//{
//	var bar = ActionBar;
//	var change = bar.DisplayOptions ^ flag;
//	ActionBar.SetDisplayOptions(change, flag);
//}

//public class UserLoginTask : AsyncTask{
//    protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
//    {
//        strSQL = "select * from WebUsers";
//        ICursor curUser = db.RawQuery(strSQL, null);
//        if (curUser.MoveToFirst())
//        {
//            do
//            {
//                if (curUser.GetString(1).Equals(mUserName))
//                {
//                    if (curUser.GetString(2).Equals(mPassword))
//                    {
//                        // Account exists, return true if the password matches.
//                        db.Close();
//                        StartActivity(new Intent(this, typeof(NamaadDBMain)));
//                        Finish();
//                        break;
//                    }
//                }
//            }
//            while (curUser.MoveToNext());
//        }
//        return true;
//    }
//    protected override void OnPostExecute(Java.Lang.Object result)
//    {

//    }
//    protected override void OnCancelled()
//    { 

//    }
//}

//try
//{
//    string dropTableSQL = "DROP TABLE IF EXISTS [WebUsers2];";
//    string createTableSQL = "CREATE TABLE [WebUsers2]([UserCode] [int] PRIMARY KEY NOT NULL,[UserID] [varchar](100) NOT NULL,[UserData] [varchar](1000) NOT NULL);";

//    dBHelper.BeginTransaction();
//    dBHelper.DropTable("WebUsers2");
//    dBHelper.ExecuteNonQuery(createTableSQL);
//    dBHelper.EndTransaction();
//    string sql = "Select * from [WebUsers];";
//    //showProgress(true);
//    dBHelper.FillToTable(dBHelper.GetDataTable(sql), "WebUsers2");
//}
//catch (Exception eDB)
//{
//    Toast.MakeText(this, eDB.Message, ToastLength.Long).Show();
//    Log.Debug(TAG, eDB.Message);
//    Log.Debug(TAG, eDB.StackTrace);
//}

//private void DBPrep()
//{
//	new Thread((ThreadStart)(() =>
//	{
//		try
//		{
//			dBHelper.dbCon(NmdDBAdapter.DBNamaad);
//			WebUsersPrep();
//		}
//		catch (Exception eDB)
//		{
//			RunOnUiThread(() =>
//			{
//				Toast.MakeText(this, eDB.Message, ToastLength.Long).Show();
//			});
//			Log.Debug(TAG, eDB.ToString());
//		}
//	}))
//	{
//		IsBackground = true
//	}.Start();
//}

//private void WebUsersPrep()
//{
//	string dropTableSQL = "DROP TABLE IF EXISTS [WebUsers];";
//	string createTableSQL = "CREATE TABLE [WebUsers]([UserCode] [int] PRIMARY KEY NOT NULL,[UserID] [varchar](100) NOT NULL,[UserData] [varchar](1000) NOT NULL);";

//	dBHelper.BeginTransaction();
//	dBHelper.DropTable("WebUsers");
//	dBHelper.ExecuteNonQuery(createTableSQL);
//	dBHelper.EndTransaction();

//	string insertSql = "INSERT INTO [WebUsers]([UserCode],[UserID],[UserData])VALUES(1,'1','1'),(2,'2','2')";
//	dBHelper.ExecuteNonQuery(insertSql);
//}