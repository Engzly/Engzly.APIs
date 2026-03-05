using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications
{
    public class ProposalByGigAndTaskerSpecification : BaseSpecification<Proposal>
    {
        public ProposalByGigAndTaskerSpecification(string gigId, string taskerId)
            : base(p => p.GigId == gigId && p.TaskerId == taskerId)
        {
            // TODO If you need to include related entities, you can do so here using the Include method
        }
    }
}
