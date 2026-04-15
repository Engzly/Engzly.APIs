using Engzly.Domain.Entities.Common;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Chat
{
    public sealed class ConversationParticipant : BaseEntity<string>
    {
        public string ConversationId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public ConversationParticipantRole Role { get; set; }
        public DateTime JoinedOn { get; set; }
        public DateTime? LeftOn { get; set; }
        public string? LeaveReason { get; set; }

        public Conversation Conversation { get; set; } = null!;
    }
}
