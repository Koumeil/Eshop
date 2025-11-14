using Domain.Entities;
using Domain.Vo;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> FindByIdAsync(Guid id);
    void Update(UserEntity user);
    void Remove(UserEntity user);
}

