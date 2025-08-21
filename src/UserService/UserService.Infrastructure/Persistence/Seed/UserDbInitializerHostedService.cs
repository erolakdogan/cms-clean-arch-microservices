using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UserService.Infrastructure.Persistence.Seed
{
    public sealed class UserDbInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<UserDbInitializerHostedService> _log;

        public UserDbInitializerHostedService(IServiceProvider services, ILogger<UserDbInitializerHostedService> log)
        {
            _services = services;
            _log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("UserDbInitializerHostedService starting…");
            await UserDbSeeder.MigrateAndSeedAsync(_services);
            _log.LogInformation("UserDbInitializerHostedService completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
