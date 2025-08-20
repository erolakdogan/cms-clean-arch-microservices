using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Web.Security;

public static class JwtServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfigurationSection section)
    {
        // 1) Konfigi oku VE options olarak kaydet
        var jwt = section.Get<JwtOptions>() ?? new JwtOptions();
        services.Configure<JwtOptions>(section);

        if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("Jwt:Key must be configured and >= 32 bytes.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

        // 2) Auth + Bearer
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                // roles/role claim mapping (uyumluluk)
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal?.Identity is ClaimsIdentity id)
                        {
                            var rolesArr = ctx.Principal.Claims.Where(c => c.Type == "roles").Select(c => c.Value);
                            foreach (var r in rolesArr) id.AddClaim(new Claim(ClaimTypes.Role, r));

                            var singleRole = ctx.Principal.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                            if (!string.IsNullOrWhiteSpace(singleRole))
                                id.AddClaim(new Claim(ClaimTypes.Role, singleRole));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
