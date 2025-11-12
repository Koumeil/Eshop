using Domain.Entities;

namespace API.Dtos.User;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    AddressDto Address,
    string Role)
{
    public static UserDto FromEntity(UserEntity user) => new(
        Id: user.Id,
        FirstName: user.FirstName,
        LastName: user.LastName,
        Email: user.Email.Value,
        PhoneNumber: user.PhoneNumber.Value,
        Address: new AddressDto(
            Street: user.Address.Street,
            City: user.Address.City,
            PostalCode: user.Address.PostalCode,
            Country: user.Address.Country
        ),
        Role: user.Role.Value       
    );
}
