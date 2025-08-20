using MediatR;

namespace ContentService.Application.Contents.Command.Create
{

    public sealed record CreateContentCommand(string Title, string Body, Guid AuthorId, string? Slug)
        : IRequest<ContentDto>;
}
