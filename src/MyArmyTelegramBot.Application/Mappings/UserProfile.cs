using AutoMapper;
using MyArmyTelegramBot.Application.DTOs;
using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserEntity, UserDTO>()
                .ForMember(d => d.Role,
                o => o.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.Status,
                o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}