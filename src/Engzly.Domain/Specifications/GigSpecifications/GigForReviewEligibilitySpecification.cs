using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Specifications.GigSpecifications;

public sealed class GigForReviewEligibilitySpecification : BaseSpecification<Gig>
{
    public GigForReviewEligibilitySpecification(
        string gigId,
        string reviewerId,
        string reviewedUserId)
        : base(g =>
            g.Status == GigStatus.Completed &&
            (
                g.OwnerId == reviewerId &&
                 g.TaskersAssignments.Any(t => t.TaskerId == reviewedUserId)
                ||
                g.OwnerId == reviewedUserId &&
                 g.TaskersAssignments.Any(t => t.TaskerId == reviewerId)
            )
        )
    {
    }
}