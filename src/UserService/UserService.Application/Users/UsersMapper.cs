using Riok.Mapperly.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Application.Users
{
    [Mapper(UseDeepCloning = false, ThrowOnMappingNullMismatch = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class UsersMapper
    {
        // Entity -> DTO
        public partial UserDto ToDto(User user);

        public partial List<UserDto> ToDtoList(List<User> users);
    }
}
