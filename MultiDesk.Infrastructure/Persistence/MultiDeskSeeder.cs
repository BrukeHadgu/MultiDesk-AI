using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;

namespace MultiDesk.Infrastructure.Persistence;

public class MultiDeskSeeder(
    MultiDeskDbContext context,
    ILogger<MultiDeskSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Apply any pending migrations first
        await context.Database.MigrateAsync(ct);

        // Only seed if database is empty
        if (await context.Tenants.AnyAsync(ct))
        {
            logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        // ── 1. Tenant ──────────────────────────────────────────────
        var tenant = new Tenant
        {
            Name      = "CoTBE University",
            Subdomain = "cotbe",
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Tenant created: {TenantName}", tenant.Name);

        // ── 2. Departments ─────────────────────────────────────────
        var departments = new List<Department>
        {
            new() { Name = "IT Helpdesk",      Description = "Technical support for hardware, software, and network issues.",  TenantId = tenant.Id },
            new() { Name = "Enrollment Office", Description = "Support for registration, course enrollment, and academic records.", TenantId = tenant.Id },
            new() { Name = "Library",           Description = "Library resources, book reservations, and research support.",  TenantId = tenant.Id },
            new() { Name = "Thesis Office",     Description = "Thesis submission, supervision, and defense coordination.",    TenantId = tenant.Id }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Departments seeded: {Count}", departments.Count);

        // ── 3. Categories (2 per department) ───────────────────────
        var categories = new List<Category>
        {
            // IT Helpdesk
            new() { Name = "Password Reset",     Description = "Account and password recovery.",          DepartmentId = departments[0].Id, TenantId = tenant.Id },
            new() { Name = "Software Issue",     Description = "Problems with university software tools.", DepartmentId = departments[0].Id, TenantId = tenant.Id },

            // Enrollment Office
            new() { Name = "Course Registration", Description = "Help with adding or dropping courses.",   DepartmentId = departments[1].Id, TenantId = tenant.Id },
            new() { Name = "Transcript Request",  Description = "Official academic transcript requests.",  DepartmentId = departments[1].Id, TenantId = tenant.Id },

            // Library
            new() { Name = "Book Reservation",   Description = "Reserve physical or digital library books.", DepartmentId = departments[2].Id, TenantId = tenant.Id },
            new() { Name = "Research Support",   Description = "Help finding academic sources and databases.", DepartmentId = departments[2].Id, TenantId = tenant.Id },

            // Thesis Office
            new() { Name = "Thesis Submission",  Description = "Submit thesis drafts and final documents.", DepartmentId = departments[3].Id, TenantId = tenant.Id },
            new() { Name = "Defense Scheduling", Description = "Schedule and coordinate thesis defense.",   DepartmentId = departments[3].Id, TenantId = tenant.Id }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Categories seeded: {Count}", categories.Count);

        // ── 4. Users ───────────────────────────────────────────────
        var users = new List<User>
        {
            // Admin
            new()
            {
                Email        = "admin@cotbe.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName    = "System",
                LastName     = "Admin",
                Role         = UserRole.Admin,
                IsActive     = true,
                TenantId     = tenant.Id,
                DepartmentId = null
            },

            // Agent — assigned to IT Helpdesk
            new()
            {
                Email        = "agent@cotbe.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                FirstName    = "Dawit",
                LastName     = "Bekele",
                Role         = UserRole.Agent,
                IsActive     = true,
                TenantId     = tenant.Id,
                DepartmentId = departments[0].Id   // IT Helpdesk
            },

            // Student
            new()
            {
                Email        = "student@cotbe.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                FirstName    = "Liya",
                LastName     = "Kebede",
                Role         = UserRole.Student,
                IsActive     = true,
                TenantId     = tenant.Id,
                DepartmentId = null
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Users seeded: {Count}", users.Count);

        // ── 5. Sample Ticket ───────────────────────────────────────
        var sampleTicket = new Ticket
        {
            Title        = "Cannot access student portal",
            Description  = "I have been trying to log in to the student portal for two days but keep getting an error. My student ID is LK-2024-001.",
            Status       = TicketStatus.Open,
            Priority     = TicketPriority.High,
            StudentId    = users[2].Id,     // Liya
            AgentId      = users[1].Id,     // Dawit
            DepartmentId = departments[0].Id,
            CategoryId   = categories[0].Id,
            TenantId     = tenant.Id,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        context.Tickets.Add(sampleTicket);
        await context.SaveChangesAsync(ct);

        // ── 6. Sample Message on the Ticket ───────────────────────
        var sampleMessage = new Message
        {
            Content  = "Hello Liya, I have received your ticket. Can you please tell me which browser you are using and what error message appears?",
            TicketId = sampleTicket.Id,
            SenderId = users[1].Id,     // Dawit (agent)
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Messages.Add(sampleMessage);
        await context.SaveChangesAsync(ct);

        // ── 7. Sample AI Suggestions ───────────────────────────────
        var suggestions = new List<AiSuggestion>
        {
            new()
            {
                SuggestedText = "Thank you for reaching out. Please try clearing your browser cache and cookies, then attempt to log in again. If the issue persists, try a different browser.",
                TicketId  = sampleTicket.Id,
                TenantId  = tenant.Id,
                Accepted  = false
            },
            new()
            {
                SuggestedText = "Hello, I can see your account in our system. It appears your password may have expired. Please use the 'Forgot Password' link on the login page to reset it.",
                TicketId  = sampleTicket.Id,
                TenantId  = tenant.Id,
                Accepted  = false
            },
            new()
            {
                SuggestedText = "Hi Liya, could you please confirm whether you are trying to access the portal from on-campus or off-campus? Some services require a VPN connection when accessed remotely.",
                TicketId  = sampleTicket.Id,
                TenantId  = tenant.Id,
                Accepted  = false
            }
        };

        context.AiSuggestions.AddRange(suggestions);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Sample ticket seeded with {SuggestionCount} AI suggestions.",
            suggestions.Count);

        logger.LogInformation("Database seeding completed successfully.");
    }
}