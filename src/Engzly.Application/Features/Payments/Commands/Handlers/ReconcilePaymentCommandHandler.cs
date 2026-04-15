using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Commands.Models;
using Engzly.Application.Interfaces.Payments;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Payments.Commands.Handlers
{
    public sealed class ReconcilePaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        ILogger<ReconcilePaymentCommandHandler> logger)
        : ResponseHandler, IRequestHandler<ReconcilePaymentCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(ReconcilePaymentCommand request, CancellationToken ct)
        {
            var payment = await unitOfWork.Payments.GetByIdAsync(request.PaymentId, ct);
            if (payment == null)
                return NotFound<string>("Payment not found.");

            if (payment.Status is PaymentStatus.Funded
                or PaymentStatus.Released
                or PaymentStatus.Refunded
                or PaymentStatus.Failed)
            {
                return Success("Terminal status, nothing to reconcile.");
            }

            if (string.IsNullOrWhiteSpace(payment.ProviderInvoiceId))
                return BadRequest<string>("Payment has no provider invoice id.");

            var status = await paymentGateway.GetInvoiceStatusAsync(payment.ProviderInvoiceId!, ct);

            await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
            {
                Id = Guid.NewGuid().ToString(),
                PaymentId = payment.Id,
                Type = PaymentEventType.ReconciliationChecked,
                RawPayload = Truncate(status.RawPayload ?? status.RawStatus ?? status.Error, 3800),
                OccurredAtUtc = DateTime.UtcNow
            }, ct);

            if (!status.Ok)
            {
                await unitOfWork.Payments.CompleteAsync(ct);
                logger.LogWarning(
                    "Reconcile: gateway query failed for payment {PaymentId}: {Error}",
                    payment.Id, status.Error);
                return Success("Gateway query failed; recorded.");
            }

            if (status.Status == payment.Status ||
                (status.Status == PaymentStatus.Pending && payment.Status == PaymentStatus.AwaitingFunding))
            {
                await unitOfWork.Payments.CompleteAsync(ct);
                return Success("Status unchanged.");
            }

            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                if (status.Status == PaymentStatus.Funded)
                {
                    var gig = await unitOfWork.Gigs.GetByIdAsync(payment.GigId, ct);
                    if (gig == null)
                    {
                        await unitOfWork.RollbackTransactionAsync(ct);
                        return NotFound<string>("Gig missing for funded payment.");
                    }

                    payment.Status = PaymentStatus.Funded;
                    payment.FundedAtUtc = now;
                    payment.UpdatedAtUtc = now;
                    unitOfWork.Payments.Update(payment);

                    if (gig.Status == GigStatus.HelpersAssigned)
                    {
                        gig.Status = GigStatus.InProgress;
                        gig.LastModifiedOn = now;
                        unitOfWork.Gigs.Update(gig);
                    }

                    await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentId = payment.Id,
                        Type = PaymentEventType.Funded,
                        OccurredAtUtc = now
                    }, ct);

                    logger.LogInformation(
                        "Reconcile: payment {PaymentId} funded via polling, gig {GigId} moved to InProgress",
                        payment.Id, gig.Id);
                }
                else if (status.Status is PaymentStatus.Failed or PaymentStatus.Expired)
                {
                    payment.Status = status.Status;
                    payment.UpdatedAtUtc = now;
                    unitOfWork.Payments.Update(payment);
                    await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentId = payment.Id,
                        Type = PaymentEventType.Failed,
                        RawPayload = status.RawStatus,
                        OccurredAtUtc = now
                    }, ct);
                }

                await unitOfWork.Payments.CompleteAsync(ct);
                await unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            return Success("Reconciled.");
        }

        private static string? Truncate(string? s, int max)
            => s == null ? null : (s.Length <= max ? s : s.Substring(0, max));
    }
}
