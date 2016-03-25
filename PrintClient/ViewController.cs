using System;

using UIKit;
using System.Net.Http;
using Foundation;

namespace PrintClient
{
	public partial class ViewController : UIViewController
	{
		string _makeUppercase = "/MakesUppercase";
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			_hostIpTextView.Text = "192.168.1.135:8080";
			_upperCaseButton.TouchUpInside += UpperCaseButtonTouchUpInside;
		}

		async void UpperCaseButtonTouchUpInside (object sender, EventArgs e)
		{
			HttpResponseMessage result;
			using (HttpClientHandler handler = new HttpClientHandler())
			{
				using (HttpClient client = new HttpClient(handler))
				{
					try
					{
						var content = new StringContent(_inputField.Text);
						result = await client.PostAsync("http://" + _hostIpTextView.Text + _makeUppercase, content);
						var resultString = await result.Content.ReadAsStringAsync();
						_outputLabel.Text = resultString;

					}
					catch
					{
						
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

