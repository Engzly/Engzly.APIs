using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Payments.Commands.Models
{
    public sealed class ReconcilePaymentCommand : IRequest<Response<string>>
    {
        public string PaymentId { get; set; } = null!;
    }
}
