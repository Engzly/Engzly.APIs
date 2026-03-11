using System.Linq.Expressions;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Common;
using Engzly.Domain.Specifications;
using Engzly.Infrastructure.Persistence.Data;
using Engzly.Infrastructure.Persistence.Specification;
using Microsoft.EntityFrameworkCore;

namespace Engzly.Infrastructure.Persistence.Repositories
{
    internal sealed class GenericRepository<TEntity, TKey>(
    EngzlyDbContext context)
    : IGenericRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : IEquatable<TKey>
    {
        private readonly EngzlyDbContext _context = context;
        private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

        // =========================
        // Query
        // =========================

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
            => await ApplySpecification(specification)
                .ToListAsync(cancellationToken);

        public async Task<TEntity?> GetByIdAsync(
            TKey id,
            CancellationToken cancellationToken = default)
            => await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken);

        public async Task<TEntity?> GetByIdAsync(
            TKey id,
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
            => await ApplySpecification(specification)
                .FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken);

        public async Task<int> CountAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
            => await ApplySpecification(specification)
                .CountAsync(cancellationToken);

        public async Task<bool> ExistsAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
            => await ApplySpecification(specification)
                .AnyAsync(cancellationToken);

        // =========================
        // Commands
        // =========================

        public async Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public void Update(TEntity entity)
            => _dbSet.Update(entity);

        public void Delete(TEntity entity)
            => _dbSet.Remove(entity);

        public async Task<int> CompleteAsync(
            CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);


        private IQueryable<TEntity> ApplySpecification(
            ISpecification<TEntity> specification)
            => SpecificationEvaluator<TEntity>
                .GetQuery(_dbSet.AsQueryable(), specification)
                .AsNoTracking();



        public async Task<TEntity?> GetByIdLockedAsync(
           TKey id,
           CancellationToken ct = default,
           params Expression<Func<TEntity, object>>[] includes)
        {
            var entityType = _context.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
                throw new InvalidOperationException($"Entity {typeof(TEntity).Name} is not mapped in the context");

            var tableName = entityType.GetTableName();

            IQueryable<TEntity> query = _context.Set<TEntity>()
                  .FromSqlRaw($"SELECT * FROM {tableName} WITH (UPDLOCK, ROWLOCK) WHERE Id = {{0}}", id)
                  .AsTracking();


            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(ct);
        }
    }
}

