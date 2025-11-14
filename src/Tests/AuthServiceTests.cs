using Application.ApplicationExceptions;
using Application.Dtos;
using Application.Services;
using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Vo;
using FluentAssertions;
using Moq;

namespace Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly JwtSettings _jwtSettings = new()
    {
        SecretKey = "SuperSecretKey123456789012345678901234567890",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    };

    private AuthService CreateService() =>
        new AuthService(_userRepoMock.Object, _refreshRepoMock.Object, _unitOfWorkMock.Object, _jwtSettings);


    // TEST REGISTER
    [Fact]
    public async Task RegisterAsync_WithNewUser_ReturnsTokens()
    {
        var dto = new CreateUserCommand(
        "Koumeïl",
        "Testeur",
        "newuser@example.com",
        "+33123456789",
        "Password@123",
        "Rue",
        "Bruxelles",
        "1000",
        "Belgique"
        );

        _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.Is<EmailAddress>(e => e.Value == dto.Email)))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsConflictException()
    {
        // Arrange
        var dto = new CreateUserCommand(

            "Koumeïl",
            "Testeur",
            "existing@example.com",
            "+33123456789",
            "Password@123",
            "Rue",
            "Bruxelles",
            "1000",
            "Belgique"
        );

        _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.Is<EmailAddress>(e => e.Value == dto.Email)))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(dto));
    }

    // TEST LOGIN
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = new UserEntity(
            "Koumeïl", "Test",
            new Address("Rue", "Bruxelles", "1000", "Belgique"),
            new EmailAddress("test@example.com"),
            new PhoneNumber("+33123456789"),
            new Password("Password@123")
        );

        _userRepoMock.Setup(r => r.FindByEmailAsync(new EmailAddress("test@example.com")))
            .ReturnsAsync(user);

        var service = CreateService();

        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password@123"
        };

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = new UserEntity(
            "Koumeïl", "Test",
            new Address("Rue", "Bruxelles", "1000", "Belgique"),
            new EmailAddress("test@example.com"),
            new PhoneNumber("+33123456789"),
            new Password("Passwor@d123")
        );

        _userRepoMock.Setup(r => r.FindByEmailAsync(new EmailAddress("test@example.com")))
            .ReturnsAsync(user);

        var service = CreateService();

        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    // TEST REFRESH TOKEN
    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var user = new UserEntity(
            "Koumeïl", "Testeur",
            new Address("Rue", "Bruxelles", "1000", "Belgique"),
            new EmailAddress("test@example.com"),
            new PhoneNumber("+32123456789"),
            new Password("Password@123")
        );

        var validToken = new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = "valid-refresh-token",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _refreshRepoMock.Setup(r => r.GetByTokenAsync("valid-refresh-token"))
            .ReturnsAsync(validToken);

        _userRepoMock.Setup(r => r.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        var result = await service.RefreshTokenAsync("valid-refresh-token");

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }


    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsUnauthorized()
    {
        var expiredToken = new RefreshTokenEntity
        {
            UserId = Guid.NewGuid(),
            Token = "expired-token",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        _refreshRepoMock.Setup(r => r.GetByTokenAsync("expired-token"))
            .ReturnsAsync(expiredToken);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync("expired-token"));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ThrowsUnauthorized()
    {
        var revokedToken = new RefreshTokenEntity
        {
            UserId = Guid.NewGuid(),
            Token = "revoked-token",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };

        _refreshRepoMock.Setup(r => r.GetByTokenAsync("revoked-token"))
            .ReturnsAsync(revokedToken);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync("revoked-token"));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUnknownToken_ThrowsUnauthorized()
    {
        _refreshRepoMock.Setup(r => r.GetByTokenAsync("unknown-token"))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync("unknown-token"));
    }

    // TEST REVOKE TOKEN

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_RevokesSuccessfully()
    {
        // Arrange
        var token = new RefreshTokenEntity
        {
            UserId = Guid.NewGuid(),
            Token = "valid-token",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _refreshRepoMock.Setup(r => r.GetByTokenAsync("valid-token"))
            .ReturnsAsync(token);

        _refreshRepoMock.Setup(r => r.RevokeAsync("valid-token"))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.RevokeTokenAsync("valid-token");

        // Assert
        _refreshRepoMock.Verify(r => r.RevokeAsync("valid-token"), Times.Once);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithAlreadyRevokedToken_DoesNothing()
    {
        var token = new RefreshTokenEntity
        {
            UserId = Guid.NewGuid(),
            Token = "revoked-token",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };

        _refreshRepoMock.Setup(r => r.GetByTokenAsync("revoked-token"))
            .ReturnsAsync(token);

        var service = CreateService();

        await service.RevokeTokenAsync("revoked-token");

        _refreshRepoMock.Verify(r => r.RevokeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithUnknownToken_DoesNothing()
    {
        _refreshRepoMock.Setup(r => r.GetByTokenAsync("unknown-token"))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var service = CreateService();

        await service.RevokeTokenAsync("unknown-token");

        _refreshRepoMock.Verify(r => r.RevokeAsync(It.IsAny<string>()), Times.Never);
    }
}