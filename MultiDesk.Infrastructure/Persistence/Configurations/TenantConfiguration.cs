using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Departments)
            .WithOne(d => d.Tenant)
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Tickets)
            .WithOne(ti => ti.Tenant)
            .HasForeignKey(ti => ti.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}