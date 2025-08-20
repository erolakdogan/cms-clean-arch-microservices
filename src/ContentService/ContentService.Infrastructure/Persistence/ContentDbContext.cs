using ContentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Persistence
{
    public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options)
    {
        public DbSet<Content> Contents => Set<Content>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.HasPostgresExtension("citext"); // slug için case-insensitive

            modelBuilder.Entity<Content>(b =>
            {
                b.ToTable("contents");

                b.HasKey(x => x.Id);

                b.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(x => x.Body)
                    .IsRequired()
                    .HasColumnType("text");

                b.Property(x => x.AuthorId)
                    .IsRequired();

                b.Property(x => x.Status)
                    .HasConversion<int>()       // enum -> int
                    .IsRequired();

                // Slug benzersiz ve case-insensitive (citext)
                b.Property(x => x.Slug)
                    .IsRequired()
                    .HasMaxLength(240)
                    .HasColumnType("citext");

                b.HasIndex(x => x.Slug).IsUnique();

                b.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("now()");

                b.Property(x => x.UpdatedAt);

                // Sık kullanılacak filtre/sıralamalar için indexler
                b.HasIndex(x => x.AuthorId);
                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.CreatedAt);

            });
        }
    }
}
