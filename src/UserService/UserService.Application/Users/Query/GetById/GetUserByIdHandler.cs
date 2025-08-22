using MediatR;
using UserService.Application.Common.Abstractions;

namespace UserService.Application.Users.Queries;

public sealed class GetUserByIdHandler(IUserRepository repo, UsersMapper mapper)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery req, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(req.Id, ct); 
        if (entity is null)
            throw new KeyNotFoundException($"User '{req.Id}' not found.");
        return mapper.ToDto(entity);
    }
}
