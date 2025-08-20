using ContentService.Application.Common.Abstractions;
using MediatR;
namespace ContentService.Application.Contents.Query.GetBySlug
{
    public sealed class GetContentBySlugHandler(IContentRepository repo, ContentMapper mapper)
    : IRequestHandler<GetContentBySlugQuery, ContentDto?>
    {
        public async Task<ContentDto?> Handle(GetContentBySlugQuery req, CancellationToken ct)
        {
            var e = await repo.GetBySlugAsync(req.Slug.ToLowerInvariant(), ct);
            return e is null ? null : mapper.ToDto(e);
        }
    }
}
