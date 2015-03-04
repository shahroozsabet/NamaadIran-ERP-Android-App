/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;

using NamaadDB.Entity;
using NamaadDB.Util;
using NamaadDB.WebService;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace NamaadDB
{
	[Activity(Label = "@string/RefreshData")]
	public class RefreshData : ActionBaseForm
	{
		/// <summary>
		/// The tag For Debugging purpose
		/// </summary>
		private const string TAG = "NamaadDB.RefreshData";

		private NmdDBAdapter dBHelper;

		private List<RefreshDataEntity> tableDesc = null;//TODO: Check add to list in order to underestand JavaList usage instead.

		// UI references.
		private ListView listView;
		private View mStatusView;
		private View mFormView;
		private View mRefbtn;
		private Button chkAllbtn;

		private bool _logging;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			_logging = Prefs.getLogging(this);
			SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);

			SetContentView(Resource.Layout.refresh_data);

			listView = FindViewById<ListView>(Resource.Id.listView);
			mRefbtn = FindViewById(Resource.Id.Refbtn);
			chkAllbtn = (Button)FindViewById(Resource.Id.ChkAllbtn);
			mStatusView = FindViewById(Resource.Id.status);
			mFormView = FindViewById(Resource.Id.form);

			tableDesc = new List<RefreshDataEntity>();
			string strSQL = "Select * From WebTableCode Where CompanyCode= " + ((SharedEnviroment)ApplicationContext).CompanyCode + " And TableCode>1000";

			try
			{
				using (dBHelper = new NmdDBAdapter(this))
				{
					dBHelper.OpenOrCreateDatabase(dBHelper.DBNamaad);
					using (SqliteDataReader reader = dBHelper.ExecuteReader(strSQL))
					{
						while (reader.Read())
						{
							RefreshDataEntity refDataEnt = new RefreshDataEntity();
							refDataEnt.TableDesc = reader["TableDesc"].ToString();
							refDataEnt.TableCode = (int)reader["TableCode"];
							refDataEnt.CompanyCode = (int)reader["CompanyCode"];
							refDataEnt.TableName = reader["TableName"].ToString();

							tableDesc.Add(refDataEnt);
						}
					}
				}
			}
			catch (Exception e)
			{
				ExceptionHandler.toastMsg(this, e.Message);
				ExceptionHandler.logDActivity(e.ToString(), _logging, TAG);
			}

			listView.Adapter = new ArrayAdapter<RefreshDataEntity>
				(this, Android.Resource.Layout.SimpleListItemMultipleChoice, tableDesc);

			listView.ItemsCanFocus = false;
			listView.ChoiceMode = ChoiceMode.Multiple;
			listView.TextFilterEnabled = true;

		}

		private void chkAllbtn_Click(object sender, EventArgs e)
		{
			bool check = listView.IsItemChecked(0);
			int size = listView.Count;
			for (int i = 0; i <= size; i++)
				listView.SetItemChecked(i, !check);
		}
		protected override void OnResume()
		{
			base.OnResume();
			mRefbtn.Click += RefreshData_Click;
			chkAllbtn.Click += chkAllbtn_Click;
		}
		protected override void OnPause()
		{
			base.OnPause();
			mRefbtn.Click -= RefreshData_Click;
			chkAllbtn.Click -= chkAllbtn_Click;
		}
		private void RefreshData_Click(object sender, EventArgs e)
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
					using (dBHelper = new NmdDBAdapter(this))
					{
						dBHelper.OpenOrCreateDatabase(dBHelper.DBNamaad);
						using (DataTable dt = dBHelper.GetDataTable("Select * From WebTableCode Where CompanyCode=" + ((SharedEnviroment)ApplicationContext).CompanyCode + " And TableCode in(" + strTableCode.ToString() + ")"))
						{
							var task = nmdWS.RefreshWSCall(dt, 0);
							task.Wait();
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

		//private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
		//{
		//	var listView = sender as ListView;
		//	var t = tableDesc[e.Position];
		//	//Android.Widget.Toast.MakeText(this, t.TableDesc, Android.Widget.ToastLength.Short).Show();
		//	//Console.WriteLine("Clicked on " + t);
		//}
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