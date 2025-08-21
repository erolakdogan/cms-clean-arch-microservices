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
/// İçerik yönetimi uç noktaları.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/contents")]
[Produces("application/json")]
[Tags("İçerikler")]
public sealed class ContentsController(IMediator mediator) : ControllerBase
{
    /// <summary>İçerikleri sayfalı listele.</summary>
    /// <remarks>
    /// `search` başlık/slug içinde arama yapar.
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "İçerik listesi (sayfalı)", Description = "page, pageSize ve search ile filtreleme/sayfalama.")]
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

    /// <summary>Tek bir içeriği getir.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Detay (Id ile)", Description = "İçeriği kimliği ile getirir.")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetContentByIdQuery(id), ct);
        return Ok(dto);
    }

    /// <summary>Yeni içerik oluştur.</summary>
    [HttpPost]
    [Authorize]
    [SwaggerOperation(Summary = "Oluştur (Admin)", Description = "Yeni içerik kaydı oluşturur.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContentCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get),
            new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" },
            new { id });
    }

    /// <summary>İçeriği güncelle.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(Summary = "Güncelle (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentCommand body, CancellationToken ct)
    {
        var cmd = body with { Id = id };
        await mediator.Send(cmd, ct);
        return NoContent();
    }

    /// <summary>İçeriği sil.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(Summary = "Sil (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteContentCommand(id), ct);
        return NoContent();
    }
}
