using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Domain.Entities.Gigs
{
    public sealed class GigAssignment : BaseEntity<string>
    {
        public string GigId { get; set; } = null!;
        public string TaskerId { get; set; } = null!;
        public DateTime AssignedOn { get; set; }

        public Gig Gig { get; set; } = null!;
        public User Tasker { get; set; } = null!;
    }
}
