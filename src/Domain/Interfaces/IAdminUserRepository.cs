using Domain.Entities;
using Domain.Vo;

namespace Domain.Interfaces;

public interface IAdminUserRepository
{
    Task<UserEntity?> FindByIdAsync(Guid id);
    Task<List<UserEntity>> GetAllAsync();
    void AddAsync(UserEntity user);
    void Update(UserEntity user);
    void Remove(UserEntity user);
    Task<bool> ExistsByEmailAsync(EmailAddress email);
}