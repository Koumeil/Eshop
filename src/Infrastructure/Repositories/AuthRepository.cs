using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> FindByEmailAsync(EmailAddress email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserEntity?> FindByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }


    public async Task<bool> ExistsByEmailAsync(EmailAddress email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email.Value);
    }
    public void Add(UserEntity user)
    {
        _context.Users.Add(user);
    }
}