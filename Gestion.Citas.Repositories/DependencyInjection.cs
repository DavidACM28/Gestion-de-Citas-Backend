using Gestion.Citas.DataAccess.Entities;
using Gestion.Citas.Repositories.Implementations;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace Gestion.Citas.Repositories
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IBusinessHoursRepository, BusinessHoursRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAppointmentBlockRepository, AppointmentBlockRepository>();
            return services;
        }
    }
}
