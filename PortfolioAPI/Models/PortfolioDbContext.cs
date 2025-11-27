using Microsoft.EntityFrameworkCore;

namespace PortfolioAPI.Models
{
    public class PortfolioDbContext : DbContext
    {
        public IConfiguration Configuration { get; }

        public PortfolioDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "server=localhost;port=3306;database=portfoliodb;user=portfolioUser;password=portfolioPassword";
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    options => { options.EnableRetryOnFailure(); }
                );
            }
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
    }
}
