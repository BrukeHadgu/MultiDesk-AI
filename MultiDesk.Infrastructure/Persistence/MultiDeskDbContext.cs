using Microsoft.EntityFrameworkCore;
using MultiDesk.Domain.Entities;

namespace MultiDesk.Infrastructure.Persistence;

public class MultiDeskDbContext(DbContextOptions<MultiDeskDbContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<AiSuggestion> AiSuggestions => Set<AiSuggestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MultiDeskDbContext).Assembly);
    }
}