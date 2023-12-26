using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using UIKit;

namespace IOSExtensionSample
{
    public class CustomWindowHandler : ElementHandler<IWindow, UIWindow>, IWindowHandler
    {
        public static IPropertyMapper<IWindow, IWindowHandler> Mapper = new PropertyMapper<IWindow, IWindowHandler>(ElementHandler.ElementMapper)
        {
        };

        public CustomWindowHandler() : base(Mapper)
        {
        }

        protected override UIWindow CreatePlatformElement()
        {
            // Haven't tested
            //return UIApplication.SharedApplication.Delegate.GetWindow();
            return Platform.GetCurrentUIViewController().View.Window;
        }
    }
}

