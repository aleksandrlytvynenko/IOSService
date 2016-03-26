using System;

using UIKit;
using System.Net.Http;
using Foundation;
using System.Threading.Tasks;
using System.Threading;

namespace PrintClient
{
	public partial class ViewController : UIViewController
	{
		string _makeUppercase = "/MakesUppercase";
		string _printWifi = "/PrintWiFi";
		string _printBT = "/PrintBT";


		private void IsLoading (bool value)
		{
			if (value) {
				_activityIndicator.StartAnimating ();
			} else {
				_activityIndicator.StopAnimating ();
			}
		}

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			_activityIndicator.HidesWhenStopped = true;
			_hostIpTextView.Text = "http://192.168.1.135:8080";
			_upperCaseButton.TouchUpInside += UpperCaseButtonTouchUpInside;
			_wifiPrint.TouchUpInside += WifiButtonTouchUpInside;
			_btPrint.TouchUpInside += BtPrintTouchUpInside;
		}

		async void BtPrintTouchUpInside (object sender, EventArgs e)
		{
			await CallServer (_hostIpTextView.Text + _printBT, new StringContent (_inputField.Text));
		}

		async void WifiButtonTouchUpInside (object sender, EventArgs e)
		{
			await CallServer (_hostIpTextView.Text + _printWifi, new StringContent (_inputField.Text));
		}

		async void UpperCaseButtonTouchUpInside (object sender, EventArgs e)
		{
			await CallServer (_hostIpTextView.Text + _makeUppercase, new StringContent (_inputField.Text));

		}
		private int _countOfRetry = 3;
		private async Task CallServer (string url, StringContent content)
		{
			for (int i = 0; i < _countOfRetry; i++) {
				IsLoading (true);
				HttpResponseMessage result;
				string resultString = "error";
				using (var client = new HttpClient (new HttpClientHandler ())) {
					try {
						result = await client.PostAsync (url, content);
						resultString = await result.Content.ReadAsStringAsync ();
						IsLoading (false);
						_outputLabel.Text = resultString;
						return;

					} catch (Exception e) {
						resultString = e.Message;
					}
				}
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}
	}


}

