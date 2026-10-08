using Microsoft.EntityFrameworkCore;
using MyArmyTelegramBot.Application.Interfaces.Repositories;
using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Infrastructure.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<Question>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Questions
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .AsNoTracking().ToListAsync(ct);
        }

        public async Task<IEnumerable<Question>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Questions.Where(q => q.UserId == userId).AsNoTracking().ToListAsync(ct);
        }

        public async Task AddAsync(Question question, CancellationToken ct = default)
        {
            await _context.Questions.AddAsync(question, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
