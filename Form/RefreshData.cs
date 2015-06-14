/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;
using NamaadMobile.SharedElement;
using NamaadMobile.Data;
using NamaadMobile.Entity;
using NamaadMobile.Util;
using NamaadMobile.WebService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
#endregion
namespace NamaadMobile
{
    [Activity(Label = "@string/RefreshData")]
    public class RefreshData : NamaadMobile.SharedElement.NamaadFormBase
    {
        #region Define
        private NmdMobileDBAdapter dbHelper;

        private List<RefreshDataEntity> tableDesc = null;//TODO: Check add to list in order to underestand JavaList usage instead.
        // UI references.
        private ListView listView;
        private View viewStatus;
        private View viewForm;
        private Button btnRef;
        private Button btnChkAll;

        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Prefs.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.RefreshData";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);

            SetContentView(Resource.Layout.refresh_data);

            listView = FindViewById<ListView>(Resource.Id.listView);
            btnRef = (Button)FindViewById(Resource.Id.btnRef);
            btnChkAll = (Button)FindViewById(Resource.Id.btnChkAll);
            viewStatus = FindViewById(Resource.Id.status);
            viewForm = FindViewById(Resource.Id.form);

            tableDesc = new List<RefreshDataEntity>();
            string strSQL = "Select * From WebTableCode Where SystemCode= " + ((SharedEnviroment)ApplicationContext).SystemCode + " And TableCode>1000";

            try
            {
                using (dbHelper = new NmdMobileDBAdapter(this))
                {
                    dbHelper.OpenOrCreateDatabase(dbHelper.DBNamaad);
                    using (SqliteDataReader reader = dbHelper.ExecuteReader(strSQL))
                    {
                        while (reader.Read())
                        {
                            RefreshDataEntity refDataEnt = new RefreshDataEntity();
                            refDataEnt.TableDesc = reader["TableDesc"].ToString();
                            refDataEnt.TableCode = (int)reader["TableCode"];
                            refDataEnt.SystemCode = (int)reader["SystemCode"];
                            refDataEnt.TableName = reader["TableName"].ToString();

                            tableDesc.Add(refDataEnt);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(this, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
            }

            listView.Adapter = new ArrayAdapter<RefreshDataEntity>
                (this, Android.Resource.Layout.SimpleListItemMultipleChoice, tableDesc);

            listView.ItemsCanFocus = false;
            listView.ChoiceMode = ChoiceMode.Multiple;
            listView.TextFilterEnabled = true;

        }
        protected override void OnResume()
        {
            base.OnResume();
            btnRef.Click += btnRef_Click;
            btnChkAll.Click += btnChkAll_Click;
        }
        protected override void OnPause()
        {
            base.OnPause();
            btnRef.Click -= btnRef_Click;
            btnChkAll.Click -= btnChkAll_Click;
        }
        //private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        //{
        //	var listView = sender as ListView;
        //	var t = tableDesc[e.Position];
        //	//Android.Widget.Toast.MakeText(this, t.TableDesc, Android.Widget.ToastLength.Short).Show();
        //	//Console.WriteLine("Clicked on " + t);
        //}
        #endregion
        #region Function
        private void btnChkAll_Click(object sender, EventArgs e)
        {
            bool check = listView.IsItemChecked(0);
            int size = listView.Count;
            for (int i = 0; i <= size; i++)
                listView.SetItemChecked(i, !check);
        }
        private void btnRef_Click(object sender, EventArgs e)
        {
            showProgress(true);
            new Thread((ThreadStart)(() =>
            {
                int size = listView.Count;
                StringBuilder strTableCode = null;
                for (int i = 0; i < size; i++)
                {
                    if (listView.IsItemChecked(i))
                    {
                        var t = tableDesc[i];
                        if (strTableCode != null)
                            strTableCode.Append(",");
                        else if (strTableCode == null)
                            strTableCode = new StringBuilder();
                        strTableCode.Append(t.TableCode);
                    }
                }
                try
                {
                    using (NmdWSAdapter nmdWS = new NmdWSAdapter(this))
                    using (dbHelper = new NmdMobileDBAdapter(this))
                    {
                        dbHelper.OpenOrCreateDatabase(dbHelper.DBNamaad);
                        using (DataTable dt = dbHelper.ExecuteSQL("Select * From WebTableCode Where SystemCode=" + ((SharedEnviroment)ApplicationContext).SystemCode + " And TableCode in(" + strTableCode.ToString() + ")"))
                        {
                            var task = nmdWS.RefreshWsCall(dt, 0);
                            task.Wait();
                        }
                    }
                }
                catch (AggregateException ae)
                {
                    ExceptionHandler.logDActivity(ae.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
                    RunOnUiThread(() =>
                    {
                        ExceptionHandler.toastAggregateException(this, ae);
                    });
                }
                catch (Exception ex)
                {
                    ExceptionHandler.logDActivity(ex.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
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