using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications.GigSpecifications
{
    public class GigsByOwnerIdSpecification : BaseSpecification<Gig>
    {
        public GigsByOwnerIdSpecification(string ownerId) : base(g => g.OwnerId == ownerId)
        {
            AddInclude(g => g.Category);
        }
    }
}
