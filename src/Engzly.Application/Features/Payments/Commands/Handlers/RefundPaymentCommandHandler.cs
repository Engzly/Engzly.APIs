using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.PaymentsResponse;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Payments.Commands.Handlers
{
    public sealed class RefundPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<RefundPaymentCommandHandler> logger)
        : ResponseHandler, IRequestHandler<RefundPaymentCommand, Response<PaymentStatusResponse>>
    {
        public async Task<Response<PaymentStatusResponse>> Handle(RefundPaymentCommand request, CancellationToken ct)
        {
            var me = currentUser.GetCurrentUser();
            if (me == null)
                return Unauthorized<PaymentStatusResponse>("Must be logged in.");

            var payment = await unitOfWork.Payments.GetByIdAsync(request.PaymentId, ct);
            if (payment == null)
                return NotFound<PaymentStatusResponse>("Payment not found.");

            if (payment.Status == PaymentStatus.Refunded)
                return BadRequest<PaymentStatusResponse>("Payment is already refunded.");

            if (payment.Status == PaymentStatus.Released)
                return BadRequest<PaymentStatusResponse>("Cannot refund a payment that was already released to the helper.");

            if (payment.Status is not PaymentStatus.Funded and not PaymentStatus.AwaitingFunding)
                return BadRequest<PaymentStatusResponse>($"Cannot refund a payment in status {payment.Status}.");

            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAtUtc = now;
                payment.UpdatedAtUtc = now;
                unitOfWork.Payments.Update(payment);

                var gig = await unitOfWork.Gigs.GetByIdAsync(payment.GigId, ct);
                if (gig != null &&
                    gig.Status is GigStatus.HelpersAssigned or GigStatus.InProgress or GigStatus.PendingVerification)
                {
                    gig.Status = GigStatus.Canceled;
                    gig.LastModifiedOn = now;
                    unitOfWork.Gigs.Update(gig);
                }

                await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    PaymentId = payment.Id,
                    Type = PaymentEventType.Refunded,
                    RawPayload = BuildReason(me.Id, request.Reason),
                    OccurredAtUtc = now
                }, ct);

                await unitOfWork.Payments.CompleteAsync(ct);
                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            logger.LogInformation(
                "RefundPayment: admin {AdminId} refunded payment {PaymentId} for gig {GigId}. Reason: {Reason}",
                me.Id, payment.Id, payment.GigId, request.Reason ?? "(none)");

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
            }, "Payment refunded.");
        }

        private static string BuildReason(string adminId, string? reason)
            => string.IsNullOrWhiteSpace(reason)
                ? $"admin={adminId}"
                : $"admin={adminId}; reason={reason}";
    }
}
