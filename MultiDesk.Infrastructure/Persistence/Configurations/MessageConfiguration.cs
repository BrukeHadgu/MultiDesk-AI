using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(m => m.SenderId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(m => m.Ticket)
            .WithMany(t => t.Messages)
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.TenantId)
            .HasDatabaseName("IX_Messages_TenantId");

        builder.HasIndex(m => m.TicketId)
            .HasDatabaseName("IX_Messages_TicketId");

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}