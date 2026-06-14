using BakeryPay.Application.Interfaces.Repositories;
using BakeryPay.Domain.Common;
using BakeryPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BakeryPay.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly BakeryPayDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(BakeryPayDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => DbSet.Update(entity);

    public virtual void Remove(T entity) => DbSet.Remove(entity);
}
