using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.VerificationResponses
{
    public sealed class SubmitVerificationResponse
    {
        public string VerificationId { get; set; } = null!;
        public VerificationStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}
