using MediatR;

namespace UserService.Application.Users.Query.GetById
{
    public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
}
