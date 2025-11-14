
using Application.ApplicationExceptions;
using Application.Dtos;
using Application.Interfaces;
using Domain.DomainExceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;

namespace Application.Services;



public class AdminUserService : IAdminUserService
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdminUserService(IAdminUserRepository adminUserRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _adminUserRepository = adminUserRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private void EnsureAdmin(UserEntity currentUser)
    {
        if (!currentUser.Role.IsAdmin)
            throw new UnauthorizedAccessException("Only admins can access this operation.");
    }

    public async Task<List<UserEntity>> GetAllAsync()
    {
        return await _adminUserRepository.GetAllAsync();
    }

    public async Task<UserEntity> GetByIdAsync(Guid id)
    {
        return await _adminUserRepository.FindByIdAsync(id)
               ?? throw new NotFoundException("User", id);
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        if (await _adminUserRepository.ExistsByEmailAsync(user.Email))
            throw new ConflictException($"User with email '{user.Email.Value}' already exists.");

        _adminUserRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntity> UpdateAsync(Guid id, UpdateUserCommand cmd)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        EnsureAdmin(currentUser);

        var user = await _adminUserRepository.FindByIdAsync(id)
                   ?? throw new NotFoundException("User", id);

        if (cmd.FirstName != null || cmd.LastName != null)
            user.UpdateName(cmd.FirstName ?? user.FirstName, cmd.LastName ?? user.LastName);

        if (cmd.Email != null)
            await UpdateEmailAsync(id, new EmailAddress(cmd.Email));

        if (cmd.PhoneNumber != null)
            user.UpdatePhone(new PhoneNumber(cmd.PhoneNumber));

        if (cmd.Password != null)
            user.SetPassword(cmd.Password);

        if (cmd.Street != null || cmd.City != null || cmd.PostalCode != null || cmd.Country != null)
        {
            var newAddress = new Address(
                cmd.Street ?? user.Address.Street,
                cmd.City ?? user.Address.City,
                cmd.PostalCode ?? user.Address.PostalCode,
                cmd.Country ?? user.Address.Country
            );
            user.UpdateAddress(newAddress);
        }

        _adminUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _adminUserRepository.FindByIdAsync(id)
                   ?? throw new NotFoundException("User", id);

        _adminUserRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserEntity> UpdateEmailAsync(Guid id, EmailAddress newEmail)
    {
        if (newEmail is null) throw new DomainValidationException("Email cannot be null.");

        var user = await _adminUserRepository.FindByIdAsync(id)
                   ?? throw new NotFoundException("User", id);

        if (user.Email.Value != newEmail.Value && await _adminUserRepository.ExistsByEmailAsync(newEmail))
            throw new ConflictException("User", newEmail.Value);

        user.UpdateEmail(newEmail);
        _adminUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task ChangeUserRoleAsync(Guid userId, string newRole)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        EnsureAdmin(currentUser);

        var user = await _adminUserRepository.FindByIdAsync(userId)
                   ?? throw new NotFoundException("User", userId);

        user.SetRole(newRole);
        _adminUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

}

