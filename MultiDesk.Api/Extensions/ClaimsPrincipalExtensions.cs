using System.Security.Claims;

namespace MultiDesk.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static int GetTenantId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue("tenantId")!);

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role)!;
}