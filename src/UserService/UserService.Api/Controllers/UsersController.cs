using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Common.Models;
using UserService.Application.Users;
using UserService.Application.Users.Commands;
using UserService.Application.Users.Queries;

namespace UserService.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
[Tags("Users")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>List users with paging & search</summary>
    [HttpGet]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListUsersQuery(page, pageSize, search), ct);
        return Ok(result);
    }

    /// <summary>Get single user</summary>
    [HttpGet("{id:guid}")]
    [Authorize] 
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Get(Guid id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(dto);
    }

    /// <summary>Create a user</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get),
            new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" },
            new { id });
    }

    /// <summary>Update a user</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand body, CancellationToken ct)
    {
        // Komutun route id’siyle uyum
        var cmd = body with { Id = id };
        await mediator.Send(cmd, ct);
        return NoContent();
    }

    /// <summary>Delete a user</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
}
