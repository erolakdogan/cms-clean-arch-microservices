using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;
using ContentService.Application.Common.Abstractions;
using ContentService.Application.Common.Behaviors;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Commands;
using ContentService.Application.UsersExternal;
using ContentService.Infrastructure.Persistence;
using ContentService.Infrastructure.Repositories;
using ContentService.Infrastructure.UsersExternal;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Shared.Web.Caching;
using Shared.Web.Middleware;
using Shared.Web.Security;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("service", ctx.HostingEnvironment.ApplicationName)
      .Enrich.WithProperty("env", ctx.HostingEnvironment.EnvironmentName)
      .WriteTo.Console();

    var lokiUrl = ctx.Configuration["Loki:Url"];
    if (!string.IsNullOrWhiteSpace(lokiUrl))
    {
        lc.WriteTo.GrafanaLoki(lokiUrl, labels: new[]
        {
            new Serilog.Sinks.Grafana.Loki.LokiLabel { Key = "app", Value = ctx.HostingEnvironment.ApplicationName },
            new Serilog.Sinks.Grafana.Loki.LokiLabel { Key = "env", Value = ctx.HostingEnvironment.EnvironmentName }
        });
    }
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
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ContentMapper>();

// DbContext
builder.Services.AddDbContext<ContentDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Db");
    opt.UseNpgsql(cs).UseSnakeCaseNamingConvention();
#if DEBUG
    opt.EnableSensitiveDataLogging();
#endif
});

// MediatR + Validation + Pipelines
var appAssembly = typeof(CreateContentCommand).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(appAssembly));
builder.Services.AddValidatorsFromAssembly(appAssembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Redis cache + behaviors
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));

// UsersClient + Options
builder.Services.Configure<UsersClientOptions>(builder.Configuration.GetSection("Users"));
builder.Services.AddSingleton<IServiceTokenProvider, ServiceTokenProvider>();

// Polly policies (local functions)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    var jitter = new Random();
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => (int)r.StatusCode is >= 500 or 408)
        .WaitAndRetryAsync(3, retry =>
            TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry))
          + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
}
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => (int)r.StatusCode is >= 500 or 408)
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(20));
}

// Typed HttpClient (UserService)
builder.Services.AddHttpClient<IUsersClient, UsersClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UsersClientOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3)));

// Named HttpClient (proxy login için)
builder.Services.AddHttpClient("UsersAuth", (sp, http) =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UsersClientOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3)));

// ProblemDetails, Health, Swagger
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Environment.ApplicationName, // "UserService" / "ContentService"
        Version = "v1"
    });

    // XML yorumlarını dahil et (xml dosyası varsa)
    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xml);
    if (System.IO.File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

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

// Dev: migrate
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    await db.Database.MigrateAsync();
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

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(opt =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        opt.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
            $"{builder.Environment.ApplicationName} {desc.ApiVersion}");
    }

    opt.RoutePrefix = "swagger";
    opt.DocumentTitle = builder.Environment.ApplicationName;
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready" }));
app.MapGet("/ping", () => Results.Ok("pong"));

app.Run();
