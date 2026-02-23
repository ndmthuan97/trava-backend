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
using System.Text.Json;
using Trava.Application.Features.Notifications.Responses;
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

            CreateMap<UserNotification, NotificationResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.NotificationId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Notification.Type))
                .ForMember(dest => dest.Payload, opt => opt.MapFrom(src => JsonSerializer.Deserialize<object>(src.Notification.Payload, (JsonSerializerOptions)null!) ?? new object()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Notification.CreatedAt))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead));

            CreateMap<User, UserResponse>();
        }
    }
}