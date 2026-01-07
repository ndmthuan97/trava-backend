using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trava.Domain.Common;
using Trava.Domain.Entities;
using System.Linq.Expressions;

namespace Trava.Infrastructure.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        protected AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<UserNotification> UserNotifications { get; set; } = default!;
        public DbSet<TaskComment> TaskComments { get; set; } = default!;
        public DbSet<SpaceMember> SpaceMembers { get; set; } = default!;
        public DbSet<Space> Spaces { get; set; } = default!;
        public DbSet<TaskItem> TaskItems { get; set; } = default!;
        public DbSet<SpaceInvitation> SpaceInvitations { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps<Guid>();
            UpdateTimestamps<int>();
            UpdateUserTimestamps();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps<TKey>()
        {
            var entries = ChangeTracker.Entries<BaseTimeEntity<TKey>>();

            foreach (var entry in entries)
            {
                var entity = entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entity.DeletedAt = DateTimeOffset.UtcNow;
                        break;
                }
            }
        }


        private void UpdateUserTimestamps()
        {
            var userEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is User &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in userEntries)
            {
                var user = (User)entry.Entity;
                if (entry.State == EntityState.Modified)
                {
                    user.LastModifiedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var deletedAtProperty = entityType.FindProperty("DeletedAt");
                if (deletedAtProperty != null && deletedAtProperty.ClrType == typeof(DateTimeOffset?))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "x");
                    var propertyAccess = Expression.Property(parameter, "DeletedAt");
                    var nullValue = Expression.Constant(null, typeof(DateTimeOffset?));
                    var equal = Expression.Equal(propertyAccess, nullValue);
                    var lambda = Expression.Lambda(equal, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }
}
