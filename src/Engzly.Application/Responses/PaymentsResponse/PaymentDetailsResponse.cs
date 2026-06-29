using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.PaymentsResponse
{
    public class PaymentDetailsResponse
    {
        public string PaymentId { get; set; }

        public string GigId { get; set; }

        public decimal Amount { get; set; }

        public decimal PlatformCommission { get; set; }

        public decimal HelperAmount { get; set; }

        public string Currency { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime? FundedAtUtc { get; set; }

        public DateTime? ReleasedAtUtc { get; set; }

        public DateTime? RefundedAtUtc { get; set; }

        public string ProviderInvoiceId { get; set; }

        public string PaymentUrl { get; set; }
    }
}
