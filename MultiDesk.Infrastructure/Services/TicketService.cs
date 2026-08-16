using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiDesk.Application.DTOs.AiSuggestions;
using MultiDesk.Application.DTOs.Messages;
using MultiDesk.Application.DTOs.Tickets;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class TicketService(
    MultiDeskDbContext context,
    ILogger<TicketService> logger) : ITicketService
{
    public async Task<PagedTicketResponse> GetPagedAsync(
        int tenantId, int page, int pageSize,
        TicketStatus? status = null,
        CancellationToken ct = default)
    {
        var query = context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketResponse(
                t.Id,
                t.Title,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.Department.Name,
                t.Category.Name,
                t.Student.FirstName + " " + t.Student.LastName,
                t.Agent != null
                    ? t.Agent.FirstName + " " + t.Agent.LastName
                    : null,
                t.CreatedAt,
                t.UpdatedAt,
                t.ResolvedAt,
                t.Messages.Count))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedTicketResponse(
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            page < totalPages,
            page > 1);
    }

    public async Task<TicketDetailResponse?> GetByIdAsync(
        int ticketId, int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticketId && t.TenantId == tenantId)
            .Include(t => t.Student)
            .Include(t => t.Agent)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sender)
            .Include(t => t.AiSuggestions)
            .FirstOrDefaultAsync(ct);

        if (ticket is null) return null;

        return new TicketDetailResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.Department.Name,
            ticket.Category.Name,
            ticket.StudentId,
            ticket.Student.FullName,
            ticket.Student.Email,
            ticket.AgentId,
            ticket.Agent?.FullName,
            ticket.Agent?.Email,
            ticket.AttachmentPath,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.ResolvedAt,
            ticket.Messages.Select(m => new MessageResponse(
                m.Id,
                m.Content,
                m.SenderId,
                m.Sender.FullName,
                m.Sender.Role.ToString(),
                m.CreatedAt)).ToList(),
            ticket.AiSuggestions.Select(a => new AiSuggestionResponse(
                a.Id,
                a.SuggestedText,
                a.Accepted,
                a.CreatedAt)).ToList());
    }

    public async Task<TicketResponse> CreateAsync(
        CreateTicketRequest request,
        int studentId,
        int tenantId,
        CancellationToken ct = default)
    {
        var ticket = new Ticket
        {
            Title        = request.Title,
            Description  = request.Description,
            Priority     = request.Priority,
            DepartmentId = request.DepartmentId,
            CategoryId   = request.CategoryId,
            StudentId    = studentId,
            TenantId     = tenantId,
            Status       = TicketStatus.Open,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow,
            AttachmentPath = request.AttachmentPath
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Ticket {TicketId} created by student {StudentId}",
            ticket.Id, studentId);

        return (await GetByIdAsync(ticket.Id, tenantId, ct))!
            .ToTicketResponse();
    }

    public async Task<TicketResponse> UpdateAsync(
        int ticketId,
        UpdateTicketRequest request,
        int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Ticket {ticketId} not found.");

        if (request.Title is not null)
            ticket.Title = request.Title;

        if (request.Description is not null)
            ticket.Description = request.Description;

        if (request.Priority.HasValue)
            ticket.Priority = request.Priority.Value;

        if (request.Status.HasValue)
        {
            ticket.Status = request.Status.Value;
            if (request.Status.Value == TicketStatus.Resolved)
                ticket.ResolvedAt = DateTime.UtcNow;
        }

        if (request.AgentId.HasValue)
            ticket.AgentId = request.AgentId.Value;

        ticket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Ticket {TicketId} updated", ticketId);

        return (await GetByIdAsync(ticketId, tenantId, ct))!
            .ToTicketResponse();
    }

    public async Task DeleteAsync(
        int ticketId,
        int tenantId,
        int deletedBy,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Ticket {ticketId} not found.");

        // Soft delete — never physically remove records
        ticket.IsDeleted  = true;
        ticket.DeletedAt  = DateTime.UtcNow;
        ticket.DeletedBy  = deletedBy;
        ticket.UpdatedAt  = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Ticket {TicketId} soft-deleted by user {UserId}",
            ticketId, deletedBy);
    }

    public async Task<TicketResponse> AssignAgentAsync(
        int ticketId, int agentId, int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Ticket {ticketId} not found.");

        ticket.AgentId   = agentId;
        ticket.Status    = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Ticket {TicketId} assigned to agent {AgentId}",
            ticketId, agentId);

        return (await GetByIdAsync(ticketId, tenantId, ct))!
            .ToTicketResponse();
    }

    public async Task<TicketResponse> ChangeStatusAsync(
        int ticketId, TicketStatus newStatus, int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Ticket {ticketId} not found.");

        ticket.Status    = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved)
            ticket.ResolvedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(ticketId, tenantId, ct))!
            .ToTicketResponse();
    }
}