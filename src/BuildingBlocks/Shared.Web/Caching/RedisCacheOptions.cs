namespace Shared.Web.Caching
{
    public sealed class RedisCacheOptions
    {
        public string ConnectionString { get; init; } = string.Empty;
        public string InstanceName { get; init; } = "cmspoc:";
        public int DefaultTtlSeconds { get; init; } = 60;
    }
}
