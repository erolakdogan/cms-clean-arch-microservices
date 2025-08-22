using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Commands;

public sealed class DeleteUserHandler(IUserRepository repo, IUnitOfWork uow)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand req, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(req.Id, ct); 
        if (user is null)
            throw new KeyNotFoundException($"User '{req.Id}' not found.");

        repo.Remove(user);
        await uow.SaveChangesAsync(ct);
    }
}
