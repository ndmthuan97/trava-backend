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
using Trava.Application.Features.Users.Responses;
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
                .ForMember(dest => dest.CountMember, opt => opt.MapFrom(src => src.Members.Count));

            CreateMap<CreateTaskItemCommand, TaskItem>();
            CreateMap<TaskItem, TaskItemResponse>();

            CreateMap<CreateSpaceInvitationCommand, SpaceInvitation>();
            CreateMap<SpaceInvitation, SpaceInvitationResponse>();

            CreateMap<User, UserResponse>();
        }
    }
}