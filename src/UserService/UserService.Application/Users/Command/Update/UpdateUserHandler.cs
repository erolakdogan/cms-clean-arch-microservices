using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Command.Update
{
    public sealed class DeleteUserHandler(IUserRepository repo, IUnitOfWork uow, UsersMapper mapper)
    : IRequestHandler<UpdateUserCommand, UserDto>
    {
        public async Task<UserDto> Handle(UpdateUserCommand req, CancellationToken ct)
        {
            var u = await repo.Query().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
            if (u is null) throw new KeyNotFoundException("User not found.");

            u.DisplayName = req.DisplayName;
            u.Roles = req.Roles;

            await uow.SaveChangesAsync(ct);
            return mapper.ToDto(u);
        }
    }
}
