using AutoMapper;
using Engzly.Application.Features.Categories.Commands.Models;
using Engzly.Application.Features.Gigs.Commands.Models;
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
                   .ForMember(dest => dest.ClientInfo, opt => opt.MapFrom(src => src.Client))
                   .ForMember(dest => dest.MediaUrls, opt => opt.MapFrom(src => src.Medias.Select(m => m.Url).ToList()));

        CreateMap<User, ClientInfoResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));
        //.ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.ProfileImageUrl));

        CreateMap<PublishTaskCommand, Gig>()
                   .ForMember(d => d.Location, opt => opt.Ignore())
                   .ForMember(d => d.Medias, opt => opt.Ignore())
                   .ForMember(d => d.Id, opt => opt.Ignore())
                   .ForMember(d => d.OwnerId, opt => opt.Ignore())
                   .ForMember(d => d.Status, opt => opt.Ignore())
                   .ForMember(d => d.CreatedOn, opt => opt.Ignore())
                   .ForMember(d => d.LastModifiedOn, opt => opt.Ignore())
                   .ForMember(d => d.CompletedOn, opt => opt.Ignore())

                   .ForMember(d => d.Client, opt => opt.Ignore())
                   .ForMember(d => d.Category, opt => opt.Ignore())
                   .ForMember(d => d.TaskersAssignments, opt => opt.Ignore());

        CreateMap<EditTaskCommand, Gig>()
            .ForMember(d => d.Location, opt => opt.Ignore())
            .ForMember(d => d.Medias, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.OwnerId, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.CreatedOn, opt => opt.Ignore())
            .ForMember(d => d.LastModifiedOn, opt => opt.Ignore())
            .ForMember(d => d.CompletedOn, opt => opt.Ignore())
            .ForMember(d => d.Client, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.TaskersAssignments, opt => opt.Ignore());


        CreateMap<AddCategoryCommand, Category>();
    }
}