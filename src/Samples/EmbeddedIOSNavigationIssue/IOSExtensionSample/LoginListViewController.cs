using System;
using Foundation;
using UIKit;

namespace Bit.iOS.Autofill
{
    public partial class LoginListViewController : UIViewController
    {
        public Action DismissModalAction { get; set; }

        public LoginListViewController(IntPtr handle)
            : base(handle)
        {
            DismissModalAction = Cancel;
        }

        public CredentialProviderViewController CPViewController { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavItem.Title = "Items";
            CancelBarButton.Title = "Cancel";

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44;
            TableView.BackgroundColor = UIColor.LightGray;
            TableView.Source = new TableSource(this);
        }

        partial void CancelBarButton_Activated(UIBarButtonItem sender)
        {
            Cancel();
        }

        private void Cancel()
        {
            CPViewController.CompleteRequest();
        }

        public class TableSource : UITableViewSource
        {
            private LoginListViewController _controller;

            public TableSource(LoginListViewController controller)
            {
                _controller = controller;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return 3;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("TableCell");

                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, "TableCell");
                    cell.TextLabel.Text = "Some Item";
                }
                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow(indexPath, true);
                tableView.EndEditing(true);

                _controller.CPViewController.CompleteRequest("qer", "myUser", "myPassword", "myTOTP");
            }
        }
    }
}
