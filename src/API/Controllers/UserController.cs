using API.Dtos.User;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Vo;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        var dtos = users.ConvertAll(UserDto.FromEntity);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(UserDto.FromEntity(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        var address = new Address(dto.Street, dto.City, dto.PostalCode, dto.Country);
        var email = new EmailAddress(dto.Email);
        var phone = new PhoneNumber(dto.PhoneNumber);
        var password = new Password(dto.Password);

        var newUser = new UserEntity(dto.FirstName, dto.LastName, address, email, phone, password);
        var createdUser = await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, UserDto.FromEntity(createdUser));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserDto dto)
    {
        var command = new UpdateUserCommand
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

        var updatedUser = await _userService.UpdateAsync(id, command);
        return Ok(UserDto.FromEntity(updatedUser));
    }

 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/email")]
    public async Task<ActionResult<UserDto>> UpdateEmail(Guid id, UpdateEmailDto dto)
    {
        var newEmail = new EmailAddress(dto.Email);
        var updatedUser = await _userService.UpdateEmailAsync(id, newEmail);
        return Ok(UserDto.FromEntity(updatedUser));
    }

    [HttpPatch("{id:guid}/password")]
    public async Task<ActionResult<UserDto>> UpdatePassword(Guid id, UpdatePasswordDto dto)
    {
        var updatedUser = await _userService.UpdatePasswordAsync(id, dto.NewPassword);
        return Ok(UserDto.FromEntity(updatedUser));
    }
}
