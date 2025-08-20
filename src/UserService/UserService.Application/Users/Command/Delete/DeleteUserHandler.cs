using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Command.Delete
{
    public sealed class DeleteUserHandler(IUserRepository repo, IUnitOfWork uow)
    : IRequestHandler<DeleteUserCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteUserCommand req, CancellationToken ct)
        {
            var deleteUserItem = await repo.GetByIdAsync(req.Id, ct) ?? throw new KeyNotFoundException("User not found.");
            repo.Remove(deleteUserItem);
            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
