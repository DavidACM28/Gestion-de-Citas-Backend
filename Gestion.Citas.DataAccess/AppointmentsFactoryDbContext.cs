using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gestion.Citas.DataAccess
{
    public class AppointmentsFactoryDbContext : IDesignTimeDbContextFactory<AppointmentsDbContext>
    {
        public AppointmentsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppointmentsDbContext>();
            optionsBuilder.UseSqlServer("server=localhost, 1501; database=dbappointments; uid=sa; password=Password2026; encrypt=False;");
            return new AppointmentsDbContext(optionsBuilder.Options);
        }
    }
}
