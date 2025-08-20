using Asp.Versioning;
using ContentService.Application.Common.Models;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Commands;
using ContentService.Application.Contents.Queries;
using ContentService.Application.Contents.Query.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/contents")]
[Produces("application/json")]
[Tags("Contents")]
public sealed class ContentsController(IMediator mediator) : ControllerBase
{
    /// <summary>List contents with paging & search</summary>
    [HttpGet]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(PagedResult<ContentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ContentDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListContentsQuery(page, pageSize, search), ct);
        return Ok(result);
    }

    /// <summary>Get single content</summary>
    [HttpGet("{id:guid}")]
    [Authorize] 
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetContentByIdQuery(id), ct);
        return Ok(dto);
    }

    /// <summary>Create a content</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContentCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get),
            new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" },
            new { id });
    }

    /// <summary>Update a content</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentCommand body, CancellationToken ct)
    {
        var cmd = body with { Id = id };
        await mediator.Send(cmd, ct);
        return NoContent();
    }

    /// <summary>Delete a content</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteContentCommand(id), ct);
        return NoContent();
    }
}
