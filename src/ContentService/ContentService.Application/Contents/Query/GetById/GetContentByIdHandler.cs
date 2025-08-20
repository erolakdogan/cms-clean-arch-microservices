using ContentService.Application.Common.Abstractions;
using MediatR;
namespace ContentService.Application.Contents.Query.GetById
{
    public sealed class GetContentByIdHandler(IContentRepository repo, ContentMapper mapper)
    : IRequestHandler<GetContentByIdQuery, ContentDto?>
    {
        public async Task<ContentDto?> Handle(GetContentByIdQuery req, CancellationToken ct)
        {
            var contentItem = await repo.GetByIdAsync(req.Id, ct);
            return contentItem is null ? null : mapper.ToDto(contentItem);
        }
    }
}
