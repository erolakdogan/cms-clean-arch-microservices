using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Infrastructure.Persistence;

public sealed class UserApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IContainer? _postgres;
    private string? _db;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Db"] = _db,
                ["Caching:Enabled"] = "false",
                ["Loki:Url"] = "",

                ["Jwt:Issuer"] = "cmspoc",
                ["Jwt:Audience"] = "cmspoc.clients",
                ["Jwt:Key"] = "P@ssw0rd!_DevOnly_Key_ChangeMe_1234567890abcd",
                ["Jwt:AccessTokenMinutes"] = "60",

                // seed hosted service Development'ta çalışıyor; ekstra bir şey yapmaya gerek yok
            };
            cfg.AddInMemoryCollection(dict!);
        });

        builder.ConfigureTestServices(services =>
        {
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            db.Database.Migrate();
        });
    }

    public async Task InitializeAsync()
    {
        _postgres = new ContainerBuilder()
            .WithImage("postgres:16")
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "users_it")
            .WithPortBinding(0, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await _postgres.StartAsync();

        var host = _postgres.Hostname;
        var port = _postgres.GetMappedPublicPort(5432);
        _db = $"Host={host};Port={port};Database=users_it;Username=postgres;Password=postgres;Pooling=true";
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null) await _postgres.StopAsync();
    }
}
