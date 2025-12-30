using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
    {
        public void Configure(EntityTypeBuilder<UserNotification> builder)
        {
            // Composite key
            builder.HasKey(x => new { x.TargetUserId, x.NotificationId });

            builder.HasOne(x => x.TargetUser)
                .WithMany()
                .HasForeignKey(x => x.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Notification)
                .WithMany()
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
