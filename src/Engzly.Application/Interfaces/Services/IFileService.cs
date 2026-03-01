using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engzly.Application.Interfaces.Services
{
    using Microsoft.AspNetCore.Http;

    namespace Engzly.Application.Interfaces
    {
        public interface IFileService
        {
            Task<string> UploadFileAsync(IFormFile file, string folderName);
        }
    }

}
