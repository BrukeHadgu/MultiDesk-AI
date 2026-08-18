using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Api.Extensions;
using MultiDesk.Application.DTOs.AiSuggestions;
using MultiDesk.Application.Services;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/ai-suggestions")]
[Authorize(Roles = "Agent,Admin")]
[Produces("application/json")]
[Tags("AI Suggestions")]
public class AiSuggestionsController(
    IAiSuggestionService aiSuggestionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<AiSuggestionResponse>),
        StatusCodes.Status200OK)]
    [EndpointSummary("Get existing AI suggestions for a ticket")]
    public async Task<IActionResult> GetSuggestions(
        int ticketId, CancellationToken ct)
    {
        var tenantId    = User.GetTenantId();
        var suggestions = await aiSuggestionService
            .GetByTicketAsync(ticketId, tenantId, ct);
        return Ok(suggestions);
    }

    [HttpPost("generate")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AiSuggestionResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Generate 3 AI reply suggestions using OpenAI")]
    [EndpointDescription(
        "Uses the OpenAI API to generate 3 AI reply suggestions for a ticket. " +
        "The suggestions are based on the ticket's messages and context.")]
    public async Task<IActionResult> Generate(
        int ticketId, CancellationToken ct)
    {
        var tenantId    = User.GetTenantId();
        var suggestions = await aiSuggestionService
            .GenerateSuggestionsAsync(ticketId, tenantId, ct);
        return Ok(suggestions);
    }

    [HttpPost("{suggestionId:int}/accept")]
    [ProducesResponseType(
        typeof(AiSuggestionResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Accept a suggestion create it as a message reply")]
    public async Task<IActionResult> Accept(
        int ticketId,
        int suggestionId,
        CancellationToken ct)
    {
        var agentId  = User.GetUserId();
        var tenantId = User.GetTenantId();
        var result   = await aiSuggestionService
            .AcceptSuggestionAsync(suggestionId, agentId, tenantId, ct);
        return Ok(result);
    }
}