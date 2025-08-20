using MediatR;

namespace ContentService.Application.Contents.Query.GetBySlug
{
    public sealed record GetContentBySlugQuery(string Slug) : IRequest<ContentDto?>;
}
