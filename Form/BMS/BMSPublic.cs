/*
 * Author: Shahrooz Sabet
 * Date: 20150503
 * */
#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NamaadMobile.Util;
using NamaadMobile.SharedElement;
using Mono.Data.Sqlite;
using NamaadMobile.Data;
using System.Net;
using System.Collections.Specialized;
using System.Data;
using FlatUI;
using NamaadMobile.Function;
#endregion
namespace NamaadMobile
{
    class BMSPublic
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
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
                    object obj = dbHelper.EXECUTESCALAR("SELECT Count(ID) FROM Device Where CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode);
                    int recCount = 0;
                    if (obj != DBNull.Value) recCount = int.Parse(obj.ToString());
                    using (SqliteDataReader reader = dbHelper.ExecuteReader("Select * From Device Where CategoryID=" + ((SharedEnviroment)owner.ApplicationContext).ActionCode))
                    {
                        int layCount = 0;
                        int count = 0;
                        Switch swDyn;
                        LinearLayout layout = null;
                        while (reader.Read())
                        {
                            count++;
                            swDyn = new Switch(owner);
                            swDyn.Text = reader["Name"].ToString();
                            swDyn.Id = (int)reader["ID"];
                            swDyn.Enabled = NamaadMobile.Function.NConvert.Int2Bool(GetAccessID(owner)) || HasAccess(owner, swDyn.Id);
                            swDyn.Checked = (reader["Value"] is DBNull || (int)reader["Value"] == 0) ? false : true;
                            if (swDyn.Enabled) swDyn.CheckedChange += BMSPublic.swDyn_Click;
                            if (recCount > 3)
                            {
                                if (layCount == 0)
                                {
                                    layout = new LinearLayout(owner);
                                    LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                                    lp.BottomMargin = (int)owner.Resources.GetDimension(Resource.Dimension.small_border);
                                    lp.TopMargin = (int)owner.Resources.GetDimension(Resource.Dimension.small_border);
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
                    Button btnReset = new Button(owner);
                    btnReset.Id = -1;
                    btnReset.Text = owner.GetString(Resource.String.Reset);
                    btnReset.Click += BMSPublic.btnReset_Click;
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
        public static DataRow GetDeviceDr(Context owner, int deviceID)
        {
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
                    return dbHelper.ExecuteSQL("SELECT * FROM Device Where ID=" + deviceID).Rows[0];
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
        public static int GetAccessID(Context owner, int userCode = -1)
        {
            int accessIDInt = -1;
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
                    if (userCode == -1) userCode = ((SharedEnviroment)owner.ApplicationContext).UserCode;
                    object accessID = dbHelper.EXECUTESCALAR("SELECT AccessID FROM User Where ID=" + userCode);
                    if (accessID != DBNull.Value) accessIDInt = int.Parse(accessID.ToString());
                    return accessIDInt;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return accessIDInt;
        }
        /// <summary>
        /// Gets the device identifier by access identifier.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="accessID">The access identifier.</param>
        /// <returns>deviceID by its accessID code, if it doesnt exist the method returns -1</returns>
        public static int GetDeviceIDByAccessID(Context owner, int accessID)
        {
            int deviceIDInt = -1;
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
                    object deviceID = dbHelper.EXECUTESCALAR("SELECT DeviceID FROM Access_Device Where AccessID=" + accessID);
                    if (deviceID != DBNull.Value) deviceIDInt = int.Parse(deviceID.ToString());
                    return deviceIDInt;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return deviceIDInt;
        }
        /// <summary>
        /// Gets the access identifier by device identifier.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns>Device AccessID if it exists, otherwise it return -1</returns>
        public static int GetAccessIDByDeviceID(Context owner, int deviceID)
        {
            int accessIDInt = -1;
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).DbNameClient);
                    object accessID = dbHelper.EXECUTESCALAR("SELECT AccessID FROM Access_Device Where DeviceID=" + deviceID);
                    if (accessID != DBNull.Value) accessIDInt = int.Parse(accessID.ToString());
                    return accessIDInt;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return accessIDInt;
        }
        /// <summary>
        /// Determines whether the specified owner has access.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="deviceID">The device identifier.</param>
        /// <param name="userCode">The user code if not specified it gets from owner Context</param>
        /// <returns>A Boolean indicates whether userCode has deviceID permision or not </returns>
        public static bool HasAccess(Context owner, int deviceID, int userCode = -1)
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
                               + "Where DeviceID=" + deviceID + " \n"
                               + ") As t \n"
                               + "On u.AccessID=t.AccessID \n"
                               + "Where t.DeviceID=" + deviceID + " And u.ID=" + userCode;
                    if (dbHelper.EXECUTESCALAR(sql) != null) return true;
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
                    dbHelper.ExecuteNonQuery("Update Device Set Value=null");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.toastMsg((((Button)sender).Context), ex.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)((Button)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Button)sender).Context.ApplicationContext).TAG);
            }
        }
        private static void swDyn_Click(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            DataRow drDevice = GetDeviceDr(((Switch)sender).Context, ((Switch)sender).Id);
            int port = (int)drDevice["Port"];
            int value = 0;
            if (drDevice["Value"] != DBNull.Value) value = (int)drDevice["Value"];
            try
            {
                if (value == 0)
                {
                    bool isValidate = SendAndValidate(((Switch)sender).Context, port, true);
                    drDevice["Value"] = 1;
                }
                else
                {
                    bool isValidate = SendAndValidate(((Switch)sender).Context, port, false);
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
                //((Switch)sender).Checked = !((Switch)sender).Checked;
                ExceptionHandler.toastMsg(((Switch)sender).Context, ex.Message);
                ExceptionHandler.logDActivity(ex.ToString(), ((SharedEnviroment)((Switch)sender).Context.ApplicationContext).Logging, ((SharedEnviroment)((Switch)sender).Context.ApplicationContext).TAG);
            }
        }
        #endregion
        #region Controller Communication
        private static bool SendAndValidate(Context owner, int portNumber, bool status)
        {
            int byteNumber = ((portNumber - 1) / 4) + 5;
            int pairNumber = ((portNumber - 1) % 4) + 1;
            byte[] FData = new byte[21];
            FData[0] = BitConverter.GetBytes(49).First();
            FData[1] = BitConverter.GetBytes(54).First();
            FData[2] = BitConverter.GetBytes(49).First();
            FData[3] = BitConverter.GetBytes(49).First();
            FData[4] = BitConverter.GetBytes(55).First();
            for (int i = 5; i < 21; i++)
                FData[i] = BitConverter.GetBytes(21 + 48).First();
            if (status)
                switch (pairNumber)
                {
                    case 1: FData[byteNumber] = BitConverter.GetBytes(22 + 48).First(); break;
                    case 2: FData[byteNumber] = BitConverter.GetBytes(25 + 48).First(); break;
                    case 3: FData[byteNumber] = BitConverter.GetBytes(37 + 48).First(); break;
                    case 4: FData[byteNumber] = BitConverter.GetBytes(21 + 48).First(); break;
                }
            else
                switch (pairNumber)
                {
                    case 1: FData[byteNumber] = BitConverter.GetBytes(20 + 48).First(); break;
                    case 2: FData[byteNumber] = BitConverter.GetBytes(17 + 48).First(); break;
                    case 3: FData[byteNumber] = BitConverter.GetBytes(5 + 48).First(); break;
                    case 4: FData[byteNumber] = BitConverter.GetBytes(21 + 48).First(); break;
                }
            var encoding = Encoding.GetEncoding("iso-8859-1");
            string AllOuts = encoding.GetString(FData);
            string s = SendReq(AllOuts);
            //Toast.MakeText(owner, s, ToastLength.Long);
            return true;
        }
        private static string SendReq(string req)
        {
            WebClient client = new WebClient();
            NameValueCollection QueryStrings = new NameValueCollection();
            QueryStrings.Add("aip", "1.1.1.2");
            QueryStrings.Add("lcd1", "");
            QueryStrings.Add("lcd2", req);
            client.QueryString.Add(QueryStrings);
            client.UseDefaultCredentials = true;
            string s = client.DownloadString("http://1.1.1.2/");
            //HtmlString htmlString = new HtmlString(s);
            return s;
        }
        #endregion
    }
}