using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.PaymentsResponse;
using MediatR;

namespace Engzly.Application.Features.Payments.Queries.Models
{
    public sealed class GetPaymentStatusQuery : IRequest<Response<PaymentStatusResponse>>
    {
        public string GigId { get; set; } = null!;
    }
}
