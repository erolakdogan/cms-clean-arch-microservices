using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentService.Infrastructure.UsersExternal;

public interface IServiceTokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

public sealed class ServiceTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<UsersClientOptions> options,
    ILogger<ServiceTokenProvider> log) : IServiceTokenProvider
{
    private readonly UsersClientOptions _opt = options.Value;
    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_token is not null && _expiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            return _token;

        await _lock.WaitAsync(ct);
        try
        {
            if (_token is not null && _expiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
                return _token;

            var client = httpClientFactory.CreateClient("UsersLogin");
            client.BaseAddress = new Uri(_opt.BaseUrl);

            var req = new { email = _opt.ServiceAccount.Email, password = _opt.ServiceAccount.Password };
            var resp = await client.PostAsJsonAsync("/api/v1/auth/login", req, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct)
                       ?? throw new InvalidOperationException("Login response was empty.");

            _token = json.AccessToken ?? json.Token ?? json.Jwt ?? throw new InvalidOperationException("Token not found.");
            var minutes = json.ExpiresIn > 0 ? json.ExpiresIn : 30;
            _expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);
            log.LogInformation("Service token acquired, expires at {Expires}", _expiresAt);
            return _token!;
        }
        finally { _lock.Release(); }
    }

    private sealed class LoginResponse
    {
        public string? AccessToken { get; set; }
        public string? Token { get; set; }
        public string? Jwt { get; set; }
        public int ExpiresIn { get; set; }
    }
}
