using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Application.DTOs.Tenants;
using MultiDesk.Application.Services;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Roles = "SuperAdmin")]
[Produces("application/json")]
[Tags("Tenants (SuperAdmin)")]
public class TenantsController(
    ITenantService tenantService) : ControllerBase
{
  [HttpGet]
  [ProducesResponseType(typeof(IReadOnlyList<TenantResponse>), StatusCodes.Status200OK)]
  [EndpointSummary("Get all tenants — SuperAdmin only")]
  public async Task<IActionResult> GetAll(CancellationToken ct)
  {
    var tenants = await tenantService.GetAllAsync(ct);
    return Ok(tenants);
  }

  [HttpGet("{id:int}")]
  [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("Get a tenant by ID — SuperAdmin only")]
  public async Task<IActionResult> GetById(int id, CancellationToken ct)
  {
    var tenant = await tenantService.GetByIdAsync(id, ct);
    return tenant is null ? NotFound() : Ok(tenant);
  }

  [HttpPost]
  [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [EndpointSummary("Create a new tenant — SuperAdmin only")]
  [EndpointDescription(
      "Creates a new university tenant with default departments, " +
      "categories, and the first admin user.")]
  public async Task<IActionResult> Create(
      [FromBody] CreateTenantRequest request,
      CancellationToken ct)
  {
    var tenant = await tenantService.CreateAsync(request, ct);
    return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
  }

  // Public — no auth required — Angular needs this before login
  [HttpGet("{id:int}/branding")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [EndpointSummary("Get tenant branding info — public")]
  public async Task<IActionResult> GetBranding(
      int id,
      CancellationToken ct)
  {
    var tenant = await tenantService.GetByIdAsync(id, ct);
    if (tenant is null) return NotFound();

    return Ok(new
    {
      tenantId = tenant.Id,
      name = tenant.Name,
      subdomain = tenant.Subdomain,
      logoUrl = $"assets/logos/{tenant.Subdomain}.png",
      fallbackLogo = "assets/logos/default.svg",
      primaryColor = "#3f51b5"   // default
    });
  }
}