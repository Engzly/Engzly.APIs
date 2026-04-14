using Engzly.Application.Features.Verification.Commands.Models;
using Engzly.Application.Features.Verification.Queries.Models;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/verifications")]
    [ApiController]
    public sealed class AdminVerificationController(ISender _mediator) : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] VerificationStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetVerificationsQuery(status, page, pageSize));
            return Resolve(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var result = await _mediator.Send(new GetVerificationDetailsQuery(id));
            return Resolve(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            var result = await _mediator.Send(new ApproveVerificationCommand(id));
            return Resolve(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] RejectVerificationBody body)
        {
            var result = await _mediator.Send(new RejectVerificationCommand(id, body.Reason));
            return Resolve(result);
        }

        public sealed record RejectVerificationBody(string Reason);
    }
}
