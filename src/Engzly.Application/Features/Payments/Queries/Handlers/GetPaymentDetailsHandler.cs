using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Queries.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.PaymentsResponse;
using Engzly.Domain.Entities.Payments;
using MediatR;

namespace Engzly.Application.Features.Payments.Queries.Handlers
{
    public class GetPaymentDetailsHandler(
      IGenericRepository<Payment, string> paymentRepo)
      : ResponseHandler,
        IRequestHandler<
            GetPaymentDetailsQuery,
            Response<PaymentDetailsResponse>>
    {
        public async Task<Response<PaymentDetailsResponse>> Handle(
            GetPaymentDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var payment =
                await paymentRepo.GetByIdAsync(
                    request.PaymentId,
                    cancellationToken);

            if (payment == null)
                return NotFound<PaymentDetailsResponse>(
                    "Payment not found");

            var result = new PaymentDetailsResponse
            {
                PaymentId = payment.Id,
                GigId = payment.GigId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                FundedAtUtc = payment.FundedAtUtc,
                ReleasedAtUtc = payment.ReleasedAtUtc,
                RefundedAtUtc = payment.RefundedAtUtc,
                ProviderInvoiceId = payment.ProviderInvoiceId,
                PaymentUrl = payment.ProviderPaymentUrl,
                HelperAmount = payment.HelperAmount,
                PlatformCommission = payment.PlatformCommission
            };

            return Success(result);
        }
    }
}
