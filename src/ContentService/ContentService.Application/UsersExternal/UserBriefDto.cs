namespace ContentService.Application.UsersExternal;

public sealed class UserBriefDto
{
    public Guid Id { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
}
