using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IAuthRepository _authRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IAuthRepository authRepository, IHttpContextAccessor httpContextAccessor)
    {
        _authRepository = authRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserEntity> GetCurrentUserAsync()
    {
        var userId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("sub")!.Value);
        return await _authRepository.FindByIdAsync(userId) ?? throw new Exception("User not found");
    }
}
