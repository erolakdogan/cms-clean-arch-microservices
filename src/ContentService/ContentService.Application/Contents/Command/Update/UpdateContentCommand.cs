using MediatR;
using ContentService.Application.Common.Caching;
using ContentService.Domain.Entities;

namespace ContentService.Application.Contents.Commands;

public sealed record UpdateContentCommand(
    Guid Id,
    string? Title,
    string? Body,
    Guid? AuthorId,
    string? Slug,
    ContentStatus? Status
) : IRequest, ICacheInvalidator
{
    public string[] PrefixesToInvalidate => new[]
    {
        $"{ContentCacheKeys.ContentsByIdPrefix}{Id:N}",
        ContentCacheKeys.ContentsListPrefix
    };
}
