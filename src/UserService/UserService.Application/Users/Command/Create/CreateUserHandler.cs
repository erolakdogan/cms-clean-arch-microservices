using MediatR;
using UserService.Application.Common.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Application.Users.Commands;

public sealed class CreateUserHandler(
    IUserRepository repo,
    IUnitOfWork uow,
    IPasswordHasherService hasher)
    : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand req, CancellationToken ct)
    {
        var entity = new User
        {
            Email = req.Email.Trim(),
            PasswordHash = hasher.Hash(req.Password),
            DisplayName = req.DisplayName.Trim(),
            Roles = req.Roles ?? Array.Empty<string>()
        };

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}
