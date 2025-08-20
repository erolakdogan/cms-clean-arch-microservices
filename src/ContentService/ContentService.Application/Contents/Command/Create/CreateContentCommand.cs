using ContentService.Application.Common.Caching;
using ContentService.Domain.Entities;
using MediatR;

namespace ContentService.Application.Contents.Commands;

public sealed record CreateContentCommand(
    string Title,
    string Body,
    Guid AuthorId,
    string? Slug,
    ContentStatus? Status = null
) : IRequest<Guid>, ICacheInvalidator
{
    public string[] PrefixesToInvalidate => new[] { ContentCacheKeys.ContentsListPrefix };
}
