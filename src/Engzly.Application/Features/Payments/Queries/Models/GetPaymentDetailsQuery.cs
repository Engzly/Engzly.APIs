using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.PaymentsResponse;
using MediatR;

namespace Engzly.Application.Features.Payments.Queries.Models
{
    public record GetPaymentDetailsQuery(string PaymentId)
    : IRequest<Response<PaymentDetailsResponse>>;
}
