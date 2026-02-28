using Engzly.Application.Bases;
using Engzly.Domain.Enums;
using MediatR;

namespace Englzly.Application.Features.Users.Commands.Models
{
    public class ResetPasswordCommand : IRequest<Response<string>>
    {
        public string Email { get; set; }
        public OtpChannel Channel { get; set; } = OtpChannel.Email;
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}