using MultiDesk.Application.DTOs.AiSuggestions;
using MultiDesk.Application.DTOs.Messages;

namespace MultiDesk.Application.DTOs.Tickets;

// Used in detail view — includes full conversation and AI suggestions
public record TicketDetailResponse(
    int Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string DepartmentName,
    string CategoryName,
    int StudentId,
    string StudentName,
    string StudentEmail,
    int? AgentId,
    string? AgentName,
    string? AgentEmail,
    string? AttachmentPath,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    IReadOnlyList<MessageResponse> Messages,
    IReadOnlyList<AiSuggestionResponse> AiSuggestions);