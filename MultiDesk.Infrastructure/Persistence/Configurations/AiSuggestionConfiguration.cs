using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence.Configurations;

public class AiSuggestionConfiguration : IEntityTypeConfiguration<AiSuggestion>
{
    public void Configure(EntityTypeBuilder<AiSuggestion> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.SuggestedText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.AiSuggestions)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_AiSuggestions_TenantId");

        builder.HasIndex(a => a.TicketId)
            .HasDatabaseName("IX_AiSuggestions_TicketId");

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}