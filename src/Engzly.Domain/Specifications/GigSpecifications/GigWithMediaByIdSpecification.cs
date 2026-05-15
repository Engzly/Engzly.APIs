using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications.GigSpecifications
{
    public sealed class GigWithMediasByIdSpecification : BaseSpecification<Gig>
    {
        public GigWithMediasByIdSpecification()
        {
            AddInclude(g => g.Medias);
        }
    }
}