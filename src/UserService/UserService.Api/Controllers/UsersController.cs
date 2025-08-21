using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UserService.Application.Common.Models;
using UserService.Application.Users;
using UserService.Application.Users.Commands;
using UserService.Application.Users.Queries;

namespace UserService.Api.Controllers;

/// <summary>
/// Kullanıcı yönetimi uç noktaları.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
[Tags("Kullanıcılar")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;


    /// <summary>Kullanıcıları sayfalı listele.</summary>
    [HttpGet]
    [AllowAnonymous] 
    [SwaggerOperation(Summary = "Kullanıcı listesi (sayfalı)", Description = "page ve pageSize ile sayfalama yapar.")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListUsersQuery(page, pageSize, search), ct);
        return Ok(result);
    }

    /// <summary>Tek bir kullanıcıyı getir.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Detay (Id ile)", Description = "Kullanıcıyı kimliği ile getirir.")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(dto);
    }

    /// <summary>Yeni kullanıcı oluştur.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(Summary = "Oluştur (Admin)", Description = "Yeni kullanıcı kaydı oluşturur.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get),
            new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" },
            new { id });
    }

    /// <summary>Kullanıcı bilgilerini güncelle.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(Summary = "Güncelle (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand body, CancellationToken ct)
    {
        // Komutun route id’siyle uyum
        var cmd = body with { Id = id };
        await _mediator.Send(cmd, ct);
        return NoContent();
    }

    /// <summary>Kullanıcıyı sil.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(Summary = "Sil (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
}
