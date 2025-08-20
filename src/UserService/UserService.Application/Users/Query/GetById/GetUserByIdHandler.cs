using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Query.GetById
{
    public sealed class GetUserByIdHandler(IUserRepository repo, UsersMapper mapper)
    : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        public async Task<UserDto?> Handle(GetUserByIdQuery req, CancellationToken ct)
        {
            var userItem = await repo.GetByIdAsync(req.Id, ct);
            return userItem is null ? null : mapper.ToDto(userItem);
        }
    }
}
