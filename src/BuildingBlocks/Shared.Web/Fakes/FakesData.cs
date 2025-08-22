
using ContentService.Domain.Entities;
using UserService.Domain.Entities;

namespace Shared.Web.Fakes
{
    public static class FakesData
    {
        public static IEnumerable<User> ManyUsers(int count = 10)
        {
            var roles = new[] { "Admin", "Editor", "User" };
            for (int i = 1; i <= count; i++)
                yield return new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"user{i}@cms.local",
                    DisplayName = $"User {i}",
                    PasswordHash = $"HASH{i}",
                    Roles = new[] { roles[i % roles.Length] },
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
        }

        public static IEnumerable<Content> ManyContents(int count = 10)
        {
            for (int i = 1; i <= count; i++)
                yield return new Content
                {
                    Id = Guid.NewGuid(),
                    Title = $"Title {i}",
                    Body = $"Body {i}",
                    Slug = $"title-{i}",
                    AuthorId = Guid.NewGuid(),
                    Status = ContentStatus.Published,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
        }
    }
}
