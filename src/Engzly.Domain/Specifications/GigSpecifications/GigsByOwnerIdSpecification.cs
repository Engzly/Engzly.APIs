using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications.GigSpecifications
{
    public class GigsByOwnerIdSpecification(string ownerId)
        : BaseSpecification<Gig>(g => g.OwnerId == ownerId)
    {

    }
}
