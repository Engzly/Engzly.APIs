using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.PaymentsResponse
{
    public sealed class PaymentStatusResponse
    {
        public string PaymentId { get; set; } = null!;
        public string GigId { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal HelperAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentStatus Status { get; set; }
        public string? ProviderInvoiceId { get; set; }
        public string? PaymentUrl { get; set; }
        public DateTime? FundedAtUtc { get; set; }
        public DateTime? ReleasedAtUtc { get; set; }
    }
}
