using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Command.Delete
{
    public sealed class DeleteUserHandler(IUserRepository repo, IUnitOfWork uow)
    : IRequestHandler<DeleteUserCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteUserCommand req, CancellationToken ct)
        {
            var u = await repo.GetByIdAsync(req.Id, ct) ?? throw new KeyNotFoundException("User not found.");
            repo.Remove(u);
            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
