using System;

using UIKit;
using System.Net;
using CoreGraphics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;
using AVFoundation;
using Foundation;
using CoreMedia;
using ExternalAccessory;
using System.Linq;
using ObjCRuntime;

namespace IOSService
{
	public partial class ViewController : UIViewController
	{
		AVAudioPlayer _player;
		NSUrl _asset;
		UIPrinterPickerController _controller;
		NSUrl _printerURL;
		HttpListener _listener;



		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			InitPlayer ();
			GetIP ();
			InitListener ();
			//SetupDefaultWiFiPrinter ();

			NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds(0.1), (obj) => SetupDefaultWiFiPrinter()); //HACK

//			var button = new UIButton (new CGRect(50,50,250,300));
//			button.SetTitle ("Stop", UIControlState.Normal);
//			button.BackgroundColor = UIColor.Gray;
//			button.TouchUpInside += ButtonTouchUpInside;
//			this.View.AddSubview (button);


		}

//		void ButtonTouchUpInside (object sender, EventArgs e)
//		{
//			SetupDefaultWiFiPrinter ();
//		}
		void InitPlayer()
		{
			_asset = (NSUrl.FromFilename ("30.mp3"));
			NSError err;
			_player = new AVAudioPlayer (_asset, "mp3", out err); 
			_player.NumberOfLoops = -1;
			_player.Volume = 0;
			_player.Play ();
			_player.EndInterruption += PlayerEndInterruption; 
			_player.BeginInterruption += PlayerBeginInterruption;
		}

		void GetIP()
		{
			var myIpLabel = new UILabel (new CGRect(50,50,250,200)){Lines = 0};
			foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces()) {
				if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
					netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) {
					foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses) {
						if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork) {
							var ipAddress = addrInfo.Address;
							myIpLabel.Text += ipAddress.ToString() + Environment.NewLine;

						}
					}
				}  
			}
			this.View.AddSubview (myIpLabel);

		}

		void PlayerBeginInterruption (object sender, EventArgs e)
		{
			_player.Pause ();
			_listener.Stop ();
			_listener.Close ();
			_listener = null;
		}

		void PlayerEndInterruption (object sender, EventArgs e)
		{
			_player.Play ();
			InitListener ();
		}

		private void InitListener()
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add("http://*:8080/");
			_listener.Start();

			Console.WriteLine("Listening...");
			Task.Factory.StartNew (() => {
				for (;;) {
					HttpListenerContext ctx = _listener.GetContext ();
					new Thread (new Worker (ctx, _printerURL).ProcessRequest).Start ();
				}
			});
		}
		private void SetupDefaultWiFiPrinter()
		{
			_controller = new UIPrinterPickerControllerWrapper ();
			if (_controller != null) {
				_controller.Delegate = new UIPrinterPickerControllerDelegateWrapper (this);

				var defaultPrinter = _controller.SelectedPrinter;
				if (defaultPrinter == null) {
					if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {

						_controller.PresentFromRect (new CGRect(200,200,200,200), this.View, false, UIPrintInteractionCompletionHan);

					} else {
						_controller.Present (true, UIPrintInteractionCompletionHan);
					}
				}
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
		void UIPrintInteractionCompletionHan (UIPrinterPickerController printInteractionController,Boolean completed,NSError error)
		{
			if(completed && _controller != null && _controller.SelectedPrinter !=null)
			_printerURL = _controller.SelectedPrinter.Url;
		}


	}
	class Worker
	{
		const string _makeUppercase = "/Uppercase";
		const string _printWifi = "/PrintWiFi";
		const string _printBT = "/PrintBT";

		private HttpListenerContext context;
		NSUrl printerUrl;
		public Worker(HttpListenerContext context, NSUrl printerUrl)
		{
			this.context = context;
			this.printerUrl = printerUrl;
		}

		public void ProcessRequest()
		{
			if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == _makeUppercase) {
				string text;
				using (var reader = new StreamReader(context.Request.InputStream,
					context.Request.ContentEncoding))
				{
					
					text = reader.EndOfStream ? "NULL" : reader.ReadToEnd();
				}

				Console.WriteLine(text);
				Uppercase (text);
			}
			if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == _printWifi) {
				string text;
				using (var reader = new StreamReader(context.Request.InputStream,
					context.Request.ContentEncoding))
				{
					text = reader.EndOfStream ? "NULL" : reader.ReadToEnd();
				}
				Console.WriteLine(text);
				using (var pool = new NSAutoreleasePool ()) {
					try{
						pool.InvokeOnMainThread(delegate {
							PrintWiFi(text);
						});
					}
					catch {

					}
				}

			}
			if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == _printBT) {
				string text;
				using (var reader = new StreamReader(context.Request.InputStream,
					context.Request.ContentEncoding))
				{
					text = reader.EndOfStream ? "NULL" : reader.ReadToEnd();
				}
				Console.WriteLine(text);
				using (var pool = new NSAutoreleasePool ()) {
					try{
						pool.InvokeOnMainThread(delegate {
							PrintBT(text);
						});
					}
					catch {

					}
				}

			}
			if (context.Request.Url.AbsolutePath == "/IsAlive") {
				SendResponce ("Yes, I`m Alive");
			}
		}

		private void PrintBT(string text)
		{
			var manager = EAAccessoryManager.SharedAccessoryManager;
			var starPrinter = manager.ConnectedAccessories.FirstOrDefault (p => p.Name.IndexOf ("Star") >= 0); // this does find the EAAccessory correctly

			if (starPrinter == null) {
				SendResponce ("Cant Find Star Printer");
				return;
			}

			var session = new EASession (starPrinter, starPrinter.ProtocolStrings [0]); // the second parameter resolves to "jp.star-m.starpro"
			session.OutputStream.Open (); //WORKAROUND HACK:https://forums.xamarin.com/discussion/50712/ios9-easession
			session.OutputStream.Schedule (NSRunLoop.Main,/*Current*/ NSRunLoop.NSDefaultRunLoopMode); 
			session.OutputStream.Open (); 


			byte[] toSend = Encoding.UTF8.GetBytes(text); // short text

			if (session.OutputStream.HasSpaceAvailable()) {
				nint bytesWritten = session.OutputStream.Write (toSend, (nuint)toSend.Length);  
				if (bytesWritten < 0) { 
					System.Diagnostics.Debug.WriteLine ("ERROR WRITING DATA"); 
				} else {
					System.Diagnostics.Debug.WriteLine("Some data written, ignoring the rest, just a test");
				} 
			} else
			{
				NSRunLoop.Main.RunUntil(NSDate.FromTimeIntervalSinceNow(0.5));
				System.Diagnostics.Debug.WriteLine ("NO SPACE");
			}
			session.OutputStream.Close ();
			session.OutputStream.Dispose ();
			session.InputStream.Close ();
			session.InputStream.Dispose ();
			session.Dispose ();
		}

		private void Uppercase(string text)
		{
			SendResponce (text.ToUpper());
		}

		private void PrintWiFi(string text)
		{
			var printInfo = UIPrintInfo.PrintInfo;
			printInfo.OutputType = UIPrintInfoOutputType.General;
			printInfo.JobName = "My first Print Job";

			var textFormatter = new UISimpleTextPrintFormatter (text) {
				StartPage = 0,
				ContentInsets = new UIEdgeInsets (72, 72, 72, 72),
				MaximumContentWidth = 6 * 72,
			};

			var printer = UIPrintInteractionController.SharedPrintController;
			printer.PrintInfo = printInfo;
			printer.PrintFormatter = textFormatter;
			if (printerUrl != null) {
				var defaultPrinter = UIPrinter.FromUrl (printerUrl);
				if (defaultPrinter != null) {
					printer.PrintToPrinter (defaultPrinter, UIPrintInteractionCompletionHandler);

				}
			} else {
				SendResponce ("No printer");
			}
		}

		void UIPrintInteractionCompletionHandler (UIPrintInteractionController printInteractionController,Boolean completed,NSError error)
		{
			if(completed)
				SendResponce ("Printed");
			else
				SendResponce ("PrintError");

		}


		private void SendResponce(string responce)
		{
			byte[] b = Encoding.UTF8.GetBytes(responce);
			context.Response.ContentLength64 = b.Length;
			context.Response.OutputStream.Write(b, 0, b.Length);
			context.Response.OutputStream.Close();

		}
	}

	public class UIPrinterPickerControllerWrapper : UIPrinterPickerController
	{
		public UIPrinterPickerControllerWrapper () : base(NSObjectFlag.Empty)
		{
			
		}
	}

	public class UIPrinterPickerControllerDelegateWrapper : UIPrinterPickerControllerDelegate
	{
		UIViewController controller;
		public UIPrinterPickerControllerDelegateWrapper (UIViewController controller) : base()
		{
			this.controller = controller;
		}
		public override UIViewController GetParentViewController (UIPrinterPickerController printerPickerController)
		{
			return controller;
		}
	}

	public class ContentController : UIViewController
	{
		public ContentController (NSObject obj) : base()
		{
			
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
		}

		public ContentController (IntPtr handle) : base (handle)
		{
		}
	}
}
