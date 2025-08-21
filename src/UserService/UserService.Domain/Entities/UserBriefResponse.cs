namespace UserService.Domain.Entities
{
    public sealed class UserBriefResponse
    {
        public Guid Id { get; init; }
        public string? Email { get; init; }
        public string? DisplayName { get; init; }
    }
}
