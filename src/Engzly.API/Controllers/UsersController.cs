using Englzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Users.Commands.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    public sealed class UsersController(ISender _mediator) : BaseApiController
    {
        #region EndPoints 

        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateUserCommand command)
        {
            var response = await _mediator.Send(command);
            return Resolve(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var response = await _mediator.Send(command);
            return Resolve(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        {
            var response = await _mediator.Send(command);
            return Resolve(response);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenCommand command)
        {
            var response = await _mediator.Send(command);
            return Resolve(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUserById([FromRoute] Guid id)
        {
            var response = await _mediator.Send(new DeleteUserCommand(id));
            return Resolve(response);
        }

         [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand cmd)
            => Ok(await _mediator.Send(cmd));


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var response = await _mediator.Send(command);
            return Resolve(response);
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand cmd)
            => Ok(await _mediator.Send(cmd));
        #endregion

    }

}
