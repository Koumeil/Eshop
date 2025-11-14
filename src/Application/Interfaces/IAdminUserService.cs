using Application.Dtos;
using Domain.Entities;
using Domain.Vo;

namespace Application.Interfaces;

public interface IAdminUserService
{
    Task ChangeUserRoleAsync(Guid userId, string newRole);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task DeleteAsync(Guid id);
    Task<List<UserEntity>> GetAllAsync();
    Task<UserEntity> GetByIdAsync(Guid id);
    Task<UserEntity> UpdateAsync(Guid id, UpdateUserCommand cmd);
    Task<UserEntity> UpdateEmailAsync(Guid id, EmailAddress newEmail);
}