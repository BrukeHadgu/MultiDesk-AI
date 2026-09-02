using System.ComponentModel.DataAnnotations;

namespace MultiDesk.Application.DTOs.Tenants;

public record CreateTenantRequest
{
  [Required]
  [MaxLength(200)]
  public required string Name { get; init; }

  [Required]
  [MaxLength(100)]
  public required string Subdomain { get; init; }

  [Required]
  [MaxLength(100)]
  public required string EmailDomain { get; init; }

  // First admin email for this tenant
  [Required]
  [EmailAddress]
  public required string AdminEmail { get; init; }

  [Required]
  public required string AdminFirstName { get; init; }

  [Required]
  public required string AdminLastName { get; init; }

  [Required]
  [MinLength(12)]
  public required string AdminPassword { get; init; }
}