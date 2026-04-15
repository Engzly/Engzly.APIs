using Engzly.Domain.Enums;

namespace Engzly.Application.Features.Chat.Responses
{
    public sealed record ChatMessageResponse(
        string Id,
        string ConversationId,
        string SenderId,
        ChatMessageType Type,
        string? Text,
        string? ImageUrl,
        double? Latitude,
        double? Longitude,
        DateTime SentOn,
        bool IsRead,
        DateTime? EditedOn,
        bool IsDeleted);

    public sealed record ConversationParticipantItem(
        string UserId,
        string UserName,
        ConversationParticipantRole Role,
        DateTime JoinedOn,
        DateTime? LeftOn,
        string? LeaveReason);

    public sealed record ConversationSummaryResponse(
        string Id,
        bool IsBot,
        string? GigId,
        string? GigTitle,
        string Title,
        List<ConversationParticipantItem> Participants,
        DateTime LastMessageOn,
        string? LastMessageText,
        ChatMessageType? LastMessageType,
        int UnreadCount);

    public sealed record ChatBotReplyResponse(
        string ConversationId,
        ChatMessageResponse UserMessage,
        ChatMessageResponse BotMessage,
        string Intent,
        double Confidence);
}
