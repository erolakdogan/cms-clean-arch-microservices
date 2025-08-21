using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Shared.Web.Caching
{
    public static class RedisRegistrationExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<RedisCacheOptions>(cfg.GetSection("Redis"));
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
                var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("Redis");
                var cs = opt.ConnectionString?.Trim();

                if (string.IsNullOrWhiteSpace(cs))
                {
                    var host = Environment.GetEnvironmentVariable("Redis__Host")
                               ?? Environment.GetEnvironmentVariable("REDIS_HOST");
                    var port = Environment.GetEnvironmentVariable("Redis__Port")
                               ?? Environment.GetEnvironmentVariable("REDIS_PORT");

                    var inContainer = string.Equals(
                        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                        "true", StringComparison.OrdinalIgnoreCase);

                    host ??= inContainer ? "redis" : "localhost";
                    port ??= "6379";

                    cs = $"{host}:{port},abortConnect=false,connectRetry=5,connectTimeout=5000";
                    logger?.LogWarning("Redis:ConnectionString boş geldi; fallback kullanıldı: {ConnStr}", cs);
                }

                var conf = ConfigurationOptions.Parse(cs!, ignoreUnknown: true);
                conf.AbortOnConnectFail = false;    
                conf.ResolveDns = true;
                if (conf.ConnectTimeout < 5000) conf.ConnectTimeout = 5000;
                if (conf.ConnectRetry < 3) conf.ConnectRetry = 3;

                logger?.LogInformation("Redis bağlanıyor: {Endpoints}", string.Join(",", conf.EndPoints));
                var mux = ConnectionMultiplexer.Connect(conf);
                if (!mux.IsConnected)
                    logger?.LogWarning("Redis henüz bağlı değil; kitaplık otomatik yeniden deneyecektir.");

                return mux;
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }
    }
}
