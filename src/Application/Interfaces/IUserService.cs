using Application.Dtos;
using Domain.Entities;
using Domain.Vo;

namespace Application.Interfaces;

public interface IUserService
{
    Task<UserEntity> GetByIdAsync(Guid id);
    Task<UserEntity> UpdateAsync(Guid id, UpdateUserCommand cmd);
    Task<UserEntity> UpdateEmailAsync(Guid userId, EmailAddress newEmail);
    Task<UserEntity> UpdatePasswordAsync(Guid userId, string newPassword);
    Task DeleteOwnAccountAsync();

}
