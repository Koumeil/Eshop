using Domain.Entities;
using Domain.Vo;

namespace Domain.Interfaces;

public interface IAuthRepository
{

    Task<UserEntity?> FindByEmailAsync(EmailAddress email);
    Task<UserEntity?> FindByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(EmailAddress email);
    void Add(UserEntity user);
}