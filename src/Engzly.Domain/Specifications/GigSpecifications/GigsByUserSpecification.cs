using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications.GigSpecifications
{
    public class GigsByUserSpecification : BaseSpecification<Gig>
    {
        public GigsByUserSpecification(string userId)
            : base(g =>
                g.OwnerId == userId ||
                g.TaskersAssignments.Any(a => a.TaskerId == userId))
        {
            AddInclude(g => g.Category);
            AddInclude(g => g.Location);
            AddInclude(g => g.TaskersAssignments);
        }
    }
}
