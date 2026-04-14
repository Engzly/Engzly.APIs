using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.VerificationResponses
{
    public sealed class VerificationListItemResponse
    {
        public string VerificationId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public VerificationStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? ReviewedOn { get; set; }
    }
}
