using Asp.Versioning;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog (console)
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();
});

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

var app = builder.Build();

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
