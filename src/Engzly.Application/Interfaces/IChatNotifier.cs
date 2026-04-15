using Engzly.Application.Features.Chat.Responses;

namespace Engzly.Application.Interfaces
{
    public interface IChatNotifier
    {
        Task NotifyMessageAsync(string conversationId, IEnumerable<string> participantUserIds, ChatMessageResponse message, CancellationToken cancellationToken = default);

        Task NotifyMessageEditedAsync(string conversationId, IEnumerable<string> participantUserIds, ChatMessageResponse message, CancellationToken cancellationToken = default);

        Task NotifyMessageDeletedAsync(string conversationId, IEnumerable<string> participantUserIds, string messageId, CancellationToken cancellationToken = default);

        Task NotifyParticipantJoinedAsync(string conversationId, IEnumerable<string> participantUserIds, ConversationParticipantItem participant, CancellationToken cancellationToken = default);

        Task NotifyParticipantLeftAsync(string conversationId, IEnumerable<string> participantUserIds, string userId, string? reason, CancellationToken cancellationToken = default);
    }
}
