using CommunityToolkit.Maui.Views;

namespace PlayViApp;

public partial class PlayerPage : ContentPage
{
    public PlayerPage(string title, string url)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        Player.Source = MediaSource.FromUri(url);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        Player.Stop();
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Player.Stop();
        Player.Handler?.DisconnectHandler();
    }
}