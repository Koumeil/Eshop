using Application.ApplicationExceptions;
using Application.Dtos;
using Application.Interfaces;
using Domain.DomainExceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrentUserService _currentUserService;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UserEntity> GetByIdAsync(Guid userId)
    {
        return await _userRepository.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);
    }

    public async Task<UserEntity> UpdateAsync(Guid userId, UpdateUserCommand cmd)
    {
        var user = await _userRepository.FindByIdAsync(userId)
                   ?? throw new NotFoundException("User", userId);

        if (cmd.FirstName != null || cmd.LastName != null)
            user.UpdateName(cmd.FirstName ?? user.FirstName, cmd.LastName ?? user.LastName);

        if (cmd.Email != null)
            await UpdateEmailAsync(userId, new EmailAddress(cmd.Email));

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

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntity> UpdateEmailAsync(Guid userId, EmailAddress newEmail)
    {
        if (newEmail is null) throw new DomainValidationException("Email cannot be null.");

        var user = await _userRepository.FindByIdAsync(userId)
                   ?? throw new NotFoundException("User", userId);

        // Ici on peut enlever ExistsByEmailAsync si on considère que l'email du user est unique
        user.UpdateEmail(newEmail);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntity> UpdatePasswordAsync(Guid userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new DomainValidationException("Password cannot be null or empty.");

        var user = await _userRepository.FindByIdAsync(userId)
                   ?? throw new NotFoundException("User", userId);

        user.SetPassword(newPassword);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }
    public async Task DeleteOwnAccountAsync()
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();

        var user = await _userRepository.FindByIdAsync(currentUser.Id)
                   ?? throw new NotFoundException("User", currentUser.Id);

        _userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync();

        // Déconnexion / invalidation de session
    }
}
