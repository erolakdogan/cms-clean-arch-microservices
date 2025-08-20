namespace UserService.Api.Contracts.Auth
{
    public sealed record LoginRequest(string Email, string Password);
    public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
}
