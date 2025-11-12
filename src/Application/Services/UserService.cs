using Application.ApplicationExceptions;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Errors;
using Domain.Interfaces;
using Domain.Vo;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserEntity>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Count > 0 ? users : new List<UserEntity>();
    }


    public async Task<UserEntity> GetByIdAsync(Guid id)
    {
        return await _userRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("User", id);
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        if (await _userRepository.ExistsByEmailAsync(user.Email))
            throw new ConflictException($"User with email '{user.Email.Value}' already exists.");

        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync();
        return user;
    }
    public async Task<UserEntity> UpdateAsync(Guid id, UpdateUserCommand cmd)
    {
        var existingUser = await _userRepository.FindByIdAsync(id);

        if (existingUser is null)
            throw new NotFoundException("User", id);

        if (cmd.FirstName != null || cmd.LastName != null)
        {
            var firstName = cmd.FirstName ?? existingUser.FirstName;
            var lastName = cmd.LastName ?? existingUser.LastName;
            existingUser.UpdateName(firstName, lastName);
        }

        if (cmd.Email != null)
        {
            var newEmail = new EmailAddress(cmd.Email);
            if (existingUser.Email.Value != newEmail.Value &&
                await _userRepository.ExistsByEmailAsync(newEmail))
                throw new ConflictException("User", newEmail.Value);

            existingUser.UpdateEmail(newEmail);
        }

        if (cmd.PhoneNumber != null)
            existingUser.UpdatePhone(new PhoneNumber(cmd.PhoneNumber));

        if (cmd.Password != null)
            existingUser.SetPassword(cmd.Password);

        if (cmd.Street != null || cmd.City != null || cmd.PostalCode != null || cmd.Country != null)
        {
            var newAddress = new Address(
                cmd.Street ?? existingUser.Address.Street,
                cmd.City ?? existingUser.Address.City,
                cmd.PostalCode ?? existingUser.Address.PostalCode,
                cmd.Country ?? existingUser.Address.Country
            );
            existingUser.UpdateAddress(newAddress);
        }

        _userRepository.Update(existingUser);
        await _unitOfWork.SaveChangesAsync();

        return existingUser;
    }


    public async Task DeleteAsync(Guid id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        _userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync();
    }



    public async Task<UserEntity> UpdateEmailAsync(Guid userId, EmailAddress newEmail)
    {
        if (newEmail is null)
            throw new DomainValidationException("Email cannot be null.");

        var user = await _userRepository.FindByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User", userId);

        if (user.Email.Value != newEmail.Value)
        {
            if (await _userRepository.ExistsByEmailAsync(newEmail))
                throw new ConflictException("User", newEmail.Value);

            user.UpdateEmail(newEmail);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        return user;
    }

    public async Task<UserEntity> UpdatePasswordAsync(Guid userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new DomainValidationException("Password cannot be null or empty.");

        var user = await _userRepository.FindByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User", userId);

        user.SetPassword(newPassword);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

}
