using Engzly.Application.Bases;
using Engzly.Application.Interfaces;
using Engzly.Application.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Handlers.Users.Commands
{
    public class ForgotPasswordCommandHandler : ResponseHandler, IRequestHandler<ForgotPasswordCommand, Response<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IOtpService _otp;

        public ForgotPasswordCommandHandler(UserManager<User> userManager, IOtpService otp)
        {
            _userManager = userManager;
            _otp = otp;
        }

        public async Task<Response<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return NotFound<string>("User not found.");

            var destination = request.Channel == OtpChannel.Email ? user.Email : request.PhoneE164;
            if (string.IsNullOrWhiteSpace(destination))
                return BadRequest<string>("Destination is required.");

            var (ok, error) = await _otp.SendOtpAsync(
                user.Id, OtpPurpose.ForgotPassword, request.Channel, destination, cancellationToken);

            if (!ok) return BadRequest<string>(error ?? "Failed to send OTP.");

            return Success("OTP sent for password reset.");
        }
    }
}