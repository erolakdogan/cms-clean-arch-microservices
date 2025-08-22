using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public sealed class UsersApiTests : IClassFixture<UserApiFactory>
{
    private readonly UserApiFactory _factory;

    public UsersApiTests(UserApiFactory factory) => _factory = factory;

    private sealed class LoginRequest { public string? Email { get; set; } public string? Password { get; set; } }
    private sealed class LoginResponse { public string? AccessToken { get; set; } public int ExpiresIn { get; set; } }
    private sealed class PagedUsers { public List<UserDto>? Items { get; set; } }
    private sealed class CreateUserCommand
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public string[]? Roles { get; set; }
    }
    private sealed class UpdateUserCommand
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public string[]? Roles { get; set; }
    }
    private sealed class UserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string[]? Roles { get; set; }
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "admin@cms.local",
            Password = "P@ssw0rd!"
        });
        res.EnsureSuccessStatusCode();
        var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
        return payload!.AccessToken!;
    }

    [Fact]
    public async Task Auth_Login_Should_Return_Token()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "admin@cms.local",
            Password = "P@ssw0rd!"
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Users_List_Should_Return_200_Without_Token()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/v1/users?page=1&pageSize=10");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Users_Get_Should_Return_401_Without_Token()
    {
        var client = _factory.CreateClient();
        var anyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var res = await client.GetAsync($"/api/v1/users/{anyId}");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Users_Get_Should_Return_200_With_Token()
    {
        var token = await GetAdminTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var seededId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // seed dosyasında var
        var res = await client.GetAsync($"/api/v1/users/{seededId}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<UserDto>();
        dto!.Id.Should().Be(seededId);
    }

    [Fact]
    public async Task Users_Crud_Flow_With_Admin_Token()
    {
        var token = await GetAdminTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create
        var create = new CreateUserCommand
        {
            Email = "it.user@cms.local",
            Password = "ItPass!1",
            DisplayName = "IT User",
            Roles = new[] { "Editor" }
        };
        var createRes = await client.PostAsJsonAsync("/api/v1/users", create);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = (await createRes.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["id"];
        var id = Guid.Parse(createdId);

        // Get
        var getRes = await client.GetAsync($"/api/v1/users/{id}");
        getRes.EnsureSuccessStatusCode();

        // Update
        var update = new UpdateUserCommand
        {
            Id = id,
            DisplayName = "IT User Updated",
            Roles = new[] { "Admin" }
        };
        var updRes = await client.PutAsJsonAsync($"/api/v1/users/{id}", update);
        updRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Delete
        var delRes = await client.DeleteAsync($"/api/v1/users/{id}");
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
