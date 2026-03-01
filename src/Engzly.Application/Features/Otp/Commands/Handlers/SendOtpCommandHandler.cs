using Engzly.Application.Bases;
using Engzly.Application.Interfaces;
using Engzly.Application.Features.Otp.Commands.Models;
using MediatR;

namespace Engzly.Application.Features.Otp.Commands.Handlers
{
    public class SendOtpCommandHandler : ResponseHandler, IRequestHandler<SendOtpCommand, Response<string>>
    {
        private readonly IOtpService _otp;
        public SendOtpCommandHandler(IOtpService otp) => _otp = otp;

        public async Task<Response<string>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            var (ok, error) = await _otp.SendOtpAsync(
                request.UserId, request.Purpose, request.Channel, request.Destination, cancellationToken);

            if (!ok) return BadRequest<string>(error ?? "Failed to send OTP.");
            return Success("OTP sent.");
        }
    }
}
