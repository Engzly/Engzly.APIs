using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Specifications;

public sealed class GigForReviewEligibilitySpecification : BaseSpecification<Gig>
{
    public GigForReviewEligibilitySpecification(
        string gigId,
        string reviewerId,
        string reviewedUserId)
        : base(g =>
            g.Status == GigStatus.Completed &&
            (
                (g.OwnerId == reviewerId && 
                 g.Taskers.Any(t => t.Id == reviewedUserId))
                ||
                (g.OwnerId == reviewedUserId && 
                 g.Taskers.Any(t => t.Id == reviewerId))
            )
        )
    {
        // لو محتاج تحمل navigation properties
        // AddInclude(g => g.Client);
        // AddInclude(g => g.Freelancer);
    }
}