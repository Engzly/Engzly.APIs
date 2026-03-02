using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Application.Common.Bases;
using Engzly.Application.Responses.GigsResponse;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Models
{
    public sealed record GetTaskDetailsQuery(string TaskId)
    : IRequest<Response<TaskDetailedResponse>>;
}
