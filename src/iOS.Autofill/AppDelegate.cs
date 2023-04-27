using Foundation;
using UIKit;

namespace Bit.iOS.Autofill
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window
        {
            get; set;
        }

        public override void FinishedLaunching(UIApplication application)
        {

            var ln = @"libbitwarden_c.framework/libbitwarden_c";
            var documentsPath = NSBundle.MainBundle.BundlePath;
            var filePath = System.IO.Path.Combine(documentsPath, "Frameworks", ln);
            var ptr = ObjCRuntime.Dlfcn.dlopen(filePath, 0);

            var sdkClient = new BitwardenClient();
            var test = sdkClient.Fingerprint();
            
            
            base.FinishedLaunching(application);
        }

        public override void OnResignActivation(UIApplication application)
        {
        }

        public override void DidEnterBackground(UIApplication application)
        {
        }

        public override void WillEnterForeground(UIApplication application)
        {
        }

        public override void WillTerminate(UIApplication application)
        {
        }
    }
}
