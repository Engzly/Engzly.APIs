using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Engzly.Application.Interfaces.Services;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Engzly.Infrastructure.Blobs
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Invalid file type");

            var folderPath = Path.Combine(_environment.WebRootPath, folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{folderName}/{fileName}";
        }

        public async Task<List<MediaUploadResponse>> UploadMediaFilesAsync(List<IFormFile> files)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic" };
            var responses = new List<MediaUploadResponse>();

            var folderPath = Path.Combine(_environment.WebRootPath, "media");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    throw new Exception("Unsupported file format.");

                if (file.Length > 5 * 1024 * 1024)
                    throw new Exception("File too large. Max size is 5MB.");

                var mediaId = Guid.NewGuid();
                var fileName = $"{mediaId}{extension}";
                var fullPath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                var relativeUrl = $"/media/{fileName}";

                var media = new Media
                {
                    Id = mediaId,
                    Url = relativeUrl,
                    IsTemp = true, 
                    GigId = null  
                };

               

                responses.Add(new MediaUploadResponse
                {
                    MediaId = mediaId,
                    Url = relativeUrl
                });
            }

            return responses;
        }
    }
}