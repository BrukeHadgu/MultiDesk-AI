using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiDesk.Application.DTOs.Tenants;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class TenantService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<TenantService> logger) : ITenantService
{
  public async Task<IReadOnlyList<TenantResponse>> GetAllAsync(
      CancellationToken ct = default)
  {
    var tenants = await context.Tenants
        .AsNoTracking()
        .ToListAsync(ct);

    var result = new List<TenantResponse>();

    foreach (var tenant in tenants)
    {
      var userCount = await userManager.Users
          .CountAsync(u => u.TenantId == tenant.Id, ct);

      var ticketCount = await context.Tickets
          .IgnoreQueryFilters()
          .CountAsync(t => t.TenantId == tenant.Id, ct);

      var openCount = await context.Tickets
          .IgnoreQueryFilters()
          .CountAsync(t => t.TenantId == tenant.Id
                        && t.Status == TicketStatus.Open, ct);

      result.Add(new TenantResponse(
          tenant.Id,
          tenant.Name,
          tenant.Subdomain,
          tenant.EmailDomain,
          tenant.IsActive,
          tenant.CreatedAt,
          userCount,
          ticketCount,
          openCount));
    }

    return result;
  }

  public async Task<TenantResponse?> GetByIdAsync(
      int tenantId, CancellationToken ct = default)
  {
    var tenant = await context.Tenants
        .AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

    if (tenant is null) return null;

    var userCount = await userManager.Users
        .CountAsync(u => u.TenantId == tenantId, ct);

    var ticketCount = await context.Tickets
        .IgnoreQueryFilters()
        .CountAsync(t => t.TenantId == tenantId, ct);

    var openCount = await context.Tickets
        .IgnoreQueryFilters()
        .CountAsync(t => t.TenantId == tenantId
                      && t.Status == TicketStatus.Open, ct);

    return new TenantResponse(
        tenant.Id,
        tenant.Name,
        tenant.Subdomain,
        tenant.EmailDomain,
        tenant.IsActive,
        tenant.CreatedAt,
        userCount,
        ticketCount,
        openCount);
  }

  public async Task<TenantResponse> CreateAsync(
      CreateTenantRequest request,
      CancellationToken ct = default)
  {
    // Check subdomain is unique
    if (await SubdomainExistsAsync(request.Subdomain, ct))
      throw new InvalidOperationException(
          $"Subdomain '{request.Subdomain}' is already taken.");

    // Check email domain is unique
    var domainExists = await context.Tenants
        .AnyAsync(t => t.EmailDomain == request.EmailDomain, ct);

    if (domainExists)
      throw new InvalidOperationException(
          $"Email domain '{request.EmailDomain}' is already registered.");

    // 1. Create the tenant
    var tenant = new Tenant
    {
      Name = request.Name,
      Subdomain = request.Subdomain,
      EmailDomain = request.EmailDomain,
      IsActive = true,
      CreatedAt = DateTime.UtcNow
    };

    context.Tenants.Add(tenant);
    await context.SaveChangesAsync(ct);

    logger.LogInformation(
        "Tenant created: {TenantName} (Id: {TenantId})",
        tenant.Name, tenant.Id);

    // 2. Seed default departments for this tenant
    await SeedDefaultDepartmentsAsync(tenant.Id, ct);

    // 3. Ensure roles exist
    foreach (var role in new[] { "Admin", "Agent", "Student" })
    {
      if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));
    }

    // 4. Create the first admin for this tenant
    var adminTenantUserId = $"ADM-001";

    var admin = new MultiDeskUser
    {
      UserName = request.AdminEmail,
      Email = request.AdminEmail,
      FirstName = request.AdminFirstName,
      LastName = request.AdminLastName,
      TenantId = tenant.Id,
      IsActive = true,
      EmailConfirmed = true,
      TenantUserId = adminTenantUserId,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    var result = await userManager.CreateAsync(admin, request.AdminPassword);
    if (!result.Succeeded)
    {
      var errors = string.Join(", ",
          result.Errors.Select(e => e.Description));
      throw new InvalidOperationException(
          $"Failed to create admin user: {errors}");
    }

    await userManager.AddToRoleAsync(admin, "Admin");

    logger.LogInformation(
        "Admin user created for tenant {TenantId}: {Email}",
        tenant.Id, request.AdminEmail);

    return (await GetByIdAsync(tenant.Id, ct))!;
  }

  public async Task<int?> ResolveTenantFromEmailAsync(
      string email, CancellationToken ct = default)
  {
    // Extract domain from email: "student@cotbe.edu" → "cotbe.edu"
    var atIndex = email.IndexOf('@');
    if (atIndex < 0) return null;

    var domain = email[(atIndex + 1)..].ToLower();

    var tenant = await context.Tenants
        .AsNoTracking()
        .FirstOrDefaultAsync(
            t => t.EmailDomain == domain && t.IsActive, ct);

    return tenant?.Id;
  }

  public async Task<bool> SubdomainExistsAsync(
      string subdomain, CancellationToken ct = default) =>
      await context.Tenants
          .AnyAsync(t => t.Subdomain == subdomain, ct);

//private helpers

  private async Task SeedDefaultDepartmentsAsync(
      int tenantId, CancellationToken ct)
  {
    var departments = new List<Department>
        {
            new() { Name = "IT Helpdesk",       Description = "Technical support.",      TenantId = tenantId },
            new() { Name = "Enrollment Office",  Description = "Registration support.",   TenantId = tenantId },
            new() { Name = "Library",            Description = "Library resources.",      TenantId = tenantId },
            new() { Name = "Thesis Office",      Description = "Thesis coordination.",    TenantId = tenantId }
        };

    context.Departments.AddRange(departments);
    await context.SaveChangesAsync(ct);

    // Seed default categories with priorities
    var categories = new List<Category>
        {
            new() { Name = "Password Reset",      DefaultPriority = TicketPriority.Medium, DepartmentId = departments[0].Id, TenantId = tenantId },
            new() { Name = "Software Issue",      DefaultPriority = TicketPriority.High,   DepartmentId = departments[0].Id, TenantId = tenantId },
            new() { Name = "Course Registration", DefaultPriority = TicketPriority.High,   DepartmentId = departments[1].Id, TenantId = tenantId },
            new() { Name = "Transcript Request",  DefaultPriority = TicketPriority.Low,    DepartmentId = departments[1].Id, TenantId = tenantId },
            new() { Name = "Book Reservation",    DefaultPriority = TicketPriority.Low,    DepartmentId = departments[2].Id, TenantId = tenantId },
            new() { Name = "Research Support",    DefaultPriority = TicketPriority.Medium, DepartmentId = departments[2].Id, TenantId = tenantId },
            new() { Name = "Thesis Submission",   DefaultPriority = TicketPriority.High,   DepartmentId = departments[3].Id, TenantId = tenantId },
            new() { Name = "Defense Scheduling",  DefaultPriority = TicketPriority.Urgent, DepartmentId = departments[3].Id, TenantId = tenantId }
        };

    context.Categories.AddRange(categories);
    await context.SaveChangesAsync(ct);

    logger.LogInformation(
        "Seeded default departments and categories for tenant {TenantId}",
        tenantId);
  }
}