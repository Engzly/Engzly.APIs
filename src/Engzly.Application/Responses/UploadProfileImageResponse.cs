using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Responses
{
    public sealed class UploadProfileImageResponse
    {
        public IFormFile Image { get; set; } = null!;
    }
}
