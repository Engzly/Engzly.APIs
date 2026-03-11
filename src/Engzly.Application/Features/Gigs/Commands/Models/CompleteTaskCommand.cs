using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
 public sealed class CompleteTaskCommand : IRequest<Response<string>>
{
    public string Id { get; set; }

    public CompleteTaskCommand(string id)
    {
        Id = id;
    }
}

}