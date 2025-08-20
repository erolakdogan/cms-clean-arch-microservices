using ContentService.Application.Common.Abstractions;
using ContentService.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Application.Contents.Queries;

public sealed class ListContentsHandler(
    IContentRepository repo,
    ContentMapper mapper) // << sınıf adı 'ContentsMapper' olmalı
    : IRequestHandler<ListContentsQuery, PagedResult<ContentDto>>
{
    public async Task<PagedResult<ContentDto>> Handle(ListContentsQuery req, CancellationToken ct)
    {
        var page = Math.Max(1, req.Page);
        var size = Math.Clamp(req.PageSize, 1, 100);

        var q = repo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var s = req.Search.Trim();
            q = q.Where(c => c.Title.Contains(s) || c.Slug.Contains(s));
        }

        var total = await q.LongCountAsync(ct);

        var list = await q.OrderByDescending(c => c.CreatedAt)
                          .Skip((page - 1) * size)
                          .Take(size)
                          .ToListAsync(ct);

        var items = mapper.ToDtoList(list);
        return new PagedResult<ContentDto>(items, page, size, total);
    }
}
