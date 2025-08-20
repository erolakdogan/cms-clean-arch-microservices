using MediatR;

namespace ContentService.Application.Contents.Query.GetById
{
    public sealed record GetContentByIdQuery(Guid Id) : IRequest<ContentDto?>;
}
