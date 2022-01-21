using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bit.App.Controls;
using Bit.iOS.Core.Renderers;
using CoreFoundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomNavigationRenderer))]
namespace Bit.iOS.Core.Renderers
{
    public class CustomNavigationRenderer : NavigationRenderer
    {
        public override void PushViewController(UIViewController viewController, bool animated)
        {
            base.PushViewController(viewController, animated);

            var currentPage = (Element as NavigationPage)?.CurrentPage;
            if (currentPage == null)
            {
                return;
            }
            var toolbarItems = currentPage.ToolbarItems;
            if (!toolbarItems.Any())
            {
                return;
            }
            var uiBarButtonItems = TopViewController.NavigationItem.RightBarButtonItems;
            if (uiBarButtonItems == null)
            {
                return;
            }

            foreach (var toolbarItem in toolbarItems.Where(t => t is ExtendedToolbarItem eti && eti.UseOriginalImage))
            {
                var index = currentPage.ToolbarItems.IndexOf(toolbarItem) + 1;
                if (index < 0 || index >= uiBarButtonItems.Length)
                {
                    continue;
                }

                // HACK: XF PimaryToolbarItem is sealed so we can't override it, and also it doesn't provide any
                // direct way to replace it with our custom one (we can but we need to rewrite several parts of the NavigationRenderer)
                // So I think this is the easiest soolution for now to set UIImageRenderingMode.AlwaysOriginal
                // on the toolbar item image
                void ToolbarItem_PropertyChanged(object s, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == nameof(ExtendedToolbarItem.IconImageSource))
                    {
                        var uiBarButtonItems = TopViewController.NavigationItem.RightBarButtonItems;
                        if (uiBarButtonItems == null)
                        {
                            return;
                        }

                        var uiBarButtonItem = uiBarButtonItems[index];

                        Task.Run(async () => {
                            await Task.Delay(1000);
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            //uiBarButtonItem.Image = UIImage.FromBundle("info"));
                            uiBarButtonItem.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal));
                        });
                        

                        toolbarItem.PropertyChanged -= ToolbarItem_PropertyChanged;
                    }
                };
                toolbarItem.PropertyChanged += ToolbarItem_PropertyChanged;
            }
        }
    }
}
