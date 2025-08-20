using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Application.Users.Command.Create
{
    public sealed class CreateUserHandler(IUserRepository repo, IUnitOfWork uow, UsersMapper mapper)
    : IRequestHandler<CreateUserCommand, UserDto>
    {
        public async Task<UserDto> Handle(CreateUserCommand req, CancellationToken ct)
        {
            var exists = await repo.Query().AnyAsync(u => u.Email == req.Email, ct);
            if (exists) throw new InvalidOperationException("Email already exists.");

            var user = new User
            {
                Email = req.Email,
                PasswordHash = "PLACEHOLDER",
                DisplayName = req.DisplayName,
                Roles = req.Roles
            };

            await repo.AddAsync(user, ct);
            await uow.SaveChangesAsync(ct);

            return mapper.ToDto(user);
        }
    }
}
