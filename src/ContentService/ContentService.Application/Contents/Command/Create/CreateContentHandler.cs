using ContentService.Application.Common;
using ContentService.Application.Common.Abstractions;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace ContentService.Application.Contents.Command.Create
{
    public sealed class CreateContentHandler(
    IContentRepository repo,
    IUnitOfWork uow,
    ContentMapper mapper)
  : IRequestHandler<CreateContentCommand, ContentDto>
    {
        public async Task<ContentDto> Handle(CreateContentCommand req, CancellationToken ct)
        {
            // slug boşsa title’dan üret
            var baseSlug = string.IsNullOrWhiteSpace(req.Slug) ? SlugHelper.Slugify(req.Title) : req.Slug!.Trim().ToLowerInvariant();

            // unique slug üret (hello-world, hello-world-2, ...)
            var slug = await MakeUniqueSlugAsync(baseSlug, ct);

            var contentCommand = new Content
            {
                Title = req.Title,
                Body = req.Body,
                AuthorId = req.AuthorId,
                Status = ContentStatus.Draft,
                Slug = slug
            };

            await repo.AddAsync(contentCommand, ct);
            await uow.SaveChangesAsync(ct);

            return mapper.ToDto(contentCommand);
        }

        private async Task<string> MakeUniqueSlugAsync(string baseSlug, CancellationToken ct)
        {
            var slug = baseSlug;
            var i = 2;
            while (await repo.Query().AnyAsync(x => x.Slug == slug, ct))
                slug = $"{baseSlug}-{i++}";
            return slug;
        }
    }
}
