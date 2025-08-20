using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserService.Application.Abstractions;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Serilog (console)
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Controllers + JSON seçenekleri
builder.Services.AddControllers()
    .AddJsonOptions(o => { /* future: json options */ });

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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// ProblemDetails (yerleşik)
builder.Services.AddProblemDetails(opt =>
{
    // İleride detaylandırırız; env'e göre davranış
});


builder.Services.AddDbContext<UserDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Db");
    opt.UseNpgsql(cs).UseSnakeCaseNamingConvention();
    opt.EnableSensitiveDataLogging();
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await db.Database.MigrateAsync();
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
