using MultiDesk.Application.DTOs.AiSuggestions;

namespace MultiDesk.Application.Services;

public interface IAiSuggestionService
{   //generate suggestions for a ticket using AI
    Task<IReadOnlyList<AiSuggestionResponse>> GenerateSuggestionsAsync(
        int ticketId,
        int tenantId,
        CancellationToken ct = default);

    // agent accepts a suggestion, which will be added to the ticket's messages and marked as accepted
    Task<AiSuggestionResponse> AcceptSuggestionAsync(
        int suggestionId,
        int agentId,
        int tenantId,
        CancellationToken ct = default);

    // get a suggestions for a ticket
    Task<IReadOnlyList<AiSuggestionResponse>> GetByTicketAsync(
        int ticketId,
        int tenantId,
        CancellationToken ct = default);
}