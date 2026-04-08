using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EMS.DAL.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=EMSDatabase;Trusted_Connection=True;TrustServerCertificate=true",
                b => b.MigrationsAssembly("EMS.DAL"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}