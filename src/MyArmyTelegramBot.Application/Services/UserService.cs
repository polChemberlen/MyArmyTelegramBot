using AutoMapper;
using MyArmyTelegramBot.Application.DTOs;
using MyArmyTelegramBot.Application.Interfaces.Repositories;
using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Entities;
using MyArmyTelegramBot.Domain.Enums;

namespace MyArmyTelegramBot.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UserDTO> GetOrCreateUserAsync(UserDTO dto, CancellationToken ct = default)
        {
            var user = await _repository.GetByTelegramIdAsync(dto.TelegramId, ct);

            if (user == null)
            {
                user = new UserEntity(
                    telegramId: dto.TelegramId,
                    username: dto.Username,
                    firstName: dto.FirstName,
                    lastName: dto.LastName,
                    approvedAt: null
                );

                await _repository.AddAsync(user, ct);
                return _mapper.Map<UserDTO>(user);
            }

            if (user.Username != dto.Username ||
                user.FirstName != (dto.FirstName ?? string.Empty) ||
                user.LastName != dto.LastName)
            {
                user.Update(dto.Username, dto.FirstName, dto.LastName);
                await _repository.UpdateAsync(user, ct);
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetApprovedUsersAsync(CancellationToken ct = default)
        {
            var users = await _repository.GetAllByStatusAsync(UserApplicationStatus.Approved,ct);
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
        {
            var user = await _repository.GetByTelegramIdAsync(telegramId, ct);
            if (user == null) return null;

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _repository.GetByIdAsync(userId, ct);
            if (user == null) return null;
            return _mapper.Map<UserDTO>(user);
        }

        public async Task SubmitApplicationAsync(long telegramId, CancellationToken ct = default)
        {
            var user = await _repository.GetByTelegramIdAsync(telegramId);

            if (user == null) return;

            if(user.IsNew())
            {
                user.SetStatus(UserApplicationStatus.Pending);
                await _repository.UpdateAsync(user, ct);
            }
        }

        public async Task ApproveUserAsync(long telegramId, CancellationToken ct = default)
        {
            var user = await _repository.GetByTelegramIdAsync(telegramId);

            if (user == null) return;

            if (user.Status == UserApplicationStatus.Pending)
            {
                user.SetStatus(UserApplicationStatus.Approved);
                user.SetRole(UserRole.ApprovedUser);

                await _repository.UpdateAsync(user, ct);
            }
        }

        public async Task RejectUserAsync(long telegramId, CancellationToken ct = default)
        {
            var user = await _repository.GetByTelegramIdAsync(telegramId);

            if (user == null) return;

            if (user.Status == UserApplicationStatus.Pending)
            {
                user.SetStatus(UserApplicationStatus.Rejected);
                user.SetRole(UserRole.Guest);

                await _repository.UpdateAsync(user, ct);
            }
        }

        public async Task BanUserAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _repository.GetByIdAsync(userId);

            if (user == null) return;

            if (user.Status == UserApplicationStatus.Approved)
            {
                user.SetStatus(UserApplicationStatus.None);
                user.SetRole(UserRole.Guest);

                await _repository.UpdateAsync(user, ct);
            }
        }

        public async Task<UserDTO> GetApprovedTelegramIdsAsync()
        {
            var users = await _repository.GetAllByRoleAsync(UserRole.ApprovedUser);
            return _mapper.Map<UserDTO>(users);
        }
    }
}
