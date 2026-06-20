using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Otp.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Responses.OTPResponse;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Otp.Commands.Handlers
{
    public class VerifyOtpCommandHandler : ResponseHandler, IRequestHandler<VerifyOtpCommand, Response<VerifyOtpResponse>>
    {
        private readonly IOtpService _otp;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;


        public VerifyOtpCommandHandler(IOtpService otp, UserManager<User> userManager, ITokenService tokenService)
        {
            _otp = otp;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Response<VerifyOtpResponse>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest<VerifyOtpResponse>("Invalid code.");

            var (ok, error) = await _otp.VerifyOtpAsync(
                user.Id, request.Purpose, OtpChannel.Email, request.Code, cancellationToken);

            if (!ok) return BadRequest<VerifyOtpResponse>(error ?? "OTP verification failed.");

            // Generate Access Token
            var token = await _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            if (token == null) return BadRequest<VerifyOtpResponse>("Failed to generate token.");

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.IsIdentityVerified = true; // Mark the user as verified
            await _userManager.UpdateAsync(user);


            //You can return the token or any relevant information as needed
            var role = await _userManager.GetRolesAsync(user);
            var responseData = new VerifyOtpResponse
            (
                UserName: user.UserName,
                Role: role.FirstOrDefault() ?? user.AccountType.ToString(),
                UserId: user.Id,
                AccessToken: token,
                RefreshToken: refreshToken
            );
            return Success(responseData, "OTP verified.");
        }
    }
}
