using ContentService.Application.Common.Abstractions;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Application.Contents.Command.Update
{
    public sealed class UpdateContentHandler(IContentRepository repo, IUnitOfWork uow, ContentMapper mapper)
     : IRequestHandler<UpdateContentCommand, ContentDto>
    {
        public async Task<ContentDto> Handle(UpdateContentCommand req, CancellationToken ct)
        {
            var e = await repo.Query().FirstOrDefaultAsync(x => x.Id == req.Id, ct);
            if (e is null) throw new KeyNotFoundException("Content not found.");

            e.Title = req.Title;
            e.Body = req.Body;
            e.Status = Enum.Parse<ContentStatus>(req.Status, true);
            e.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(req.Slug))
            {
                var newSlug = req.Slug.Trim().ToLowerInvariant();
                if (newSlug != e.Slug)
                    e.Slug = await MakeUniqueSlugAsync(newSlug, e.Id, ct);
            }

            await uow.SaveChangesAsync(ct);
            return mapper.ToDto(e);
        }

        private async Task<string> MakeUniqueSlugAsync(string baseSlug, Guid currentId, CancellationToken ct)
        {
            var slug = baseSlug; var i = 2;
            while (await repo.Query().AnyAsync(x => x.Slug == slug && x.Id != currentId, ct))
                slug = $"{baseSlug}-{i++}";
            return slug;
        }
    }
}
