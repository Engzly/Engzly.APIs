using Engzly.Application.Features.Payments.Commands.Models;
using Engzly.Application.Features.Payments.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    public sealed class PaymentsController(ISender _mediator) : BaseApiController
    {
        [AllowAnonymous]
        [HttpPost("webhook/fawaterak")]
        public async Task<IActionResult> FawaterakWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();
            Request.Headers.TryGetValue("X-Fawaterak-Signature", out var sig);

            var cmd = new ProcessFawaterakWebhookCommand
            {
                RawBody = rawBody,
                Signature = sig.ToString()
            };
            var result = await _mediator.Send(cmd);
            return Resolve(result);
        }

        [Authorize]
        [HttpGet("gig/{gigId}/status")]
        public async Task<IActionResult> GetStatus(string gigId)
        {
            var result = await _mediator.Send(new GetPaymentStatusQuery { GigId = gigId });
            return Resolve(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{paymentId}/refund")]
        public async Task<IActionResult> Refund(string paymentId, [FromBody] RefundPaymentRequest body)
        {
            var result = await _mediator.Send(new RefundPaymentCommand
            {
                PaymentId = paymentId,
                Reason = body?.Reason
            });
            return Resolve(result);
        }

        public sealed class RefundPaymentRequest
        {
            public string? Reason { get; set; }
        }

        [AllowAnonymous]
        [HttpGet("return/success")]
        public IActionResult ReturnSuccess() => Ok(new { status = "success" });

        [AllowAnonymous]
        [HttpGet("return/fail")]
        public IActionResult ReturnFail() => Ok(new { status = "fail" });

        [AllowAnonymous]
        [HttpGet("return/pending")]
        public IActionResult ReturnPending() => Ok(new { status = "pending" });
    }
}
