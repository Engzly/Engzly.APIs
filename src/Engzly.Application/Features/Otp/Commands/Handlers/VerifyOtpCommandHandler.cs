using Engzly.Application.Bases;
using Engzly.Application.Interfaces;
using Engzly.Application.Features.Otp.Commands.Models;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Otp.Commands.Handlers
{
    public class VerifyOtpCommandHandler : ResponseHandler, IRequestHandler<VerifyOtpCommand, Response<string>>
    {
        private readonly IOtpService _otp;
        private readonly UserManager<User> _userManager;

        public VerifyOtpCommandHandler(IOtpService otp, UserManager<User> userManager)
        {
            _otp = otp;
            _userManager = userManager;
        }

        public async Task<Response<string>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var (ok, error) = await _otp.VerifyOtpAsync(
                request.UserId, request.Purpose, request.Channel, request.Code, cancellationToken);

            if (!ok) return BadRequest<string>(error ?? "OTP verification failed.");

            // Activate user after VerifyAccount
            if (request.Purpose == OtpPurpose.VerifyAccount)
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null) return NotFound<string>("User not found.");

                user.Status = UserStatus.Active;
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return Success("OTP verified.");
        }
    }
}
