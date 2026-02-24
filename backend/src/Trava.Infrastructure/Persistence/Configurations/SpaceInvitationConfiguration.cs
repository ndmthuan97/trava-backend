using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class SpaceInvitationConfiguration : IEntityTypeConfiguration<SpaceInvitation>
    {
        public void Configure(EntityTypeBuilder<SpaceInvitation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SpaceId)
                .IsRequired();

            builder.Property(x => x.InvitedUserId)
                .IsRequired();

            builder.Property(x => x.SpaceRole)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne(x => x.Space)
                .WithMany()
                .HasForeignKey(x => x.SpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.InvitedUser)
                .WithMany()
                .HasForeignKey(x => x.InvitedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
