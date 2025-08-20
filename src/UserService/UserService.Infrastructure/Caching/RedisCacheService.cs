using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UserService.Application.Common.Abstractions;

namespace UserService.Infrastructure.Caching
{
    public sealed class RedisCacheService(IDistributedCache cache) : ICacheService
    {
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var bytes = await cache.GetAsync(key, ct);
            if (bytes is null) return default;
            return JsonSerializer.Deserialize<T>(bytes, _json);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
            var opt = new DistributedCacheEntryOptions();
            if (ttl.HasValue) opt.SetAbsoluteExpiration(ttl.Value);
            await cache.SetAsync(key, bytes, opt, ct);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => cache.RemoveAsync(key, ct);
    }
}
