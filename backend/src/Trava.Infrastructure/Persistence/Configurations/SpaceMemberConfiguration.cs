using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class SpaceMemberConfiguration : IEntityTypeConfiguration<SpaceMember>
    {
        public void Configure(EntityTypeBuilder<SpaceMember> builder)
        {
            builder.HasKey(x => new { x.SpaceId, x.UserId });

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(x => x.Space)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.SpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId);
        }
    }
}