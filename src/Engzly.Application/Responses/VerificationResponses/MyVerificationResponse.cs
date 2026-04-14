using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.VerificationResponses
{
    public sealed class MyVerificationResponse
    {
        public string? VerificationId { get; set; }
        public VerificationStatus? Status { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsVerified { get; set; }
    }
}
