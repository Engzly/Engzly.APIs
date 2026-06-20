using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.OTPResponse;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Otp.Commands.Models
{
    public class VerifyOtpCommand : IRequest<Response<VerifyOtpResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public OtpPurpose Purpose { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
