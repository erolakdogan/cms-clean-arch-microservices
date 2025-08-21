using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserService.Application.Common.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// UserService için migrate + anlamlı seed veriyi uygular.
    /// Program.cs'ten: await UserDbSeeder.MigrateAndSeedAsync(app.Services);
    /// </summary>
    public static class UserDbSeeder
    {
        // Deterministic GUID'ler (Content seed ile eşleşecek)
        private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid EditorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid WriterId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid Author2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid Author3Id = Guid.Parse("55555555-5555-5555-5555-555555555555");

        public static async Task MigrateAndSeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit(Users)");

            // Postgres yeni kalkmışsa ilk sorgularda connection refused olabilir → retry
            const int retries = 5;
            var delay = TimeSpan.FromSeconds(3);

            for (int i = 1; i <= retries; i++)
            {
                try
                {
                    var db = sp.GetRequiredService<UserDbContext>();
                    await db.Database.MigrateAsync();

                    var repo = sp.GetRequiredService<IUserRepository>();
                    var uow = sp.GetRequiredService<IUnitOfWork>();
                    var hasher = sp.GetRequiredService<IPasswordHasherService>();

                    async Task Ensure(Guid id, string email, string displayName, string[] roles)
                    {
                        var exists = await repo.Query().AnyAsync(u => u.Id == id);
                        if (!exists)
                        {
                            await repo.AddAsync(new User
                            {
                                Id = id,
                                Email = email,
                                PasswordHash = hasher.Hash("P@ssw0rd!"),
                                DisplayName = displayName,
                                Roles = roles,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    await Ensure(AdminId, "admin@cms.local", "Administrator", new[] { "Admin" });
                    await Ensure(EditorId, "editor@cms.local", "Editor", new[] { "Editor" });
                    await Ensure(WriterId, "writer@cms.local", "Writer", new[] { "Writer" });
                    await Ensure(Author2Id, "author2@cms.local", "Author Two", new[] { "Author" });
                    await Ensure(Author3Id, "author3@cms.local", "Author Three", new[] { "Author" });

                    await uow.SaveChangesAsync();

                    log.LogInformation("Seed(Users) OK");
                    return; 
                }
                catch (Exception ex)
                {
                    if (i == retries)
                    {
                        // son denemede patlatalım ki görünür olsun
                        throw;
                    }

                    // ara denemeler uyarı + bekleme
                    var msg = $"Users migrate/seed failed on attempt {i}, retrying in {delay.TotalSeconds}s…";
                    log.LogWarning(ex, msg);
                    await Task.Delay(delay);
                }
            }
        }
    }
}
