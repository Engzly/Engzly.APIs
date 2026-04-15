using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.PaymentsResponse;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Payments.Queries.Handlers
{
    public sealed class GetPaymentStatusQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
        : ResponseHandler, IRequestHandler<GetPaymentStatusQuery, Response<PaymentStatusResponse>>
    {
        public async Task<Response<PaymentStatusResponse>> Handle(GetPaymentStatusQuery request, CancellationToken ct)
        {
            var me = currentUser.GetCurrentUser();
            if (me == null || string.IsNullOrWhiteSpace(me.Id))
                return Unauthorized<PaymentStatusResponse>("Must be logged in.");

            var payment = (await unitOfWork.Payments.GetAllAsync(new PaymentByGigSpec(request.GigId), ct)).FirstOrDefault();
            if (payment == null)
                return NotFound<PaymentStatusResponse>("No payment for this gig.");

            if (payment.ClientUserId != me.Id && payment.HelperUserId != me.Id)
                return Forbidden<PaymentStatusResponse>("Not allowed.");

            return Success(new PaymentStatusResponse
            {
                PaymentId = payment.Id,
                GigId = payment.GigId,
                Amount = payment.Amount,
                PlatformCommission = payment.PlatformCommission,
                HelperAmount = payment.HelperAmount,
                Currency = payment.Currency,
                Status = payment.Status,
                ProviderInvoiceId = payment.ProviderInvoiceId,
                PaymentUrl = payment.ProviderPaymentUrl,
                FundedAtUtc = payment.FundedAtUtc,
                ReleasedAtUtc = payment.ReleasedAtUtc,
                RefundedAtUtc = payment.RefundedAtUtc
            });
        }

        private sealed class PaymentByGigSpec : BaseSpecification<Payment>
        {
            public PaymentByGigSpec(string gigId) : base(p => p.GigId == gigId) { }
        }
    }
}
