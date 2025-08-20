using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContentService.Infrastructure.Persistence
{
    public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<ContentDbContext>
    {
        public ContentDbContext CreateDbContext(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("CONTENTSVC__CS")
                     ?? "Host=localhost;Port=5432;Database=contents;Username=postgres;Password=postgres";
            var builder = new DbContextOptionsBuilder<ContentDbContext>()
                .UseNpgsql(cs)
                .UseSnakeCaseNamingConvention();

            return new ContentDbContext(builder.Options);
        }
    }
}
