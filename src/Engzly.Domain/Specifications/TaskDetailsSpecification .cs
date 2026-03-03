using Engzly.Domain.Entities.Gigs;

namespace Engzly.Domain.Specifications
{
    public sealed class TaskDetailsSpecification : BaseSpecification<Gig>
    {
        public TaskDetailsSpecification(string taskId)
            : base(x => x.Id == taskId)
        {
            AddInclude(x => x.Category);
            AddInclude(x => x.Client);
            AddInclude(x => x.TaskersAssignments);
        }
    }
}
