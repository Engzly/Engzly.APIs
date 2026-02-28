using Engzly.Application.Bases;
using Engzly.Domain.Enums;
using MediatR;

namespace Engzly.Application.Users.Commands.Models
{
  public class ForgotPasswordCommand : IRequest<Response<string>>
    {
        public string Email { get; set; }
        public OtpChannel Channel { get; set; } = OtpChannel.Email;
        public string? PhoneE164 { get; set; } // only for WhatsApp
    }
}