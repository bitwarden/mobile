namespace Bit.Core;

public partial class App : Application
{
    public App(AppOptions appOptions = null)
    {
        InitializeComponent();

        MainPage = new MainPage();
    }
}

