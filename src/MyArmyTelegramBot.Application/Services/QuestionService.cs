using AutoMapper;
using MyArmyTelegramBot.Application.DTOs;
using MyArmyTelegramBot.Application.Interfaces.Repositories;
using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Application.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public QuestionService(IQuestionRepository questionRepository, IUserService userService, IMapper mapper)
        {
            _questionRepository = questionRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<QuestionDTO> CreateQuestionAsync(QuestionDTO dto, CancellationToken ct = default)
        {
            var question = new Question(
                userId: dto.UserId,
                questionText: dto.QuestionText
            );

            await _questionRepository.AddAsync(question, ct);
            return _mapper.Map<QuestionDTO>(question);
        }

        public async Task<IEnumerable<QuestionDTO>> GetAllQuestionsAsync(CancellationToken ct = default)
        {
            var questions = await _questionRepository.GetAllAsync(ct);
            return _mapper.Map<IEnumerable<QuestionDTO>>(questions);
        }

        public async Task<IEnumerable<QuestionDTO>> GetAllQuestionsByTelegramId(long telegramId, CancellationToken ct = default)
        {
            var user = await _userService.GetByTelegramIdAsync(telegramId, ct);
            if (user == null) return Enumerable.Empty<QuestionDTO>();

            var questions = await _questionRepository.GetByUserIdAsync(user.Id, ct);

            return _mapper.Map<IEnumerable<QuestionDTO>>(questions);
        }
    }
}


