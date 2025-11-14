using System.ComponentModel.DataAnnotations;

namespace Application.Settings;

public sealed class JwtSettings
{
    [Required]
    public string SecretKey { get; init; } = null!;

    [Range(1, int.MaxValue)]
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpirationDays { get; init; } = 7;

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;

}