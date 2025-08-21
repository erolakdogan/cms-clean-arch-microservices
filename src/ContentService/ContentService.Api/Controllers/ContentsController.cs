using Asp.Versioning;
using ContentService.Application.Common.Models;
using ContentService.Application.Contents;
using ContentService.Application.Contents.Commands;
using ContentService.Application.Contents.Queries;
using ContentService.Application.Contents.Query.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ContentService.Api.Controllers;
/// <summary>
/// İçerik yönetimi.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/contents")]
[Produces("application/json")]
[Tags("İçerikler")]
public sealed class ContentsController(IMediator mediator) : ControllerBase
{
    /// <summary>İçerikleri sayfalı listele.</summary>
    /// <remarks>`search` başlık/slug içinde arama yapar.</remarks>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "İçerik listesi (sayfalı)", Description = "page, pageSize ve search ile filtreleme/sayfalama.")]
    [ProducesResponseType(typeof(PagedResult<ContentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ContentDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListContentsQuery(page, pageSize, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>Tek bir içeriği getir.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Detay (Id ile)", Description = "İçeriği kimliği ile getirir.")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ContentDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var contentDto = await mediator.Send(new GetContentByIdQuery(id), cancellationToken);
        return Ok(contentDto);
    }

    /// <summary>Yeni içerik oluştur.</summary>
    [HttpPost]
    [Authorize]
    [SwaggerOperation(Summary = "Oluştur (Admin)", Description = "Yeni içerik kaydı oluşturur.")]
    [ProducesResponseType(typeof(CreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateContentCommand createContentCommand, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(createContentCommand, cancellationToken);
        var body = new CreatedResponse(id);

        return CreatedAtAction(nameof(Get),
            new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" },
            body);
    }

    /// <summary>İçeriği güncelle.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Güncelle (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentCommand updateContentCommand, CancellationToken cancellationToken)
    {
        var cmd = updateContentCommand with { Id = id };
        await mediator.Send(cmd, cancellationToken);
        return NoContent();
    }

    /// <summary>İçeriği sil.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(Summary = "Sil (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteContentCommand(id), cancellationToken);
        return NoContent();
    }
}
