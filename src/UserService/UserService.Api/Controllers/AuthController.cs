using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Web.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.Common.Abstractions;

namespace UserService.Api.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/auth")]
    public sealed class AuthController(
     IOptions<JwtOptions> jwtOpt,
     IUserRepository repo,
     IPasswordHasherService hasher) : ControllerBase
    {
        public sealed record LoginRequest(string Email, string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var user = await repo.Query().FirstOrDefaultAsync(u => u.Email == req.Email, ct);
            if (user is null || !hasher.Verify(user.PasswordHash, req.Password))
                return Unauthorized();

            var jwt = jwtOpt.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName)
        };
            foreach (var r in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r)); // policy için
                claims.Add(new Claim("role", r));          // uyumluluk için
            }

            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                accessToken,
                tokenType = "Bearer",
                expiresIn = (int)TimeSpan.FromMinutes(jwt.AccessTokenMinutes).TotalSeconds
            });
        }
    }
}
