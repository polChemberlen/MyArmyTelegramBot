using AutoMapper;
using MyArmyTelegramBot.Application.DTOs;
using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Application.Mappings
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionDTO>();
        }
    }
}
