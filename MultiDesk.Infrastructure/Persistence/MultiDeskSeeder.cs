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

        logger.LogInformation("Seeding database with three tenants...");

        //roles
        foreach (var role in new[]
            { "SuperAdmin", "Admin", "Agent", "Student" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // superadmin user
        var superAdmin = new MultiDeskUser
        {
            UserName       = "superadmin@multidesk.io",
            Email          = "superadmin@multidesk.io",
            FirstName      = "Super",
            LastName       = "Admin",
            TenantId       = 0,   // 0 = platform level, not a tenant
            IsActive       = true,
            EmailConfirmed = true,
            TenantUserId   = "SA-001",
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };
        await userManager.CreateAsync(superAdmin, "SuperAdmin@12345678!");
        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");

        logger.LogInformation("SuperAdmin created.");

        // tenant 1
        var cotbe = await CreateTenantAsync(
            name: "CoTBE University",
            subdomain: "cotbe",
            emailDomain: "cotbe.edu",
            adminEmail: "admin@cotbe.edu",
            adminPass: "Admin@12345678!",
            adminFirst: "System",
            adminLast: "Admin",
            tenantUserId: "ADM-001",
            ct);

        // tenant 2
        var aau = await CreateTenantAsync(
            name:        "Addis Ababa University",
            subdomain:   "aau",
            emailDomain: "aau.edu",
            adminEmail:  "admin@aau.edu",
            adminPass:   "Admin@12345678!",
            adminFirst:  "AAU",
            adminLast:   "Admin",
            tenantUserId: "ADM-001",
            ct);

        // tenant 3
        var unity = await CreateTenantAsync(
            name:        "Unity University",
            subdomain:   "unity",
            emailDomain: "unity.edu",
            adminEmail:  "admin@unity.edu",
            adminPass:   "Admin@12345678!",
            adminFirst:  "Unity",
            adminLast:   "Admin",
            tenantUserId: "ADM-001",
            ct);

        // seed test user for cotbe,aau and unity
        await SeedTestUsersAsync(cotbe.Id, "cotbe.edu", ct);
        await SeedTestUsersAsync(aau.Id, "aau.edu", ct);
        await SeedTestUsersAsync(unity.Id, "unity.edu", ct);

        logger.LogInformation(
            "Seeding complete. Three tenants created: CoTBE, AAU, Unity.");
    }

    //  p helpers

    private async Task<Tenant> CreateTenantAsync(
        string name, string subdomain, string emailDomain,
        string adminEmail, string adminPass,
        string adminFirst, string adminLast,
        string tenantUserId,
        CancellationToken ct)
    {
        var tenant = new Tenant
        {
            Name        = name,
            Subdomain   = subdomain,
            EmailDomain = emailDomain,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync(ct);

        // Departments
        var departments = new List<Department>
        {
            new() { Name = "IT Helpdesk",      Description = "Technical support.",    TenantId = tenant.Id },
            new() { Name = "Enrollment Office", Description = "Registration support.", TenantId = tenant.Id },
            new() { Name = "Library",           Description = "Library resources.",    TenantId = tenant.Id },
            new() { Name = "Thesis Office",     Description = "Thesis coordination.",  TenantId = tenant.Id }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync(ct);

        // Categories with priorities
        var categories = new List<Category>
        {
            new() { Name = "Password Reset",      DefaultPriority = TicketPriority.Medium, DepartmentId = departments[0].Id, TenantId = tenant.Id },
            new() { Name = "Software Issue",      DefaultPriority = TicketPriority.High,   DepartmentId = departments[0].Id, TenantId = tenant.Id },
            new() { Name = "Course Registration", DefaultPriority = TicketPriority.High,   DepartmentId = departments[1].Id, TenantId = tenant.Id },
            new() { Name = "Transcript Request",  DefaultPriority = TicketPriority.Low,    DepartmentId = departments[1].Id, TenantId = tenant.Id },
            new() { Name = "Book Reservation",    DefaultPriority = TicketPriority.Low,    DepartmentId = departments[2].Id, TenantId = tenant.Id },
            new() { Name = "Research Support",    DefaultPriority = TicketPriority.Medium, DepartmentId = departments[2].Id, TenantId = tenant.Id },
            new() { Name = "Thesis Submission",   DefaultPriority = TicketPriority.High,   DepartmentId = departments[3].Id, TenantId = tenant.Id },
            new() { Name = "Defense Scheduling",  DefaultPriority = TicketPriority.Urgent, DepartmentId = departments[3].Id, TenantId = tenant.Id }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync(ct);

        // First admin for this tenant
        var admin = new MultiDeskUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = adminFirst,
            LastName = adminLast,
            TenantId = tenant.Id,
            IsActive = true,
            EmailConfirmed = true,
            TenantUserId = tenantUserId,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userManager.CreateAsync(admin, adminPass);
        await userManager.AddToRoleAsync(admin, "Admin");

        logger.LogInformation(
            "Tenant created: {Name} (Id: {Id}, Domain: {Domain})",
            tenant.Name, tenant.Id, tenant.EmailDomain);

        return tenant;
    }

    private async Task SeedTestUsersAsync(
        int tenantId, string emailDomain,
        CancellationToken ct)
    {
        // Agent
        var agent = new MultiDeskUser
        {
            UserName = $"agent@{emailDomain}",
            Email = $"agent@{emailDomain}",
            FirstName = "Test",
            LastName = "Agent",
            TenantId = tenantId,
            IsActive = true,
            EmailConfirmed = true,
            TenantUserId = "AGT-001",
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(agent, "Agent@12345678!");
        await userManager.AddToRoleAsync(agent, "Agent");

        // Student
        var student = new MultiDeskUser
        {
            UserName       = $"student@{emailDomain}",
            Email          = $"student@{emailDomain}",
            FirstName      = "Test",
            LastName       = "Student",
            TenantId       = tenantId,
            IsActive       = true,
            EmailConfirmed = true,
            TenantUserId   = "STU-001",
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };
        await userManager.CreateAsync(student, "Student@12345678!");
        await userManager.AddToRoleAsync(student, "Student");

        logger.LogInformation(
            "Test users seeded for tenant {TenantId} ({Domain})",
            tenantId, emailDomain);
    }
}