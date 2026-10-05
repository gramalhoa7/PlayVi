using System.Globalization;
using PlayViApp.Services;

namespace PlayViApp;

public partial class HomePage : ContentPage
{
    private readonly APIService _api;
    private bool _ready;

    public HomePage(APIService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        WelcomeLabel.Text = $"Hi, {_api.CurrentProfile?.Name}";

        if (!_ready) await InitFiltersAsync();
        await SearchAsync();
    }

    private async Task InitFiltersAsync()
    {
        var genres = await _api.GetGenresAsync();
        GenrePicker.ItemsSource = new[] { "All Genres" }.Concat(genres).ToList();
        EraPicker.ItemsSource = new[] { "All Time", "Older (before 2000)", "Newer (2020 and beyond)" };
        TypePicker.ItemsSource = new[] { "Movies and Series", "Movies Only", "Series Only" };

        GenrePicker.SelectedIndex = 0;
        EraPicker.SelectedIndex = 0;
        TypePicker.SelectedIndex = 0;

        _ready = true;
    }

    private async Task SearchAsync()
    {
        var genre = GenrePicker.SelectedIndex > 0 ? GenrePicker.SelectedItem as string : null;
        var era = EraPicker.SelectedIndex switch { 1 => "Older", 2 => "Newer", _ => null };
        var type = TypePicker.SelectedIndex switch { 1 => "Movie", 2 => "Series", _ => null };

        TitlesView.ItemsSource = await _api.GetTitlesAsync(SearchBox.Text, genre, era, type);
    }

    private async void OnSearchPressed(object? sender, EventArgs e) => await SearchAsync();

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_ready && string.IsNullOrEmpty(e.NewTextValue)) await SearchAsync();
    }

    private async void OnFilterChanged(object? sender, EventArgs e)
    {
        if (_ready) await SearchAsync();
    }

    private async void OnTitleSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TitleDto title) return;
        TitlesView.SelectedItem = null;

        var result = await _api.WatchAsync(title.Id);

        if (result.NeedsPlan)
        {
            if (!await OfferPlanAsync()) return;
            result = await _api.WatchAsync(title.Id);
        }

        if (result.Ok && !string.IsNullOrWhiteSpace(result.Url))
            await Navigation.PushModalAsync(new PlayerPage(title.Name, result.Url));
        else
            await DisplayAlert("Warning", result.Message ?? "This title is not available for viewing.", "OK");
    }

    private async Task<bool> OfferPlanAsync()
    {
        var plans = await _api.GetPlansAsync();
        if (plans.Count == 0)
        {
            await DisplayAlert("Plans", "Dont was possible to load plans.", "OK");
            return false;
        }

        var br = new CultureInfo("pt-BR");
        var labels = plans
            .Select(p => $"{p.Name} — {p.Price.ToString("C", br)} ({(p.BillingPeriod == "Yearly" ? "anual" : "mensal")})")
            .ToArray();

        var choice = await DisplayActionSheet("Choose a plan to watch", "Cancel", null, labels);
        var index = Array.IndexOf(labels, choice);
        if (index < 0) return false;

        var (ok, message) = await _api.SubscribeAsync(plans[index].Id);
        if (!ok)
        {
            await DisplayAlert("Erro", message, "OK");
            return false;
        }

        await DisplayAlert("Active assignment", "Simulated payment approved!", "OK");
        return true;
    }

    private async void OnSwitchProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//profiles");
    }
}