using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Trava.Infrastructure.Persistence.Context;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Infrastructure.Extensions
{
    public static class DbExtension
    {
        public static async Task MigrationsDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            await SeedAuthDataAsync(services, context);
        }

        private static async Task SeedAuthDataAsync(IServiceProvider services, AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var admin = new User
            {
                Id = new Guid("1a1a1a1a-1a1a-1a1a-1a1a-1a1a1a1a1a1a"),
                Email = "thuanndmqe170240@fpt.edu.vn",
                FullName = "Thuan Nguyen Dao Minh",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = Role.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var user = new User
            {
                Id = new Guid("2a2a2a2a-2a2a-2a2a-2a2a-2a2a2a2a2a2a"),
                Email = "user@fpt.edu.vn",
                FullName = "User",
                Password = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = Role.User,
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await context.Users.AddRangeAsync(admin, user);
            await context.SaveChangesAsync();
        }
    }
}