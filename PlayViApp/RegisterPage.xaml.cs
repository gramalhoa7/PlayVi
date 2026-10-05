using PlayViApp.Services;

namespace PlayViApp;

public partial class RegisterPage : ContentPage
{
    private readonly APIService _api;

    public RegisterPage(APIService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Creating account...";
        var (ok, message) = await _api.RegisterAsync(
            EmailEntry.Text ?? "", PasswordEntry.Text ?? "", NameEntry.Text ?? "");

        if (ok) await Shell.Current.GoToAsync("//profiles");
        else StatusLabel.Text = message;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");
    }
}