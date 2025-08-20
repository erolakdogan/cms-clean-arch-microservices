using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.Web.Caching;
using Shared.Web.Middleware;
using Shared.Web.Security;
using System.Security.Claims;
using System.Text.Json.Serialization;
using UserService.Application.Common.Abstractions;
using UserService.Application.Common.Behaviors;
using UserService.Application.Users;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// API Versioning
builder.Services
    .AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("x-api-version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddApiExplorer(opt =>
    {
        opt.GroupNameFormat = "'v'VVV";
        opt.SubstituteApiVersionInUrl = true;
    });

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPasswordHasherService, IdentityPasswordHasherService>();
builder.Services.AddScoped<UsersMapper>();

// DbContext
builder.Services.AddDbContext<UserDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Db");
    opt.UseNpgsql(cs).UseSnakeCaseNamingConvention();
#if DEBUG
    opt.EnableSensitiveDataLogging();
#endif
});

// MediatR + Validation + Pipelines
var appAssembly = typeof(UsersMapper).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(appAssembly));
builder.Services.AddValidatorsFromAssembly(appAssembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Redis cache + behaviors
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));

// ProblemDetails, Health, Swagger
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer — sadece token'ı gir (\"Bearer\" yazma).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
Serilog.Log.Information("JWT cfg present:{Present} issuer:{Issuer} audience:{Audience} keyLen:{Len}",
    jwtSection.Exists(),
    jwtSection["Issuer"],
    jwtSection["Audience"],
    System.Text.Encoding.UTF8.GetByteCount(jwtSection["Key"] ?? ""));

builder.Services.AddJwtAuth(builder.Configuration.GetSection("Jwt"));
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// Dev: migrate + seed admin
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var sp = scope.ServiceProvider;

    var db = sp.GetRequiredService<UserDbContext>();
    await db.Database.MigrateAsync();

    var repo = sp.GetRequiredService<IUserRepository>();
    var uow = sp.GetRequiredService<IUnitOfWork>();
    var hasher = sp.GetRequiredService<IPasswordHasherService>();

    var email = "admin@cms.local";
    if (!await repo.Query().AnyAsync(u => u.Email == email))
    {
        await repo.AddAsync(new User
        {
            Email = email,
            PasswordHash = hasher.Hash("P@ssw0rd!"),
            DisplayName = "Administrator",
            Roles = new[] { "Admin" }
        });
        await uow.SaveChangesAsync();
    }
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        if (http.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var cid))
            diag.Set("CorrelationId", cid.ToString());
        diag.Set("UserId", http.User?.FindFirst("sub")?.Value);
        diag.Set("Path", http.Request.Path);
        var roles = string.Join(",", http.User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value) ?? []);
        diag.Set("Roles", roles);
    };
});

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready" }));
app.MapGet("/ping", () => Results.Ok("pong"));

app.Run();
