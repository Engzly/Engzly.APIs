using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.VerificationResponses
{
    public sealed class VerificationDetailsResponse
    {
        public string VerificationId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public VerificationStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public string? ReviewedByAdminId { get; set; }
        public string? RejectionReason { get; set; }

        public string FrontImageUrl { get; set; } = null!;
        public string BackImageUrl { get; set; } = null!;
        public string SelfieUrl { get; set; } = null!;
    }
}
