using API.Middleware;
using Application;
using Application.Settings;
using Asp.Versioning;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Configuration
// =====================
// Add secrets and environment variables
builder.Configuration
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// =====================
// Services Registration
// =====================
builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure layer
builder.Services.AddApplication();                          // Application layer

// Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("Eshop");

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings missing");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Shop API",
        Version = "v1",
        Description = "E-Commerce Platform API"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Health Checks
builder.Services.AddHealthChecks();

// =====================
// App Building
// =====================
var app = builder.Build();

// =====================
// Database Initialization
// =====================
await InitializeDatabaseAsync(app);

// =====================
// Middleware Pipeline
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnablePersistAuthorization();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

// =====================
// Application Start
// =====================
Console.WriteLine($"🚀 E-Shop API started in {app.Environment.EnvironmentName} environment");
app.Run();

// =====================
// Database Initialization Method
// =====================
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        Console.WriteLine("🔍 Checking database...");

        // Ensure DB and tables are created if missing
        // await db.Database.EnsureCreatedAsync(); 
        await db.Database.MigrateAsync();

        Console.WriteLine("✅ Database and tables created (if they didn't exist)");

        // Seed initial data
        await SeedInitialDataAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization error: {ex.Message}");
    }
}

// =====================
// Data Seeding
// =====================
static async Task SeedInitialDataAsync(ApplicationDbContext db)
{
    try
    {
        var hasUsers = await db.Users.AnyAsync();

        if (!hasUsers)
        {
            Console.WriteLine("🌱 Seeding initial data...");
            await Infrastructure.Seeds.UserSeeder.SeedUsersAsync(db);
            Console.WriteLine("✅ Initial data seeded");
        }
        else
        {
            Console.WriteLine("✅ Data already present, seeding skipped");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Data seeding error: {ex.Message}");
    }
}
