using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Query.GetById
{
    public sealed class GetUserByIdHandler(IUserRepository repo, UsersMapper mapper)
    : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        public async Task<UserDto?> Handle(GetUserByIdQuery req, CancellationToken ct)
        {
            var u = await repo.GetByIdAsync(req.Id, ct);
            return u is null ? null : mapper.ToDto(u);
        }
    }
}
