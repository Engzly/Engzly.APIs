using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Specifications;

namespace Engzly.Application.Features.Chat.Common
{
    public sealed class ConversationParticipantsSpec : BaseSpecification<ConversationParticipant>
    {
        public ConversationParticipantsSpec(string conversationId, bool activeOnly)
            : base(p => p.ConversationId == conversationId
                     && (!activeOnly || p.LeftOn == null))
        { }
    }

    public sealed class UserActiveParticipationsSpec : BaseSpecification<ConversationParticipant>
    {
        public UserActiveParticipationsSpec(string userId)
            : base(p => p.UserId == userId && p.LeftOn == null)
        { }
    }

    public sealed class UserParticipationSpec : BaseSpecification<ConversationParticipant>
    {
        public UserParticipationSpec(string conversationId, string userId)
            : base(p => p.ConversationId == conversationId && p.UserId == userId)
        { }
    }
}
