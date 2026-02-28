using Engzly.Application.Features.Otp.Commands.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OtpController(IMediator mediator) => _mediator = mediator;

        [HttpPost("send")]
        public async Task<IActionResult> Send(SendOtpCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(VerifyOtpCommand cmd)
            => Ok(await _mediator.Send(cmd));
    }
}