
namespace UserService.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
