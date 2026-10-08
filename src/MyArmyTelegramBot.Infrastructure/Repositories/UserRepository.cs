using Microsoft.EntityFrameworkCore;
using MyArmyTelegramBot.Application.Interfaces.Repositories;
using MyArmyTelegramBot.Domain.Entities;
using MyArmyTelegramBot.Domain.Enums;

namespace MyArmyTelegramBot.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<UserEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Users.AsNoTracking().ToListAsync(ct);
        }

        public async Task<IEnumerable<UserEntity>> GetAllByRoleAsync(UserRole role, CancellationToken ct = default)
        {
            return await _context.Users.Where(u => u.Role == role).AsNoTracking().ToListAsync(ct);
        }

        public async Task<IEnumerable<UserEntity>> GetAllByStatusAsync(UserApplicationStatus status, CancellationToken ct = default)
        {
            return await _context.Users.Where(u => u.Status == status).AsNoTracking().ToListAsync(ct);
        }

        public async Task AddAsync(UserEntity user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(UserEntity user, CancellationToken ct = default)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}
