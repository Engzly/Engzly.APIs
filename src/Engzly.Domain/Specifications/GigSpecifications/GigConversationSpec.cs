using Engzly.Domain.Entities.Chat;

namespace Engzly.Domain.Specifications.GigSpecifications
{
    public class GigConversationSpec(string gigId) : BaseSpecification<Conversation>(c => c.GigId == gigId && !c.IsBot)
    {
    }
}
