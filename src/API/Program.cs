using API.Middleware;
using Application;
using Asp.Versioning;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ---------------------
// Logging
// ---------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ---------------------
// Configuration
// ---------------------
builder.Configuration
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

// ---------------------
// Services
// ---------------------
builder.Services.AddInfrastructure(builder.Configuration); // DB, repos, etc.
builder.Services.AddApplication(); // Application layer services

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1",
        Description = "E-Shop API is an online E-store using Clean Architecture, Domain Drive-Design and MediatR. Manages your products, customers, orders, payments etc.."

    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// ---------------------
// Database migrations (safe)
// ---------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var retryCount = 0;
    const int maxRetries = 5;

    while (retryCount < maxRetries)
    {
        try
        {
            if (db.Database.CanConnect())
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");

                await Infrastructure.Seeds.UserSeeder.SeedUsersAsync(db);
                logger.LogInformation("User seeds applied successfully.");
                break;
            }
        }
        catch (Exception ex)
        {
            retryCount++;
            logger.LogWarning(ex, "Attempt {RetryCount}/{MaxRetries} - Database not ready yet", retryCount, maxRetries);

            if (retryCount >= maxRetries)
            {
                logger.LogError("Failed to apply migrations after {MaxRetries} attempts", maxRetries);
                throw;
            }

            await Task.Delay(TimeSpan.FromSeconds(5 * retryCount));
        }
    }
}

// ---------------------
// Middleware pipeline
// ---------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnablePersistAuthorization();
        options.DisplayRequestDuration();
    });
}

// Force HTTPS everywhere
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRouting();

// Exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Health check endpoint
app.MapHealthChecks("/health");

// Default route
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseAuthorization();
app.MapControllers();

// ---------------------
// Run app
// ---------------------
try
{
    app.Run();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Application failed to start.");
    throw;
}
