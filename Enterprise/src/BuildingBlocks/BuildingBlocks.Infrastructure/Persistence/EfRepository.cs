using BuildingBlocks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

public class EfRepository<TEntity>(DbContext context) : IRepository<TEntity>
    where TEntity : class
{
    protected DbContext Context { get; } = context;
    protected DbSet<TEntity> DbSet => Context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(TEntity entity) => DbSet.Update(entity);
    public virtual void Remove(TEntity entity) => DbSet.Remove(entity);
}

public class UnitOfWork(DbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
