using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Web.Security;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.Common.Abstractions;

namespace UserService.Api.Controllers
{
    /// <summary>
    /// Kimlik doğrulama uç noktaları.
    /// </summary>
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/auth")]
    [Produces("application/json")]
    [Tags("Kimlik Doğrulama")]
    public sealed class AuthController : ControllerBase
    {
        public sealed record LoginRequest(
            [Required, EmailAddress] string Email,
            [Required] string Password
        );

        public sealed record LoginResponse(
            string AccessToken,
            string TokenType,
            int ExpiresIn
        );

        private readonly IOptions<JwtOptions> _jwt;
        private readonly IUserRepository _repo;
        private readonly IPasswordHasherService _hasher;

        public AuthController(IOptions<JwtOptions> jwt, IUserRepository repo, IPasswordHasherService hasher)
        {
            _jwt = jwt;
            _repo = repo;
            _hasher = hasher;
        }

        /// <summary>Giriş yap ve JWT erişim anahtarı al.</summary>
        /// <remarks>
        /// Örnek istek:
        /// 
        ///     POST /api/v1/auth/login
        ///     {
        ///       "email": "admin@cms.local",
        ///       "password": "P@ssw0rd!"
        ///     }
        /// 
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "JWT token üretir", Description = "Geçerli e-posta/şifre ile oturum açıp erişim anahtarı döner.")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var user = await _repo.Query().FirstOrDefaultAsync(u => u.Email == req.Email, ct);
            if (user is null || !_hasher.Verify(user.PasswordHash, req.Password))
                return Unauthorized();

            var jwt = _jwt.Value;
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
            return Ok(new LoginResponse(accessToken, "Bearer", (int)TimeSpan.FromMinutes(jwt.AccessTokenMinutes).TotalSeconds));
        }
    }
}
