using System;

using UIKit;
using System.Net.Http;
using Foundation;
using System.Threading.Tasks;

namespace PrintClient
{
	public partial class ViewController : UIViewController
	{
		string _makeUppercase = "/MakesUppercase";
		string _printWifi = "/PrintWiFi";
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			_hostIpTextView.Text = "http://192.168.1.135:8080";
			_upperCaseButton.TouchUpInside += UpperCaseButtonTouchUpInside;
			_wifiPrint.TouchUpInside += WifiButtonTouchUpInside;
			_btPrint.TouchUpInside += BtPrintTouchUpInside;
		}

		void BtPrintTouchUpInside (object sender, EventArgs e)
		{
			
		}

		async void WifiButtonTouchUpInside (object sender, EventArgs e)
		{
			_outputLabel.Text = await CallServer (_printWifi + _printWifi, new StringContent(_inputField.Text));
		}

		async void UpperCaseButtonTouchUpInside (object sender, EventArgs e)
		{
			_outputLabel.Text = await CallServer (_hostIpTextView.Text + _makeUppercase, new StringContent(_inputField.Text));

		}

		private async Task<string> CallServer(string url, StringContent content)
		{
			HttpResponseMessage result;
			string resultString = "error";
			using (HttpClientHandler handler = new HttpClientHandler())
			{
				using (HttpClient client = new HttpClient(handler))
				{
					try
					{
						result = await client.PostAsync(url , content);
						resultString = await result.Content.ReadAsStringAsync();

					}
					catch
					{

					}
				}
			}
			return resultString;
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}
	}
}

