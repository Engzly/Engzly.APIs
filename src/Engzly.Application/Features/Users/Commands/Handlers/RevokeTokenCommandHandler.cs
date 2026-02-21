using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class RevokeTokenCommandHandler(
     UserManager<User> userManager)
     : ResponseHandler,
       IRequestHandler<RevokeTokenCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            RevokeTokenCommand request,
            CancellationToken cancellationToken)
        {
            var user = userManager.Users.FirstOrDefault(u =>
                u.RefreshToken == request.RefreshToken);

            if (user == null)
                return BadRequest<string>("Invalid Refresh Token");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await userManager.UpdateAsync(user);

            return Success("Token Revoked Successfully");
        }
    }

}
