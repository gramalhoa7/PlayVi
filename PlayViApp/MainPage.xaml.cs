using PlayViApp.Services;

namespace PlayViApp;

public partial class MainPage : ContentPage
{
    private readonly APIService _api;

    public MainPage(APIService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Logging in...";
        var (ok, message) = await _api.LoginAsync(EmailEntry.Text ?? "", PasswordEntry.Text ?? "");

        if (ok) await Shell.Current.GoToAsync("//profiles");
        else StatusLabel.Text = message;
    }

    private async void OnCreateAccountClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//register");
    }
}