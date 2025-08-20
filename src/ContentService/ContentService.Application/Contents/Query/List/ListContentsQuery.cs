using MediatR;
using ContentService.Application.Common.Caching;
using ContentService.Application.Common.Models;

namespace ContentService.Application.Contents.Queries;

public sealed record ListContentsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<PagedResult<ContentDto>>, ICacheableQuery<PagedResult<ContentDto>>
{
    public string CacheKey => $"{ContentCacheKeys.ContentsListPrefix}p{Page}:ps{PageSize}:q{(Search?.Trim().ToLower() ?? "-")}";
    public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
}
