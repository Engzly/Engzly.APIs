using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Gigs;

namespace Engzly.Application.Interfaces.Specifications
{
    public sealed class TaskDetailsSpecification : BaseSpecification<Gig>
    {
        public TaskDetailsSpecification(string taskId)
            : base(x => x.Id == taskId)
        {
            AddInclude(x => x.Category);
            AddInclude(x => x.Client);
            AddInclude(x => x.Taskers);
        }
    }
}
