using ContentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace ContentService.Infrastructure.Persistence
{
    public static class Seed
    {
        public static async Task EnsureAsync(ContentDbContext db, CancellationToken ct = default)
        {
            if (!await db.Contents.AnyAsync(ct))
            {
                db.Contents.Add(new Content
                {
                    Title = "Kıdemli Yazılım Uzmanı",
                    Body = "Bu bir mülakat için test case çalışmasıdır. Detaylar ve eksikler muhakkak olur ama sürdürülebilir bir mimaride oluşturuldu",
                    AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // placeholder
                    Status = ContentStatus.Draft,
                    Slug = "senior-software"
                });
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
