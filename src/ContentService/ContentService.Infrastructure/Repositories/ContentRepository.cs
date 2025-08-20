using ContentService.Application.Common.Abstractions;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace ContentService.Infrastructure.Repositories
{
    public sealed class ContentRepository(ContentDbContext db) : IContentRepository
    {
        public async Task<Content?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await db.Contents.FindAsync([id], ct);

        public async Task<Content?> GetBySlugAsync(string slug, CancellationToken ct = default)
            => await db.Contents.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug, ct);

        public async Task AddAsync(Content content, CancellationToken ct = default)
            => await db.Contents.AddAsync(content, ct);

        public void Remove(Content content) => db.Contents.Remove(content);

        public IQueryable<Content> Query() => db.Contents.AsNoTracking();
    }
}
