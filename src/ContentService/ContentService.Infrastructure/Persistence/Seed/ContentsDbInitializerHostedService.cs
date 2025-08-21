using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContentService.Infrastructure.Persistence.Seed
{
    public sealed class ContentsDbInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ContentsDbInitializerHostedService> _log;

        public ContentsDbInitializerHostedService(IServiceProvider services, ILogger<ContentsDbInitializerHostedService> log)
        {
            _services = services;
            _log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("ContentsDbInitializerHostedService starting…");
            await ContentsDbSeeder.MigrateAndSeedAsync(_services);
            _log.LogInformation("ContentsDbInitializerHostedService completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
