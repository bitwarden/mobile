using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Bit.Core;

public static class MauiProgram
{
    public static MauiAppBuilder ConfigureMauiAppBuilder(bool useMauiApp, Action<IMauiHandlersCollection> customHandlers = null)
    {
        var builder = MauiApp.CreateBuilder();
        if (useMauiApp)
        {
            builder.UseMauiApp<Bit.Core.App>();
        }
        builder.UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    customHandlers?.Invoke(handlers);
                });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder;
    }
}

