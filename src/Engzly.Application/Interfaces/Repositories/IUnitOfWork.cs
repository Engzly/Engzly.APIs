using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Payments;


namespace Engzly.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {

        IGenericRepository<Proposal, string> Proposals { get; }
        IGenericRepository<Gig, string> Gigs { get; }
        IGenericRepository<GigAssignment, string> GigAssignments { get; }
        IGenericRepository<Conversation, string> Conversations { get; }
        IGenericRepository<ConversationParticipant, string> ConversationParticipants { get; }
        IGenericRepository<Payment, string> Payments { get; }
        IGenericRepository<PaymentEvent, string> PaymentEvents { get; }
        IGenericRepository<HelperWallet, string> HelperWallets { get; }


        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
