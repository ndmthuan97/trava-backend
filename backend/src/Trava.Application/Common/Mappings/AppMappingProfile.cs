using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Trava.Application.Common.Models;
using Trava.Application.Features.SpaceInvitations.Commands;
using Trava.Application.Features.SpaceInvitations.Responses;
using Trava.Application.Features.Spaces.Commands;
using Trava.Application.Features.Spaces.Responses;
using Trava.Application.Features.TaskItems.Commands;
using Trava.Application.Features.TaskItems.Responses;
using Trava.Domain.Entities;

namespace Trava.Application.Common.Mappings
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap(typeof(Pagination<>), typeof(Pagination<>))
            .ConvertUsing(typeof(PaginationMapping<,>));

            CreateMap<CreateSpaceCommand, Space>();
            CreateMap<Space, SpaceResponse>()
                .ForMember(dest => dest.SpaceType, opt => opt.MapFrom(src => src.SpaceType.ToString()));

            CreateMap<CreateTaskItemCommand, TaskItem>();
            CreateMap<TaskItem, TaskItemResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            CreateMap<CreateSpaceInvitationCommand, SpaceInvitation>();
            CreateMap<SpaceInvitation, SpaceInvitationResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        }
    }
}