namespace Application.Dtos;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    string Street,
    string City,
    string PostalCode,
    string Country);
