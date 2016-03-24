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

namespace IOSService
{
	public partial class ViewController : UIViewController
	{
		HttpListener listener;
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

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
					new Thread (new Worker (ctx).ProcessRequest).Start ();
				}
			});
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

	}
	class Worker
	{
		private HttpListenerContext context;

		public Worker(HttpListenerContext context)
		{
			this.context = context;
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
		}

		private void MakesUppercase(string text)
		{
			SendResponce (text.ToUpper());
		}

		private void SendResponce(string responce)
		{
			byte[] b = Encoding.UTF8.GetBytes(responce);
			context.Response.ContentLength64 = b.Length;
			context.Response.OutputStream.Write(b, 0, b.Length);
			context.Response.OutputStream.Close();

		}
	}
}
