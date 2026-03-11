using Engzly.Domain.Entities.Gigs;


namespace Engzly.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {

        IGenericRepository<Proposal, string> Proposals { get; }
        IGenericRepository<Gig, string> Gigs { get; }
        IGenericRepository<GigAssignment, string> GigAssignments { get; }


        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
