using Domain.DomainExceptions;
using Domain.Vo;
using System;

namespace Domain.Entities;

public sealed class UserEntity : BaseEntity
{
    #region Constants
    private const int MaxNameLength = 100;
    private const int MinNameLength = 2;
    #endregion

    #region Properties
    public  string FirstName { get; private set; }
    public string LastName { get; private set; }
    public EmailAddress Email { get; private set; }
    public Address Address { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Password Password { get; private set; }
    public Role Role { get; private init; }
    public DateTime? LastLoginDate { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    #endregion

    #region Constructors
    public UserEntity(
        string firstName,
        string lastName,
        Address address,
        EmailAddress email,
        PhoneNumber phoneNumber,
        Password password)
    {
        FirstName = ValidateName(firstName, nameof(firstName));
        LastName = ValidateName(lastName, nameof(lastName));
        Email = email ?? throw new DomainValidationException("Email cannot be null.");
        PhoneNumber = phoneNumber ?? throw new DomainValidationException("PhoneNumber cannot be null.");
        Address = address ?? throw new DomainValidationException("Address cannot be null.");
        Password = password ?? throw new DomainValidationException("Password cannot be null.");
        Role = Role.User;
    }

    private UserEntity()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = null!;
        PhoneNumber = null!;
        Address = null!;
        Password = null!;
    }
    #endregion

    #region Domain Behaviors

    public void SetPassword(string rawPassword)
    {
        Password = new Password(rawPassword);
        MarkAsUpdated();
    }

    public bool VerifyPassword(string plainTextPassword)
    {
        return Password.Verify(plainTextPassword);
    }

    public void UpdateEmail(EmailAddress newEmail)
    {
        Email = newEmail ?? throw new DomainValidationException("Email cannot be null.");
        MarkAsUpdated();
    }

    public void UpdatePhone(PhoneNumber newPhone)
    {
        PhoneNumber = newPhone ?? throw new DomainValidationException("Phone cannot be null.");
        MarkAsUpdated();
    }

    public void UpdateAddress(Address newAddress)
    {
        Address = newAddress ?? throw new DomainValidationException("Address cannot be null.");
        MarkAsUpdated();
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = ValidateName(firstName, nameof(firstName));
        LastName = ValidateName(lastName, nameof(lastName));
        MarkAsUpdated();
    }

    public void RegisterLogin()
    {
        LastLoginDate = DateTime.UtcNow;
        MarkAsUpdated();
    }

    #endregion

    #region Private Methods
    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException($"{paramName} cannot be null or whitespace.");

        var trimmed = name.Trim();

        if (trimmed.Length < MinNameLength)
            throw new DomainValidationException($"{paramName} must be at least {MinNameLength} characters.");

        if (trimmed.Length > MaxNameLength)
            throw new DomainValidationException($"{paramName} cannot exceed {MaxNameLength} characters.");

        return trimmed;
    }
    #endregion
}