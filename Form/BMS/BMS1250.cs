/*
 * Author: Shahrooz Sabet
 * Date: 20150429
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
using NamaadMobile.Data;
#endregion
namespace NamaadMobile
{
    [Activity(Label = "BMS1250")]
    public class BMS1250 : NamaadMobile.SharedElement.NamaadFormBase
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        // UI references.
        private LinearLayout bms1250MainLayout;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Prefs.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.BMS1250";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            SetContentView(Resource.Layout.bms1250);
            bms1250MainLayout = (LinearLayout)FindViewById(Resource.Id.bms1250MainLayout);
            BMSPublic.AddSwitchToLayout(this, bms1250MainLayout);
        }
        #endregion
    }
}