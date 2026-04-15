using Engzly.Domain.Enums;

namespace Engzly.Application.Interfaces.Payments
{
    public interface IPaymentGateway
    {
        Task<CreateInvoiceResult> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct);
        Task<InvoiceStatusResult> GetInvoiceStatusAsync(string providerInvoiceId, CancellationToken ct);
        bool TryVerifyWebhook(string rawBody, string? signatureHeader);
        PaymentStatus MapProviderStatus(string providerStatus);
    }

    public sealed class CreateInvoiceRequest
    {
        public string PaymentId { get; set; } = null!;
        public string GigId { get; set; } = null!;
        public string GigTitle { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string CustomerEmail { get; set; } = null!;
        public string CustomerFirstName { get; set; } = null!;
        public string CustomerLastName { get; set; } = null!;
        public string? CustomerPhone { get; set; }
    }

    public sealed class CreateInvoiceResult
    {
        public bool Ok { get; set; }
        public string? ProviderInvoiceId { get; set; }
        public string? ProviderInvoiceKey { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Error { get; set; }
    }

    public sealed class InvoiceStatusResult
    {
        public bool Ok { get; set; }
        public PaymentStatus Status { get; set; }
        public string? RawStatus { get; set; }
        public string? RawPayload { get; set; }
        public string? Error { get; set; }
    }
}
