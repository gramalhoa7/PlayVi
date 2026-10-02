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
        WelcomeLabel.Text = $"Olá, {_api.CurrentProfile?.Name}";

        if (!_ready) await InitFiltersAsync();
        await SearchAsync();
    }

    private async Task InitFiltersAsync()
    {
        var genres = await _api.GetGenresAsync();
        GenrePicker.ItemsSource = new[] { "Todos os gêneros" }.Concat(genres).ToList();
        EraPicker.ItemsSource = new[] { "Todas as épocas", "Antigos (antes de 2000)", "Novos (2020 em diante)" };
        TypePicker.ItemsSource = new[] { "Filmes e séries", "Só filmes", "Só séries" };

        GenrePicker.SelectedIndex = 0;
        EraPicker.SelectedIndex = 0;
        TypePicker.SelectedIndex = 0;

        _ready = true;
    }

    private async Task SearchAsync()
    {
        var genre = GenrePicker.SelectedIndex > 0 ? GenrePicker.SelectedItem as string : null;
        var era = EraPicker.SelectedIndex switch { 1 => "antigos", 2 => "novos", _ => null };
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

        if (result.Ok)
            await DisplayAlert(title.Name, "Reprodução liberada! No próximo passo, o player abre aqui.", "OK");
        else
            await DisplayAlert("Aviso", result.Message ?? "Não foi possível abrir este título.", "OK");
    }

    private async Task<bool> OfferPlanAsync()
    {
        var plans = await _api.GetPlansAsync();
        if (plans.Count == 0)
        {
            await DisplayAlert("Planos", "Não foi possível carregar os planos.", "OK");
            return false;
        }

        var br = new CultureInfo("pt-BR");
        var labels = plans
            .Select(p => $"{p.Name} — {p.Price.ToString("C", br)} ({(p.BillingPeriod == "Yearly" ? "anual" : "mensal")})")
            .ToArray();

        var choice = await DisplayActionSheet("Escolha um plano para assistir", "Cancelar", null, labels);
        var index = Array.IndexOf(labels, choice);
        if (index < 0) return false;

        var (ok, message) = await _api.SubscribeAsync(plans[index].Id);
        if (!ok)
        {
            await DisplayAlert("Erro", message, "OK");
            return false;
        }

        await DisplayAlert("Assinatura ativa", "Pagamento simulado aprovado!", "OK");
        return true;
    }

    private async void OnSwitchProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//profiles");
    }
}