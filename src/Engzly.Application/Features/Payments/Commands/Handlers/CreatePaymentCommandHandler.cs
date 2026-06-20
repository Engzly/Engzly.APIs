using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Commands.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Payments;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Payments.Commands.Handlers
{
    public sealed class CreatePaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        ICurrentUserService currentUser)
        : ResponseHandler, IRequestHandler<CreatePaymentCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(CreatePaymentCommand request, CancellationToken ct)
        {
            var me = currentUser.GetCurrentUser();
            if (me == null || string.IsNullOrWhiteSpace(me.Id))
                return Unauthorized<string>("Must be logged in.");

            var gig = await unitOfWork.Gigs.GetByIdAsync(request.GigId, ct);
            if (gig == null)
                return NotFound<string>("Gig not found.");

            var gigBudget = gig.Budget;

            var paymentId = Guid.NewGuid().ToString();
            var payment = new Payment
            {
                Id = paymentId,
                GigId = gig.Id,
                ClientUserId = me.Id,
                Amount = gigBudget,
                Status = PaymentStatus.AwaitingFunding,
                Provider = PaymentProvider.Fawaterak
            };

            var invoiceReq = new CreateInvoiceRequest
            {
                PaymentId = paymentId,
                GigId = gig.Id,
                GigTitle = gig.Title ?? "Gig Payment",
                Amount = gigBudget,
                CustomerEmail = me.Email ?? "customer@email.com",
                CustomerFirstName = "Client",
                CustomerLastName = "User"
            };


            var gatewayResult = await paymentGateway.CreateInvoiceAsync(invoiceReq, ct);
            if (!gatewayResult.Ok)
                return BadRequest<string>(gatewayResult.Error ?? "Gateway error");

            payment.ProviderInvoiceId = gatewayResult.ProviderInvoiceId;
            payment.ProviderPaymentUrl = gatewayResult.PaymentUrl;

            await unitOfWork.Payments.AddAsync(payment, ct);
            await unitOfWork.Payments.CompleteAsync(ct);

            return Success(gatewayResult.PaymentUrl!);
        }
    }
}