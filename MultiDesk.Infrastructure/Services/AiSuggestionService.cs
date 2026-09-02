using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiDesk.Application.DTOs.AiSuggestions;
using MultiDesk.Application.DTOs.Messages;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Persistence;
using MultiDesk.Infrastructure.Identity;

namespace MultiDesk.Infrastructure.Services;

public class AiSuggestionService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager,
    IMessageService messageService,
    ILogger<AiSuggestionService> logger) : IAiSuggestionService
{
    public async Task<IReadOnlyList<AiSuggestionResponse>> GenerateSuggestionsAsync(
        int ticketId, int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId && t.TenantId == tenantId)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .Include(t => t.AiSuggestions)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        // Load student name separately
        var student = await userManager.FindByIdAsync(ticket.StudentId);
        var studentName = student?.FullName ?? "Student";

        var similarTickets = await context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId
                     && t.Status == TicketStatus.Resolved
                     && t.DepartmentId == ticket.DepartmentId
                     && t.CategoryId == ticket.CategoryId
                     && t.Id != ticketId)
            .Include(t => t.Messages)
            .OrderByDescending(t => t.ResolvedAt)
            .Take(5)
            .ToListAsync(ct);

        var prompt = BuildPrompt(ticket, studentName, similarTickets);
        var suggestions = await CallOpenAiAsync(prompt, ct);

        var oldSuggestions = await context.AiSuggestions
            .Where(a => a.TicketId == ticketId)
            .ToListAsync(ct);
        context.AiSuggestions.RemoveRange(oldSuggestions);

        var entities = suggestions.Select(text => new AiSuggestion
        {
            SuggestedText = text,
            TicketId = ticketId,
            TenantId = tenantId,
            Accepted = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        context.AiSuggestions.AddRange(entities);
        await context.SaveChangesAsync(ct);

        return entities.Select(a => new AiSuggestionResponse(
            a.Id, a.SuggestedText, a.Accepted, a.CreatedAt)).ToList();
    }

    public async Task<AiSuggestionResponse> AcceptSuggestionAsync(
        int suggestionId,
        string agentId,
        int tenantId,
        CancellationToken ct = default)
    {
        var suggestion = await context.AiSuggestions
            .Include(a => a.Ticket)
            .FirstOrDefaultAsync(a => a.Id == suggestionId
                                   && a.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Suggestion {suggestionId} not found.");

        // Mark as accepted
        suggestion.Accepted = true;
        await context.SaveChangesAsync(ct);

        // Create a real message from the accepted suggestion
        await messageService.CreateAsync(
            suggestion.TicketId,
            new CreateMessageRequest { Content = suggestion.SuggestedText },
            agentId,
            "Agent",
            tenantId,
            ct);

        logger.LogInformation(
            "Agent {AgentId} accepted suggestion {SuggestionId} for ticket {TicketId}",
            agentId, suggestionId, suggestion.TicketId);

        return new AiSuggestionResponse(
            suggestion.Id,
            suggestion.SuggestedText,
            suggestion.Accepted,
            suggestion.CreatedAt);
    }

    public async Task<IReadOnlyList<AiSuggestionResponse>> GetByTicketAsync(
        int ticketId,
        int tenantId,
        CancellationToken ct = default) =>
        await context.AiSuggestions
            .AsNoTracking()
            .Where(a => a.TicketId == ticketId && a.TenantId == tenantId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AiSuggestionResponse(
                a.Id,
                a.SuggestedText,
                a.Accepted,
                a.CreatedAt))
            .ToListAsync(ct);

    // ── Private Helpers ──────────────────────────────────────────────

    private static string BuildPrompt(
     Ticket ticket,
     string studentName,
     IReadOnlyList<Ticket> similarTickets)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("You are a helpful university support agent.");
        sb.AppendLine("Generate exactly 3 professional reply suggestions.");
        sb.AppendLine("Each on a new line prefixed with 'SUGGESTION:'");
        sb.AppendLine();
        sb.AppendLine($"Student: {studentName}");
        sb.AppendLine($"Department: {ticket.Department.Name}");
        sb.AppendLine($"Category: {ticket.Category.Name}");
        sb.AppendLine($"Title: {ticket.Title}");
        sb.AppendLine($"Description: {ticket.Description}");

        if (ticket.Messages.Any())
        {
            sb.AppendLine("\nConversation so far:");
            foreach (var message in ticket.Messages)
                sb.AppendLine($"  Message: {message.Content}");
        }

        sb.AppendLine("\nGenerate 3 SUGGESTION: replies:");
        return sb.ToString();
    }
    private Task<List<string>> CallOpenAiAsync(
    string prompt,
    CancellationToken ct)
    {
        // Mock AI responses — realistic enough for demo and development
        // Replace this method body with real OpenAI call when API key is available
        var suggestions = new List<string>
        {
            $"Thank you for reaching out to us. I have reviewed your request and I am looking into this matter right away. Could you please confirm your student ID so I can locate your account and resolve this as quickly as possible?",

            $"Hello, I understand how frustrating this must be for you. Our team has been notified about this issue and we are working to resolve it. In the meantime, please try clearing your browser cache and attempting again. I will follow up with you within 24 hours.",

            $"I appreciate your patience. Based on similar cases we have handled recently, this issue is typically resolved by resetting your account credentials. I will send you a password reset link to your registered email address shortly. Please check your spam folder if you do not see it within 5 minutes."
        };

        return Task.FromResult(suggestions);
    }
}