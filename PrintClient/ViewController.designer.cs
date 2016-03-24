// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace PrintClient
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton _btPrint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField _hostIpTextView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField _inputField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel _outputLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton _upperCaseButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton _wifiButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (_btPrint != null) {
				_btPrint.Dispose ();
				_btPrint = null;
			}
			if (_hostIpTextView != null) {
				_hostIpTextView.Dispose ();
				_hostIpTextView = null;
			}
			if (_inputField != null) {
				_inputField.Dispose ();
				_inputField = null;
			}
			if (_outputLabel != null) {
				_outputLabel.Dispose ();
				_outputLabel = null;
			}
			if (_upperCaseButton != null) {
				_upperCaseButton.Dispose ();
				_upperCaseButton = null;
			}
			if (_wifiButton != null) {
				_wifiButton.Dispose ();
				_wifiButton = null;
			}
		}
	}
}
