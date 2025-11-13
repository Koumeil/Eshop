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

        // Essayer plusieurs chemins possibles pour trouver le fichier
        var possiblePaths = new[]
        {
            // Chemin dans l'environnement de développement (en dehors de Docker)
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Infrastructure", "Seeds", "userSeeds.json"),
            // Chemin dans Docker après la publication
            Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Seeds", "userSeeds.json"),
            // Chemin alternatif dans Docker
            Path.Combine(Directory.GetCurrentDirectory(), "Seeds", "userSeeds.json"),
            // Chemin absolu dans le conteneur
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
            // Logger tous les chemins essayés pour le débogage
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