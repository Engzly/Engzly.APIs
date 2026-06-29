using Engzly.Domain.Enums;

namespace Engzly.Application.Responses.PaymentsResponse
{
    public class PaymentListItemResponse
    {
        public string PaymentId { get; set; }

        public string GigId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
