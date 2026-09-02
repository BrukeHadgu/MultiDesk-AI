using MultiDesk.Application.DTOs.Tenants;

namespace MultiDesk.Application.Services;

public interface ITenantService
{
  Task<IReadOnlyList<TenantResponse>> GetAllAsync(
      CancellationToken ct = default);

  Task<TenantResponse?> GetByIdAsync(
      int tenantId,
      CancellationToken ct = default);

  Task<TenantResponse> CreateAsync(
      CreateTenantRequest request,
      CancellationToken ct = default);

  Task<int?> ResolveTenantFromEmailAsync(
      string email,
      CancellationToken ct = default);

  Task<bool> SubdomainExistsAsync(
      string subdomain,
      CancellationToken ct = default);
}