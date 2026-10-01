using PlayViApp.Services;

namespace PlayViApp;

public partial class ProfilesPage : ContentPage
{
    private readonly APIService _api;

    public ProfilesPage(APIService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        ProfilesView.ItemsSource = await _api.GetProfilesAsync();
    }

    private async void OnProfileSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ProfileDto profile) return;

        _api.CurrentProfile = profile;
        ProfilesView.SelectedItem = null;

        // Próximo passo: ir para a tela de filmes.
        await DisplayAlert("Selected profile", $"Hi, {profile.Name}!", "OK");
    }

    private async void OnAddProfileClicked(object? sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("New profile", "Profile name:", "Create", "Cancel", maxLength: 20);
        if (string.IsNullOrWhiteSpace(name)) return;

        var isKids = await DisplayAlert("Kids profile?", "Is this a kids profile?", "Yes", "No");

        var (ok, message) = await _api.CreateProfileAsync(name, isKids);
        if (!ok) await DisplayAlert("Error", message, "OK");

        await LoadAsync();
    }
}