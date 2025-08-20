using MediatR;
using UserService.Application.Common.Caching;

namespace UserService.Application.Users.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto>, ICacheableQuery<UserDto>
{
    public string CacheKey => $"{UserCacheKeys.UsersByIdPrefix}{Id:N}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
}
