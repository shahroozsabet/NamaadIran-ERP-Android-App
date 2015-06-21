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
    [Activity(Label = "BMS1050")]
    public class BMS1050 : NamaadFormBase
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        // UI references.
        private LinearLayout bms1050MainLayout;
        private ListView bms1050ListView;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Prefs.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.BMS1050";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            SetContentView(Resource.Layout.bms1050);
            bms1050MainLayout = (LinearLayout)FindViewById(Resource.Id.bms1050MainLayout);
            bms1050ListView = (ListView)bms1050MainLayout.FindViewById(Resource.Id.bms1050ListView);
            BMSPublic bmsPublic = new BMSPublic();
            bmsPublic.AddEditableSensorToLayout(this, bms1050ListView);
            bmsPublic.AddSwitchDeviceToLayout(this, bms1050ListView);
            bmsPublic.AddResetToLayout(this, bms1050MainLayout);
        }
        #endregion
    }
}