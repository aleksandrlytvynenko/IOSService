using Foundation;
using UIKit;
using AVFoundation;

namespace IOSService
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window {
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
//			UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval (UIApplication.BackgroundFetchIntervalMinimum);
//			System.Diagnostics.Debug.WriteLine (UIApplication.SharedApplication.BackgroundRefreshStatus);
//
			NSError error;
			AVAudioSession instance = AVAudioSession.SharedInstance();
			instance.SetCategory(new NSString("AVAudioSessionCategoryPlayback"), AVAudioSessionCategoryOptions.MixWithOthers, out error);
			instance.SetMode(new NSString("AVAudioSessionModeDefault"), out error);
			instance.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);
			return true;
		}

		public override void PerformFetch (UIApplication application, System.Action<UIBackgroundFetchResult> completionHandler)
		{
			completionHandler (UIBackgroundFetchResult.NewData);
		}

		public override void DidReceiveRemoteNotification (UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
		{
			completionHandler (UIBackgroundFetchResult.NewData);
		}
	}
}


