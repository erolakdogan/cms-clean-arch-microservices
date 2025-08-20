using MediatR;
using UserService.Application.Common.Caching;

namespace UserService.Application.Users.Commands;

public sealed record UpdateUserCommand(
    Guid Id,
    string? Email,
    string? Password,
    string? DisplayName,
    string[]? Roles
) : IRequest, ICacheInvalidator
{
    public string[] PrefixesToInvalidate => new[]
    {
        $"{UserCacheKeys.UsersByIdPrefix}{Id:N}",
        UserCacheKeys.UsersListPrefix
    };
}
