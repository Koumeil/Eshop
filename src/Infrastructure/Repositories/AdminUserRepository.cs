using System;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly ApplicationDbContext _context;

    public AdminUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> FindByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<UserEntity>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public void AddAsync(UserEntity user)
    {
         _context.Users.AddAsync(user);
    }

    public void Update(UserEntity user)
    {
        _context.Users.Update(user);
    }

    public void Remove(UserEntity user)
    {
        _context.Users.Remove(user);
    }

    public async Task<bool> ExistsByEmailAsync(EmailAddress email)
    {
        return await _context.Users.AnyAsync(u => u.Email.Value == email.Value);
    }
}
