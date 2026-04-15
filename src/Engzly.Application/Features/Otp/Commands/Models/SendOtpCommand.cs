using Engzly.Application.Common.Bases;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Otp.Commands.Models
{
    public class SendOtpCommand : IRequest<Response<string>>
    {
        public string Email { get; set; } = string.Empty;
        public OtpPurpose Purpose { get; set; }
    }
}
