using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Responses;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class RefreshTokenCommandHandler(
       UserManager<User> userManager,
       ITokenService tokenService)
       : ResponseHandler,
         IRequestHandler<RefreshTokenCommand, Response<LoginResponse>>
    {
        public async Task<Response<LoginResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var user = userManager.Users.FirstOrDefault(u =>
                u.RefreshToken == request.RefreshToken);

            if (user == null)
                return BadRequest<LoginResponse>("Invalid Refresh Token");

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest<LoginResponse>("Refresh Token Expired");

            var newJwt = await tokenService.GenerateJwtToken(user);
            var newRefresh = tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiryTime =
                DateTime.UtcNow.AddDays(7);

            await userManager.UpdateAsync(user);

            return Success(new LoginResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                AccessToken = newJwt,
                RefreshToken = newRefresh
            }, "Token Refreshed Successfully");
        }
    }

}
