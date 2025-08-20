using MediatR;

namespace ContentService.Application.Contents.Command.Delete
{
    public sealed record DeleteContentCommand(Guid Id) : IRequest<Unit>;
}
