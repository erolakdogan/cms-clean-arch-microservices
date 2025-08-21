using ContentService.Application.Common.Abstractions;
using ContentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContentService.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// ContentService için migrate + anlamlı seed veriyi uygular.
    /// Program.cs'ten: await ContentsDbSeeder.MigrateAndSeedAsync(app.Services);
    /// </summary>
    public static class ContentsDbSeeder
    {
        // Deterministic GUID'ler (Content seed ile eşleşecek)
        private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid EditorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid WriterId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public static async Task MigrateAndSeedAsync(IServiceProvider services)
        {

            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit(Contents)");

            for (var i = 1; i <= 5; i++)
            {
                try
                {
                    var db = sp.GetRequiredService<ContentDbContext>();
                    await db.Database.MigrateAsync();

                    var repo = sp.GetRequiredService<IContentRepository>();
                    var uow = sp.GetRequiredService<IUnitOfWork>();

                    if (!await repo.Query().AnyAsync())
                    {
                        var now = DateTime.UtcNow;

                        var list = new[]
                        {
                    new Content { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), Title="Hoşgeldiniz", Body="Bu bir deneme içeriğidir.", AuthorId=AdminId, Status=ContentStatus.Published, Slug="hosgeldiniz", CreatedAt=now },
                    new Content { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), Title="Editörden notlar", Body="Editörün ilk notları.", AuthorId=EditorId, Status=ContentStatus.Published, Slug="editor-notlari", CreatedAt=now },
                    new Content { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), Title="Yazar günlükleri", Body="Günlük 1", AuthorId=WriterId, Status=ContentStatus.Draft, Slug="yazar-gunlukleri", CreatedAt=now },
                    new Content { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), Title="CMS İpuçları", Body="İpucu derlemesi", AuthorId=AdminId, Status=ContentStatus.Published, Slug="cms-ipuclari", CreatedAt=now },
                    new Content { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"), Title=".NET 8 yenilikleri", Body="Özet notlar", AuthorId=EditorId, Status=ContentStatus.Published, Slug="dotnet-8-yenilikleri", CreatedAt=now }
                };

                        foreach (var c in list) await repo.AddAsync(c);
                        await uow.SaveChangesAsync();
                    }

                    log.LogInformation("Seed(Contents) OK");
                    return;
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, "Contents migrate/seed retrying…");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}
