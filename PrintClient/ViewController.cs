using System;

using UIKit;
using System.Net.Http;

namespace PrintClient
{
	public partial class ViewController : UIViewController
	{
		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.

			_hostIpTextView.Text = "192.168.1.135:8080";
			_upperCaseButton.TouchUpInside += UpperCaseButtonTouchUpInside;
		}

		async void UpperCaseButtonTouchUpInside (object sender, EventArgs e)
		{
			HttpResponseMessage result;
			string response;
			using (HttpClientHandler handler = new HttpClientHandler())
			{
				using (HttpClient client = new HttpClient(handler))
				{
					try
					{
						var content = new StringContent(_inputField.Text);
						result = await client.PostAsync("http://" + _hostIpTextView.Text + "/MakesUppercase", content);
						var resultString = await result.Content.ReadAsStringAsync();
						_outputLabel.Text = resultString;

					}
					catch (Exception exc)
					{
						
					}
				}
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}

