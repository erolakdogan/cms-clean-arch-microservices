using ContentService.Application.Common.Abstractions;
using MediatR;

namespace ContentService.Application.Contents.Command.Delete
{
    public sealed class DeleteContentHandler(IContentRepository repo, IUnitOfWork uow)
    : IRequestHandler<DeleteContentCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteContentCommand req, CancellationToken ct)
        {
            var contentItem = await repo.GetByIdAsync(req.Id, ct) ?? throw new KeyNotFoundException("Content not found.");
            repo.Remove(contentItem);
            await uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
