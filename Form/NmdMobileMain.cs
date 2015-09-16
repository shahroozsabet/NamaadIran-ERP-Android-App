/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * Updated:20150916
 * */
#region using
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;
using NamaadMobile.Adapter;
using NamaadMobile.Data;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
#endregion
namespace NamaadMobile
{
    /// <summary>
    /// TODO: Can use Navigation Drawer?
    /// </summary>
    [Activity(Label = "@string/NmdSys")]
    public class NmdMobileMain : Activity
    {
        #region Define
        internal List<ActionBase> name2class = new List<ActionBase>();

        public static NmdMobileDBAdapter dBHelper;
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadMobile.NmdMobileMain";
        private string userName;
        private int userCode;
        private bool isAdmin;
        private int systemCode;
        private short orgID;
        // UI references.
        private TextView tvUserName;
        private GridView listView;

        private bool _logging;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _logging = Pref.getLogging(this);
            SetActionBar();
            SetActionBarTitle(GetString(Resource.String.NmdSys));
            SetContentView(Resource.Layout.home_screen);
            //AddActionBarFlag(ActionBarDisplayOptions.HomeAsUp);

            listView = FindViewById<GridView>(Android.Resource.Id.List);
            tvUserName = (TextView)FindViewById(Resource.Id.UserNameLable);

            try
            {
                if (Intent.Extras != null)
                {
                    userName = Intent.Extras.GetString("UserName");
                    tvUserName.Text = GetString(Resource.String.Welcome) + " " + userName;
                    userCode = Intent.Extras.GetInt("UserCode");
                    isAdmin = Intent.Extras.GetBoolean("IsAdmin");

                    systemCode = Intent.Extras.GetInt("SystemCode");
                    orgID = Intent.Extras.GetShort("OrgID");
                    var actionCode = Intent.Extras.GetInt("ActionCode");

                    using (dBHelper = new NmdMobileDBAdapter(this))
                    {
                        dBHelper.OpenOrCreateDatabase(dBHelper.DBNamaad);
                        SetMenu(orgID, systemCode, actionCode, userCode, isAdmin);
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
        protected void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                /* Find selected activity */
                ActionBase activity_class = name2class[e.Position];
                if (activity_class.ActionType == 2)
                {
                    //dBHelper.Dispose();
                    var type = Type.GetType(activity_class.ActionSource);
                    ((SharedEnviroment)ApplicationContext).OrgID = activity_class.OrgID;
                    ((SharedEnviroment)ApplicationContext).SystemCode = activity_class.SystemCode;
                    ((SharedEnviroment)ApplicationContext).ActionCode = activity_class.ActionCode;
                    ((SharedEnviroment) ApplicationContext).ActionName = activity_class.ActionName;
                    ((SharedEnviroment)ApplicationContext).ActionSource = activity_class.ActionSource;
                    ((SharedEnviroment)ApplicationContext).ActionArgument = activity_class.ActionArgument;
                    ((SharedEnviroment)ApplicationContext).DbNameClient = activity_class.DbNameClient;
                    ((SharedEnviroment)ApplicationContext).DbNameServer = activity_class.DbNameServer;
                    ((SharedEnviroment)ApplicationContext).UserCode = userCode;
                    ((SharedEnviroment)ApplicationContext).UserName = userName;
                    ((SharedEnviroment)ApplicationContext).IsAdmin = isAdmin;
                    object obj = Activator.CreateInstance(type);
                    if (obj is NamaadFormBase)
                    {
                        NamaadFormBase form = (NamaadFormBase)obj; //new ActionBaseForm();
                        form.ShowForm(this);
                    }
                    else if (obj is NamaadPrefBase)
                    {
                        NamaadPrefBase form = (NamaadPrefBase)obj; //new ActionBaseForm();
                        form.ShowForm(this);
                    }
                }
                if (activity_class.ActionType == 1)
                {
                    Intent i = new Intent(this, typeof(NmdMobileMain));
                    i.PutExtra("UserCode", userCode);
                    i.PutExtra("UserName", userName);
                    i.PutExtra("IsAdmin", isAdmin);
                    i.PutExtra("OrgID", activity_class.OrgID);
                    i.PutExtra("SystemCode", activity_class.SystemCode);
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
        #endregion
        #region Function
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
        private void SetMenu(short orgID, int systemCode, int actionCode, int userCode, bool isAdmin)
        {
            name2class.Clear();
            /* Add Activities to list */
            bool hasRec = false;
            string strSQL;
            if (isAdmin)
            {
                strSQL = "Select Distinct * \n" +
                "From WebActions As A \n" +
                "Where OrgID=" + orgID + " And ActionStatus=1";
            }
            else
            {
                strSQL = "Select Distinct A.* \n" +
                "From WebActions As A Inner Join \n" +
                "     (Select Distinct OrgID, SystemCode From WebUserActions Where UserCode=" + userCode + ") As U \n" +
                "On A.OrgID=U.OrgID And A.SystemCode= U.SystemCode \n" +
                "Where A.OrgID=U.OrgID And A.ActionStatus=1";
            }
            if (systemCode > 0 && actionCode > 0)
                strSQL = strSQL + " And A.SystemCode=" + systemCode + " And ParentCode = " + actionCode;
            else
                strSQL = strSQL + " And ActionCode=1 ";

            using (SqliteDataReader reader = dBHelper.ExecuteReader(strSQL))
                while (reader.Read())
                {
                    hasRec = true;
                    ActionBase actBase = new ActionBase
                    {
                        OrgID = (short)reader["OrgID"],
                        SystemCode = (int)reader["SystemCode"],
                        ActionCode = (int)reader["ActionCode"],
                        ActionType = (int)reader["ActionType"],
                        ActionName = GetString(Resources.GetIdentifier(reader["ActionName"].ToString(), "string", PackageName)),
                        ParentCode = (int)reader["ParentCode"],
                        ActionSource = reader["ActionSource"].ToString(),
                        ActionArgument = reader["ActionArgument"].ToString(),
                        DbNameServer = reader["DbNameServer"].ToString(),
                        DbNameClient = reader["DbNameClient"].ToString()
                    };
                    AddActivity(actBase);
                }
            if (!hasRec)
            {
                throw new Exception(GetString(Resource.String.NoPermision));
            }
            listView.Adapter = new HomeScreenAdapter(this);// Using cursor is not feasible since we use Mono SQlite library.
        }
        private void AddActivity(ActionBase ab)
        {
            name2class.Add(ab);
        }
        #endregion
    }
}


