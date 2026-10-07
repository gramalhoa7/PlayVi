using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PlayViApp.Services;

public record ProfileDto(int Id, string Name, string? AvatarId, bool IsKids)
{
    public string Initial => Name.Length > 0 ? Name[..1].ToUpper() : "?";
    public string AvatarImage => string.IsNullOrWhiteSpace(AvatarId) ? "dotnet_bot.png" : $"{AvatarId}.png";
}

public record TitleDto(int Id, string Name, string Type, int ReleaseYear,
                       string? Description, string? PosterUrl, List<string>? Genres)
{
    public string Initial => Name.Length > 0 ? Name[..1].ToUpper() : "?";

    public string Subtitle =>
        $"{ReleaseYear} • {(Type == "Series" ? "Serie" : "Filme")}" +
        (Genres is { Count: > 0 } ? " • " + string.Join(", ", Genres) : "");
}

public record PlanDto(int Id, string Name, decimal Price, string BillingPeriod);

public record WatchResult(bool Ok, bool NeedsPlan, string? Url, string? Message);

public class APIService
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("http://localhost:5026/")
    };

    public string? Token { get; private set; }
    public ProfileDto? CurrentProfile { get; set; }

    // ---------- Conta ----------

    public Task<(bool ok, string message)> LoginAsync(string email, string password) =>
        SendAuthAsync("api/auth/login", new { email, password });

    public Task<(bool ok, string message)> RegisterAsync(string email, string password, string displayName) =>
        SendAuthAsync("api/auth/register", new { email, password, displayName });

    // ---------- Perfis ----------

    public async Task<List<ProfileDto>> GetProfilesAsync()
    {
        try { return await _http.GetFromJsonAsync<List<ProfileDto>>("api/profiles") ?? new(); }
        catch { return new(); }
    }

    public async Task<(bool ok, string message)> CreateProfileAsync(string name, string? avatarId, bool isKids)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/profiles", new { name, avatarId, isKids });
            if (response.IsSuccessStatusCode) return (true, "OK");
            return (false, await ReadErrorAsync(response));
        }
        catch (Exception ex)
        {
            return (false, "Do not connect to API. " + ex.Message);
        }
    }

    // ---------- Catálogo ----------

    public async Task<List<string>> GetGenresAsync()
    {
        try { return await _http.GetFromJsonAsync<List<string>>("api/genres") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<TitleDto>> GetTitlesAsync(string? q, string? genre, string? era, string? type)
    {
        var parts = new List<string>();
        void Add(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add($"{key}={Uri.EscapeDataString(value)}");
        }
        Add("q", q);
        Add("genre", genre);
        Add("era", era);
        Add("type", type);

        var url = "api/titles" + (parts.Count > 0 ? "?" + string.Join("&", parts) : "");

        try { return await _http.GetFromJsonAsync<List<TitleDto>>(url) ?? new(); }
        catch { return new(); }
    }

    public async Task<WatchResult> WatchAsync(int titleId)
    {
        try
        {
            var response = await _http.GetAsync($"api/titles/{titleId}/watch");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<WatchDto>();
                return new WatchResult(true, false, data?.VideoUrl, null);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return new WatchResult(false, true, null, "You need an active subscription.");

            return new WatchResult(false, false, null, await ReadErrorAsync(response));
        }
        catch (Exception ex)
        {
            return new WatchResult(false, false, null, "Do not connect to API. " + ex.Message);
        }
    }

    // ---------- Planos ----------

    public async Task<List<PlanDto>> GetPlansAsync()
    {
        try { return await _http.GetFromJsonAsync<List<PlanDto>>("api/plans") ?? new(); }
        catch { return new(); }
    }

    public async Task<(bool ok, string message)> SubscribeAsync(int planId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/subscriptions", new { planId });
            if (response.IsSuccessStatusCode) return (true, "OK");
            return (false, await ReadErrorAsync(response));
        }
        catch (Exception ex)
        {
            return (false, "Do not connect to API. " + ex.Message);
        }
    }

    // ---------- Internos ----------

    private async Task<(bool ok, string message)> SendAuthAsync(string id, object body)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(id, body);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                Token = result?.Token;
                _http.DefaultRequestHeaders.Authorization =
                    Token is null ? null : new AuthenticationHeaderValue("Bearer", Token);
                return (true, "OK");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (false, "Invalid email or password.");

            return (false, await ReadErrorAsync(response));
        }
        catch (Exception ex)
        {
            return (false, "Do not connect to API. " + ex.Message);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var err = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            if (!string.IsNullOrEmpty(err?.Message)) return err.Message;
        }
        catch { }
        return await response.Content.ReadAsStringAsync();
    }

    private record TokenResponse(string Token);
    private record ErrorResponse(string? Message);
    private record WatchDto(int Id, string Name, string? VideoUrl);
}