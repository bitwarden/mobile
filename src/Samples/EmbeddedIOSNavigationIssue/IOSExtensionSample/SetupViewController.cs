using System;
using UIKit;

namespace Bit.iOS.Autofill
{
    public partial class SetupViewController : UIViewController
    {
        public Action DismissModalAction { get; set; }

        public SetupViewController(IntPtr handle)
            : base(handle)
        {
            DismissModalAction = Cancel;
        }

        public CredentialProviderViewController CPViewController { get; set; }

        partial void BackButton_Activated(UIBarButtonItem sender)
        {
            Cancel();
        }

        private void Cancel()
        {
            CPViewController.CompleteRequest();
        }
    }
}
