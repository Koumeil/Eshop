using Domain.DomainExceptions;
using System.Security.Cryptography;

namespace Domain.Vo;

public sealed record Password
{
    private const int MinimumLength = 10;

    public string HashedValue { get; }

    public Password(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new DomainValidationException("Password cannot be null or whitespace.");

        var trimmed = rawPassword.Trim();

        if (trimmed.Length < MinimumLength)
            throw new DomainValidationException($"Password must be at least {MinimumLength} characters.");

        if (!HasUppercase(trimmed))
            throw new DomainValidationException("Password must contain at least one uppercase letter.");

        if (!HasLowercase(trimmed))
            throw new DomainValidationException("Password must contain at least one lowercase letter.");

        if (!HasDigit(trimmed))
            throw new DomainValidationException("Password must contain at least one digit.");

        if (!HasSpecialChar(trimmed))
            throw new DomainValidationException("Password must contain at least one special character.");

        HashedValue = ComputeHash(trimmed);
    }

    private static bool HasUppercase(string input) =>
        input.Any(char.IsUpper);

    private static bool HasLowercase(string input) =>
        input.Any(char.IsLower);

    private static bool HasDigit(string input) =>
        input.Any(char.IsDigit);

    private static bool HasSpecialChar(string input)
    {
        const string specials = @"!@#$%^&*()-_=+[{]}\|;:'"",<.>/?";
        return input.Any(c => specials.Contains(c));
    }

    private static string ComputeHash(string rawPassword)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);

        using var deriveBytes = new Rfc2898DeriveBytes(
            rawPassword,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256
        );

        var hashBytes = deriveBytes.GetBytes(32);
        var base64Salt = Convert.ToBase64String(saltBytes);
        var base64Hash = Convert.ToBase64String(hashBytes);

        return $"{base64Salt}:{base64Hash}";
    }

    public bool Verify(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return false;

        var parts = HashedValue.Split(':');
        if (parts.Length != 2)
            return false;

        var saltBytes = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        using var deriveBytes = new Rfc2898DeriveBytes(
            plainText,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256
        );

        var actualHash = deriveBytes.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    public override string ToString() => "***";
}