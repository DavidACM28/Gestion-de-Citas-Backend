using Gestion.Citas.Business.Implementations;
using Gestion.Citas.Business.Interfaces;
using Gestion.Citas.Repositories.Implementations;
using Gestion.Citas.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusiness(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
