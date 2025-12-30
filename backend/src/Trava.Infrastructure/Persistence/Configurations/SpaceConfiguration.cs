using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trava.Domain.Entities;

namespace Trava.Infrastructure.Persistence.Configurations
{
    public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.SpaceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Members)
            .WithOne(x => x.Space)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TaskItems)
            .WithOne(x => x.Space)
            .HasForeignKey(x => x.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CreatedBy);
        builder.HasIndex(x => new { x.CreatedBy, x.SpaceType });
    }
}
}