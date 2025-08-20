using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using UserService.Application.Common.Abstractions;
using UserService.Application.Common.Behaviors;
using UserService.Application.Users;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
var appAssembly = typeof(UsersMapper).Assembly;

// Serilog (console)
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    Assembly.Load("UserService.Application")));
// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("UserService.Application"));
// Pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
// UsersMapper
builder.Services.AddScoped<UsersMapper>();


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

static Assembly GetAppAssembly(Assembly appAssembly)
{
    return appAssembly;
}
