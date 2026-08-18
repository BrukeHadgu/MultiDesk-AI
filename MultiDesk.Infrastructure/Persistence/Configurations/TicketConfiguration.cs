using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.AttachmentPath)
            .HasMaxLength(500);

        // StudentId and AgentId are string FKs pointing to AspNetUsers
        builder.Property(t => t.StudentId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(t => t.AgentId)
            .HasMaxLength(450);

        // Department
        builder.HasOne(t => t.Department)
            .WithMany(d => d.Tickets)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite indexes
        builder.HasIndex(t => new { t.TenantId, t.Status, t.CreatedAt })
            .HasDatabaseName("IX_Tickets_TenantId_Status_CreatedAt");

        builder.HasIndex(t => new { t.AgentId, t.Status })
            .HasDatabaseName("IX_Tickets_AgentId_Status");

        builder.HasIndex(t => new { t.StudentId, t.Status })
            .HasDatabaseName("IX_Tickets_StudentId_Status");

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("IX_Tickets_TenantId");

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}