using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300)
                .HasColumnType("citext");

            builder.Property(x => x.Description)
                .HasMaxLength(4000);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Point)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.SpaceId)
                .IsRequired();

            builder.Property(x => x.StartDate);

            builder.Property(x => x.DueDate);

            builder.Property(x => x.AssignedAt);

            builder.Property(x => x.CompletedAt);

            builder.HasOne(x => x.Space)
                .WithMany(x => x.TaskItems)
                .HasForeignKey(x => x.SpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AssignedUser)
                .WithMany(x => x.AssignedTasks)
                .HasForeignKey(x => x.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Comments)
                .WithOne(x => x.TaskItem)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ParentTask)
                .WithMany(x => x.SubTasks)
                .HasForeignKey(x => x.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SpaceId);

            builder.HasIndex(x => x.AssignedUserId);

            builder.HasIndex(x => x.ParentTaskId);

            builder.HasIndex(x => new { x.SpaceId, x.Title }).IsUnique();
        }
    }
}