using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasOne(c => c.Department)
            .WithMany(d => d.Categories)
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_Categories_TenantId");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}