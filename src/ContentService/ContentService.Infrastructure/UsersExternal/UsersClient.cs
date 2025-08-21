using ContentService.Application.UsersExternal;
using Microsoft.Extensions.Options;
using Shared.Web.Security;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ContentService.Infrastructure.UsersExternal;

public sealed class UsersClient(HttpClient http, IJwtTokenService jwt, IOptions<UsersClientOptions> opts)
    : IUsersClient
{
    public async Task<UserBriefDto?> GetBriefAsync(Guid id, CancellationToken ct = default)
    {
        var token = jwt.CreateServiceToken(
             subject: "content-service",
             extraClaims: new[] { new Claim("scope", "s2s:users.read") });

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await http.GetAsync($"/api/v1/users/{id}/brief", ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            Serilog.Log.Warning("UsersClient brief failed {Status} {Body}", resp.StatusCode, body);
            resp.EnsureSuccessStatusCode();
        }

        return await http.GetFromJsonAsync<UserBriefDto>($"/api/v1/users/{id}/brief", cancellationToken: ct);
    }
}
