using ContentService.Application.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Application.Contents.Commands;

public sealed class DeleteContentHandler(IContentRepository repo, IUnitOfWork uow)
    : IRequestHandler<DeleteContentCommand>
{
    public async Task Handle(DeleteContentCommand req, CancellationToken ct)
    {
        var entity = await repo.Query().FirstOrDefaultAsync(c => c.Id == req.Id, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Content '{req.Id}' not found.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}
