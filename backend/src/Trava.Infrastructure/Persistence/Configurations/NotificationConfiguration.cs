using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Payload)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasMany(x => x.UserNotifications)
                .WithOne(x => x.Notification)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
