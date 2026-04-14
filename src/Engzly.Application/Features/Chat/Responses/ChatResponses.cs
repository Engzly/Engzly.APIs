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
        bool IsRead);

    public sealed record ConversationSummaryResponse(
        string Id,
        string OtherUserId,
        string OtherUserName,
        string? GigId,
        bool IsBot,
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
