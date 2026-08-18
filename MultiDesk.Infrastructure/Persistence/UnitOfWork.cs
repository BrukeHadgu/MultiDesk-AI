using MultiDesk.Application.Common;
using MultiDesk.Application.Interfaces;
using MultiDesk.Infrastructure.Persistence.Repositories;

namespace MultiDesk.Infrastructure.Persistence;

public class UnitOfWork(MultiDeskDbContext context) : IUnitOfWork
{
    private ITicketRepository?     _tickets;
    private IDepartmentRepository? _departments;

    public ITicketRepository Tickets =>
        _tickets ??= new TicketRepository(context);

    public IDepartmentRepository Departments =>
        _departments ??= new DepartmentRepository(context);

    // Users now managed by UserManager — not through UnitOfWork
    public IUserRepository Users =>
        throw new NotSupportedException(
            "Use IUserRepository directly — Identity users are managed by UserManager.");

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public void Dispose() => context.Dispose();
}