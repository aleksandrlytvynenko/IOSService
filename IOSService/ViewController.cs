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

namespace IOSService
{
	public partial class ViewController : UIViewController
	{
		AVPlayer _player;
		AVPlayerLayer _playerLayer;
		AVAsset _asset;
		AVPlayerItem _playerItem;
		NSObject videoEndNotificationToken;
		bool printerSelected;
		UIPrinterPickerController controller;
		NSUrl printerURL;

		HttpListener listener;
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_asset = AVAsset.FromUrl (NSUrl.FromFilename ("test.mp3"));
			_playerItem = new AVPlayerItem (_asset);   

			_player = new AVPlayer (_playerItem); 
			_player.Muted = true;

			_playerLayer = AVPlayerLayer.FromPlayer (_player);
			_playerLayer.Frame = View.Frame;

			View.Layer.AddSublayer (_playerLayer);

			_player.Play ();
			_player.ActionAtItemEnd = AVPlayerActionAtItemEnd.None;
			videoEndNotificationToken = NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, AudioDidFinishPlaying, _playerItem);
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
			listener = new HttpListener();
			listener.Prefixes.Add("http://*:8080/");
			listener.Start();

			Console.WriteLine("Listening...");
			Task.Factory.StartNew (() => {
				for (;;) {
					HttpListenerContext ctx = listener.GetContext ();
					new Thread (new Worker (ctx, printerURL).ProcessRequest).Start ();
				}
			});
			SetupDefaultWiFiPrinter (null);

		}
		private void SetupDefaultWiFiPrinter(string text)
		{
//			var printInfo = UIPrintInfo.PrintInfo;
//			printInfo.OutputType = UIPrintInfoOutputType.General;
//			printInfo.JobName = "My first Print Job";
//
//			var textFormatter = new UISimpleTextPrintFormatter (text) {
//				StartPage = 0,
//				ContentInsets = new UIEdgeInsets (72, 72, 72, 72),
//				MaximumContentWidth = 6 * 72,
//			};

//			var printer = UIPrintInteractionController.SharedPrintController;
//			printer.PrintInfo = printInfo;
//			printer.PrintFormatter = textFormatter;

			controller = new UIPrinterPickerControllerWrapper ();
			controller.Delegate = new UIPrinterPickerControllerDelegate();

			var defaultPrinter = controller.SelectedPrinter;
			if (defaultPrinter == null) {
				controller.Present (true, UIPrintInteractionCompletionHan);
			}
		}

		private void AudioDidFinishPlaying(NSNotification obj)
		{
			Console.WriteLine("Audio Finished, will now restart");
			_player.Seek (new CMTime (0, 1));
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
		void UIPrintInteractionCompletionHan (UIPrinterPickerController printInteractionController,Boolean completed,NSError error)
		{
			printerSelected = completed;
			if(completed)
			printerURL = controller.SelectedPrinter.Url;
		}

	}
	class Worker
	{
		private HttpListenerContext context;
		NSUrl printerUrl;
		public Worker(HttpListenerContext context, NSUrl printerUrl)
		{
			this.context = context;
			this.printerUrl = printerUrl;
		}

		public void ProcessRequest()
		{
			//string msg = context.Request.HttpMethod + " " + context.Request.Url;
			if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/MakesUppercase") {
				string text;
				using (var reader = new StreamReader(context.Request.InputStream,
					context.Request.ContentEncoding))
				{
					text = reader.ReadToEnd();
				}
				Console.WriteLine(text);
				MakesUppercase (text);
			}
			if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/PrintWiFi") {
				string text;
				using (var reader = new StreamReader(context.Request.InputStream,
					context.Request.ContentEncoding))
				{
					text = reader.ReadToEnd();
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

		}

		private void MakesUppercase(string text)
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
}
