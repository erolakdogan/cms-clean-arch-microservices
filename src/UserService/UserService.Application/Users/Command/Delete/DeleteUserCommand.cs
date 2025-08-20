using MediatR;

namespace UserService.Application.Users.Command.Delete
{
    public sealed record DeleteUserCommand(Guid Id) : IRequest<Unit>;
}
