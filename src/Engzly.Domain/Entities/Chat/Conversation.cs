using Engzly.Domain.Entities.Common;

namespace Engzly.Domain.Entities.Chat
{
    public sealed class Conversation : BaseEntity<string>
    {
        public string? GigId { get; set; }
        public string? OwnerId { get; set; }
        public bool IsBot { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastMessageOn { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
