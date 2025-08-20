using ContentService.Domain.Entities;

namespace ContentService.Application.Common.Abstractions
{
    public interface IContentRepository
    {
        Task<Content?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Content?> GetBySlugAsync(string slug, CancellationToken ct = default);
        Task AddAsync(Content content, CancellationToken ct = default);
        void Remove(Content content);
        IQueryable<Content> Query(); // list/sorgular (çoğunlukla AsNoTracking)
        void Update(Content entity);
    }
}
