using Asp.Versioning;
using ContentService.Application.Common.Abstractions;
using ContentService.Application.Common.Behaviors;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Command.Create;
using ContentService.Infrastructure.Persistence;
using ContentService.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// Application assembly'ini strongly-typed al
var appAssembly = typeof(CreateContentCommand).Assembly;

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(appAssembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(appAssembly);

// Pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Mapperly mapper'ı DI
builder.Services.AddScoped<ContentMapper>();

// Serilog (console)
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();
});

// Controllers + JSON seçenekleri
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// API Versioning (v1 default)
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

builder.Services.AddDbContext<ContentDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Db");
    opt.UseNpgsql(cs).UseSnakeCaseNamingConvention();
#if DEBUG
    opt.EnableSensitiveDataLogging();
#endif
});

builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// ProblemDetails (yerleşik)
builder.Services.AddProblemDetails(opt =>
{
    // İleride detaylandırırız; env'e göre davranış
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    await db.Database.MigrateAsync();
    await Seed.EnsureAsync(db);
}

// Exception handling + ProblemDetails
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(); // otomatik ProblemDetails üretir
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    // Çoklu versiyon senaryosunda explorer'dan gruplar eklenir
});

// Routing + auth middlewares (ileride eklenecek)
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/ready", () => Results.Ok(new { status = "ready" }));

// Basit ping 
app.MapGet("/ping", () => Results.Ok("pong"));
app.Run();
