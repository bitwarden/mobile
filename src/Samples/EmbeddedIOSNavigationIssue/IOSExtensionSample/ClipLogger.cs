
using System;
using System.Text;
using UIKit;

namespace IOSExtensionSample
{
    public static class ClipLogger
    {
        private static readonly StringBuilder _currentBreadcrumbs = new StringBuilder();

        public static void Log(Exception ex) => Log(ex.ToString());

        public static void Log(string breadcrumb)
        {
            _currentBreadcrumbs.AppendLine($"{DateTime.Now.ToShortTimeString()}: {breadcrumb}");
            UIPasteboard.General.String = _currentBreadcrumbs.ToString();
        }
    }
}

