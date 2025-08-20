using ContentService.Domain.Entities;

namespace ContentService.Api.Contracts.Contents
{
    public sealed record ContentCreateRequest(string Title, string Body, Guid AuthorId, string? Slug);
    public sealed record ContentUpdateRequest(string Title, string Body, string Status, string? Slug);
    public sealed record ContentResponse(
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
