using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trava.Application.Interfaces.Services;
using Trava.Infrastructure.Services.Identify;

namespace Trava.Infrastructure.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<JwtHandler>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
