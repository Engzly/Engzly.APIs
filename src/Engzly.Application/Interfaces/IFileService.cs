using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
    }
}
