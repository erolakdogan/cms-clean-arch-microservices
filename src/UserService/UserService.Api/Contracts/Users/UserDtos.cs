namespace UserService.Api.Contracts.Users
{
    public sealed record UserCreateRequest(string Email, string Password, string DisplayName, string[] Roles);
    public sealed record UserUpdateRequest(string DisplayName, string[] Roles);
    public sealed record UserResponse(Guid Id, string Email, string DisplayName, string[] Roles, DateTime CreatedAt);
}
