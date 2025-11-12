using Domain.DomainExceptions;

namespace Domain.Vo;
public sealed record Address
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string postalCode, string country)
    {
        Street = ValidateField(street, "Street", 100);
        City = ValidateField(city, "City", 50);
        PostalCode = ValidateField(postalCode, "PostalCode", 20);
        Country = ValidateField(country, "Country", 50);
    }

    private static string ValidateField(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be null or whitespace.");
        
        var trimmed = value.Trim();
        
        if (trimmed.Length > maxLength)
            throw new DomainValidationException($"{fieldName} cannot exceed {maxLength} characters.");
            
        if (trimmed.Length < 2)
            throw new DomainValidationException($"{fieldName} must be at least 2 characters.");
            
        return trimmed;
    }

    // Décomposition
    public void Deconstruct(out string street, out string city, out string postalCode, out string country)
    {
        street = Street;
        city = City;
        postalCode = PostalCode;
        country = Country;
    }

    public override string ToString() 
        => $"{Street}{Environment.NewLine}{PostalCode} {City}{Environment.NewLine}{Country}";

    public string ToSingleLine() => $"{Street}, {PostalCode} {City}, {Country}";
    
    public bool IsInCountry(params string[] countries) 
        => countries.Any(c => Country.Equals(c, StringComparison.OrdinalIgnoreCase));
}