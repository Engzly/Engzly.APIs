using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class VerifyTaskCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ILogger<VerifyTaskCommandHandler> logger)
            : ResponseHandler, IRequestHandler<VerifyTaskCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(VerifyTaskCommand request, CancellationToken ct)
        {
            var gig = await unitOfWork.Gigs.GetByIdAsync(request.Id, /*new GigWithAssignmentsByIdSpecification(),*/ ct);
            if (gig == null)
                return NotFound<string>("Task not found");

            var me = currentUser.GetCurrentUser();
            if (me == null)
                return Unauthorized<string>("User not found");

            if (gig.OwnerId != me.Id)
                return Unauthorized<string>("User is not the owner of this task.");

            if (gig.Status != GigStatus.PendingVerification)
                return BadRequest<string>("Task must be pending verification before it can be verified.");

            var payment = (await unitOfWork.Payments.GetAllAsync(new PaymentByGigSpec(gig.Id), ct)).FirstOrDefault();
            if (payment == null)
                return BadRequest<string>("No escrow payment found for this gig.");

            if (payment.Status == PaymentStatus.Released)
                return BadRequest<string>("Escrow has already been released for this gig.");

            if (payment.Status != PaymentStatus.Funded && payment.Status != PaymentStatus.Refunded)
                return BadRequest<string>($"Escrow must be funded before release (current: {payment.Status}).");

            if (string.IsNullOrWhiteSpace(payment.HelperUserId))
                return BadRequest<string>("Payment has no helper assigned.");

            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                gig.Status = GigStatus.Completed;
                gig.CompletedOn = now;
                gig.LastModifiedOn = now;
                unitOfWork.Gigs.Update(gig);

                payment.Status = PaymentStatus.Released;
                payment.ReleasedAtUtc = now;
                payment.UpdatedAtUtc = now;
                unitOfWork.Payments.Update(payment);

                var wallet = (await unitOfWork.HelperWallets.GetAllAsync(new WalletByUserSpec(payment.HelperUserId!), ct)).FirstOrDefault();
                if (wallet == null)
                {
                    wallet = new HelperWallet
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = payment.HelperUserId!,
                        Currency = payment.Currency,
                        Balance = payment.HelperAmount,
                        PendingBalance = payment.PlatformCommission,
                        UpdatedAtUtc = now
                    };
                    await unitOfWork.HelperWallets.AddAsync(wallet, ct);
                }
                else
                {
                    wallet.Balance += payment.HelperAmount;
                    wallet.UpdatedAtUtc = now;
                    unitOfWork.HelperWallets.Update(wallet);
                }

                await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    PaymentId = payment.Id,
                    Type = PaymentEventType.Released,
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
                "VerifyTask: released escrow {PaymentId} for gig {GigId} — credited {Amount} EGP pending to helper {HelperId}",
                payment.Id, gig.Id, payment.HelperAmount, payment.HelperUserId);

            return Success(gig.Id, "Task verified and escrow released to helper wallet.");
        }

        private sealed class PaymentByGigSpec : BaseSpecification<Payment>
        {
            public PaymentByGigSpec(string gigId) : base(p => p.GigId == gigId) { }
        }

        private sealed class WalletByUserSpec : BaseSpecification<HelperWallet>
        {
            public WalletByUserSpec(string userId) : base(w => w.UserId == userId) { }
        }
    }
}
