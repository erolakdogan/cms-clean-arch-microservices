namespace ContentService.Application.UsersExternal;

public interface IUsersClient
{
    Task<UserBriefDto?> GetBriefAsync(Guid id, CancellationToken ct = default);
}
