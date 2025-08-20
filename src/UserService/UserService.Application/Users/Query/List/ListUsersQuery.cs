using MediatR;

namespace UserService.Application.Users.Query.List
{
    public sealed record ListUsersQuery(int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<UserDto>>;
}
