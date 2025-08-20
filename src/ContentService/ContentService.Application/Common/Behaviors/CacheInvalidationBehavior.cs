using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Web.Caching;
using ContentService.Application.Common.Caching;

namespace ContentService.Application.Common.Behaviors
{
    public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
     ICacheService cache,
     ILogger<CacheInvalidationBehavior<TRequest, TResponse>> log)
     : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var resp = await next();

            if (request is ICacheInvalidator inv && inv.PrefixesToInvalidate.Length > 0)
            {
                foreach (var p in inv.PrefixesToInvalidate.Distinct())
                {
                    await cache.RemoveByPrefixAsync(p, ct);
                    log.LogDebug("Cache INVALIDATE: {Prefix}", p);
                }
            }
            return resp;
        }
    }
}
