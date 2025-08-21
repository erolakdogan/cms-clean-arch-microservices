using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace Shared.Web.Security;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<JwtOptions>(section);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                using var scope = services.BuildServiceProvider();
                var opt = scope.GetRequiredService<IOptions<JwtOptions>>().Value;

                o.TokenValidationParameters = new()
                {
                    ValidIssuer = opt.Issuer,
                    ValidAudience = opt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.Key)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        var sub = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                               ?? ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        var roles = string.Join(",",
                            ctx.Principal?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
                            ?? Enumerable.Empty<string>());
                        Log.Information("JWT validated sub:{Sub} roles:{Roles}", sub, roles);
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        Log.Error(ctx.Exception, "JWT auth failed");
                        return Task.CompletedTask;
                    },
                    OnChallenge = ctx =>
                    {
                        // 401 olduğunda sebebi logla
                        Log.Warning("JWT challenge. error:{Error} desc:{Desc}",
                            ctx.Error, ctx.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnForbidden = ctx =>
                    {
                        Serilog.Log.Warning("JWT forbidden at {Path}. User:{User}",
                            ctx.HttpContext.Request.Path,
                            ctx.HttpContext.User?.Identity?.Name);
                        return Task.CompletedTask;
                    }
                };
            });

        // Politikalar – gerekirse
        services.AddAuthorization();

        return services;
    }
}
