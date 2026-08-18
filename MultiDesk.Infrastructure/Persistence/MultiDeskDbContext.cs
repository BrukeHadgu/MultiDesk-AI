using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Domain.Entities;
using MultiDesk.Infrastructure.Identity;

namespace MultiDesk.Infrastructure.Persistence;

public class MultiDeskDbContext(DbContextOptions<MultiDeskDbContext> options)
    : IdentityDbContext<MultiDeskUser>(options)
{
    // Domain entities
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<AiSuggestion> AiSuggestions => Set<AiSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MUST call base first — Identity needs this
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MultiDeskDbContext).Assembly);
    }
}