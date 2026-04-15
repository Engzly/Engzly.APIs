using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Chat.Queries
{
    public sealed record GetConversationMessagesQuery(
        string ConversationId,
        int Page = 1,
        int PageSize = 50)
        : IRequest<Response<List<ChatMessageResponse>>>;

    public sealed class GetConversationMessagesQueryHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ConversationParticipant, string> _participants,
        IGenericRepository<ChatMessage, string> _messages,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetConversationMessagesQuery, Response<List<ChatMessageResponse>>>
    {
        public async Task<Response<List<ChatMessageResponse>>> Handle(
            GetConversationMessagesQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<List<ChatMessageResponse>>();

            var conversation = await _conversations.GetByIdAsync(request.ConversationId, cancellationToken);
            if (conversation is null)
                return NotFound<List<ChatMessageResponse>>("Conversation not found");

            if (conversation.IsBot)
            {
                if (conversation.OwnerId != caller.Id)
                    return Forbidden<List<ChatMessageResponse>>("Not your bot conversation");
            }
            else
            {
                var participation = await _participants.GetAllAsync(
                    new UserParticipationSpec(conversation.Id, caller.Id),
                    cancellationToken);

                if (participation.Count == 0)
                    return Forbidden<List<ChatMessageResponse>>("You are not a participant in this conversation");
            }

            var page = request.Page < 1 ? 1 : request.Page;
            var size = request.PageSize < 1 ? 50 : Math.Min(request.PageSize, 200);

            var all = await _messages.GetAllAsync(
                new ConversationMessagesSpec(conversation.Id),
                cancellationToken);

            var total = all.Count;
            var items = all
                .OrderByDescending(m => m.SentOn)
                .Skip((page - 1) * size)
                .Take(size)
                .OrderBy(m => m.SentOn)
                .Select(m => new ChatMessageResponse(
                    m.Id,
                    m.ConversationId,
                    m.SenderId,
                    m.Type,
                    m.Text,
                    m.ImageUrl,
                    m.Latitude,
                    m.Longitude,
                    m.SentOn,
                    m.IsRead,
                    m.EditedOn,
                    m.IsDeleted))
                .ToList();

            var meta = new
            {
                page,
                pageSize = size,
                total,
                totalPages = (int)Math.Ceiling(total / (double)size)
            };

            return Success(items, meta);
        }

        private sealed class ConversationMessagesSpec : BaseSpecification<ChatMessage>
        {
            public ConversationMessagesSpec(string conversationId)
                : base(m => m.ConversationId == conversationId)
            { }
        }
    }
}
