using Domain.DomainExceptions;

namespace Domain.Vo;
public sealed record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainValidationException("Phone number cannot be null or whitespace.");

        var trimmed = phone.Trim();
        
        if (trimmed.Length < 3 || trimmed.Length > 16)
            throw new DomainValidationException("Phone number must be between 3 and 16 characters.");

        if (trimmed[0] != '+')
            throw new DomainValidationException("Phone number must start with '+'.");

        if (!trimmed.Substring(1).All(char.IsDigit))
            throw new DomainValidationException("Phone number must contain only digits after '+'.");

        if (trimmed[1] == '0')
            throw new DomainValidationException("Phone number cannot start with '+0'.");

        Value = trimmed;
    }

    public override string ToString() => Value;
    
    public static implicit operator PhoneNumber(string phone) => new(phone);
}