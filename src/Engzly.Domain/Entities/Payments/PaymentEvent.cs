using Engzly.Domain.Entities.Common;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Payments
{
    public sealed class PaymentEvent : BaseEntity<string>
    {
        public string PaymentId { get; set; } = null!;
        public PaymentEventType Type { get; set; }
        public string? RawPayload { get; set; }
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        public Payment Payment { get; set; } = null!;
    }
}
