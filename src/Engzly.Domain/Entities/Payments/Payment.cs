using Engzly.Domain.Entities.Common;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;

namespace Engzly.Domain.Entities.Payments
{
    public sealed class Payment : BaseEntity<string>
    {
        public string GigId { get; set; } = null!;
        public string ClientUserId { get; set; } = null!;
        public string? HelperUserId { get; set; }

        public string Currency { get; set; } = "EGP";
        public decimal Amount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal HelperAmount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentProvider Provider { get; set; } = PaymentProvider.Fawaterak;

        public string? ProviderInvoiceId { get; set; }
        public string? ProviderInvoiceKey { get; set; }
        public string? ProviderPaymentUrl { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? FundedAtUtc { get; set; }
        public DateTime? ReleasedAtUtc { get; set; }
        public DateTime? RefundedAtUtc { get; set; }

        public Gig Gig { get; set; } = null!;
        public User ClientUser { get; set; } = null!;
        public User? HelperUser { get; set; }

        public ICollection<PaymentEvent> Events { get; set; } = new List<PaymentEvent>();
    }
}
