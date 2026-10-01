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
        StatusLabel.Text = "Entrando...";
        var (ok, message) = await _api.LoginAsync(EmailEntry.Text ?? "", PasswordEntry.Text ?? "");

        if (ok) await Shell.Current.GoToAsync("//profiles");
        else StatusLabel.Text = message;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Criando conta...";
        var (ok, message) = await _api.RegisterAsync(
            EmailEntry.Text ?? "", PasswordEntry.Text ?? "", NameEntry.Text ?? "");

        if (ok) await Shell.Current.GoToAsync("//profiles");
        else StatusLabel.Text = message;
    }
}