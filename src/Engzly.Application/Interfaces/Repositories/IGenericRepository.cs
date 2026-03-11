using System.Linq.Expressions;
using Engzly.Domain.Entities.Common;
using Engzly.Domain.Specifications;

namespace Engzly.Application.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity, TKey>
        where TEntity : BaseEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<IReadOnlyList<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> GetAllAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task<TEntity?> GetByIdAsync(
            TKey id,
            CancellationToken cancellationToken = default);

        Task<TEntity?> GetByIdAsync(
            TKey id,
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            ISpecification<TEntity> specification,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        public Task<int> CompleteAsync(
            CancellationToken cancellationToken = default);

        Task<TEntity?> GetByIdLockedAsync(TKey id, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);
    }
}
