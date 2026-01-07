using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trava.Application.Interfaces;
using Trava.Application.Interfaces.Repositories;
using Trava.Application.Interfaces.Services;
using Trava.Infrastructure.Persistence.Context;
using Trava.Infrastructure.Persistence.Repositories;
using Trava.Infrastructure.Persistence.UnitOfWork;
using Trava.Infrastructure.Services;

namespace Trava.Infrastructure.Extensions
{
    public static class InfrastructureExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsqlOptions.CommandTimeout(30);
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }), poolSize: 10);

            services.AddHealthChecks().Services.AddDbContext<AppDbContext>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
            });

            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IFactoryRepository, FactoryRepository>();

            services.AddHttpClient();
            services.AddHttpClient("TravaHttpClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            return services;
        }
    }
}