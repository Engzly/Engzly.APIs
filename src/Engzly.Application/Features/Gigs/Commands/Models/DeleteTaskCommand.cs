using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
 public sealed class DeleteTaskCommand : IRequest<Response<string>>
{
    public string Id { get; set; }

    public DeleteTaskCommand(string id)
    {
        Id = id;
    }
}

}