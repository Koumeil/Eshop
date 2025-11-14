using Domain.DomainExceptions;
using System.Security.Cryptography;
using System.Text.Json;

namespace Domain.Vo;

public sealed record Password
{
    private const int MinimumLength = 10;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 310_000;

    public string HashedValue { get; }

    public Password(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new DomainValidationException("Password cannot be empty.");

        var trimmed = rawPassword.Trim();
        ValidatePasswordRules(trimmed);
        HashedValue = HashPassword(trimmed);
    }

    public Password(string hashedValue, bool isHashed)
    {
        HashedValue = hashedValue;
    }

    private static void ValidatePasswordRules(string password)
    {
        if (password.Length < MinimumLength)
            throw new DomainValidationException($"Password must be at least {MinimumLength} characters.");

        if (!password.Any(char.IsUpper))
            throw new DomainValidationException("Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            throw new DomainValidationException("Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            throw new DomainValidationException("Password must contain at least one digit.");

        const string specials = @"!@#$%^&*()-_=+[{]}\|;:'"",<.>/?";
        if (!password.Any(c => specials.Contains(c)))
            throw new DomainValidationException("Password must contain at least one special character.");
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var hash = deriveBytes.GetBytes(HashSize);

        var PasswordHashData = new PasswordHashData
        {
            v = 1,
            i = Iterations,
            s = Convert.ToBase64String(salt),
            h = Convert.ToBase64String(hash)
        };

        return JsonSerializer.Serialize(PasswordHashData);
    }

    public static bool Verify(string plainText, string hashedJson)
    {
        if (string.IsNullOrWhiteSpace(plainText) || string.IsNullOrWhiteSpace(hashedJson))
            return false;

        try
        {
            var info = JsonSerializer.Deserialize<PasswordHashData>(hashedJson);
            if (info is null || info.v != 1)
                return false;

            string saltBase64 = info.s.Replace("\\u002B", "+");
            string hashBase64 = info.h.Replace("\\u002B", "+");

            var salt = Convert.FromBase64String(saltBase64);
            var expectedHash = Convert.FromBase64String(hashBase64);

            using var deriveBytes = new Rfc2898DeriveBytes(
                plainText,
                salt,
                info.i,
                HashAlgorithmName.SHA256);

            var actualHash = deriveBytes.GetBytes(expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => "***";

    private sealed class PasswordHashData
    {
        public int v { get; set; }
        public int i { get; set; }
        public string s { get; set; } = string.Empty;
        public string h { get; set; } = string.Empty;
    }
}
