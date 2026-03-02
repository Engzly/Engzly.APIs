using Engzly.Application.Features.Users.Commands.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    public sealed class UsersController(ISender mediator) : BaseApiController
    {
        #region EndPoints 

        [HttpPost("register")]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            var response = await mediator.Send(command);
            return Resolve(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var response = await mediator.Send(command);
            return Resolve(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        {
            var response = await mediator.Send(command);
            return Resolve(response);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenCommand command)
        {
            var response = await mediator.Send(command);
            return Resolve(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUserById([FromRoute] Guid id)
        {
            var response = await mediator.Send(new DeleteUserCommand(id));
            return Resolve(response);
        }



        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var response = await mediator.Send(command);
            return Resolve(response);
        }

        //[Authorize]
        [HttpPost("upload-profile-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfileImage([FromForm] UploadProfileImageCommand request)
        {
            var result = await mediator.Send(new UploadProfileImageCommand { UserId = request.UserId, Image = request.Image });
            return Ok(result);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand cmd)
   => Ok(await mediator.Send(cmd));

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand cmd)
    => Ok(await mediator.Send(cmd));


        #endregion

    }

}
