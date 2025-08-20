using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Api.Contracts.Auth;
using UserService.Application.Common.Abstractions;

namespace UserService.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public sealed class AuthController(
     IConfiguration cfg,
     IUserRepository users,
     IPasswordHasherService hasher) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var u = await users.GetByEmailAsync(req.Email, ct);
            if (u is null || !hasher.Verify(u.PasswordHash, req.Password))
                return Problem(title: "Invalid credentials", statusCode: 401);

            var issuer = cfg["Jwt:Issuer"] ?? "cmspoc";
            var audience = cfg["Jwt:Audience"] ?? "cmspoc.clients";
            var minutes = int.TryParse(cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 30;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"] ?? "dev-key-change-me"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new("sub", u.Id.ToString("N")),
            new(ClaimTypes.Name, u.DisplayName),
            new(ClaimTypes.Email, u.Email),
        };
            foreach (var r in u.Roles) claims.Add(new(ClaimTypes.Role, r));

            var token = new JwtSecurityToken(issuer, audience, claims,
                expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: creds);

            return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo));
        }
    }
}
