using Engzly.Application.Bases;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Features.Otp.Commands.Models
{
    public class VerifyOtpCommand : IRequest<Response<string>>
    {
        public string UserId { get; set; }
        public OtpPurpose Purpose { get; set; }
        public OtpChannel Channel { get; set; }
        public string Code { get; set; }
    }
}
