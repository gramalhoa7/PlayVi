using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PlayViApp.Services;

public record ProfileDto(int Id, string Name, string? AvatarUrl, bool IsKids)
{
    public string Initial => Name.Length > 0 ? Name[..1].ToUpper() : "?";
}

public class APIService
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("http://localhost:5026/")
    };

    public string? Token { get; private set; }
    public ProfileDto? CurrentProfile { get; set; }

    public Task<(bool ok, string message)> LoginAsync(string email, string password) =>
        SendAuthAsync("api/auth/login", new { email, password });

    public Task<(bool ok, string message)> RegisterAsync(string email, string password, string displayName) =>
        SendAuthAsync("api/auth/register", new { email, password, displayName });

    public async Task<List<ProfileDto>> GetProfilesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ProfileDto>>("api/profiles") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<(bool ok, string message)> CreateProfileAsync(string name, bool isKids)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/profiles", new { name, avatarUrl = (string?)null, isKids });
            if (response.IsSuccessStatusCode) return (true, "OK");
            return (false, await ReadErrorAsync(response));
        }
        catch (Exception ex)
        {
            return (false, "Do not connect to API. " + ex.Message);
        }
    }

    private async Task<(bool ok, string message)> SendAuthAsync(string url, object body)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(url, body);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                Token = result?.Token;
                _http.DefaultRequestHeaders.Authorization =
                    Token is null ? null : new AuthenticationHeaderValue("Bearer", Token);
                return (true, "OK");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, "E-mail ou senha incorretos.");

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
}