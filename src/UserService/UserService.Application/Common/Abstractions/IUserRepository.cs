using UserService.Domain.Entities;

namespace UserService.Application.Common.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        void Remove(User user);
        IQueryable<User> Query(); // çoğunlukla AsNoTracking
    }
}
