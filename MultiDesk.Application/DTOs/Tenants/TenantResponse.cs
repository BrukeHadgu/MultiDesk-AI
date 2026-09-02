namespace MultiDesk.Application.DTOs.Tenants;

public record TenantResponse(
    int Id,
    string Name,
    string Subdomain,
    string EmailDomain,
    bool IsActive,
    DateTime CreatedAt,
    int UserCount,
    int TicketCount,
    int OpenTicketCount);