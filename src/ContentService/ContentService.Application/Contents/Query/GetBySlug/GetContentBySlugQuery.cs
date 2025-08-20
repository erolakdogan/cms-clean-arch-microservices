using ContentService.Application.Common.Caching;
using MediatR;
using System.Linq.Dynamic.Core;

namespace ContentService.Application.Contents.Query.GetBySlug
{
    public sealed record GetContentBySlugQuery(string Slug) : IRequest<ContentDto?>, ICacheableQuery<ContentDto>
    {
        public string CacheKey => $"contents:slug:{Slug.Trim().ToLower()}";
        public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
    }
}
