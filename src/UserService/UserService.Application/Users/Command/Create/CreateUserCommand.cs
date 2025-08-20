using MediatR;
using UserService.Application.Common.Caching;

namespace UserService.Application.Users.Commands;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string DisplayName,
    string[] Roles
) : IRequest<Guid>, ICacheInvalidator
{
    public string[] PrefixesToInvalidate => new[] { UserCacheKeys.UsersListPrefix };
}
