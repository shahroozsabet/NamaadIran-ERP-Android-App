/*
 * Author: Shahrooz Sabet
 * Date: 20150429
 * */
#region using
using Android.App;
using Android.OS;
using Android.Widget;
using NamaadMobile.Data;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
#endregion
namespace NamaadMobile
{
    [Activity(Label = "BMS1150")]
    public class BMS1150 : NamaadFormBase
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        // UI references.
        private LinearLayout bms1150MainLayout;
        private ListView bms1150ListView;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Prefs.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.BMS1150";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            SetContentView(Resource.Layout.bms1150);
            bms1150MainLayout = (LinearLayout)FindViewById(Resource.Id.bms1150MainLayout);
            bms1150ListView = (ListView)bms1150MainLayout.FindViewById(Resource.Id.bms1150ListView);
            BMSPublic bmsPublic = new BMSPublic();
            bmsPublic.AddEditableSensorToLayout(this, bms1150ListView);
            bmsPublic.AddSwitchDeviceToLayout(this, bms1150ListView);
            bmsPublic.AddResetToLayout(this, bms1150MainLayout);
        }
        #endregion


    }
}