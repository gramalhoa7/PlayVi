using System.Globalization;
using PlayViApp.Services;

namespace PlayViApp;

public partial class PlansPage : ContentPage
{
    private readonly APIService _api;

    public PlansPage(APIService api)
    {
        InitializeComponent();
        _api = api;
    }

     public record PlanItem(PlanDto Plan)
    {
        public string Name => Plan.Name;
        public string PriceText => Plan.Price.ToString("C", CultureInfo.CurrentCulture);
        public string BillingText => Plan.BillingPeriod == "Yearly" ? "per year" : "per month";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var plans = await _api.GetPlansAsync();
        PlansView.ItemsSource = plans.Select(p => new PlanItem(p)).ToList();
        StatusLabel.Text = plans.Count == 0 ? "Não foi possível carregar os planos." : "";
    }

    private async void OnPlanSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PlanItem item) return;

        var confirm = await DisplayAlert("Confirm Subscription",
            $"You wish to subscribe to the {item.Name} plan for {item.PriceText} per {item.BillingText}?\n\n",
            "Subscribe", "Return");

        if (!confirm) return;

        StatusLabel.Text = "Assining...";
        var (ok, message) = await _api.SubscribeAsync(item.Plan.Id);

        if (ok) await Shell.Current.GoToAsync("//profiles");
        else StatusLabel.Text = message;
    }
}
