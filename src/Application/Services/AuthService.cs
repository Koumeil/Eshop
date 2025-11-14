using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.ApplicationExceptions;
using Application.Dtos;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, JwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.FindByEmailAsync(new EmailAddress(dto.Email))
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.Password.Verify(dto.Password))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false
        });

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(CreateUserCommand dto)
    {
        var email = new EmailAddress(dto.Email);
        if (await _userRepository.ExistsByEmailAsync(email))
            throw new ConflictException($"User with email '{email.Value}' already exists.");

        var address = new Address(dto.Street, dto.City, dto.PostalCode, dto.Country);
        var phone = new PhoneNumber(dto.PhoneNumber);
        var password = new Password(dto.Password);

        var newUser = new UserEntity(dto.FirstName, dto.LastName, address, email, phone, password);
        
        _userRepository.Add(newUser);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = GenerateJwtToken(newUser);
        var refreshToken = GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshTokenEntity
        {
            UserId = newUser.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false
        });

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userRepository.FindByIdAsync(storedToken.UserId)
            ?? throw new UnauthorizedAccessException("User not found");

        // Revoke old token
        await _refreshTokenRepository.RevokeAsync(refreshToken);

        // Generate new tokens
        var newAccessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false
        });

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (token is null || token.IsRevoked)
            return;

        await _refreshTokenRepository.RevokeAsync(refreshToken);
    }
}
