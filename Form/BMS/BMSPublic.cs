/*
 * Author: Shahrooz Sabet
 * Date: 20150503
 * Updated:20150809
 * */
#region using
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;
using NamaadMobile.Data;
using NamaadMobile.Function;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
using System.Collections.Generic;
using Android.App;
using Android.Graphics.Drawables;
using NamaadMobile.Adapter;
#endregion
namespace NamaadMobile
{
    public class BMSPublic
    {
        #region Define
        private const int timeOut = 5000;
        private List<BMSViewHolder> itemList = new List<BMSViewHolder>();
        #endregion
        #region Public Function
        /// <summary>
        /// Adds switch to layout dynamically.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="bmsListView">The main layout.</param>
        public void AddSwitchDeviceToLayout(Context owner, ListView bmsListView)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    object obj = dbHelper.ExecuteScalar("SELECT Count(ID) FROM Device Where (IOType =1 OR (IOType=0 And Editable=0)) And CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode);
                    if (obj != DBNull.Value)
                    {
                        int recCount = int.Parse(obj.ToString());
                        if (recCount == 0) return;
                    }
                    else return;
                    using (SqliteDataReader reader = dbHelper.ExecuteReader("Select * From Device Where (IOType =1 OR (IOType=0 And Editable=0)) And CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode))
                    {
                        while (reader.Read())
                        {
                            BMSViewHolder swDevice = new BMSViewHolder
                            {
                                Text = reader["Name"].ToString(),
                                Id = (int)reader["ID"],
                                IsSwitch = true,
                            };
                            swDevice.Enabled = (bool)reader["IOType"] && (NConvert.Int2Bool(GetAccessId(owner)) || HasAccess(owner, swDevice.Id));
                            swDevice.Checked = (!(reader["Value"] is DBNull) && (int)reader["Value"] != 0);
                            if (swDevice.Enabled) swDevice.CheckedChange += swDevice_Click;
                            itemList.Add(swDevice);
                        }
                    }
                }
                if (bmsListView.Adapter == null)
                    bmsListView.Adapter = new BMSViewHolderAdapter((Activity)owner, itemList);
                else
                    ((BMSViewHolderAdapter)(bmsListView.Adapter)).NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
        }
        /// <summary>
        /// Adds the sensor to layout.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="mainLayout">The main layout.</param>
        public async void AddEditableSensorToLayout(Context owner, ListView bmsListView)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    object obj = dbHelper.ExecuteScalar("SELECT Count(ID) FROM Device Where IOType=0 And Editable=1 And CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode);
                    if (obj != DBNull.Value)
                    {
                        int recCount = int.Parse(obj.ToString());
                        if (recCount == 0) return;
                    }
                    else return;
                    using (SqliteDataReader reader = dbHelper.ExecuteReader("Select * From Device Where IOType=0 And Editable=1 And CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode))
                    {
                        while (reader.Read())
                        {
                            BMSViewHolder swDevice = new BMSViewHolder
                            {
                                Text = reader["Name"].ToString(),
                                Id = (int)reader["ID"],
                                IsSwitch = false
                            };
                            swDevice.Click += btnEditableDevice_Click;
                            itemList.Add(swDevice);
                        }
                    }
                }
                if (bmsListView.Adapter == null)
                    bmsListView.Adapter = new BMSViewHolderAdapter((Activity)owner, itemList);
                else
                    ((BMSViewHolderAdapter)(bmsListView.Adapter)).NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
        }
        /// <summary>
        /// Adds the reset to layout.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="mainLayout">The main layout.</param>
        public void AddResetToLayout(Context owner, LinearLayout mainLayout)
        {
            Button btnReset = new Button(owner)
            {
                Id = -2,
                Text = owner.GetString(Resource.String.Reset)
            };
            btnReset.SetCompoundDrawablesRelativeWithIntrinsicBounds(owner.Resources.GetDrawable(Resource.Drawable.ic_btn_reset), null, null, null);
            btnReset.Click += btnReset_Click;
            btnReset.LayoutParameters = new LinearLayout.LayoutParams((int)owner.Resources.GetDimension(Android.Resource.Dimension.ThumbnailWidth), ViewGroup.LayoutParams.WrapContent);
            mainLayout.AddView(btnReset);
        }
        public void AddRefreshToLayout(Context owner, LinearLayout mainLayout)
        {
            Button btnRefresh = new Button(owner)
            {
                Id = -1,
                Text = owner.GetString(Resource.String.action_Refresh_Data)
            };
            btnRefresh.SetCompoundDrawablesRelativeWithIntrinsicBounds(owner.Resources.GetDrawable(Resource.Drawable.ic_button_refresh), null, null, null);
            btnRefresh.Click += btnRefresh_Click;
            btnRefresh.LayoutParameters = new LinearLayout.LayoutParams((int)owner.Resources.GetDimension(Android.Resource.Dimension.ThumbnailWidth), ViewGroup.LayoutParams.WrapContent);
            mainLayout.AddView(btnRefresh);
        }


        /// <summary>
        /// Gets the device dr.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        public DataRow GetDeviceDr(Context owner, int deviceId)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    return dbHelper.ExecuteSQL("SELECT * FROM Device Where ID=" + deviceId).Rows[0];
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return null;
        }
        /// <summary>
        /// Gets the access identifier.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="userCode">The optional user code which will be got from owner Context if it isnot supplied.</param>
        /// <returns>
        /// User AccessID if it exists, otherwise it return -1
        /// </returns>
        public int GetAccessId(Context owner, int userCode = -1)
        {
            int accessIdInt = -1;
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    if (userCode == -1) userCode = ((SharedEnviroment)owner.ApplicationContext).UserCode;
                    object accessId = dbHelper.ExecuteScalar("SELECT AccessID FROM User Where ID=" + userCode);
                    if (accessId != DBNull.Value) accessIdInt = int.Parse(accessId.ToString());
                    return accessIdInt;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return accessIdInt;
        }
        /// <summary>
        /// Gets the device identifier by access identifier.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="accessId">The access identifier.</param>
        /// <returns>deviceID by its accessID code, if it doesnt exist the method returns -1</returns>
        public int GetDeviceIdByAccessId(Context owner, int accessId)
        {
            int deviceIdInt = -1;
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    object deviceId = dbHelper.ExecuteScalar("SELECT DeviceID FROM Access_Device Where AccessID=" + accessId);
                    if (deviceId != DBNull.Value) deviceIdInt = int.Parse(deviceId.ToString());
                    return deviceIdInt;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return deviceIdInt;
        }
        /// <summary>
        /// Gets the access identifier by device identifier.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>Device AccessID if it exists, otherwise it return -1</returns>
        public int GetAccessIdByDeviceId(Context owner, int deviceId)
        {
            int accessIdInt = -1;
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    object accessId = dbHelper.ExecuteScalar("SELECT AccessID FROM Access_Device Where DeviceID=" + deviceId);
                    if (accessId != DBNull.Value) accessIdInt = int.Parse(accessId.ToString());
                    return accessIdInt;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return accessIdInt;
        }
        /// <summary>
        /// Determines whether the specified owner has access.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="userCode">The user code if not specified it gets from owner Context</param>
        /// <returns>A Boolean indicates whether userCode has deviceID permision or not </returns>
        public bool HasAccess(Context owner, int deviceId, int userCode = -1)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    if (userCode == -1) userCode = ((SharedEnviroment)owner.ApplicationContext).UserCode;
                    string sql = "Select ID  \n"
                               + "From User As u \n"
                               + "Inner Join \n"
                               + "( \n"
                               + "Select *  \n"
                               + "From Access_Device \n"
                               + "Where DeviceID=" + deviceId + " \n"
                               + ") As t \n"
                               + "On u.AccessID=t.AccessID \n"
                               + "Where t.DeviceID=" + deviceId + " And u.ID=" + userCode;
                    if (dbHelper.ExecuteScalar(sql) != null) return true;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return false;
        }
        /// <summary>
        /// Determines whether the specified context is tablet.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static bool IsTablet(Context context)
        {
            return (
                context.Resources.Configuration.ScreenLayout
                & ScreenLayout.SizeMask) >= ScreenLayout.SizeLarge;
        }
        /// <summary>
        /// Gets the device dr.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="port">The port.</param>
        /// <param name="ioType">if set to <c>true</c> [io type].</param>
        /// <returns></returns>
        public static DataRow GetDeviceDr(Context owner, int port, short ioType)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    return dbHelper.ExecuteSQL("SELECT * FROM Device Where Port=" + port + " And IOType=" + ioType).Rows[0];
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return null;
        }
        #endregion
        #region Private Function
        /// <summary>
        /// Handles the Click event of the btnReset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(((Button)sender).Context))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)(((Button)sender).Context).ApplicationContext).ActionArgument);
                    dbHelper.ExecuteNonQuery("Update Device Set Value=null Where CategoryID=" + ((SharedEnviroment)(((Button)sender).Context).ApplicationContext).ActionCode);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.toastMsg((((Button)sender).Context), ex.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)((Button)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Button)sender).Context.ApplicationContext).TAG);
            }
        }
        /// <summary>
        /// Handles the Click event of the swDyn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CompoundButton.CheckedChangeEventArgs"/> instance containing the event data.</param>
        private async void swDevice_Click(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            DataRow drDevice = GetDeviceDr(((Switch)sender).Context, (int)((Switch)sender).Tag);
            int port = (int)drDevice["Port"];
            bool ioType = (bool)drDevice["ioType"];
            int value = 0;
            if (drDevice["Value"] != DBNull.Value) value = (int)drDevice["Value"];
            // ***Instantiate the CancellationTokenSource.
            // ***Declare a System.Threading.CancellationTokenSource.
            CancellationTokenSource cts = new CancellationTokenSource();
            // ***Set up the CancellationTokenSource to cancel after 5 seconds.
            cts.CancelAfter(timeOut);
            try
            {
                if (!await UpdateValue(((Switch)sender).Context, port, (short)NConvert.Bool2Int(ioType)))
                {
                    if (value == 0)
                    {
                        bool isValidate = await SendAndValidate(((Switch)sender).Context, port, true, cts);
                        drDevice["Value"] = 1;
                    }
                    else
                    {
                        bool isValidate = await SendAndValidate(((Switch)sender).Context, port, false, cts);
                        drDevice["Value"] = 0;
                    }
                    using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(((Switch)sender).Context))
                    {
                        dbHelper.OpenOrCreateDatabase(
                            ((SharedEnviroment)((Switch)sender).Context.ApplicationContext).ActionArgument);
                        dbHelper.Update("Device", drDevice);
                    }
                }
                else
                {
                    ((Switch)sender).CheckedChange -= swDevice_Click;
                    ((Switch)sender).Checked = !NConvert.Int2Bool(value);
                    ((Switch)sender).CheckedChange += swDevice_Click;
                    ExceptionHandler.toastMsg(((Switch)sender).Context, ((Switch)sender).Context.GetString(Resource.String.ValueChanged));
                }
            }
            catch (Exception ex)
            {
                ((Switch)sender).CheckedChange -= swDevice_Click;
                ((Switch)sender).Checked = NConvert.Int2Bool(value);
                ((Switch)sender).CheckedChange += swDevice_Click;
                if (ex is TaskCanceledException)
                    ExceptionHandler.toastMsg(((Switch)sender).Context, ((Switch)sender).Context.GetString(Resource.String.TimeoutException));
                else if (ex is WebException)
                    ExceptionHandler.toastMsg(((Switch)sender).Context, ((Switch)sender).Context.GetString(Resource.String.WebException));
                else if (ex is HttpRequestException)
                    ExceptionHandler.toastMsg(((Switch)sender).Context, ((Switch)sender).Context.GetString(Resource.String.BMSHttpRequestException));
                else
                    ExceptionHandler.toastMsg(((Switch)sender).Context, ex.Message);
                ExceptionHandler.logDActivity(ex.ToString(), ((SharedEnviroment)((Switch)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Switch)sender).Context.ApplicationContext).TAG);
            }
            cts = null;
        }
        /// <summary>
        /// Handles the Click event of the btnEditableDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void btnEditableDevice_Click(object sender, EventArgs e)
        {
            EditText etDyn = (EditText)((NamaadFormBase)((Button)sender).Context).FindViewById(Resource.Id.etLabledNumberButton);
        }
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                await UpdateValue(((Button)sender).Context);

            }
            catch (Exception ex)
            {
                ExceptionHandler.toastMsg((((Button)sender).Context), ex.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)((Button)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Button)sender).Context.ApplicationContext).TAG);
            }
        }
        #endregion
        #region Controller Communication
        /// <summary>
        /// Sends the and validate.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="portNumber">The port number.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <param name="cts">The CancellationTokenSource CTS.</param>
        /// <returns></returns>
        private static async Task<bool> SendAndValidate(Context owner, int portNumber, bool status, CancellationTokenSource cts)
        {
            int byteNumber = ((portNumber - 1) / 4) + 5;
            int pairNumber = ((portNumber - 1) % 4) + 1;
            byte[] fData = new byte[21];
            fData[0] = BitConverter.GetBytes(1).First();
            fData[1] = BitConverter.GetBytes(6).First();
            fData[2] = BitConverter.GetBytes(7).First();
            fData[3] = BitConverter.GetBytes(1).First();
            fData[4] = BitConverter.GetBytes(1).First();
            for (int i = 5; i < 21; i++)
                fData[i] = BitConverter.GetBytes(21).First();
            if (status)
            {
                switch (pairNumber)
                {
                    case 1: fData[byteNumber] = BitConverter.GetBytes(86).First(); break;
                    case 2: fData[byteNumber] = BitConverter.GetBytes(89).First(); break;
                    case 3: fData[byteNumber] = BitConverter.GetBytes(101).First(); break;
                    case 4: fData[byteNumber] = BitConverter.GetBytes(149).First(); break;
                }
            }
            else
            {
                switch (pairNumber)
                {
                    case 1: fData[byteNumber] = BitConverter.GetBytes(84).First(); break;
                    case 2: fData[byteNumber] = BitConverter.GetBytes(81).First(); break;
                    case 3: fData[byteNumber] = BitConverter.GetBytes(69).First(); break;
                    case 4: fData[byteNumber] = BitConverter.GetBytes(21).First(); break;
                }
            }
            var encoding = Encoding.GetEncoding("iso-8859-1");
            string allOuts = encoding.GetString(fData);
            string s = await SendReq(owner, allOuts, cts);
            string inputs = s.Substring(s.IndexOf("T3 sp") + 23, 4);
            //Toast.MakeText(owner, s, ToastLength.Long);
            return true;
        }
        /// <summary>
        /// Sends the req.
        /// </summary>
        /// <param name="owner">The Owner activity</param>
        /// <param name="req">The req.</param>
        /// <param name="cts">The CancellationTokenSource CTS.</param>
        /// <returns></returns>
        private static async Task<string> SendReq(Context owner, string req, CancellationTokenSource cts)
        {
            HttpClient httpClient = new HttpClient();
            string ipAddress = BMSPref.GetControllerIp(owner);
            UriBuilder builder = new UriBuilder
            {
                Scheme = "http",
                Host = ipAddress,
            };
            builder.Query = string.Format("aip={0}&lcd1=&lcd2={1}", ipAddress, req);
            HttpResponseMessage hrTask = await httpClient.GetAsync(builder.Uri, cts.Token);
            hrTask.EnsureSuccessStatusCode();
            // Retrieve the website contents from the HttpResponseMessage.
            string urlContents = await hrTask.Content.ReadAsStringAsync();
            return urlContents;
        }

        /// <summary>
        /// Updates the value in BMS DB's Device table from controller response.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="port">specific port to check</param>
        /// <param name="ioType">Type of the io.</param>
        /// <returns>True if value was updated, false if Controller values were changed, work when port no. and iotype is specified other wize always return false</returns>
        public async Task<bool> UpdateValue(Context owner, int port = -1, short ioType = -1)
        {
            bool valueIsChanged = false;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(timeOut);
            string s = await SendReq(owner, "", cts);
            string[] status = { s.Substring(s.IndexOf("T3 sp") + 23, 4), s.Substring(s.IndexOf("T3 sp") + 23, 4) };
            char[] inStatus = ToBin(int.Parse(status[0]), 8).ToCharArray();
            char[] outStatus = ToBin(int.Parse(status[1]), 8).ToCharArray();
            using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
            {
                dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                dbHelper.BeginTransaction();
                for (int i = 7, j = 1; i >= 0; i--, j++)
                {
                    DataRow dr = GetDeviceDr(owner, j, 0);
                    if (port == j && ioType == 0 && dr["Value"].ToString() != inStatus[i].ToString())
                    {
                        valueIsChanged = true;
                        dr["Value"] = inStatus[i].ToString();
                        dbHelper.Update("Device", dr);
                    }
                    else if (dr["Value"].ToString() != inStatus[i].ToString())
                    {
                        dr["Value"] = inStatus[i].ToString();
                        dbHelper.Update("Device", dr);
                    }
                }
                dbHelper.EndTransaction();
            }
            return valueIsChanged;
        }
        /// <summary>
        /// To the binary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="len">The length.</param>
        /// <returns></returns>
        public static string ToBin(int value, int len)
        {
            return (len > 1 ? ToBin(value >> 1, len - 1) : null) + "01"[value & 1];
        }
        #endregion
    }
}

//int layCount = 0;
//int count = 0;
//LinearLayout layout = null;
//if ((recCount > 3 && (isTablet(owner) || ((NamaadFormBase)owner).Resources.Configuration.Orientation == Orientation.Landscape) || (recCount > 1 && isTablet(owner) && ((NamaadFormBase)owner).Resources.Configuration.Orientation == Orientation.Landscape)))
//{
//    if (layCount == 0)
//    {
//        layout = new LinearLayout(owner);
//        LinearLayout.LayoutParams lp =
//            new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
//                ViewGroup.LayoutParams.WrapContent)
//            {
//                BottomMargin =
//                    (int)owner.Resources.GetDimension(Resource.Dimension.large_border),
//                TopMargin =
//                    (int)owner.Resources.GetDimension(Resource.Dimension.medium_border)
//            };
//        layout.LayoutParameters = lp;
//    }
//    if (layCount < 1)
//    {
//        swDevice.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1);
//        layout.AddView(swDevice);
//        layCount++;
//        if (recCount == count)
//        {
//            mainLayout.AddView(layout);
//            layCount = 0;
//        }
//    }
//    else
//    {
//        swDevice.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1);
//        layout.AddView(swDevice);
//        mainLayout.AddView(layout);
//        layCount = 0;
//    }
//}
//else
//{
//    LinearLayout.LayoutParams lp =
//        new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
//            ViewGroup.LayoutParams.WrapContent)
//        {
//            BottomMargin =
//                (int)owner.Resources.GetDimension(Resource.Dimension.large_border),
//            TopMargin = (int)owner.Resources.GetDimension(Resource.Dimension.medium_border)
//        };
//    mainLayout.AddView(swDevice, lp);
//}

//private void bmsListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
//{
//    try
//    {
//        /* Find selected activity */
//        BMSViewHolder bmsViewHolder = itemList[e.Position];
//        if (bmsViewHolder.IsSwitch)
//        {

//        }
//        else
//        {

//        }

//    }
//    catch (Exception ex)
//    {
//        ExceptionHandler.toastMsg((((Button)sender).Context), ex.Message);
//        ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)((Button)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Button)sender).Context.ApplicationContext).TAG);
//    }
//}