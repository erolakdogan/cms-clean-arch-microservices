namespace Shared.Web.Caching
{
    public sealed class RedisCacheOptions
    {
        public string Connection { get; set; } = "localhost:6379";
        public string? Instance { get; set; } 
        public int ScanPageSize { get; set; } = 500; 
    }
}
