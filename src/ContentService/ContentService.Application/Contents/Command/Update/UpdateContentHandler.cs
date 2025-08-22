using ContentService.Application.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Application.Contents.Commands;

public sealed class UpdateContentHandler(IContentRepository repo, IUnitOfWork uow)
    : IRequestHandler<UpdateContentCommand>
{
    public async Task Handle(UpdateContentCommand req, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(req.Id, ct); // Query() yerine
        if (entity is null)
            throw new KeyNotFoundException($"Content '{req.Id}' not found.");

        if (!string.IsNullOrWhiteSpace(req.Title)) entity.Title = req.Title.Trim();
        if (!string.IsNullOrWhiteSpace(req.Body)) entity.Body = req.Body;
        if (req.AuthorId.HasValue) entity.AuthorId = req.AuthorId.Value;
        if (!string.IsNullOrWhiteSpace(req.Slug)) entity.Slug = req.Slug.Trim();
        if (req.Status.HasValue) entity.Status = req.Status.Value;
        entity.UpdatedAt = DateTime.UtcNow;

        repo.Update(entity);
        await uow.SaveChangesAsync(ct);
    }
}
