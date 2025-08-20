using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Web.Caching;
using UserService.Application.Common.Caching;

namespace UserService.Application.Common.Behaviors
{
    public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cache,
    ILogger<CachingBehavior<TRequest, TResponse>> log)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var key = request.CacheKey;
            var hit = await cache.GetAsync<TResponse>(key, ct);
            if (hit is not null)
            {
                log.LogDebug("Cache HIT: {Key}", key);
                return hit;
            }

            var resp = await next();
            await cache.SetAsync(key, resp, request.Expiration ?? TimeSpan.FromSeconds(60), ct);
            log.LogDebug("Cache SET: {Key}", key);
            return resp;
        }
    }
}
