using ContentService.Domain.Entities;

namespace ContentService.Application.Contents
{
    public sealed record ContentDto(
    Guid Id,
    string Title,
    string Body,
    Guid AuthorId,
    ContentStatus Status,
    string Slug,
    DateTime CreatedAt,
    DateTime? UpdatedAt
    );
}
