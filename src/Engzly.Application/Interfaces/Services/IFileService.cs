using Engzly.Application.Responses.GigsResponse;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Interfaces.Services
{
    namespace Engzly.Application.Interfaces
    {
        public interface IFileService
        {
            Task<string> UploadFileAsync(IFormFile file, string folderName);
            Task<List<MediaUploadResponse>> UploadMediaFilesAsync(List<IFormFile> files);
        }
    }

}
