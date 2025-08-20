using Asp.Versioning;
using ContentService.Api.Contracts.Contents;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Command.Create;
using ContentService.Application.Contents.Command.Delete;
using ContentService.Application.Contents.Command.Update;
using ContentService.Application.Contents.Query.GetById;
using ContentService.Application.Contents.Query.GetBySlug;
using ContentService.Application.Contents.Query.List;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/contents")]
    public sealed class ContentsController(IMediator mediator) : ControllerBase
    {
        private static ContentResponse ToResponse(ContentDto d) =>
            new(d.Id, d.Title, d.Body, d.AuthorId, d.Status, d.Slug, d.CreatedAt, d.UpdatedAt);

        [HttpPost]
        public async Task<ActionResult<ContentResponse>> Create([FromBody] ContentCreateRequest req, CancellationToken ct)
        {
            var dto = await mediator.Send(new CreateContentCommand(req.Title, req.Body, req.AuthorId, req.Slug), ct);
            var resp = ToResponse(dto);
            return CreatedAtAction(nameof(GetById), new { id = resp.Id, version = "1.0" }, resp);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ContentResponse>> GetById(Guid id, CancellationToken ct)
        {
            var dto = await mediator.Send(new GetContentByIdQuery(id), ct);
            return dto is null ? NotFound() : Ok(ToResponse(dto));
        }

        [HttpGet("by-slug/{slug}")]
        public async Task<ActionResult<ContentResponse>> GetBySlug(string slug, CancellationToken ct)
        {
            var dto = await mediator.Send(new GetContentBySlugQuery(slug), ct);
            return dto is null ? NotFound() : Ok(ToResponse(dto));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ContentResponse>>> List(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            [FromQuery] Guid? authorId = null,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var list = await mediator.Send(new ListContentsQuery(page, pageSize, status, authorId, search), ct);
            return Ok(list.Select(ToResponse).ToList());
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ContentResponse>> Update(Guid id, [FromBody] ContentUpdateRequest req, CancellationToken ct)
        {
            var dto = await mediator.Send(new UpdateContentCommand(id, req.Title, req.Body, req.Status, req.Slug), ct);
            return Ok(ToResponse(dto));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await mediator.Send(new DeleteContentCommand(id), ct);
            return NoContent();
        }
    }
}
