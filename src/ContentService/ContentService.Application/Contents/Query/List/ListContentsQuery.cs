using MediatR;

namespace ContentService.Application.Contents.Query.List
{
    public sealed record ListContentsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,       // "Draft|Published|Archived"
    Guid? AuthorId = null,
    string? Search = null        // title contains
) : IRequest<IReadOnlyList<ContentDto>>;
}
