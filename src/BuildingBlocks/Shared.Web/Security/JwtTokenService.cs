using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shared.Web.Security
{
    public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
    {
        private readonly JwtOptions _opt = options.Value;

        private SigningCredentials GetCreds()
            => new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key)), SecurityAlgorithms.HmacSha256);

        public string CreateUserToken(Guid userId, string email, string displayName, IEnumerable<string> roles,
                                      int? minutesOverride = null)
        {
            var now = DateTime.UtcNow;
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, displayName),
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwt = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(minutesOverride ?? _opt.AccessTokenMinutes),
                signingCredentials: GetCreds());

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string CreateServiceToken(string subject, IEnumerable<Claim>? extraClaims = null, int? minutesOverride = null)
        {
            var now = DateTime.UtcNow;
            var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, subject) };
            if (extraClaims is not null) claims.AddRange(extraClaims);

            var jwt = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(minutesOverride ?? _opt.AccessTokenMinutes),
                signingCredentials: GetCreds());

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
