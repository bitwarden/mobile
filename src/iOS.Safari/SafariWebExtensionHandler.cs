using System;
using Foundation;

namespace iOS.Safari
{
    [Register("SafariWebExtensionHandler")]
    public class SafariWebExtensionHandler : NSExtensionRequestHandling
    {
        public SafariWebExtensionHandler()
        {
        }

        public override void BeginRequestWithExtensionContext(NSExtensionContext context)
        {
            var item = context.InputItems[0];
            Console.WriteLine(item);
            var random = context.GetType();
        }
    }
}
