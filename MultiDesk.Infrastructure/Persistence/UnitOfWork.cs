using MultiDesk.Application.Common;
using MultiDesk.Application.Interfaces;
using MultiDesk.Infrastructure.Persistence.Repositories;

namespace MultiDesk.Infrastructure.Persistence;

public class UnitOfWork(MultiDeskDbContext context) : IUnitOfWork
{
    // Lazy initialization — repositories created only when first accessed
    private ITicketRepository? _tickets;
    private IUserRepository? _users;
    private IDepartmentRepository? _departments;

    public ITicketRepository Tickets =>
        _tickets ??= new TicketRepository(context);

    public IUserRepository Users =>
        _users ??= new UserRepository(context);

    public IDepartmentRepository Departments =>
        _departments ??= new DepartmentRepository(context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public void Dispose() =>
        context.Dispose();
}