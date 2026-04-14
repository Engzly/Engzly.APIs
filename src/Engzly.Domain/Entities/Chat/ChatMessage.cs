using Engzly.Domain.Entities.Common;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Chat
{
    public sealed class ChatMessage : BaseEntity<string>
    {
        public string ConversationId { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public ChatMessageType Type { get; set; }
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime SentOn { get; set; }
        public bool IsRead { get; set; }

        public Conversation Conversation { get; set; } = null!;
    }
}
