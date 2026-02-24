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

            // 1. Seed Users
            var adminT = new User
            {
                Id = new Guid("1a1a1a1a-1a1a-1a1a-1a1a-1a1a1a1a1a1a"),
                Email = "admin@fpt.edu.vn",
                FullName = "Admin",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = Role.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var admin = new User
            {
                Id = new Guid("2a2a2a2a-2a2a-2a2a-2a2a-2a2a2a2a2a2a"),
                Email = "thuanndmqe170240@fpt.edu.vn",
                FullName = "Thuan Nguyen Dao Minh",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = Role.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var user1 = new User
            {
                Id = new Guid("3a3a3a3a-3a3a-3a3a-3a3a-3a3a3a3a3a3a"),
                Email = "user@fpt.edu.vn",
                FullName = "John Doe",
                Password = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = Role.User,
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var user2 = new User
            {
                Id = new Guid("4a4a4a4a-4a4a-4a4a-4a4a-4a4a4a4a4a4a"),
                Email = "user1@fpt.edu.vn",
                FullName = "Jane Smith",
                Password = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = Role.User,
                Status = UserStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await context.Users.AddRangeAsync(adminT, admin, user1, user2);

            // 2. Seed Spaces
            var adminPersonalSpace = new Space
            {
                Id = new Guid("1b1b1b1b-1b1b-1b1b-1b1b-1b1b1b1b1b1b"),
                Name = "Admin's Personal Space",
                Description = "A private space for admin tasks",
                SpaceType = SpaceType.Personal,
                CreatedBy = admin.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var userPersonalSpace = new Space
            {
                Id = new Guid("2b2b2b2b-2b2b-2b2b-2b2b-2b2b2b2b2b2b"),
                Name = "John's Workspace",
                Description = "My private task list",
                SpaceType = SpaceType.Personal,
                CreatedBy = user1.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var teamSpace = new Space
            {
                Id = new Guid("3b3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"),
                Name = "Trava Development Team",
                Description = "Collaborative space for the backend development team",
                SpaceType = SpaceType.Team,
                CreatedBy = admin.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await context.Spaces.AddRangeAsync(adminPersonalSpace, userPersonalSpace, teamSpace);

            // 3. Seed SpaceMembers
            var members = new List<SpaceMember>
            {
                // Team Space Members
                new SpaceMember { SpaceId = teamSpace.Id, UserId = admin.Id, SpaceRole = SpaceRole.Owner, JoinedAt = DateTimeOffset.UtcNow },
                new SpaceMember { SpaceId = teamSpace.Id, UserId = user1.Id, SpaceRole = SpaceRole.Member, JoinedAt = DateTimeOffset.UtcNow },
                new SpaceMember { SpaceId = teamSpace.Id, UserId = user2.Id, SpaceRole = SpaceRole.Member, JoinedAt = DateTimeOffset.UtcNow }
            };

            await context.SpaceMembers.AddRangeAsync(members);

            // 4. Seed TaskItems
            var task1 = new TaskItem
            {
                Id = Guid.NewGuid(),
                SpaceId = teamSpace.Id,
                Title = "Setup Project Structure",
                Description = "Initialize the .NET Solution and projects",
                Status = TaskItemStatus.Completed,
                Priority = TaskItemPriority.High,
                Point = 5,
                AssignedUserId = admin.Id,
                AssignedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            };

            var task2 = new TaskItem
            {
                Id = Guid.NewGuid(),
                SpaceId = teamSpace.Id,
                Title = "Implement Auth Logic",
                Description = "Create JWT token generation and validation",
                Status = TaskItemStatus.InProgress,
                Priority = TaskItemPriority.High,
                Point = 8,
                AssignedUserId = user1.Id,
                AssignedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            };

            var task3 = new TaskItem
            {
                Id = Guid.NewGuid(),
                SpaceId = teamSpace.Id,
                Title = "Design Database Schema",
                Description = "Create ERD for the whole system",
                Status = TaskItemStatus.NotStart,
                Priority = TaskItemPriority.Medium,
                Point = 3,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            };

            var task4 = new TaskItem
            {
                Id = Guid.NewGuid(),
                SpaceId = userPersonalSpace.Id,
                Title = "Weekly Report",
                Description = "Draft the progress report for this week",
                Status = TaskItemStatus.InProgress,
                Priority = TaskItemPriority.Low,
                Point = 2,
                AssignedUserId = user1.Id,
                AssignedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await context.TaskItems.AddRangeAsync(task1, task2, task3, task4);

            // 5. Seed TaskComments
            var comments = new List<TaskComment>
            {
                new TaskComment
                {
                    Id = Guid.NewGuid(),
                    TaskItemId = task2.Id,
                    UserId = admin.Id,
                    Content = "Please focus on refresh token implementation as well.",
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-2)
                },
                new TaskComment
                {
                    Id = Guid.NewGuid(),
                    TaskItemId = task2.Id,
                    UserId = user1.Id,
                    Content = "Will do. I'm currently working on the middleare.",
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                }
            };

            await context.TaskComments.AddRangeAsync(comments);

            // 6. Seed SpaceInvitations
            var invitation = new SpaceInvitation
            {
                Id = Guid.NewGuid(),
                SpaceId = teamSpace.Id,
                InvitedUserId = user2.Id,
                Status = InvitationStatus.Pending
            };

            await context.SpaceInvitations.AddAsync(invitation);

            // 7. Seed Notifications
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Type = "System",
                Payload = "{\"message\": \"Welcome to Trava!\"}",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await context.Notifications.AddAsync(notification);

            var userNotification = new UserNotification
            {
                NotificationId = notification.Id,
                TargetUserId = user1.Id,
                IsRead = false
            };

            await context.UserNotifications.AddAsync(userNotification);
            
            await context.SaveChangesAsync();
        }
    }
}