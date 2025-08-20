using MediatR;
using UserService.Application.Common.Caching;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Queries;

public sealed record ListUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<PagedResult<UserDto>>, ICacheableQuery<PagedResult<UserDto>>
{
    public string CacheKey => $"{UserCacheKeys.UsersListPrefix}p{Page}:ps{PageSize}:q{(Search?.Trim().ToLower() ?? "-")}";
    public TimeSpan? Expiration => TimeSpan.FromSeconds(30);
}
