/*
 * Author: Shahrooz Sabet
 * Date: 20150627
 * Updated:20150809
 * */
#region using
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using NamaadMobile.Data;
using NamaadMobile.SharedElement;
using System;
using NamaadMobile.Util;
#endregion
namespace NamaadMobile
{
    /// <summary>
    /// TODO: A GUI can be created.
    /// TODO: Additional methods can be reduced.
    /// </summary>
    [Activity(Label = "BMSPrefs")]
    public class BMSPrefs : NamaadPrefBase
    {
        #region Defins
        // Option names and default values
        private const string ControllerIpAddress = "ControlerIpAddress";
        private static string ControllerIpAddressDef;
        #endregion
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            ControllerIpAddressDef = SetControllerIpAddressDef(this);
            AddPreferencesFromResource(Resource.Xml.bms_setting);
        }

        private static string SetControllerIpAddressDef(Context owner)
        {
            try
            {
                using (NmdMobileDBAdapter dbHelper = new NmdMobileDBAdapter(owner))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)owner.ApplicationContext).ActionArgument);
                    ControllerIpAddressDef =
                        dbHelper.ExecuteScalar("SELECT IP FROM ControllerBoard Where ID=3").ToString();
                    return ControllerIpAddressDef;
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(owner, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)owner.ApplicationContext).Logging, ((SharedEnviroment)owner.ApplicationContext).TAG);
            }
            return null;
        }
        public static string GetControllerIp(Context context)
        {
            SetControllerIpAddressDef(context);
            return PreferenceManager.GetDefaultSharedPreferences(context).GetString(ControllerIpAddress, ControllerIpAddressDef);
        }
    }
}