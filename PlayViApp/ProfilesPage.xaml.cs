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
        await Shell.Current.GoToAsync("//home");
    }

    private async void OnAddProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//new-profile");
    }
}