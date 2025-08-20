using MediatR;

namespace ContentService.Application.Common.Caching
{
    public interface ICacheableQuery<TResponse> : IRequest<TResponse>
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; } 
    }
}
