using ContentService.Application.Common.Abstractions;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Application.Contents.Query.List
{
    public sealed class ListContentsHandler(IContentRepository repo, ContentMapper mapper)
    : IRequestHandler<ListContentsQuery, IReadOnlyList<ContentDto>>
    {
        public async Task<IReadOnlyList<ContentDto>> Handle(ListContentsQuery req, CancellationToken ct)
        {
            var page = Math.Max(1, req.Page);
            var size = Math.Clamp(req.PageSize, 1, 100);

            var contentQueryList = repo.Query();

            if (!string.IsNullOrWhiteSpace(req.Status) && Enum.TryParse<ContentStatus>(req.Status, true, out var st))
                contentQueryList = contentQueryList.Where(x => x.Status == st);

            if (req.AuthorId is { } aid && aid != Guid.Empty)
                contentQueryList = contentQueryList.Where(x => x.AuthorId == aid);

            if (!string.IsNullOrWhiteSpace(req.Search))
                contentQueryList = contentQueryList.Where(x => EF.Functions.Like(x.Title, $"%{req.Search}%")); // pg_trgm varsa hızlı

            var data = await contentQueryList
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return mapper.ToDtoList(data);
        }
    }
}
