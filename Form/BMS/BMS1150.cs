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
            BMSPublic.AddEditableSensorToLayout(this, bms1150MainLayout);
            BMSPublic.AddSwitchDeviceToLayout(this, bms1150MainLayout);
            BMSPublic.AddResetToLayout(this, bms1150MainLayout);
        }
        #endregion


    }
}