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
        private const string PrivateRootFolder = "App_Data";

        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        private string PrivateRoot => Path.Combine(_environment.ContentRootPath, PrivateRootFolder);

        public async Task<string> SavePrivateAsync(IFormFile file, string subPath)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heic" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Unsupported file format. Allowed: jpg, jpeg, png, heic");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File too large. Max size is 5MB.");

            var safeSub = subPath.Replace('\\', '/').TrimStart('/');
            var absoluteFolder = Path.Combine(PrivateRoot, safeSub.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(absoluteFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var absolutePath = Path.Combine(absoluteFolder, fileName);

            await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
            await file.CopyToAsync(stream);

            return $"{safeSub}/{fileName}";
        }

        public Stream OpenPrivateRead(string relativePath)
        {
            var absolute = ResolvePrivatePath(relativePath);
            return new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public bool PrivateFileExists(string relativePath)
        {
            try
            {
                var absolute = ResolvePrivatePath(relativePath);
                return File.Exists(absolute);
            }
            catch
            {
                return false;
            }
        }

        private string ResolvePrivatePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Relative path is required", nameof(relativePath));

            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            var absolute = Path.GetFullPath(Path.Combine(PrivateRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
            var rootFull = Path.GetFullPath(PrivateRoot) + Path.DirectorySeparatorChar;

            if (!absolute.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Path escapes the private storage root");

            return absolute;
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