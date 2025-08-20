using UserService.Application.Abstractions;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories
{
    public sealed class UnitOfWork(UserDbContext db) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    }
}
