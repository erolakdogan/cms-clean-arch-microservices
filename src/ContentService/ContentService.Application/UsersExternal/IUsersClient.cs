namespace ContentService.Application.UsersExternal
{
    public sealed record UserSummary(Guid Id, string Email, string DisplayName, string[] Roles);

    public interface IUsersClient
    {
        Task<UserSummary?> GetUserAsync(Guid id, CancellationToken ct = default);
    }
}
