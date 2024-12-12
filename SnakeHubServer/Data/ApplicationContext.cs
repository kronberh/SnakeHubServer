using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SnakeHubServer.Model;

namespace SnakeHubServer.Data
{
    public class ApplicationContext: IdentityDbContext<User>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().Property(b => b.TotalGames).HasDefaultValue(0);
            builder.Entity<User>().Property(b => b.TotalScore).HasDefaultValue(0);
        }
    }
}
