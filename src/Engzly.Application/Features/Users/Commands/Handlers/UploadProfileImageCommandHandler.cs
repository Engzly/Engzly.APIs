using System.Security.Claims;
using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public sealed class UploadProfileImageCommandHandler(
        UserManager<User> userManager,
        IFileService fileStorage,
        IHttpContextAccessor httpContext, IHttpContextAccessor _httpContextAccessor)
        : ResponseHandler,
          IRequestHandler<UploadProfileImageCommand, Response<string>>
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IFileService _fileStorage = fileStorage;

        public async Task<Response<string>> Handle(
            UploadProfileImageCommand request,
            CancellationToken cancellationToken)
        {

            var file = request.Image;
            if (file == null)
                return BadRequest<string>("No file uploaded");

            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowed.Contains(ext))
                return BadRequest<string>("Only JPG, JPEG, PNG are allowed");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest<string>("Max image size is 2MB");

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            var _user = await _userManager.FindByIdAsync(userId);
            if (_user == null)
                return BadRequest<string>("User not found");

            var imageUrl = await _fileStorage.UploadFileAsync(file, "profile");


            _user.ProfileImageUrl = imageUrl;
            var result = await _userManager.UpdateAsync(_user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest<string>("Failed to update profile image", errors);
            }


            return Success(imageUrl, "Profile image uploaded successfully");
        }
    }
}