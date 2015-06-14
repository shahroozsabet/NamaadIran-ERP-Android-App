/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Scandit;
using System;
using Scandit.Interfaces;

#endregion
namespace NamaadMobile.Util
{
    /// <summary>
    /// TODO: Showing it in a fragment activity feasibility study.
    /// </summary>
    [Activity(Label = "ScanActivity")]
    public class ScanActivity : Activity, Scandit.Interfaces.IScanditSDKListener
    {
        private ScanditSDKAutoAdjustingBarcodePicker picker;
        public static string appKey = "kHMT0MH9EeOar9WwtPjRz35jlsF+mRqFXkUXLECpDk0";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            // Setup the barcode scanner
            picker = new ScanditSDKAutoAdjustingBarcodePicker(this, appKey, ScanditSDK.CameraFacingBack);
            picker.OverlayView.AddListener(this);

            // Show the scan user interface
            SetContentView(picker);
        }

        public void DidScanBarcode(string barcode, string symbology)
        {
            //Console.WriteLine("barcode scanned: {0}, '{1}'", symbology, barcode);

            // stop the camera
            picker.StopScanning();

            //AlertDialog alert = new AlertDialog.Builder(this)
            //	.SetTitle(symbology + " Barcode Detected")
            //		.SetMessage(barcode)
            //		.SetPositiveButton("OK", delegate
            //{
            //	picker.StartScanning();
            //})
            //		.Create();

            //alert.Show();

            /* Create new result intent */
            Intent reply = new Intent();
            reply.PutExtra("barcode", barcode);
            reply.PutExtra("symbology", symbology);
            SetResult(Result.Ok, reply);
            Finish();   // Close this activity
        }

        public void DidCancel()
        {
            Console.WriteLine("Cancel was pressed.");
            Finish();
        }

        public void DidManualSearch(string text)
        {
            Console.WriteLine("Search was used.");
        }

        protected override void OnResume()
        {
            picker.StartScanning();
            base.OnResume();
        }

        protected override void OnPause()
        {
            picker.StopScanning();
            base.OnPause();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
        }
    }
}

