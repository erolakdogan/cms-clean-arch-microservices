using ContentService.Application.Common.Abstractions;
using MediatR;
namespace ContentService.Application.Contents.Query.GetById
{
    public sealed class GetContentByIdHandler(IContentRepository repo, ContentMapper mapper)
    : IRequestHandler<GetContentByIdQuery, ContentDto?>
    {
        public async Task<ContentDto?> Handle(GetContentByIdQuery req, CancellationToken ct)
        {
            var e = await repo.GetByIdAsync(req.Id, ct);
            return e is null ? null : mapper.ToDto(e);
        }
    }
}
