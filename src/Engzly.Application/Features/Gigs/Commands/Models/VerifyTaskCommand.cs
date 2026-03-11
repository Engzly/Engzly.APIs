using Engzly.Application.Common.Bases;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Models
{
 public sealed class VerifyTaskCommand : IRequest<Response<string>>
{
    public string Id { get; set; }

    public VerifyTaskCommand(string id)
    {
        Id = id;
    }
}

}