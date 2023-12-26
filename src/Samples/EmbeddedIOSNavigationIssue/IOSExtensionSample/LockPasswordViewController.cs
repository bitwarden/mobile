using System;
using System.Threading.Tasks;
using Core;
using UIKit;

namespace Bit.iOS.Autofill
{
    public partial class LockPasswordViewController : UIViewController
    {
        public Action DismissModalAction { get; set; }

        public LockPasswordViewController(IntPtr handle)
            : base(handle)
        {
            DismissModalAction = Cancel;
        }

        public UIBarButtonItem BaseCancelButton => CancelButton;
        public UIBarButtonItem BaseSubmitButton => SubmitButton;
        public Action Success => () => CPViewController.DismissLockAndContinue();
        public Action Cancel => () => CPViewController.CompleteRequest();
        public Action LaunchHomePage { get; set; }

        public CredentialProviderViewController CPViewController { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        partial void SubmitButton_Activated(UIBarButtonItem sender)
        {
            CheckPasswordAsync().FireAndForget();
        }

        private async Task CheckPasswordAsync()
        {
            // some logic
            await Task.Delay(100);
            Success();
        }

        partial void CancelButton_Activated(UIBarButtonItem sender)
        {
            Cancel();
        }
    }
}
