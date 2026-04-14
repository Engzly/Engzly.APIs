using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Domain.Entities.Chat
{
    public sealed class Conversation : BaseEntity<string>
    {
        public string UserAId { get; set; } = null!;
        public string UserBId { get; set; } = null!;
        public string? GigId { get; set; }
        public bool IsBot { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastMessageOn { get; set; }

        public User UserA { get; set; } = null!;
        public User UserB { get; set; } = null!;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
