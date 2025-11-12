namespace API.Dtos.User;

public record UpdateUserDto(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? PhoneNumber = null,
    string? Password = null,
    string? Street = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null
);
