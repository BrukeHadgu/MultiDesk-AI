using MultiDesk.Application.Interfaces;

namespace MultiDesk.Application.Common;

public interface IUnitOfWork : IDisposable
{
    ITicketRepository Tickets { get; }
    IUserRepository Users { get; }
    IDepartmentRepository Departments { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}