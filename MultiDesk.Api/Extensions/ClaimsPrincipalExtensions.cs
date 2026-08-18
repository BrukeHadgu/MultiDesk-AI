using System.Security.Claims;

namespace MultiDesk.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException(
            "The authenticated user has no NameIdentifier claim.");

    public static int GetTenantId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue("tenantId")
        ?? throw new InvalidOperationException(
            "The authenticated user has no tenantId claim."));

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role)
        ?? throw new InvalidOperationException(
            "The authenticated user has no role claim.");
}