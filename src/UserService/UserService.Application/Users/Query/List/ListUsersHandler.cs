using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Query.List
{
    public sealed class ListUsersHandler(IUserRepository repo, UsersMapper mapper)
    : IRequestHandler<ListUsersQuery, IReadOnlyList<UserDto>>
    {
        public async Task<IReadOnlyList<UserDto>> Handle(ListUsersQuery req, CancellationToken ct)
        {
            var page = Math.Max(1, req.Page);
            var size = Math.Clamp(req.PageSize, 1, 100);

            var list = await repo.Query()
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return mapper.ToDtoList(list);
        }
    }
}
