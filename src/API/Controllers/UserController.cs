using API.Dtos.User;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Vo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Helper 
    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException("Utilisateur non authentifié"));

    // GET /api/users/me 
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
    {
        var user = await _userService.GetByIdAsync(GetCurrentUserId());
        return Ok(UserDto.FromEntity(user));
    }

    // PATCH /api/users/me/profile 
    [HttpPatch("me/profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateUserDto dto)
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

        var updatedUser = await _userService.UpdateAsync(GetCurrentUserId(), command);
        return Ok(UserDto.FromEntity(updatedUser));
    }

    // PATCH /api/users/me/email 
    [HttpPatch("me/email")]
    public async Task<ActionResult<UserDto>> UpdateEmail(UpdateEmailDto dto)
    {
        var newEmail = new EmailAddress(dto.Email);
        var updatedUser = await _userService.UpdateEmailAsync(GetCurrentUserId(), newEmail);
        return Ok(UserDto.FromEntity(updatedUser));
    }

    // PATCH /api/users/me/password 
    [HttpPatch("me/password")]
    public async Task<ActionResult<UserDto>> UpdatePassword(UpdatePasswordDto dto)
    {
        var updatedUser = await _userService.UpdatePasswordAsync(GetCurrentUserId(), dto.NewPassword);
        return Ok(UserDto.FromEntity(updatedUser));
    }
}
