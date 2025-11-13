using System.Text.Json;
using Domain.Entities;
using Domain.Vo;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeds;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
            return;

        var possiblePaths = new[]
        {
            // env dev (out of Docker)
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Infrastructure", "Seeds", "userSeeds.json"),
            // in docker after publish
            Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Seeds", "userSeeds.json"),
            // alternativ in docker
            Path.Combine(Directory.GetCurrentDirectory(), "Seeds", "userSeeds.json"),
            // absolu in the container
            "/app/Infrastructure/Seeds/userSeeds.json",
            "/app/Seeds/userSeeds.json"
        };

        string? jsonData = null;
        string foundPath = "";

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                jsonData = await File.ReadAllTextAsync(path);
                foundPath = path;
                break;
            }
        }

        if (string.IsNullOrEmpty(jsonData))
        {
            // log all path for debog
            var triedPaths = string.Join(", ", possiblePaths);
            throw new FileNotFoundException($"Seed file not found. Tried paths: {triedPaths}");
        }

        var userDtos = JsonSerializer.Deserialize<List<UserSeedDto>>(jsonData, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        if (userDtos == null || !userDtos.Any())
            return;

        var users = userDtos.Select(dto => new UserEntity(
            dto.FirstName,
            dto.LastName,
            new Address(dto.Street, dto.City, dto.PostalCode, dto.Country),
            new EmailAddress(dto.Email),
            new PhoneNumber(dto.PhoneNumber),
            new Password(dto.Password)
        )).ToList();

        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
    }

    private class UserSeedDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}