using AutoMapper;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Application.Mapping;

public sealed class EngzlyProfile : Profile
{
    public EngzlyProfile()
    {
        CreateMap<CreateUserCommand, User>();

        CreateMap<Gig, TaskDetailedResponse>()
                   .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                   .ForMember(dest => dest.RequiredHelpers, opt => opt.MapFrom(src => src.NumberOfTaskersNeeded))
                   .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                   .ForMember(dest => dest.ClientInfo, opt => opt.MapFrom(src => src.Client));

        CreateMap<User, ClientInfoResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));
            //.ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.ProfileImageUrl));

    }
}