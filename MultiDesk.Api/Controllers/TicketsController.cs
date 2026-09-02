using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiDesk.Api.Extensions;
using MultiDesk.Application.DTOs.Messages;
using MultiDesk.Application.DTOs.Tickets;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace MultiDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
[Produces("application/json")]
[Tags("Tickets")]
public class TicketsController(
    ITicketService ticketService,
    IMessageService messageService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedTicketResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Get paginated list of tickets")]
    public async Task<IActionResult> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] TicketStatus? status = null,
    CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        var role = User.GetRole();
        var userId = User.GetUserId();

        // Students only see their own tickets
        // Agents and Admins see all tenant tickets
        var studentId = role == "Student" ? userId : null;

        var result = await ticketService.GetPagedAsync(
            tenantId, page, pageSize, status, studentId, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var userId = User.GetUserId();
        var role = User.GetRole();

        var ticket = await ticketService.GetByIdAsync(
            id, tenantId, userId, role, ct);
        return ticket is null ? NotFound() : Ok(ticket);
    }
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken ct)
    {
        var studentId = User.GetUserId();   // now returns string
        var tenantId = User.GetTenantId();
        var ticket = await ticketService.CreateAsync(
            request, studentId, tenantId, ct);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Agent,Admin")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update ticket details")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var ticket   = await ticketService.UpdateAsync(id, request, tenantId, ct);
        return Ok(ticket);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Delete a ticket")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var deletedBy = User.GetUserId();   // now returns string
        await ticketService.DeleteAsync(id, tenantId, deletedBy, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/assign")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Admin assigns an agent from the ticket's department")]
    public async Task<IActionResult> AssignAgent(
        int id,
        [FromQuery] string agentId,   // string GUID now
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var ticket = await ticketService.AssignAgentAsync(
            id, agentId, tenantId, ct);
        return Ok(ticket);
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Roles = "Agent,Admin")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Change ticket status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromQuery] TicketStatus status,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var ticket   = await ticketService.ChangeStatusAsync(
            id, status, tenantId, ct);
        return Ok(ticket);
    }

    // message endpoints for a ticket

    [HttpGet("{id:int}/messages")]
    [ProducesResponseType(typeof(IReadOnlyList<MessageResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all messages for a ticket")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken ct)
    {
        var tenantId  = User.GetTenantId();
        var messages  = await messageService.GetByTicketAsync(id, tenantId, ct);
        return Ok(messages);
    }

    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> AddMessage(
    int id,
    [FromBody] CreateMessageRequest request,
    CancellationToken ct)
    {
        var senderId = User.GetUserId();
        var senderRole = User.GetRole();
        var tenantId = User.GetTenantId();

        var message = await messageService.CreateAsync(
            id, request, senderId, senderRole, tenantId, ct);
        return CreatedAtAction(nameof(GetMessages), new { id }, message);
    }

    [HttpPost("{id:int}/claim")]
    [Authorize(Roles = "Agent")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [EndpointSummary("Agent claims an unassigned ticket in their department")]
    public async Task<IActionResult> ClaimTicket(
    int id,
    [FromServices] UserManager<MultiDeskUser> userManager,
    CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var agentId = User.GetUserId();

        // Verify agent belongs to the ticket's department
        var agent = await userManager.FindByIdAsync(agentId);
        var ticket = await ticketService.GetByIdAsync(id, tenantId, ct: ct);

        if (ticket is null)
            return NotFound();

        if (agent?.DepartmentId == null)
            return Forbid();

        // Get department name to compare
        var agentDeptName = ticket.DepartmentName;

        // Assign agent and move to InProgress
        var result = await ticketService.AssignAgentAsync(
            id, agentId, tenantId, ct);

        return Ok(result);
    }
    
}