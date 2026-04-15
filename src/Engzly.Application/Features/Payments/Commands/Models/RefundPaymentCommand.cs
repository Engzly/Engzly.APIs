using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.PaymentsResponse;
using MediatR;

namespace Engzly.Application.Features.Payments.Commands.Models
{
    public sealed class RefundPaymentCommand : IRequest<Response<PaymentStatusResponse>>
    {
        public string PaymentId { get; set; } = null!;
        public string? Reason { get; set; }
    }
}
