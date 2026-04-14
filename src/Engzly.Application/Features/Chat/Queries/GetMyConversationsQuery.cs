using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Chat.Queries
{
    public sealed record GetMyConversationsQuery()
        : IRequest<Response<List<ConversationSummaryResponse>>>;

    public sealed class GetMyConversationsQueryHandler(
        IGenericRepository<Conversation, string> _conversations,
        IGenericRepository<ChatMessage, string> _messages,
        UserManager<User> _userManager,
        ICurrentUserService _currentUser)
        : ResponseHandler, IRequestHandler<GetMyConversationsQuery, Response<List<ConversationSummaryResponse>>>
    {
        public async Task<Response<List<ConversationSummaryResponse>>> Handle(
            GetMyConversationsQuery request,
            CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<List<ConversationSummaryResponse>>();

            var myConversations = await _conversations.GetAllAsync(
                new UserConversationsSpec(caller.Id),
                cancellationToken);

            var ordered = myConversations
                .OrderByDescending(c => c.LastMessageOn)
                .ToList();

            var result = new List<ConversationSummaryResponse>(ordered.Count);

            foreach (var conversation in ordered)
            {
                var otherId = conversation.UserAId == caller.Id
                    ? conversation.UserBId
                    : conversation.UserAId;

                var otherUser = await _userManager.FindByIdAsync(otherId);
                var otherName = otherUser?.UserName ?? "Unknown";

                var lastMessages = await _messages.GetAllAsync(
                    new LatestMessageSpec(conversation.Id),
                    cancellationToken);
                var last = lastMessages
                    .OrderByDescending(m => m.SentOn)
                    .FirstOrDefault();

                var unread = await _messages.CountAsync(
                    new UnreadForUserSpec(conversation.Id, caller.Id),
                    cancellationToken);

                result.Add(new ConversationSummaryResponse(
                    conversation.Id,
                    otherId,
                    otherName,
                    conversation.GigId,
                    conversation.IsBot,
                    conversation.LastMessageOn,
                    last?.Text,
                    last?.Type,
                    unread));
            }

            return Success(result);
        }

        private sealed class UserConversationsSpec : BaseSpecification<Conversation>
        {
            public UserConversationsSpec(string userId)
                : base(c => c.UserAId == userId || c.UserBId == userId)
            { }
        }

        private sealed class LatestMessageSpec : BaseSpecification<ChatMessage>
        {
            public LatestMessageSpec(string conversationId)
                : base(m => m.ConversationId == conversationId)
            { }
        }

        private sealed class UnreadForUserSpec : BaseSpecification<ChatMessage>
        {
            public UnreadForUserSpec(string conversationId, string userId)
                : base(m => m.ConversationId == conversationId
                         && m.SenderId != userId
                         && !m.IsRead)
            { }
        }
    }
}
