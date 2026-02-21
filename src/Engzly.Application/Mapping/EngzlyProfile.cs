using AutoMapper;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Application.Mapping;

public sealed class EngzlyProfile : Profile
{
    public EngzlyProfile()
    {
        CreateMap<CreateUserCommand, User>();
    }
}