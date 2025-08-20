
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace UserService.Infrastructure.Persistence
{
    public sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.HasPostgresExtension("citext"); // e-posta için case-insensitive

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users");
                b.HasKey(x => x.Id);
                b.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("citext");
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(512);
                b.Property(x => x.DisplayName)
                    .IsRequired()
                    .HasMaxLength(200);
                b.Property(x => x.Roles)
                    .HasColumnType("text[]"); // GIN indexi migration'da raw SQL ile eklenecek
                b.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("now()");
                b.HasIndex(x => x.CreatedAt);
            });
        }
    }
}
