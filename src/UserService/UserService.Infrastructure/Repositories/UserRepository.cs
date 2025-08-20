
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Abstractions;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories
{
    public sealed class UserRepository(UserDbContext db) : IUserRepository
    {
        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await db.Users.FindAsync([id], ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);

        public async Task AddAsync(User user, CancellationToken ct = default)
            => await db.Users.AddAsync(user, ct);

        public void Remove(User user) => db.Users.Remove(user);

        public IQueryable<User> Query() => db.Users.AsNoTracking();
    }
}
