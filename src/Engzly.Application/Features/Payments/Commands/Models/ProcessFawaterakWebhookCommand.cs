using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Payments.Commands.Models
{
    public sealed class ProcessFawaterakWebhookCommand : IRequest<Response<string>>
    {
        public string RawBody { get; set; } = string.Empty;
        public string? Signature { get; set; }
    }
}
