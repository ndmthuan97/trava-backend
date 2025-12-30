using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
    {
        public void Configure(EntityTypeBuilder<TaskComment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.TaskItemId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TaskItemId);
            builder.HasIndex(x => x.UserId);
        }
    }
}