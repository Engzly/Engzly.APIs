using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Otp.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Otp.Commands.Handlers
{
    public class SendOtpCommandHandler : ResponseHandler, IRequestHandler<SendOtpCommand, Response<string>>
    {
        private readonly IOtpService _otp;
        private readonly UserManager<User> _userManager;

        public SendOtpCommandHandler(IOtpService otp, UserManager<User> userManager)
        {
            _otp = otp;
            _userManager = userManager;
        }

        public async Task<Response<string>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Success("If the account exists, a code has been sent.");

            var (ok, error) = await _otp.SendOtpAsync(
                user.Id, request.Purpose, OtpChannel.Email, user.Email!, cancellationToken);

            if (!ok) return BadRequest<string>(error ?? "Failed to send OTP.");
            return Success("OTP sent.");
        }
    }
}
