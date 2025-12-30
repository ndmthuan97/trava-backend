using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .HasMaxLength(200);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.AvatarUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.EmailConfirmed)
                .IsRequired();

            builder.Property(x => x.RefreshToken)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.LastLoginAt);

            builder.Property(x => x.LastModifiedAt);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasIndex(x => x.Role);

            builder.HasIndex(x => x.Status);

            builder.HasMany(x => x.Spaces)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SpaceMembers)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.AssignedTasks)
                .WithOne(x => x.AssignedUser)
                .HasForeignKey(x => x.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}