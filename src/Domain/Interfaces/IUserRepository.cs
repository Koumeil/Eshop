using Domain.Entities;
using Domain.Vo;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<List<UserEntity>> GetAllAsync();
    Task<UserEntity?> FindByIdAsync(Guid id);
    Task<UserEntity?> FindByEmailAsync(EmailAddress email);
    void Add(UserEntity user);
    void Update(UserEntity user);
    void Remove(UserEntity user);
    Task<bool> ExistsByEmailAsync(EmailAddress email);

}

