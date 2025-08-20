using MediatR;
using ContentService.Application.Common.Caching;

namespace ContentService.Application.Contents.Query.GetById
{
    public sealed record GetContentByIdQuery(Guid Id) : IRequest<ContentDto>, ICacheableQuery<ContentDto>
    {
        public string CacheKey => $"{ContentCacheKeys.ContentsByIdPrefix}{Id:N}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
    }
}
