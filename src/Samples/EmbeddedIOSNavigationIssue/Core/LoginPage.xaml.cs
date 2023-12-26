namespace Bit.Core;

public partial class LoginPage : ContentPage
{
    public LoginPage(AppOptions appOptions = null)
    {
        InitializeComponent();

        if (appOptions is null)
        {
            LogInSuccessAction = () => DisplayAlert("Login", "Success", "Cancel");
            CloseAction = () => Navigation.PopModalAsync();
        }
    }

    public Action LogInSuccessAction { get; set; }
    public Action CloseAction { get; set; }

    void LoginSuccess_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        LogInSuccessAction();
    }

    void Close_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        CloseAction();
    }
}

