using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications
{
   public sealed class GigWithAssignmentsByIdSpecification : BaseSpecification<Gig>
    {
        public GigWithAssignmentsByIdSpecification()
        {
            AddInclude(g => g.TaskersAssignments);
        }
    }
    
}
