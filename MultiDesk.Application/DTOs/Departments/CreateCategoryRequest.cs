using System.ComponentModel.DataAnnotations;

namespace MultiDesk.Application.DTOs.Departments;

public record CreateCategoryRequest
{
  [Required]
  [MaxLength(100)]
  public required string Name { get; init; }

  [MaxLength(500)]
  public string? Description { get; init; }

  public string DefaultPriority { get; init; } = "Medium";
}