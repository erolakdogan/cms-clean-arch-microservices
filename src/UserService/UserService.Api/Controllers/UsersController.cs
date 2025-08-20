using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Contracts.Users;
using UserService.Application.Users;
using UserService.Application.Users.Command.Create;
using UserService.Application.Users.Command.Delete;
using UserService.Application.Users.Command.Update;
using UserService.Application.Users.Query.GetById;
using UserService.Application.Users.Query.List;

namespace UserService.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public sealed class UsersController(IMediator mediator, UsersMapper mapper) : ControllerBase
    {
        private static UserResponse ToResponse(UserDto d)
            => new(d.Id, d.Email, d.DisplayName, d.Roles, d.CreatedAt);
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult<UserResponse>> Create([FromBody] UserCreateRequest req, CancellationToken ct)
        {
            var dto = await mediator.Send(new CreateUserCommand(req.Email, req.Password, req.DisplayName, req.Roles), ct);
            var resp = ToResponse(dto);
            return CreatedAtAction(nameof(GetById), new { id = resp.Id, version = "1.0" }, resp);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
        {
            var dto = await mediator.Send(new GetUserByIdQuery(id), ct);
            return dto is null ? NotFound() : Ok(ToResponse(dto));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var list = await mediator.Send(new ListUsersQuery(page, pageSize), ct);
            return Ok(list.Select(ToResponse).ToList());
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UserUpdateRequest req, CancellationToken ct)
        {
            var dto = await mediator.Send(new UpdateUserCommand(id, req.DisplayName, req.Roles), ct);
            return Ok(ToResponse(dto));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await mediator.Send(new DeleteUserCommand(id), ct);
            return NoContent();
        }
    }
}
