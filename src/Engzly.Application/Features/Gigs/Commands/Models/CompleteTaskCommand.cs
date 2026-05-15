using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
    public sealed class CompleteTaskCommand : IRequest<Response<string>>
    {
        public string GigId { get; set; }

        public CompleteTaskCommand(string gigId)
        {
            GigId = gigId;
        }
    }

}