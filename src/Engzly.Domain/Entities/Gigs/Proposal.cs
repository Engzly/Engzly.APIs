using Engzly.Domain.Entities.Common;
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



        public static Proposal Create(string gigId, string taskerId, string message)
        {
            return new Proposal
            {
                Id = Guid.NewGuid().ToString(),
                GigId = gigId,
                TaskerId = taskerId,
                Message = message,
                Status = ProposalStatus.Pending,
                SubmittedOn = DateTime.UtcNow
            };
        }
    }
}
