using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Responses.UsersResponse
{
    public sealed class UploadProfileImageResponse
    {
        public IFormFile Image { get; set; } = null!;
    }
}
