using MediatR;
using UserService.Application.Common.Caching;

namespace UserService.Application.Users.Commands;

public sealed record DeleteUserCommand(Guid Id) : IRequest, ICacheInvalidator
{
    public string[] PrefixesToInvalidate => new[]
    {
        $"{UserCacheKeys.UsersByIdPrefix}{Id:N}",
        UserCacheKeys.UsersListPrefix
    };
}
