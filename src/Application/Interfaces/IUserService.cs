using Application.Dtos;
using Domain.Entities;
using Domain.Vo;

namespace Application.Interfaces;

public interface IUserService
{
    Task<List<UserEntity>> GetAllAsync();
    Task<UserEntity> GetByIdAsync(Guid id);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task<UserEntity> UpdateAsync(Guid id, UpdateUserCommand cmd);
    Task DeleteAsync(Guid id);
    Task<UserEntity> UpdateEmailAsync(Guid userId, EmailAddress newEmail);
    Task<UserEntity> UpdatePasswordAsync(Guid userId, string newPassword);
}
