using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Payments;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class StartGigCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        UserManager<User> userManager,
        IPaymentGateway paymentGateway,
        ILogger<StartGigCommandHandler> logger)
        : ResponseHandler, IRequestHandler<StartGigCommand, Response<StartGigResponse>>
    {
        private const decimal PlatformCommissionRate = 0.05m;

        public async Task<Response<StartGigResponse>> Handle(StartGigCommand request, CancellationToken ct)
        {
            var me = currentUser.GetCurrentUser();
            if (me == null || string.IsNullOrWhiteSpace(me.Id))
                return Unauthorized<StartGigResponse>("Must be logged in.");

            var gig = await unitOfWork.Gigs.GetByIdAsync(request.GigId, new GigWithAssignmentsByIdSpecification(), ct);
            if (gig == null)
                return NotFound<StartGigResponse>("Gig not found.");

            if (gig.OwnerId != me.Id)
                return Forbidden<StartGigResponse>("Only the gig owner can start the gig.");

            if (gig.Status != GigStatus.HelpersAssigned)
                return BadRequest<StartGigResponse>($"Gig must be in HelpersAssigned state to start (current: {gig.Status}).");

            var assignment = gig.TaskersAssignments.FirstOrDefault();
            if (assignment == null)
                return BadRequest<StartGigResponse>("Gig has no helper assigned.");

            var existing = (await unitOfWork.Payments.GetAllAsync(new PaymentByGigSpec(gig.Id), ct)).FirstOrDefault();
            if (existing != null && (existing.Status == PaymentStatus.Funded || existing.Status == PaymentStatus.Released))
                return Conflict<StartGigResponse>("Gig is already funded.");

            if (gig.Budget <= 0)
                return BadRequest<StartGigResponse>("Gig budget must be positive.");

            var clientUser = await userManager.FindByIdAsync(me.Id);
            if (clientUser == null || string.IsNullOrWhiteSpace(clientUser.Email))
                return BadRequest<StartGigResponse>("Client account is missing an email.");

            var amount = decimal.Round(gig.Budget, 2, MidpointRounding.AwayFromZero);
            var commission = decimal.Round(amount * PlatformCommissionRate, 2, MidpointRounding.AwayFromZero);
            var helperAmount = amount - commission;

            Payment payment;
            if (existing != null && existing.Status == PaymentStatus.Pending)
            {
                payment = existing;
                payment.Amount = amount;
                payment.PlatformCommission = commission;
                payment.HelperAmount = helperAmount;
                payment.HelperUserId = assignment.TaskerId;
                payment.UpdatedAtUtc = DateTime.UtcNow;
            }
            else
            {
                payment = new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    GigId = gig.Id,
                    ClientUserId = me.Id,
                    HelperUserId = assignment.TaskerId,
                    Currency = "EGP",
                    Amount = amount,
                    PlatformCommission = commission,
                    HelperAmount = helperAmount,
                    Status = PaymentStatus.Pending,
                    Provider = PaymentProvider.Fawaterak
                };
                await unitOfWork.Payments.AddAsync(payment, ct);
            }

            await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
            {
                Id = Guid.NewGuid().ToString(),
                PaymentId = payment.Id,
                Type = existing == null ? PaymentEventType.Created : PaymentEventType.InvoiceCreated,
                OccurredAtUtc = DateTime.UtcNow
            }, ct);

            await unitOfWork.Payments.CompleteAsync(ct);

            var invoiceResult = await paymentGateway.CreateInvoiceAsync(new CreateInvoiceRequest
            {
                PaymentId = payment.Id,
                GigId = gig.Id,
                GigTitle = gig.Title,
                Amount = amount,
                Currency = "EGP",
                CustomerEmail = clientUser.Email!,
                CustomerFirstName = clientUser.FirstName,
                CustomerLastName = clientUser.LastName,
                CustomerPhone = clientUser.PhoneNumber
            }, ct);

            if (!invoiceResult.Ok || string.IsNullOrWhiteSpace(invoiceResult.PaymentUrl))
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAtUtc = DateTime.UtcNow;
                unitOfWork.Payments.Update(payment);
                await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    PaymentId = payment.Id,
                    Type = PaymentEventType.Failed,
                    RawPayload = invoiceResult.Error,
                    OccurredAtUtc = DateTime.UtcNow
                }, ct);
                await unitOfWork.Payments.CompleteAsync(ct);

                logger.LogWarning("StartGig: gateway rejected invoice for gig {GigId}: {Error}", gig.Id, invoiceResult.Error);
                return InternalServerError<StartGigResponse>("Could not create payment invoice.");
            }

            payment.ProviderInvoiceId = invoiceResult.ProviderInvoiceId;
            payment.ProviderInvoiceKey = invoiceResult.ProviderInvoiceKey;
            payment.ProviderPaymentUrl = invoiceResult.PaymentUrl;
            payment.Status = PaymentStatus.AwaitingFunding;
            payment.UpdatedAtUtc = DateTime.UtcNow;
            unitOfWork.Payments.Update(payment);

            await unitOfWork.PaymentEvents.AddAsync(new PaymentEvent
            {
                Id = Guid.NewGuid().ToString(),
                PaymentId = payment.Id,
                Type = PaymentEventType.InvoiceCreated,
                RawPayload = invoiceResult.ProviderInvoiceId,
                OccurredAtUtc = DateTime.UtcNow
            }, ct);

            await unitOfWork.Payments.CompleteAsync(ct);

            logger.LogInformation(
                "StartGig: created invoice {InvoiceId} for gig {GigId} amount {Amount} EGP",
                invoiceResult.ProviderInvoiceId, gig.Id, amount);

            return Success(new StartGigResponse
            {
                GigId = gig.Id,
                PaymentId = payment.Id,
                Amount = amount,
                PlatformCommission = commission,
                HelperAmount = helperAmount,
                Currency = "EGP",
                PaymentUrl = invoiceResult.PaymentUrl!,
                ProviderInvoiceId = invoiceResult.ProviderInvoiceId!
            });
        }

        private sealed class PaymentByGigSpec : BaseSpecification<Payment>
        {
            public PaymentByGigSpec(string gigId)
                : base(p => p.GigId == gigId)
            {
            }
        }
    }
}
