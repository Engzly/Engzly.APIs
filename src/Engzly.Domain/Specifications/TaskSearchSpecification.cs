using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications
{
    public sealed class TaskSearchSpecification : BaseSpecification<Gig>
    {
        public TaskSearchSpecification()
        {
            AddInclude(g => g.Category);
        }
    }
}
