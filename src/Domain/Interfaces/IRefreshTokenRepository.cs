using Domain.Entities;

namespace Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshTokenEntity token);
    Task<RefreshTokenEntity?> GetByTokenAsync(string token);
    Task RevokeAsync(string token);
}