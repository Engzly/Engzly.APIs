using Engzly.Application.Features.Verification.Commands.Models;
using Engzly.Application.Features.Verification.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Domain.Entities.Verification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engzly.API.Controllers
{
    [Authorize]
    public sealed class VerificationController(
        ISender _mediator,
        ICurrentUserService _currentUser,
        IGenericRepository<IdentityVerification, string> _verificationRepo,
        IGenericRepository<VerificationAccessLog, Guid> _accessLogRepo,
        IFileService _fileService)
        : BaseApiController
    {
        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Submit([FromForm] SubmitVerificationCommand command)
        {
            var result = await _mediator.Send(command);
            return Resolve(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var result = await _mediator.Send(new GetMyVerificationQuery());
            return Resolve(result);
        }

        [HttpGet("file/{verificationid}/{Filekind}")]
        public async Task<IActionResult> GetFile(string verificationid, string Filekind, CancellationToken cancellationToken)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized();

            var verification = await _verificationRepo.GetByIdAsync(verificationid, cancellationToken);
            if (verification is null)
                return NotFound();

            var isAdmin = caller.Roles.Contains("Admin");
            var isOwner = verification.UserId == caller.Id;
            if (!isAdmin && !isOwner)
                return Forbid();

            var relativePath = Filekind.ToLowerInvariant() switch
            {
                "front" => verification.NationalIdFrontPath,
                "back" => verification.NationalIdBackPath,
                "selfie" => verification.SelfiePath,
                _ => null
            };
            if (relativePath is null)
                return NotFound();

            if (!_fileService.PrivateFileExists(relativePath))
                return NotFound();

            await _accessLogRepo.AddAsync(new VerificationAccessLog
            {
                Id = Guid.NewGuid(),
                VerificationId = verification.Id,
                AccessedByUserId = caller.Id,
                AccessedOn = DateTime.UtcNow,
                DocumentKind = Filekind.ToLowerInvariant()
            }, cancellationToken);
            await _accessLogRepo.CompleteAsync(cancellationToken);

            var stream = _fileService.OpenPrivateRead(relativePath);
            var contentType = GetContentType(relativePath);
            return File(stream, contentType);
        }

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
