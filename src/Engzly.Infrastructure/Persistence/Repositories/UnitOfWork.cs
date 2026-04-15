using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Engzly.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EngzlyDbContext _context;
        private IDbContextTransaction? _transaction;

        private IGenericRepository<Proposal, string>? _proposals;
        private IGenericRepository<Gig, string>? _gigs;
        private IGenericRepository<GigAssignment, string>? _gigAssignments;
        private IGenericRepository<Conversation, string>? _conversations;
        private IGenericRepository<ConversationParticipant, string>? _conversationParticipants;
        public UnitOfWork(EngzlyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IGenericRepository<Proposal, string> Proposals
            => _proposals ??= new GenericRepository<Proposal, string>(_context);

        public IGenericRepository<Gig, string> Gigs
            => _gigs ??= new GenericRepository<Gig, string>(_context);

        public IGenericRepository<GigAssignment, string> GigAssignments => _gigAssignments ??= new GenericRepository<GigAssignment, string>(_context);

        public IGenericRepository<Conversation, string> Conversations
            => _conversations ??= new GenericRepository<Conversation, string>(_context);

        public IGenericRepository<ConversationParticipant, string> ConversationParticipants
            => _conversationParticipants ??= new GenericRepository<ConversationParticipant, string>(_context);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            await _transaction.CommitAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                return;

            await _transaction.RollbackAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            await _context.DisposeAsync();
        }
    }
}
