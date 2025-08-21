using System.Security.Claims;

namespace Shared.Web.Security
{
    public interface IJwtTokenService
    {
        string CreateUserToken(Guid userId, string email, string displayName, IEnumerable<string> roles,
                          int? minutesOverride = null);
        string CreateServiceToken(string subject, IEnumerable<Claim>? extraClaims = null, int? minutesOverride = null);
    }
}
