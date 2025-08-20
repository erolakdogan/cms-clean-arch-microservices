using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Web.Caching
{
    public sealed class RedisCacheService : ICacheService, IAsyncDisposable
    {
        private readonly ILogger<RedisCacheService> _log;
        private readonly RedisCacheOptions _opt;
        private readonly ConnectionMultiplexer _muxer;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        private readonly string _ns;

        public RedisCacheService(IOptions<RedisCacheOptions> options, ILogger<RedisCacheService> log)
        {
            _log = log;
            _opt = options.Value;
            _muxer = ConnectionMultiplexer.Connect(_opt.Connection);
            _db = _muxer.GetDatabase();
            _ns = string.IsNullOrWhiteSpace(_opt.Instance) ? "" : $"{_opt.Instance}:";
        }

        private string K(string key) => $"{_ns}{key}";

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var v = await _db.StringGetAsync(K(key));
            if (v.IsNullOrEmpty) return default;
            try { return JsonSerializer.Deserialize<T>(v!, _json); }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Redis deserialization failed for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            var payload = JsonSerializer.Serialize(value, _json);
            await _db.StringSetAsync(K(key), payload, ttl);
        }

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => _db.KeyDeleteAsync(K(key));

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            var server = _muxer.GetServers().First();
            var full = K(prefix);
            var keys = server.Keys(pattern: $"{full}*", pageSize: _opt.ScanPageSize);
            var tasks = new List<Task>();
            foreach (var k in keys) tasks.Add(_db.KeyDeleteAsync(k));
            await Task.WhenAll(tasks);
        }

        public async ValueTask DisposeAsync() => await _muxer.CloseAsync();
    }
}
