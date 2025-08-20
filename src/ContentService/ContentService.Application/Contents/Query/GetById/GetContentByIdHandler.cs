using ContentService.Application.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Application.Contents.Query.GetById
{
    public sealed class GetContentByIdHandler(IContentRepository repo, ContentMapper mapper)
    : IRequestHandler<GetContentByIdQuery, ContentDto>
    {
        public async Task<ContentDto> Handle(GetContentByIdQuery req, CancellationToken ct)
        {
            var entity = await repo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == req.Id, ct);

            if (entity is null)
                throw new KeyNotFoundException($"Content '{req.Id}' not found.");

            return mapper.ToDto(entity);
        }
    }
}
