using API.Dtos.User;
using Application.Dtos;
using Application.Interfaces;
using Domain.DomainExceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Properties;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminService;

    public AdminUserController(IAdminUserService adminService, IUserRepository userRepository)
    {
        _adminService = adminService;
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var address = new Address(dto.Street, dto.City, dto.PostalCode, dto.Country);
        var email = new EmailAddress(dto.Email);
        var phone = new PhoneNumber(dto.PhoneNumber);
        var password = new Password(dto.Password);

        var newUser = new UserEntity(dto.FirstName, dto.LastName, address, email, phone, password);
        var createdUser = await _adminService.CreateAsync(newUser);

        return Ok(new { createdUser.Id, createdUser.FirstName, createdUser.LastName, createdUser.Role });
    }

    // READ ALL
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllAsync();
        return Ok(users.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.Role }));
    }

    // READ BY ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _adminService.GetByIdAsync(id) ?? throw new DomainValidationException("User not found");
        return Ok(new { user.Id, user.FirstName, user.LastName, user.Email, user.Role });
    }

    // UPDATE
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto dto)
    {
        var updateUserCommand = new UpdateUserCommand()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Password = dto.Password,
            Street = dto.Street,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };

        await _adminService.UpdateAsync(id, updateUserCommand);

        return NoContent();
    }

    // CHANGE ROLE
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeRoleDto dto)
    {
        await _adminService.ChangeUserRoleAsync(id, dto.NewRole);
        return NoContent();
    }

    // DELETE
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _adminService.DeleteAsync(id);
        return NoContent();
    }
}
