using ContentService.Application.Common.Abstractions;
using ContentService.Infrastructure.Persistence;


namespace ContentService.Infrastructure.Repositories
{
    public sealed class UnitOfWork(ContentDbContext db) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    }
}
