using System;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICurrentUserService
{
    Task<UserEntity> GetCurrentUserAsync();
}
