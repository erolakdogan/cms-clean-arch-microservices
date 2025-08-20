using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration cfg, string section = "Redis")
        {
            services.Configure<RedisCacheOptions>(cfg.GetSection(section));
            services.AddSingleton<ICacheService, RedisCacheService>(); 
            return services;
        }
    }
}
