using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.Common;

namespace MultiDesk.Infrastructure.Persistence.Repositories;

public class Repository<T>(MultiDeskDbContext context)
    : IRepository<T> where T : class
{
    protected readonly MultiDeskDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await DbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await DbSet.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default) =>
        predicate is null
            ? await DbSet.CountAsync(ct)
            : await DbSet.CountAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken ct = default) =>
        await DbSet.AddRangeAsync(entities, ct);

    public void Update(T entity) =>
        DbSet.Update(entity);

    public void Remove(T entity) =>
        DbSet.Remove(entity);
}