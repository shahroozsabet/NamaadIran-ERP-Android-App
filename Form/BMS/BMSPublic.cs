/*
 * Author: Shahrooz Sabet
 * Date: 20150503
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
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;
using NamaadMobile.Data;
using NamaadMobile.Function;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
#endregion
namespace NamaadMobile
{
    class BMSPublic
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        private const int Tout = 5000;
        private static string ControlerIpAddress = "1.1.1.2";
        private static UriBuilder builder = new UriBuilder
        {
            Scheme = "http",
            Host = ControlerIpAddress
        };
        #endregion
        #region Public Function
        /// <summary>
        /// Adds switch to layout dynamically.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="mainLayout">The main layout.</param>
        public static void AddSwitchToLayout(Context owner, LinearLayout mainLayout)
        {
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
                    object obj = dbHelper.ExecuteScalar("SELECT Count(ID) FROM Device Where CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode);
                    int recCount = 0;
                    if (obj != DBNull.Value) recCount = int.Parse(obj.ToString());
                    using (SqliteDataReader reader = dbHelper.ExecuteReader("Select * From Device Where CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode))
                    {
                        int layCount = 0;
                        int count = 0;
                        LinearLayout layout = null;
                        while (reader.Read())
                        {
                            count++;
                            Switch swDyn = new Switch(owner)
                            {
                                Text = reader["Name"].ToString(),
                                Id = (int)reader["ID"]
                            };
                            swDyn.Enabled = NConvert.Int2Bool(GetAccessId(owner)) || HasAccess(owner, swDyn.Id);
                            swDyn.Checked = (!(reader["Value"] is DBNull) && (int)reader["Value"] != 0);
                            if (swDyn.Enabled) swDyn.CheckedChange += swDyn_Click;
                            if (recCount > 3)
                            {
                                if (layCount == 0)
                                {
                                    layout = new LinearLayout(owner);
                                    LinearLayout.LayoutParams lp =
                                        new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                            ViewGroup.LayoutParams.WrapContent)
                                        {
                                            BottomMargin =
                                                (int)owner.Resources.GetDimension(Resource.Dimension.small_border),
                                            TopMargin =
                                                (int)owner.Resources.GetDimension(Resource.Dimension.small_border)
                                        };
                                    layout.LayoutParameters = lp;

                                }
                                if (layCount < 1)
                                {
                                    swDyn.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1);
                                    layout.AddView(swDyn);
                                    layCount++;
                                    if (recCount == count)
                                    {
                                        mainLayout.AddView(layout);
                                        layCount = 0;
                                    }
                                }
                                else
                                {
                                    swDyn.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1);
                                    layout.AddView(swDyn);
                                    mainLayout.AddView(layout);
                                    layCount = 0;
                                }
                            }
                            else
                            {
                                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                                lp.BottomMargin = (int)owner.Resources.GetDimension(Resource.Dimension.small_border);
                                lp.TopMargin = (int)owner.Resources.GetDimension(Resource.Dimension.medium_border);
                                mainLayout.AddView(swDyn, lp);
                            }
                        }
                    }
                    Button btnReset = new Button(owner)
                    {
                        Id = -1,
                        Text = owner.GetString(Resource.String.Reset)
                    };
                    btnReset.Click += btnReset_Click;
                    btnReset.LayoutParameters = new LinearLayout.LayoutParams((int)owner.Resources.GetDimension(Android.Resource.Dimension.ThumbnailWidth), ViewGroup.LayoutParams.WrapContent);
                    mainLayout.AddView(btnReset);
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
        }
        public static DataRow GetDeviceDr(Context owner, int deviceId)
        {
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
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
        public static int GetAccessId(Context owner, int userCode = -1)
        {
            int accessIdInt = -1;
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
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
        public static int GetDeviceIdByAccessId(Context owner, int accessId)
        {
            int deviceIdInt = -1;
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
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
        public static int GetAccessIdByDeviceId(Context owner, int deviceId)
        {
            int accessIdInt = -1;
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
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
        public static bool HasAccess(Context owner, int deviceId, int userCode = -1)
        {
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
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
        #endregion
        #region Private Function
        private static void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(((Button)sender).Context))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)(((Button)sender).Context).ApplicationContext).DbNameClient);
                    dbHelper.ExecuteNonQuery("Update Device Set Value=null Where CategoryID=" + ((SharedEnviroment)(((Button)sender).Context).ApplicationContext).ActionCode);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.toastMsg((((Button)sender).Context), ex.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)((Button)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Button)sender).Context.ApplicationContext).TAG);
            }
        }
        private static async void swDyn_Click(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            DataRow drDevice = GetDeviceDr(((Switch)sender).Context, ((Switch)sender).Id);
            int port = (int)drDevice["Port"];
            int value = 0;
            if (drDevice["Value"] != DBNull.Value) value = (int)drDevice["Value"];
            // ***Instantiate the CancellationTokenSource.
            // ***Declare a System.Threading.CancellationTokenSource.
            CancellationTokenSource cts = new CancellationTokenSource();
            // ***Set up the CancellationTokenSource to cancel after 5 seconds.
            cts.CancelAfter(Tout);
            try
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
                using (dbHelper = new NmdMobileDBAdapter(((Switch)sender).Context))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)((Switch)sender).Context.ApplicationContext).DbNameClient);
                    dbHelper.Update("Device", drDevice);
                }
            }
            catch (Exception ex)
            {
                ((Switch)sender).CheckedChange -= swDyn_Click;
                ((Switch)sender).Checked = NConvert.Int2Bool(value);
                ((Switch)sender).CheckedChange += swDyn_Click;
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
        #endregion
        #region Controller Communication
        private static async Task<bool> SendAndValidate(Context owner, int portNumber, bool status, CancellationTokenSource cts)
        {
            int byteNumber = ((portNumber - 1) / 4) + 5;
            int pairNumber = ((portNumber - 1) % 4) + 1;
            byte[] fData = new byte[21];
            fData[0] = BitConverter.GetBytes(49).First();
            fData[1] = BitConverter.GetBytes(54).First();
            fData[2] = BitConverter.GetBytes(49).First();
            fData[3] = BitConverter.GetBytes(49).First();
            fData[4] = BitConverter.GetBytes(55).First();
            for (int i = 5; i < 21; i++)
                fData[i] = BitConverter.GetBytes(21 + 48).First();
            if (status)
                switch (pairNumber)
                {
                    case 1: fData[byteNumber] = BitConverter.GetBytes(22 + 48).First(); break;
                    case 2: fData[byteNumber] = BitConverter.GetBytes(25 + 48).First(); break;
                    case 3: fData[byteNumber] = BitConverter.GetBytes(37 + 48).First(); break;
                    case 4: fData[byteNumber] = BitConverter.GetBytes(21 + 48).First(); break;
                }
            else
                switch (pairNumber)
                {
                    case 1: fData[byteNumber] = BitConverter.GetBytes(20 + 48).First(); break;
                    case 2: fData[byteNumber] = BitConverter.GetBytes(17 + 48).First(); break;
                    case 3: fData[byteNumber] = BitConverter.GetBytes(5 + 48).First(); break;
                    case 4: fData[byteNumber] = BitConverter.GetBytes(21 + 48).First(); break;
                }
            var encoding = Encoding.GetEncoding("iso-8859-1");
            string allOuts = encoding.GetString(fData);
            string s = await SendReq(allOuts, cts);
            //Toast.MakeText(owner, s, ToastLength.Long);
            return true;
        }
        private static async Task<string> SendReq(string req, CancellationTokenSource cts)
        {
            HttpClient httpClient = new HttpClient();
            builder.Query = string.Format("aip={0}&lcd1=&lcd2={1}", ControlerIpAddress, req);
            HttpResponseMessage hrTask = await httpClient.GetAsync(builder.Uri, cts.Token);
            hrTask.EnsureSuccessStatusCode();
            // Retrieve the website contents from the HttpResponseMessage.
            string urlContents = await hrTask.Content.ReadAsStringAsync();
            return urlContents;
        }
        #endregion
    }
}