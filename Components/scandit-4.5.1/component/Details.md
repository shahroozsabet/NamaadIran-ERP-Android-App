Scandit Barcode Scanner
=======================

**Enterprise-Grade Barcode Scanning for your Mobile App**

Scandit's lightning-fast and accurate Barcode Scanner is a valuable addition to any enterprise application. Our Xamarin component utilizes our unique blurry barcode scan technology that works across platforms to scan any barcode type from any angle.  

Pricing
-------


### Enterprise Edition(s)

Starting at $199/month
Includes:

* Support for UPC-A, EAN13, EAN8, UPC-E, Code39, Code93, Code128, Codabar, MSI Plessey, ITF (2 of 5), GS1 Databar, QR, Datamatrix, PDF417 (incl. Truncated PDF417) and Aztec codes.
* Variable device limits with unlimited scans
* Premium support with response time of 48hr or less

**Register for a free Test SDK and get your App Key on our [pricing page](http://www.scandit.com/pricing/).**


Features and Benefits
---------------------

#### Lightning-Fast Scanning from any Angle

Our scan technology can capture barcodes before the camera focuses, from any angle, creating an amazingly fast and pleasant user experience.

#### Supports all major 1D and 2D code types

Our barcode scanner module supports UPC-A, EAN13, EAN8, UPC-E, Code39, Code93, Code128, Codabar, MSI Plessey, ITF (2 of 5), GS1 Databar, QR, Datamatrix, PDF417 (incl. Truncated PDF417) and Aztec codes.

#### Support for devices lacking autofocus

Our barcode scanner module works for a wide range of low-end devices that lack autofocus capabilities, and thereby significantly expands the addressable market for your app.

Example
-------

Integrating the Scandit SDK into your Xamarin app is very straightforward:

iOS:

```csharp
// Setup the barcode scanner
var picker = new ScanditSDK.SIBarcodePicker ("---- ENTER YOUR APP KEY HERE ----");
picker.OverlayController.Delegate = new YourDelegate ();

// Display the Scandit user interface
some_ui_view_controller_object.PresentViewController (picker, true, null);

picker.StartScanning ();
```

```csharp
using ScanditSDK;

public class YourDelegate : SIOverlayControllerDelegate
{
	public override void DidScanBarcode (SIOverlayController overlayController, NSDictionary barcode) {
		// perform actions after a barcode was scanned
		);
	}
}
```


Android:

```csharp

public class ScanActivity : Activity, Scandit.Interfaces.IScanditSDKListener
	{
		private ScanditSDKBarcodePicker picker;
		public static string appKey = "---- ENTER YOUR APP KEY HERE ----";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Setup the barcode scanner
			picker = new ScanditSDKBarcodePicker (this, appKey);
			picker.OverlayView.AddListener (this);

			// Start scanning
			picker.StartScanning ();

			// Show scan user interface
			SetContentView (picker);
		}

		public void DidScanBarcode (string barcode, string symbology) {
			Console.WriteLine ("barcode scanned: {0}, '{1}'", symbology, barcode);

		public void DidCancel () {
			Console.WriteLine ("Cancel was pressed.");
		}

		public void DidManualSearch (string text) {
			Console.WriteLine ("Search was used.");
		}
}
```

More information
----------------

Try our demo apps for Android and iOS: 

http://www.scandit.com/resources/demo-apps/

Access our documentation and getting started guides: 

http:/docs.scandit.com



Support
-------

Questions? Contact us at `info@scandit.com`.



