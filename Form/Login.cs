/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * Updated:20150801
 * */
// Decompiled by dotPeek after system crashing and Login.cs file lost on 2013/11/25 by Shahrooz Sabet
#region using
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Mono.Data.Sqlite;
using NamaadMobile.Data;
using NamaadMobile.Function;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
using NamaadMobile.WebService;
#endregion

namespace NamaadMobile
{
    /// <summary>
    /// Activity which displays a login screen to the user "Namaad Mobile"
    /// TODO: Using Cryptography to Store Credentials Safely
    /// TODO: Should check for Username and Password length and should store and compare
    /// Hashcode of password instead of clear text.
    /// TODO: Admin user should be online?
    /// </summary>
    [Activity(MainLauncher = true, Label = "@string/app_name")]
    public class Login : NamaadFormBase
    {
        #region Define
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadMobile.Login";
        /// <summary>
        /// Keep track of the login task to ensure we can cancel it if requested.
        /// </summary>
        private bool authTask = false;
        // UI references.
        private EditText etUserName;
        private EditText etPassword;
        private TextView tvLoginStatusMessage;
        private View viewStatus;
        private View viewForm;
        //private View mRefbtn;
        private Button btnSignIn;
        private Button btnDemo;
        // Values for email and password at the time of the login attempt.
        private string strUserName;
        private string strPassword;

        private NmdMobileDBAdapter dbHelper;
        private string strSQL;
        private bool _logging;
        //EventHandler
        private EventHandler<TextView.EditorActionEventArgs> etPassword_EditorAction;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _logging = Pref.getLogging(this);

            LoginActionBar();

            SetContentView(Resource.Layout.login);

            etUserName = FindViewById<EditText>(Resource.Id.userName);
            etPassword = FindViewById<EditText>(Resource.Id.password);
            viewStatus = FindViewById(Resource.Id.status);
            viewForm = FindViewById(Resource.Id.form);

            tvLoginStatusMessage = FindViewById<TextView>(Resource.Id.login_status_message);
            btnSignIn = FindViewById<Button>(Resource.Id.btnSignIn);
            //mRefbtn = FindViewById<Button>(Resource.Id.Refbtn);
            btnDemo = FindViewById<Button>(Resource.Id.btnDemo);
        }
        protected override void OnResume()
        {
            base.OnResume();
            etPassword_EditorAction = (object sender, TextView.EditorActionEventArgs e) =>
            {
                e.Handled = false;
                if (e.ActionId == ImeAction.Done || e.ActionId == ImeAction.ImeNull)
                {
                    attemptLogin();
                    e.Handled = true;
                }
                e.Handled = false;
            };
            etPassword.EditorAction += etPassword_EditorAction;
            //mRefbtn.Click += Refresh_Click;
            btnSignIn.Click += btnSignIn_Click;
            btnDemo.Click += btnDemo_Click;
        }
        protected override void OnPause()
        {
            base.OnPause();
            etPassword.EditorAction -= etPassword_EditorAction;
            //mRefbtn.Click -= Refresh_Click;
            btnSignIn.Click -= btnSignIn_Click;
            btnDemo.Click -= btnDemo_Click;
        }
        #endregion
        #region Function
        private void btnSignIn_Click(object sender, EventArgs e)
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
                    Pref.PutIsDBInSDCard(this);
                    using (NmdWSAdapter nmdWS = new NmdWSAdapter(this))
                    using (dbHelper = new NmdMobileDBAdapter(this))
                    {
                        dbHelper.OpenOrCreateDatabase(dbHelper.DBNamaad);//ToDo: We should create a preference UI which will write to XML resource file to read DBNamaad name.
                        ((SharedEnviroment)ApplicationContext).DbNameServer = ((SharedEnviroment)ApplicationContext).DbNameClient = dbHelper.DBNamaad;
                        DataTable dt = null;
                        using (dt = dbHelper.ExecuteSQL("Select Cast(1 As int) As SystemCode, Cast(1 As int) As TableCode ,'WebTableName' As TableName"))
                        {
                            nmdWS.RefreshWsCall(dt, 0).Wait();//task.Wait();//For Throwing exception, otherwise next line will be executed in case of exception.
                            if (dbHelper.Exist("WebTableCode"))
                            {
                                dt = dbHelper.ExecuteSQL("Select * From WebTableCode Where SystemCode=1 And TableCode>1 And TableCode<1000 ");
                                nmdWS.RefreshWsCall(dt, 0).Wait();//task.Wait();
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
            }
        ))
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
            if (authTask)
                return;

            // Reset errors.
            etUserName.Error = null;
            etPassword.Error = null;

            // Store values at the time of the login attempt.
            strUserName = etUserName.Text;
            strPassword = etPassword.Text;

            bool cancel = false;
            View view = null;

            // Check for a valid password.
            if (TextUtils.IsEmpty(strPassword))
            {
                etPassword.Error = "*";
                view = etPassword;
                cancel = true;
            }

            // Check for a valid User Name.
            if (TextUtils.IsEmpty(strUserName))
            {
                etUserName.Error = "*";
                view = etUserName;
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
                authTask = true;
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
                    Pref.PutIsDBInSDCard(this);
                    using (dbHelper = new NmdMobileDBAdapter(this))
                    {
                        dbHelper.OpenOrCreateDatabase(dbHelper.DBNamaad);//ToDo: We should create a preference UI which will write to XML resource file to read DBNamaad name.
                        ((SharedEnviroment)ApplicationContext).DbNameServer = ((SharedEnviroment)ApplicationContext).DbNameClient = dbHelper.DBNamaad;
                        byte[] data = Encoding.ASCII.GetBytes(strPassword);
                        byte[] sha1Password;
                        using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                            sha1Password = sha1.ComputeHash(data);
                        bool userExist = false;
                        using (SqliteDataReader reader = dbHelper.ExecuteReader(strSQL))
                            while (reader.Read())
                                if (reader["UserID"].Equals(strUserName))
                                {
                                    userExist = true;
                                    if (reader["UserData"].Equals(Encoding.UTF8.GetString(sha1Password)))
                                    {
                                        // Account exists, return true if the password matches.
                                        /* Create new result intent */
                                        Intent intentMenu = new Intent(this, typeof(NmdMobileMain));
                                        intentMenu.PutExtra("OrgID", (short)reader["OrgID"]);
                                        intentMenu.PutExtra("UserCode", (int)reader["UserCode"]);
                                        intentMenu.PutExtra("UserName", reader["FullName"].ToString());
                                        intentMenu.PutExtra("IsAdmin", Convert.ToBoolean(reader["IsAdmin"]));

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
                                            etPassword.RequestFocus();
                                        });
                                    }
                                }
                        // TODO: register the new account here if needed.
                        //if (!userExist) RegisterUser(sha1data);
                    }
                }
                catch (Exception eDB)
                {
                    RunOnUiThread(() =>
                    {
                        ExceptionHandler.toastMsg(this,
                            eDB.Message.Contains("SQLite error\r\nno such table:")
                                ? GetString(Resource.String.error_reRefresh_login)
                                : eDB.Message);
                    });
                    ExceptionHandler.logDActivity(eDB.ToString(), _logging, TAG);
                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        authTask = false;
                        showProgress(false);
                    });
                }
            }))
            {
                IsBackground = true
            }.Start();
        }
        /// <summary>
        /// Registers the user.
        /// </summary>
        /// <param name="sha1Password">The sha1Password.</param>
        private void RegisterUser(byte[] sha1Password)
        {
            using (DataTable dt = dbHelper.GetEmpty("WebUsers"))
            {
                DataRow dr = dt.NewRow();
                dr["OrgID"] = 1;
                dr["UserCode"] = (long)dbHelper.ExecuteScalar("Select IFNULL(Max(UserCode),0) From WebUsers") + 1;
                dr["UserID"] = strUserName;
                dr["UserData"] = Encoding.UTF8.GetString(sha1Password);
                dr["FullName"] = "کاربر نماد توسعه آریا";
                dr["IsAdmin"] = false;
                dr["TableDataVersion"] = 1;
                dbHelper.Insert("WebUsers", dr);
            }
        }
        private void btnDemo_Click(object sender, EventArgs e)
        {
            new Thread((ThreadStart)(() =>
            {
                try
                {
                    Pref.PutIsNotDBInSDCard(this);
                    if (!DataFunction.ExistInternalDB(this, GetString(Resource.String.DBNamaad)))
                    {
                        DataFunction.CopyDBFromAssetToInternalStorage(this, GetString(Resource.String.DBNamaad));
                        using (dbHelper = new NmdMobileDBAdapter(this))
                        {
                            ((SharedEnviroment)ApplicationContext).DbNameServer =
                                ((SharedEnviroment)ApplicationContext).DbNameClient = dbHelper.DBNamaad;
                            dbHelper.OpenOrCreateDatabase(dbHelper.DBNamaad);
                            using (SqliteDataReader reader = dbHelper.ExecuteReader("Select Distinct ActionArgument From WebActions"))
                                while (reader.Read())
                                    DataFunction.CopyDBFromAssetToInternalStorage(this, reader["ActionArgument"].ToString());
                        }
                    }
                    Intent intentMenu = new Intent(this, typeof(NmdMobileMain));
                    intentMenu.PutExtra("OrgID", (short)1);
                    intentMenu.PutExtra("UserCode", 1);
                    intentMenu.PutExtra("UserName", "کاربر دمو");
                    intentMenu.PutExtra("IsAdmin", true);

                    StartActivity(intentMenu);
                    Finish();
                }
                catch (Exception eDB)
                {
                    RunOnUiThread(() =>
                    {
                        ExceptionHandler.toastMsg(this,
                            eDB.Message.Contains("SQLite error\r\nno such table:")
                                ? GetString(Resource.String.error_reRefresh_login)
                                : eDB.Message);
                    });
                    ExceptionHandler.logDActivity(eDB.ToString(), _logging, TAG);
                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        authTask = false;
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
                viewStatus.Visibility = ViewStates.Visible;
                viewStatus.Animate().SetDuration(integer).Alpha(show ? 1 : 0).SetListener(new ObjectAnimatorListenerAdapter(viewStatus, show));

                viewForm.Visibility = ViewStates.Visible;
                viewForm.Animate().SetDuration(integer).Alpha(show ? 0 : 1).SetListener(new ObjectAnimatorListenerAdapter2(viewForm, show));
            }
            else
            {
                // The ViewPropertyAnimator APIs are not available, so simply show
                // and hide the relevant UI components.
                viewStatus.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
                viewForm.Visibility = show ? ViewStates.Gone : ViewStates.Visible;
            }
        }
        #endregion
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