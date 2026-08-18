using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Identity;

namespace MultiDesk.Infrastructure.Persistence;

public class MultiDeskSeeder(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<MultiDeskSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        if (await context.Tenants.AnyAsync(ct))
        {
            logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        // ── 1. Tenant ──────────────────────────────────────────────
        var tenant = new Tenant
        {
            Name = "CoTBE University",
            Subdomain = "cotbe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync(ct);

        // ── 2. Departments ─────────────────────────────────────────
        var departments = new List<Department>
        {
            new() { Name = "IT Helpdesk",       Description = "Technical support.", TenantId = tenant.Id },
            new() { Name = "Enrollment Office",  Description = "Registration support.", TenantId = tenant.Id },
            new() { Name = "Library",            Description = "Library resources.", TenantId = tenant.Id },
            new() { Name = "Thesis Office",      Description = "Thesis coordination.", TenantId = tenant.Id }
        };
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync(ct);

        // ── 3. Categories ──────────────────────────────────────────
        var categories = new List<Category>
        {
            new() { Name = "Password Reset",      DepartmentId = departments[0].Id, TenantId = tenant.Id },
            new() { Name = "Software Issue",      DepartmentId = departments[0].Id, TenantId = tenant.Id },
            new() { Name = "Course Registration", DepartmentId = departments[1].Id, TenantId = tenant.Id },
            new() { Name = "Transcript Request",  DepartmentId = departments[1].Id, TenantId = tenant.Id },
            new() { Name = "Book Reservation",    DepartmentId = departments[2].Id, TenantId = tenant.Id },
            new() { Name = "Research Support",    DepartmentId = departments[2].Id, TenantId = tenant.Id },
            new() { Name = "Thesis Submission",   DepartmentId = departments[3].Id, TenantId = tenant.Id },
            new() { Name = "Defense Scheduling",  DepartmentId = departments[3].Id, TenantId = tenant.Id }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync(ct);

        // ── 4. Roles ───────────────────────────────────────────────
        foreach (var role in new[] { "Admin", "Agent", "Student" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── 5. Seed Users via UserManager ──────────────────────────
        var adminUser = new MultiDeskUser
        {
            UserName = "admin@cotbe.edu",
            Email = "admin@cotbe.edu",
            FirstName = "System",
            LastName = "Admin",
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin@12345678!");
        await userManager.AddToRoleAsync(adminUser, "Admin");

        var agentUser = new MultiDeskUser
        {
            UserName = "agent@cotbe.edu",
            Email = "agent@cotbe.edu",
            FirstName = "Dawit",
            LastName = "Bekele",
            Department = "IT Helpdesk",
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(agentUser, "Agent@12345678!");
        await userManager.AddToRoleAsync(agentUser, "Agent");

        var studentUser = new MultiDeskUser
        {
            UserName = "student@cotbe.edu",
            Email = "student@cotbe.edu",
            FirstName = "Liya",
            LastName = "Kebede",
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(studentUser, "Student@12345678!");
        await userManager.AddToRoleAsync(studentUser, "Student");

        logger.LogInformation("Users seeded with Identity.");

        // ── 6. Sample Ticket ───────────────────────────────────────
        var sampleTicket = new Ticket
        {
            Title = "Cannot access student portal",
            Description = "I have been trying to log in for two days.",
            Status = TicketStatus.Open,
            Priority = TicketPriority.High,
            StudentId = studentUser.Id,
            AgentId = agentUser.Id,
            DepartmentId = departments[0].Id,
            CategoryId = categories[0].Id,
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Tickets.Add(sampleTicket);
        await context.SaveChangesAsync(ct);

        // ── 7. Sample Message ──────────────────────────────────────
        var sampleMessage = new Message
        {
            Content = "Hello Liya, I have received your ticket. Which browser are you using?",
            TicketId = sampleTicket.Id,
            SenderId = agentUser.Id,
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Messages.Add(sampleMessage);

        // ── 8. Sample AI Suggestions ───────────────────────────────
        var suggestions = new List<AiSuggestion>
        {
            new() { SuggestedText = "Try clearing your browser cache and cookies.", TicketId = sampleTicket.Id, TenantId = tenant.Id },
            new() { SuggestedText = "Your password may have expired. Use the Forgot Password link.", TicketId = sampleTicket.Id, TenantId = tenant.Id },
            new() { SuggestedText = "Are you accessing from off-campus? You may need a VPN.", TicketId = sampleTicket.Id, TenantId = tenant.Id }
        };
        context.AiSuggestions.AddRange(suggestions);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Database seeding completed.");
    }
}