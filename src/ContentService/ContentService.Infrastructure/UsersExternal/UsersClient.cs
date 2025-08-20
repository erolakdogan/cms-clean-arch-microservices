using System.Net;
using System.Net.Http.Json;
using ContentService.Application.UsersExternal;
using Microsoft.Extensions.Logging;

namespace ContentService.Infrastructure.UsersExternal;

public sealed class UsersClient(HttpClient http, IServiceTokenProvider tokens, ILogger<UsersClient> log)
    : IUsersClient
{
    public async Task<UserSummary?> GetUserAsync(Guid id, CancellationToken ct = default)
    {
        var bearer = await tokens.GetTokenAsync(ct);
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{id}");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer);

        var resp = await http.SendAsync(req, ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();

        var u = await resp.Content.ReadFromJsonAsync<UserDtoInternal>(cancellationToken: ct)
                 ?? throw new InvalidOperationException("User deserialization failed.");

        return new UserSummary(u.Id, u.Email, u.DisplayName, u.Roles);
    }

    private sealed class UserDtoInternal
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
