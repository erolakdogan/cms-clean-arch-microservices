namespace Shared.Web.Security
{
    public sealed class JwtOptions
    {
        public string Issuer { get; init; } = default!;
        public string Audience { get; init; } = default!;
        public string Key { get; init; } = default!; // en az 32 byte
        public int AccessTokenMinutes { get; init; } = 30;
    }
}
