using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Otp.Commands.Models;
using Engzly.Application.Interfaces;
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
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest<string>("Invalid code.");

            var (ok, error) = await _otp.VerifyOtpAsync(
                user.Id, request.Purpose, OtpChannel.Email, request.Code, cancellationToken);

            if (!ok) return BadRequest<string>(error ?? "OTP verification failed.");
            return Success("OTP verified.");
        }
    }
}
