using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Entities.Gigs;
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
        IGenericRepository<ConversationParticipant, string> _participants,
        IGenericRepository<ChatMessage, string> _messages,
        IGenericRepository<Gig, string> _gigs,
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

            var result = new List<ConversationSummaryResponse>();

            var myParticipations = await _participants.GetAllAsync(
                new UserActiveParticipationsSpec(caller.Id),
                cancellationToken);

            var conversationIds = myParticipations.Select(p => p.ConversationId).Distinct().ToList();

            foreach (var convId in conversationIds)
            {
                var conversation = await _conversations.GetByIdAsync(convId, cancellationToken);
                if (conversation is null || conversation.IsBot)
                    continue;

                var participants = await _participants.GetAllAsync(
                    new ConversationParticipantsSpec(conversation.Id, activeOnly: false),
                    cancellationToken);

                var participantItems = new List<ConversationParticipantItem>(participants.Count);
                foreach (var p in participants)
                {
                    var u = await _userManager.FindByIdAsync(p.UserId);
                    participantItems.Add(new ConversationParticipantItem(
                        p.UserId,
                        u?.UserName ?? "Unknown",
                        p.Role,
                        p.JoinedOn,
                        p.LeftOn,
                        p.LeaveReason));
                }

                string? gigTitle = null;
                if (!string.IsNullOrWhiteSpace(conversation.GigId))
                {
                    var gig = await _gigs.GetByIdAsync(conversation.GigId!, cancellationToken);
                    gigTitle = gig?.Title;
                }

                var lastMessages = await _messages.GetAllAsync(
                    new LatestMessageSpec(conversation.Id),
                    cancellationToken);
                var last = lastMessages.OrderByDescending(m => m.SentOn).FirstOrDefault();

                var unread = await _messages.CountAsync(
                    new UnreadForUserSpec(conversation.Id, caller.Id),
                    cancellationToken);

                var lastText = last is null
                    ? null
                    : last.IsDeleted
                        ? "This message was deleted"
                        : last.Text;

                var otherParticipant = participantItems.FirstOrDefault(p => p.UserId != caller.Id);
                var title = conversation.IsBot ? "Engzly Assistant" : otherParticipant?.UserName ?? gigTitle ?? "Gig Chat";

                result.Add(new ConversationSummaryResponse(
                    conversation.Id,
                    IsBot: false,
                    conversation.GigId,
                    gigTitle,
                    Title: title,
                    participantItems,
                    conversation.LastMessageOn,
                    lastText,
                    last?.Type,
                    unread));
            }

            var bot = (await _conversations.GetAllAsync(
                new BotConversationForUserSpec(caller.Id),
                cancellationToken)).FirstOrDefault();

            if (bot is not null)
            {
                var lastBotMessages = await _messages.GetAllAsync(
                    new LatestMessageSpec(bot.Id),
                    cancellationToken);
                var lastBot = lastBotMessages.OrderByDescending(m => m.SentOn).FirstOrDefault();

                var unreadBot = await _messages.CountAsync(
                    new UnreadForUserSpec(bot.Id, caller.Id),
                    cancellationToken);

                result.Add(new ConversationSummaryResponse(
                    bot.Id,
                    IsBot: true,
                    GigId: null,
                    GigTitle: null,
                    Title: "Engzly Assistant",
                    Participants: new List<ConversationParticipantItem>(),
                    bot.LastMessageOn,
                    lastBot?.Text,
                    lastBot?.Type,
                    unreadBot));
            }

            var ordered = result.OrderByDescending(c => c.LastMessageOn).ToList();
            return Success(ordered);
        }

        private sealed class BotConversationForUserSpec : BaseSpecification<Conversation>
        {
            public BotConversationForUserSpec(string userId)
                : base(c => c.IsBot && c.OwnerId == userId)
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
                         && !m.IsRead
                         && !m.IsDeleted)
            { }
        }
    }
}
