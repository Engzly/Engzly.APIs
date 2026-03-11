using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications
{
public sealed class GigWithMediasByIdSpecification : BaseSpecification<Gig>
{
    public GigWithMediasByIdSpecification()
    {
        AddInclude(g => g.Medias);
    }
}
}