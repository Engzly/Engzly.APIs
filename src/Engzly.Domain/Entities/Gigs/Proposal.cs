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

        public void Approve()
        {
            if (Status != ProposalStatus.Pending)
                throw new InvalidOperationException("Cannot approve a proposal that is not Pending");

            Status = ProposalStatus.Approved;
        }

        public void Reject()
        {
            if (Status != ProposalStatus.Pending)
                throw new InvalidOperationException("Cannot reject a proposal that is not Pending");

            Status = ProposalStatus.Rejected;
        }

        public void Withdraw()
        {
            if (Status != ProposalStatus.Pending)
                throw new InvalidOperationException("Cannot withdraw a proposal that is not Pending");

            Status = ProposalStatus.Withdrawn;
        }

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
