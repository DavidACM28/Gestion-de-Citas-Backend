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
            return services;
        }
    }
}
