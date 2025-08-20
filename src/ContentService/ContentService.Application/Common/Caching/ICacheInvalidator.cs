namespace ContentService.Application.Common.Caching
{
    public interface ICacheInvalidator
    {
        string[] PrefixesToInvalidate { get; }
    }
}
