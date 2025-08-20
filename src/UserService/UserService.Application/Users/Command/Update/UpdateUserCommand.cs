using MediatR;

namespace UserService.Application.Users.Command.Update
{
    public sealed record UpdateUserCommand(Guid Id, string DisplayName, string[] Roles)
    : IRequest<UserDto>;
}
