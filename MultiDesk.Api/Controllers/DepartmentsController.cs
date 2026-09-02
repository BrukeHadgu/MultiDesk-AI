using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Api.Extensions;
using MultiDesk.Application.DTOs.Departments;
using MultiDesk.Application.Services;
using MultiDesk.Application.Interfaces;
using MultiDesk.Application.DTOs.Users;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
[Produces("application/json")]
[Tags("Departments")]
public class DepartmentsController(
    IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all departments for the tenant")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var result   = await departmentService.GetAllAsync(tenantId, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get department by ID")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var dept     = await departmentService.GetByIdAsync(id, tenantId, ct);
        return dept is null ? NotFound() : Ok(dept);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
    [EndpointSummary("Create a new department (Admin only)")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var dept     = await departmentService.CreateAsync(request, tenantId, ct);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, dept);
    }

    [HttpGet("{id:int}/categories")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Get categories for a department")]
    public async Task<IActionResult> GetCategories(int id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var categories = await departmentService.GetCategoriesAsync(id, tenantId, ct);
        return Ok(categories);
    }

    [HttpPost("{departmentId:int}/categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [EndpointSummary("Create a category in a department (Admin only)")]
    public async Task<IActionResult> CreateCategory(
    int departmentId,
    [FromBody] CreateCategoryRequest request,
    CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var category = await departmentService.CreateCategoryAsync(
            departmentId, request, tenantId, ct);
        return CreatedAtAction(
            nameof(GetCategories),
            new { id = departmentId },
            category);
    }

    [HttpGet("{id:int}/agents")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all agents in a department")]
    public async Task<IActionResult> GetAgents(
    int id,
    [FromServices] IUserRepository userRepository,
    CancellationToken ct)
    {
        var agents = await userRepository.GetByDepartmentAsync(id, ct);
        return Ok(agents);
    }
}