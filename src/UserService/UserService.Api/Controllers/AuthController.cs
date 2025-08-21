using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Web.Security;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
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
    public sealed class AuthController(IUserRepository repo,IPasswordHasherService hasher,IJwtTokenService jwt) : ControllerBase
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
            var user = await repo.Query().SingleOrDefaultAsync(u => u.Email == req.Email, ct);
            if (user is null || !hasher.Verify(user.PasswordHash, req.Password))
                return Unauthorized();

            Log.Information("Issuing token for {Email} with roles {Roles}", user.Email, string.Join(",", user.Roles));

            var token = jwt.CreateUserToken(user.Id, user.Email, user.DisplayName, user.Roles);
            return Ok(new LoginResponse (token, "Bearer", 60 * 30 ));
        }
    }
}
