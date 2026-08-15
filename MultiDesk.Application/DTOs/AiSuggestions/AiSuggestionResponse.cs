namespace MultiDesk.Application.DTOs.AiSuggestions;

public record AiSuggestionResponse(
    int Id,
    string SuggestedText,
    bool Accepted,
    DateTime CreatedAt);