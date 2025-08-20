using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Infrastructure.Persistence
{
    public sealed class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            // Önce env var -> yoksa dev fallback
            var cs = Environment.GetEnvironmentVariable("USERSVC__CS")
                     ?? "Host=localhost;Port=5432;Database=users;Username=postgres;Password=postgres";

            var opts = new DbContextOptionsBuilder<UserDbContext>()
                .UseNpgsql(cs)
                .UseSnakeCaseNamingConvention()  
                .Options;

            return new UserDbContext(opts);
        }
    }
}
