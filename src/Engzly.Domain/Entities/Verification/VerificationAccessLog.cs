using Engzly.Domain.Entities.Common;

namespace Engzly.Domain.Entities.Verification
{
    public sealed class VerificationAccessLog : BaseEntity<Guid>
    {
        public string VerificationId { get; set; } = null!;
        public string AccessedByUserId { get; set; } = null!;
        public DateTime AccessedOn { get; set; }
        public string DocumentKind { get; set; } = null!;

        public IdentityVerification Verification { get; set; } = null!;
    }
}
