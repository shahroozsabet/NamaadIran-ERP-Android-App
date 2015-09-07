/*
 * Author: Shahrooz Sabet
 * Date: 20150429
 * Updated:20150907
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
        private Button _btnReset;
        private Button _btnRef;
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
            _btnReset = (Button)FindViewById(Resource.Id.btnReset);
            _btnRef = (Button)FindViewById(Resource.Id.btnRef);
            BMSPublic bmsPublic = new BMSPublic();
            bmsPublic.AddEditableSensorToLayout(this, _bmsListView);
            bmsPublic.AddSwitchDeviceToLayout(this, _bmsListView);
            _btnReset.Click += bmsPublic.btnReset_Click;
            _btnRef.Click += bmsPublic.btnRefresh_Click;
        }
        #endregion
    }
}