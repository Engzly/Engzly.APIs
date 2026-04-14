using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.VerificationResponses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Verification.Commands.Models
{
    public sealed class SubmitVerificationCommand : IRequest<Response<SubmitVerificationResponse>>
    {
        public IFormFile FrontImage { get; set; } = null!;
        public IFormFile BackImage { get; set; } = null!;
        public IFormFile Selfie { get; set; } = null!;
    }
}
