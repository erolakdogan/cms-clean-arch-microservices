using MediatR;


namespace UserService.Application.Users.Command.Create
{
    public sealed record CreateUserCommand(string Email, string Password, string DisplayName, string[] Roles)
        : IRequest<UserDto>;
}
