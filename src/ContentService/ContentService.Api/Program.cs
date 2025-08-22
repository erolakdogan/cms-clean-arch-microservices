using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using ContentService.Application.Common.Abstractions;
using ContentService.Application.Common.Behaviors;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Commands;
using ContentService.Application.UsersExternal;
using ContentService.Infrastructure.Persistence;
using ContentService.Infrastructure.Persistence.Seed;
using ContentService.Infrastructure.Repositories;
using ContentService.Infrastructure.UsersExternal;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using Shared.Web.Caching;
using Shared.Web.Middleware;
using Shared.Web.Security;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging (Serilog + optional Loki) ----------
Serilog.Debugging.SelfLog.Enable(m => Console.Error.WriteLine($"[Serilog] {m}"));
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.MinimumLevel.Information()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("System", LogEventLevel.Warning)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("service", ctx.HostingEnvironment.ApplicationName)
      .Enrich.WithProperty("env", ctx.HostingEnvironment.EnvironmentName)
      .WriteTo.Console();

    var lokiUrl = ctx.Configuration["Loki:Url"]; // compose: http://loki:3100
    if (!string.IsNullOrWhiteSpace(lokiUrl))
    {
        lc.WriteTo.GrafanaLoki(
            lokiUrl,
            labels: new[]
            {
                new LokiLabel { Key = "app", Value = ctx.HostingEnvironment.ApplicationName },
                new LokiLabel { Key = "env", Value = ctx.HostingEnvironment.EnvironmentName }
            },
            restrictedToMinimumLevel: LogEventLevel.Information
        );
    }
});

// ---------- MVC + JSON ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---------- API Versioning + Explorer ----------
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

// ---------- Health/ProblemDetails/Swagger ----------
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = builder.Environment.ApplicationName, Version = "v1" });

    var xml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer token giriniz.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

// ---------- DbContext ----------
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDb");
builder.Services.AddDbContext<ContentDbContext>(opt =>
{
    if (useInMemory || builder.Environment.IsEnvironment("Testing"))
    {
        opt.UseInMemoryDatabase("contents-tests");
    }
    else
    {
        var cs = builder.Configuration.GetConnectionString("Db");
        opt.UseNpgsql(cs).UseSnakeCaseNamingConvention();
    }
#if DEBUG
    opt.EnableSensitiveDataLogging();
#endif
});


// ---------- DI (Repos/UoW/Mapper) ----------
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ContentMapper>();

// ---------- MediatR + Validation + Behaviors ----------
var appAssembly = typeof(CreateContentCommand).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(appAssembly));
builder.Services.AddValidatorsFromAssembly(appAssembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ---------- Cache (Redis) ----------
builder.Services.AddRedisCache(builder.Configuration);

// ---------- Security (JWT) ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
Log.Information("JWT Issuer:{Issuer} Audience:{Audience} KeyLen:{Len}",
    jwtSection["Issuer"], jwtSection["Audience"],
    (jwtSection["Key"] ?? "").Length);
builder.Services.AddJwtAuth(builder.Configuration.GetSection("Jwt"));

// UsersClient
builder.Services.Configure<UsersClientOptions>(builder.Configuration.GetSection("Users")); // Users:BaseUrl

// ---------- UsersClient + Polly ----------
builder.Services.Configure<UsersClientOptions>(builder.Configuration.GetSection("Users"));
builder.Services.AddHttpClient<IUsersClient, UsersClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UsersClientOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3)));

builder.Services.AddHttpClient("UsersAuth", (sp, http) =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UsersClientOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3)));

// ---------- Seed (HostedService ----------
builder.Services.AddHostedService<ContentsDbInitializerHostedService>();

var app = builder.Build();

// ---------- Middleware Pipeline ----------
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler();

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
        var roles = string.Join(",", http.User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value) ?? Array.Empty<string>());
        diag.Set("Roles", roles);
    };
});

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger(c => c.RouteTemplate = "swagger/{documentName}/swagger.json");
app.UseSwaggerUI(opt =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var desc in provider.ApiVersionDescriptions)
        opt.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"{builder.Environment.ApplicationName} {desc.ApiVersion}");
    opt.RoutePrefix = "swagger";
    opt.DocumentTitle = builder.Environment.ApplicationName;
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready" }));
app.MapGet("/ping", () => Results.Ok("pong"));

app.Run();

// ---------------- Local Polly policies ----------------
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

// WebApplicationFactory<T> için gerekli (top-level statements ile partial Program tanımı)
public partial class Program { }
