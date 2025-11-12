namespace API.Dtos.User;

public record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country);
