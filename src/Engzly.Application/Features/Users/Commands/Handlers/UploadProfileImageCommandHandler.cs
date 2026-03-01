using System.Security.Claims;
using Engzly.Application.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Services;
using Engzly.Application.Responses;
using Engzly.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public sealed class UploadProfileImageCommandHandler(
        UserManager<User> userManager,
        IFileStorageService fileStorage,
        IHttpContextAccessor httpContext)
        : ResponseHandler,
          IRequestHandler<UploadProfileImageCommand, Response<string>>
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IFileStorageService _fileStorage = fileStorage;
        private readonly IHttpContextAccessor _httpContext = httpContext;

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

           
            var userId = _httpContext.HttpContext!.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest<string>("User not found");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest<string>("User not found");

            
            var imageUrl = await _fileStorage.UploadAsync(file, "profiles");

            
            user.ProfileImageUrl = imageUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest<string>("Failed to update profile image", errors);
            }

            
            return Success(imageUrl, "Profile image uploaded successfully");
        }
    }
}