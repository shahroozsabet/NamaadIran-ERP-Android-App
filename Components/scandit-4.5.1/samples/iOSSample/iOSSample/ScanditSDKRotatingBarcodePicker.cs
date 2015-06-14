using System;
using System.Drawing;
using Foundation;
using UIKit;
using ScanditSDK;

namespace iOSSample
{
	public class ScanditSDKRotatingBarcodePicker : SIBarcodePicker
	{
		public ScanditSDKRotatingBarcodePicker (string appKey) : base(appKey)
		{

		}
		

		public override bool ShouldAutorotate () {
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations () {
			return UIInterfaceOrientationMask.All;
		}
	}
}

