using Englzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Bases;
using Engzly.Application.Interfaces;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Users.Commands.Handlers
{
      public class ResetPasswordCommandHandler : ResponseHandler, IRequestHandler<ResetPasswordCommand, Response<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IOtpService _otp;

        public ResetPasswordCommandHandler(UserManager<User> userManager, IOtpService otp)
        {
            _userManager = userManager;
            _otp = otp;
        }

        public async Task<Response<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return NotFound<string>("User not found.");

            var (ok, error) = await _otp.VerifyOtpAsync(
                user.Id, OtpPurpose.ForgotPassword, request.Channel, request.Code, cancellationToken);

            if (!ok) return BadRequest<string>(error ?? "Invalid OTP.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest<string>("Failed to reset password", result.Errors.Select(e => e.Description).ToList());

            return Success("Password reset successfully.");
        }
    }
}