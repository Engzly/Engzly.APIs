using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed record UploadMediaCommand(List<IFormFile> Images)
: IRequest<Response<List<MediaUploadResponse>>>;
}
