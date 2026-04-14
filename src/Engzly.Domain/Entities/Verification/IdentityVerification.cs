using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Verification
{
    public sealed class IdentityVerification : BaseEntity<string>
    {
        public string UserId { get; set; } = null!;
        public string NationalIdFrontPath { get; set; } = null!;
        public string NationalIdBackPath { get; set; } = null!;
        public string SelfiePath { get; set; } = null!;

        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        public DateTime SubmittedOn { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public string? ReviewedByAdminId { get; set; }
        public string? RejectionReason { get; set; }

        public User User { get; set; } = null!;
        public User? ReviewedByAdmin { get; set; }

        public static IdentityVerification Create(
            string userId,
            string frontPath,
            string backPath,
            string selfiePath)
        {
            return new IdentityVerification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                NationalIdFrontPath = frontPath,
                NationalIdBackPath = backPath,
                SelfiePath = selfiePath,
                Status = VerificationStatus.Pending,
                SubmittedOn = DateTime.UtcNow
            };
        }

        public void Approve(string adminId)
        {
            if (Status is VerificationStatus.Approved)
                throw new InvalidOperationException("Verification is already approved");
            Status = VerificationStatus.Approved;
            ReviewedByAdminId = adminId;
            ReviewedOn = DateTime.UtcNow;
            RejectionReason = null;
        }

        public void Reject(string adminId, string reason)
        {
            if (Status is VerificationStatus.Approved)
                throw new InvalidOperationException("Cannot reject an already approved verification");
            Status = VerificationStatus.Rejected;
            ReviewedByAdminId = adminId;
            ReviewedOn = DateTime.UtcNow;
            RejectionReason = reason;
        }
    }
}
