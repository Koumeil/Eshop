using System.Text.RegularExpressions;
using Domain.DomainExceptions;

namespace Domain.Vo;

public sealed record EmailAddress
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(250) // Timeout 
    );

    public string Value { get; }

    public EmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email cannot be null or whitespace.");

        var trimmed = email.Trim();
        
        // Validation  (RFC 5321)
        if (trimmed.Length > 254)
            throw new DomainValidationException("Email cannot exceed 254 characters.");
            
        if (trimmed.Length < 3)
            throw new DomainValidationException("Email must be at least 3 characters.");

        // Validation format avec timeout
        try
        {
            if (!EmailRegex.IsMatch(trimmed))
                throw new DomainValidationException($"Invalid email format: '{email}'.");
        }
        catch (RegexMatchTimeoutException)
        {
            throw new DomainValidationException("Email validation timeout.");
        }

        Value = trimmed.ToLowerInvariant(); 
    }

    public override string ToString() => Value;

    public string GetDomain() => Value.Split('@').LastOrDefault() ?? string.Empty;
    
    public bool IsFromDomain(params string[] domains) 
        => domains.Any(domain => GetDomain().Equals(domain, StringComparison.OrdinalIgnoreCase));
}