/*
 * Author: Shahrooz Sabet
 * Date: 20150429
 * Updated:20150905
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
    [Activity(Label = "BMSGenericActivity")]
    public class BMSGenericActivity : NamaadFormBase
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        // UI references.
        private LinearLayout _bmsMainLayout;
        private ListView _bmsListView;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Pref.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.BMSGenericActivity";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            SetContentView(Resource.Layout.bms_generic_activity);
            _bmsMainLayout = (LinearLayout)FindViewById(Resource.Id.bmsMainLayout);
            _bmsListView = (ListView)_bmsMainLayout.FindViewById(Resource.Id.bmsListView);
            BMSPublic bmsPublic = new BMSPublic();
            bmsPublic.AddEditableSensorToLayout(this, _bmsListView);
            bmsPublic.AddSwitchDeviceToLayout(this, _bmsListView);
            bmsPublic.AddRefreshToLayout(this, _bmsMainLayout);
            bmsPublic.AddResetToLayout(this, _bmsMainLayout);
        }
        #endregion
    }
}