using Microsoft.Maui.Hosting;

namespace EmbeddedIOSNavigationIssue;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return Bit.Core.MauiProgram.ConfigureMauiAppBuilder(true).Build();
    }
}

