using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Payments.Commands.Models
{
    public sealed class CreatePaymentCommand : IRequest<Response<string>>
    {
        public string GigId { get; set; } = null!;
    }
}