using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gestion.Citas.DataAccess
{
    public class AppointmentsFactoryDbContext
        : IDesignTimeDbContextFactory<AppointmentsDbContext>
    {
        public AppointmentsDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable(
                    "ConnectionStrings__DbAppointments"
                );

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "No se encontró la cadena de conexión."
                );

            var optionsBuilder =
                new DbContextOptionsBuilder<AppointmentsDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new AppointmentsDbContext(
                optionsBuilder.Options
            );
        }
    }
}