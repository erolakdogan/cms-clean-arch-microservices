using ContentService.Application.Common.Abstractions;
using ContentService.Domain.Entities;
using MediatR;

namespace ContentService.Application.Contents.Commands;

public sealed class CreateContentHandler(IContentRepository repo, IUnitOfWork uow)
    : IRequestHandler<CreateContentCommand, Guid>
{
    public async Task<Guid> Handle(CreateContentCommand req, CancellationToken ct)
    {
        var entity = new Content
        {
            Title = req.Title.Trim(),
            Body = req.Body,
            AuthorId = req.AuthorId,
            Status = req.Status ?? ContentStatus.Draft,
            Slug = string.IsNullOrWhiteSpace(req.Slug)
                ? Slugify(req.Title)
                : req.Slug.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }

    private static string Slugify(string input)
        => string.Join("-", input.ToLowerInvariant()
                                 .Split(' ', StringSplitOptions.RemoveEmptyEntries))
           .Replace(".", "")
           .Replace(",", "");
}
