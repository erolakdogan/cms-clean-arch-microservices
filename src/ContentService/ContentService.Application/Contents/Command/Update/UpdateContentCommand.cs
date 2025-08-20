using MediatR;

namespace ContentService.Application.Contents.Command.Update
{
    public sealed record UpdateContentCommand(
    Guid Id,
    string Title,
    string Body,
    string Status,
    string? Slug
) : IRequest<ContentDto>;
}
