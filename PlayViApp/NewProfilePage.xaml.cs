using PlayViApp.Services;

namespace PlayViApp;

public partial class NewProfilePage : ContentPage
{
    private readonly APIService _api;
    private AvatarOption? _selected;

    public NewProfilePage(APIService api)
    {
        InitializeComponent();
        _api = api;
        AvatarsView.ItemsSource = Avatars.All;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _selected = null;
        AvatarsView.SelectedItem = null;
        PreviewImage.Source = "dotnet_bot.png";
        NameEntry.Text = "";
        KidsSwitch.IsToggled = false;
        StatusLabel.Text = "";
    }

    private void OnAvatarSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selected = e.CurrentSelection.FirstOrDefault() as AvatarOption;
        PreviewImage.Source = _selected?.Image ?? "dotnet_bot.png";
    }

    private async void OnCreateProfileClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusLabel.Text = "Please enter a profile name.";
            return;
        }

        StatusLabel.Text = "Creating...";
        var (ok, message) = await _api.CreateProfileAsync(name, _selected?.Id, KidsSwitch.IsToggled);

        if (ok) await Shell.Current.GoToAsync("//profiles");
        else StatusLabel.Text = message;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//profiles");
    }
}