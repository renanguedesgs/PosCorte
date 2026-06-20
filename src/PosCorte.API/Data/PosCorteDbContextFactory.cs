using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PosCorte.API.Data
{
    public class PosCorteDbContextFactory : IDesignTimeDbContextFactory<PosCorteDbContext>
    {
        public PosCorteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PosCorteDbContext>();

            // Lê diretamente o Development para uso no CLI de migrations
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            return new PosCorteDbContext(optionsBuilder.Options);
        }
    }
}
