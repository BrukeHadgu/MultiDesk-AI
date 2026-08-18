namespace MultiDesk.Application.DTOs.Messages;

public record MessageResponse(
    int Id,
    string Content,
    string SenderId,
    string SenderName,
    string SenderRole,
    DateTime CreatedAt);