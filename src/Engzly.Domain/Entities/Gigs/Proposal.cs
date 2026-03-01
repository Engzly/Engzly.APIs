using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Gigs
{
    public sealed class Proposal : BaseEntity<string>
    {
        public string GigId { get; set; } = null!;
        public string TaskerId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public ProposalStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }

        public Gig Gig { get; set; } = null!;
        public User Tasker { get; set; } = null!;
    }
}
