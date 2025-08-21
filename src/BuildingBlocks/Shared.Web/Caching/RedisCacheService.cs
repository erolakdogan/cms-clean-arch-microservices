using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Shared.Web.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _mux;
    private readonly ILogger<RedisCacheService> _log;
    private readonly RedisCacheOptions _opt;

    public RedisCacheService(IConnectionMultiplexer mux, IOptions<RedisCacheOptions> opt, ILogger<RedisCacheService> log)
    {
        _mux = mux;
        _opt = opt.Value;
        _log = log;
    }

    private IDatabase Db => _mux.GetDatabase();

    private string K(string key) => string.Concat(_opt.InstanceName, key);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var v = await Db.StringGetAsync(K(key));
        if (v.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(v!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        var expiry = ttl ?? TimeSpan.FromSeconds(_opt.DefaultTtlSeconds);
        await Db.StringSetAsync(K(key), json, expiry);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => Db.KeyDeleteAsync(K(key));

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        // Demo amaçlı SCAN; prod’da keyspace tarama pahalıdır.
        var endpoints = _mux.GetEndPoints();
        foreach (var ep in endpoints)
        {
            var server = _mux.GetServer(ep);
            var pattern = K(prefix) + "*";
            var keys = server.Keys(pattern: pattern);
            foreach (var k in keys)
                await Db.KeyDeleteAsync(k);
        }
    }
}
