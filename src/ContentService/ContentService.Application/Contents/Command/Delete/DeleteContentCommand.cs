using MediatR;
using ContentService.Application.Common.Caching;

namespace ContentService.Application.Contents.Commands;

public sealed record DeleteContentCommand(Guid Id) : IRequest, ICacheInvalidator
{
    public string[] PrefixesToInvalidate => new[]
    {
        $"{ContentCacheKeys.ContentsByIdPrefix}{Id:N}",
        ContentCacheKeys.ContentsListPrefix
    };
}
