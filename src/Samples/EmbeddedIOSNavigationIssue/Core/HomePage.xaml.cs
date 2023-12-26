namespace Bit.Core;

public partial class HomePage : ContentPage
{
    public HomePage(AppOptions appOptions = null)
    {
        InitializeComponent();

        if (appOptions is null)
        {
            StartLoginAction = () => Navigation.PushModalAsync(new LoginPage());
            CloseAction = () => Navigation.PopModalAsync();
        }
    }

    public Action StartLoginAction { get; set; }
    public Action CloseAction { get; set; }

    void Login_Clicked(System.Object sender, System.EventArgs e)
    {
        StartLoginAction();
    }

    void Cancel_Clicked(System.Object sender, System.EventArgs e)
    {
        CloseAction();
    }
}
