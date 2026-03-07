using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces.Services;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class UploadMediaCommandHandler
        : IRequestHandler<UploadMediaCommand, Response<List<MediaUploadResponse>>>
    {
        private readonly IFileService _fileService;

        public UploadMediaCommandHandler(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<Response<List<MediaUploadResponse>>> Handle(
            UploadMediaCommand request,
            CancellationToken cancellationToken)
        {
            var response = new Response<List<MediaUploadResponse>>(null);

            if (request.Images == null || !request.Images.Any())
            {
                response.Succeeded = false;
                response.Message = "No files uploaded";
                return response;
            }

            var uploadedMedias = await _fileService.UploadMediaFilesAsync(request.Images);

            response.Data = uploadedMedias;
            response.Succeeded = true;
            response.Message = "Media uploaded successfully";

            return response;
        }
    }
}