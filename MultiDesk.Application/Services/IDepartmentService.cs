using MultiDesk.Application.DTOs.Departments;

namespace MultiDesk.Application.Services;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        int tenantId,
        CancellationToken ct = default);

    Task<DepartmentResponse?> GetByIdAsync(
        int departmentId,
        int tenantId,
        CancellationToken ct = default);

    Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request,
        int tenantId,
        CancellationToken ct = default);

    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        int departmentId,
        int tenantId,
        CancellationToken ct = default);
}