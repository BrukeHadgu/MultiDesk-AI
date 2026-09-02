using Microsoft.EntityFrameworkCore;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class TenantUserIdGenerator(MultiDeskDbContext context)
{
  public async Task<string> GenerateAsync(
      string role,
      int tenantId,
      CancellationToken ct = default)
  {
    // Get the prefix based on role
    var prefix = role switch
    {
      "Admin" => "ADM",
      "Agent" => "AGT",
      "Student" => "STU",
      _ => "USR"
    };

    // Count existing users of this role in this tenant
    // to determine the next sequential number
    var existingCount = await context.Users
        .Where(u => u.TenantId == tenantId
                 && u.TenantUserId.StartsWith(prefix))
        .CountAsync(ct);

    var nextNumber = existingCount + 1;

    // Format: STU-001, STU-002, ... STU-999, STU-1000
    return $"{prefix}-{nextNumber:D3}";
  }
}