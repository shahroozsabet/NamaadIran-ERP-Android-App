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
    [Activity(Label = "BMS1100")]
    public class BMS1100 : NamaadFormBase
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        // UI references.
        private LinearLayout bms1100MainLayout;
        private ListView bms1100ListView;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Prefs.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.BMS1100";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            SetContentView(Resource.Layout.bms1100);
            bms1100MainLayout = (LinearLayout)FindViewById(Resource.Id.bms1100MainLayout);
            bms1100ListView = (ListView)bms1100MainLayout.FindViewById(Resource.Id.bms1100ListView);
            BMSPublic bmsPublic = new BMSPublic();
            bmsPublic.AddEditableSensorToLayout(this, bms1100ListView);
            bmsPublic.AddSwitchDeviceToLayout(this, bms1100ListView);
            bmsPublic.AddResetToLayout(this, bms1100MainLayout);
        }
        #endregion


    }
}