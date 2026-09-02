using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiDesk.Application.DTOs.AiSuggestions;
using MultiDesk.Application.DTOs.Messages;
using MultiDesk.Application.DTOs.Tickets;
using MultiDesk.Application.Services;
using MultiDesk.Domain.Entities;
using MultiDesk.Domain.Enums;
using MultiDesk.Infrastructure.Identity;
using MultiDesk.Infrastructure.Persistence;

namespace MultiDesk.Infrastructure.Services;

public class TicketService(
    MultiDeskDbContext context,
    UserManager<MultiDeskUser> userManager,
    ILogger<TicketService> logger) : ITicketService
{
    public async Task<PagedTicketResponse> GetPagedAsync(
        int tenantId, int page, int pageSize,
        TicketStatus? status = null,
        string? studentId = null,
        CancellationToken ct = default)
    {
        var query = context.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrEmpty(studentId))
            query = query.Where(t => t.StudentId == studentId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages)
            .ToListAsync(ct);

        // Load user data separately
        var userIds = tickets
            .SelectMany(t => new[] { t.StudentId, t.AgentId })
            .Where(id => id != null)
            .Distinct()
            .ToList();

        var users = await GetUsersByIds(userIds!);

        var items = tickets.Select(t =>
        {
            users.TryGetValue(t.StudentId, out var student);
            var agentName = t.AgentId != null && users.TryGetValue(t.AgentId, out var agent)
                ? agent.FullName : null;

            return new TicketResponse(
                t.Id,
                t.Title,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.Department.Name,
                t.Category.Name,
                student?.FullName ?? "Unknown",
                agentName,
                t.CreatedAt,
                t.UpdatedAt,
                t.ResolvedAt,
                t.Messages.Count);
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedTicketResponse(
            items, totalCount, page, pageSize,
            totalPages, page < totalPages, page > 1);
    }

    public async Task<TicketDetailResponse?> GetByIdAsync(
        int ticketId,
        int tenantId,
        string? requestingUserId = null, 
        string? requestingUserRole = null, 
        CancellationToken ct = default)
    {
        var query = context.Tickets
       .AsNoTracking()
       .Where(t => t.Id == ticketId && t.TenantId == tenantId);

        // Students can only view their own tickets
        if (requestingUserRole == "Student")
            query = query.Where(t => t.StudentId == requestingUserId);

        var ticket = await query
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .Include(t => t.AiSuggestions)
            .FirstOrDefaultAsync(ct);

        if (ticket is null) return null;

        // Load users separately
        var userIds = new List<string> { ticket.StudentId };
        if (ticket.AgentId != null) userIds.Add(ticket.AgentId);

        var senderIds = ticket.Messages.Select(m => m.SenderId).Distinct();
        userIds.AddRange(senderIds);

        var users = await GetUsersByIds(userIds.Distinct().ToList());

        users.TryGetValue(ticket.StudentId, out var student);
        var agent = ticket.AgentId != null && users.TryGetValue(ticket.AgentId, out var a)
            ? a : null;

        // Get roles for message senders
        var messages = new List<MessageResponse>();
        foreach (var m in ticket.Messages)
        {
            users.TryGetValue(m.SenderId, out var sender);
            var senderRoles = sender != null
                ? await userManager.GetRolesAsync(sender)
                : new List<string>();

            messages.Add(new MessageResponse(
                m.Id,
                m.Content,
                m.SenderId,
                sender?.FullName ?? "Unknown",
                senderRoles.FirstOrDefault() ?? "Student",
                m.CreatedAt));
        }

        return new TicketDetailResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.Department.Name,
            ticket.Category.Name,
            ticket.StudentId,
            student?.FullName ?? "Unknown",
            student?.Email ?? string.Empty,
            ticket.AgentId,
            agent?.FullName,
            agent?.Email,
            ticket.AttachmentPath,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.ResolvedAt,
            messages,
            ticket.AiSuggestions.Select(a => new AiSuggestionResponse(
                a.Id, a.SuggestedText, a.Accepted, a.CreatedAt)).ToList());
    }

    public async Task<TicketResponse> CreateAsync(
        CreateTicketRequest request,
        string studentId,
        int tenantId,
        CancellationToken ct = default)
    {
        // Load the category to get its default priority
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId
                                   && c.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException(
                $"Category {request.CategoryId} not found.");

        // Priority comes from the category not from the student
        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Priority = category.DefaultPriority,  // system assigned
            DepartmentId = request.DepartmentId,
            CategoryId = request.CategoryId,
            StudentId = studentId,
            TenantId = tenantId,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AttachmentPath = request.AttachmentPath
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Ticket {TicketId} created with system priority {Priority} " +
            "from category {CategoryId}",
            ticket.Id, ticket.Priority, request.CategoryId);

        return (await GetByIdAsync(ticket.Id, tenantId, ct: ct))!
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
            ?? throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        if (request.Title is not null) ticket.Title = request.Title;
        if (request.Description is not null) ticket.Description = request.Description;
        if (request.Priority.HasValue) ticket.Priority = request.Priority.Value;
        if (request.AgentId is not null) ticket.AgentId = request.AgentId;

        if (request.Status.HasValue)
        {
            ticket.Status = request.Status.Value;
            if (request.Status.Value == TicketStatus.Resolved)
                ticket.ResolvedAt = DateTime.UtcNow;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(ticketId, tenantId, null, null, ct))!.ToTicketResponse();
    }

    public async Task DeleteAsync(
        int ticketId, int tenantId, string deletedBy,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        ticket.IsDeleted = true;
        ticket.DeletedAt = DateTime.UtcNow;
        ticket.DeletedBy = deletedBy;
        ticket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task<TicketResponse> AssignAgentAsync(
        int ticketId, string agentId, int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        ticket.AgentId = agentId;
        ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(ticketId, tenantId, ct: ct))!
            .ToTicketResponse();
    }

    public async Task<TicketResponse> ChangeStatusAsync(
        int ticketId, TicketStatus newStatus, int tenantId,
        CancellationToken ct = default)
    {
        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId
                                   && t.TenantId == tenantId, ct)
            ?? throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved)
            ticket.ResolvedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return (await GetByIdAsync(ticketId, tenantId, null, null, ct))!.ToTicketResponse();
    }

    // ── Private Helper ──────────────────────────────────────────────

    private async Task<Dictionary<string, MultiDeskUser>> GetUsersByIds(
        List<string> userIds)
    {
        var users = await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        return users.ToDictionary(u => u.Id);
    }
}