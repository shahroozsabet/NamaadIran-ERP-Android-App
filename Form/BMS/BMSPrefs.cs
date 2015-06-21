/*
 * Author: Shahrooz Sabet
 * Date: 20150627
 * Updated:20150628
 * */
#region using
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using NamaadMobile.SharedElement;
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
        private const string ControlerIpAddress = "ControlerIpAddress";
        private const string ControlerIpAddressDef = "1.1.1.2";
        #endregion
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            AddPreferencesFromResource(Resource.Xml.bms_setting);
        }
        public static string GetControlerIpAddress(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context).GetString(ControlerIpAddress, ControlerIpAddressDef);
        }
    }
}