using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Commands;

public sealed class UpdateUserHandler(
    IUserRepository repo,
    IUnitOfWork uow,
    IPasswordHasherService hasher)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand req, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(req.Id, ct); // Query() yerine
        if (user is null)
            throw new KeyNotFoundException($"User '{req.Id}' not found.");

        if (!string.IsNullOrWhiteSpace(req.Email))
            user.Email = req.Email.Trim();

        if (!string.IsNullOrWhiteSpace(req.Password))
            user.PasswordHash = hasher.Hash(req.Password);

        if (!string.IsNullOrWhiteSpace(req.DisplayName))
            user.DisplayName = req.DisplayName.Trim();

        if (req.Roles is not null)
            user.Roles = req.Roles;

        repo.Update(user);
        await uow.SaveChangesAsync(ct);
    }
}
