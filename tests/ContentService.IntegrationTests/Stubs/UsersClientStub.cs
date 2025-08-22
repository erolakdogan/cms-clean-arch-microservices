using ContentService.Application.UsersExternal;

namespace ContentService.IntegrationTests.Stubs;

public sealed class UsersClientStub : IUsersClient
{
    public Task<UserBriefDto?> GetBriefAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult<UserBriefDto?>(new UserBriefDto
        {
            Id = id,
            DisplayName = "Stubbed Author",
            Email = "stub@cms.local"
        });
}
