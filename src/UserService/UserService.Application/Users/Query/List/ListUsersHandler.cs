using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Abstractions;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Queries;

public sealed class ListUsersHandler(IUserRepository repo, UsersMapper mapper)
    : IRequestHandler<ListUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(ListUsersQuery req, CancellationToken ct)
    {
        var page = Math.Max(1, req.Page);
        var size = Math.Clamp(req.PageSize, 1, 100);

        var q = repo.Query();            // repo.Query() EF tarafında NoTracking
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var s = req.Search.Trim();
            q = q.Where(u => u.Email.Contains(s) || u.DisplayName.Contains(s));
        }

        var total = await q.LongCountAsync(ct);
        var list = await q.OrderByDescending(u => u.CreatedAt)
                           .Skip((page - 1) * size)
                           .Take(size)
                           .ToListAsync(ct);

        var items = mapper.ToDtoList(list);
        return new PagedResult<UserDto>(items, page, size, total);
    }
}
