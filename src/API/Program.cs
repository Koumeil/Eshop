using API.Middleware;
using Application;
using Asp.Versioning;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;

Console.WriteLine("=== 🚀 API STARTING ===");

try
{
    // ---------------------
    // Builder Configuration
    // ---------------------
    Console.WriteLine("=== Creating WebApplication Builder ===");
    var builder = WebApplication.CreateBuilder(args);
    Console.WriteLine("✅ Builder created successfully");

    // ---------------------
    // Logging
    // ---------------------
    Console.WriteLine("=== Configuring Logging ===");
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
    Console.WriteLine("✅ Logging configured");

    // ---------------------
    // Configuration
    // ---------------------
    Console.WriteLine("=== Loading Configuration ===");
    builder.Configuration
        .AddUserSecrets<Program>(optional: true)
        .AddEnvironmentVariables();
    Console.WriteLine("✅ Configuration loaded");

    // ---------------------
    // Services
    // ---------------------
    Console.WriteLine("=== Registering Services ===");
    
    Console.WriteLine("=== Adding Infrastructure Services ===");
    builder.Services.AddInfrastructure(builder.Configuration);
    Console.WriteLine("✅ Infrastructure services registered");
    
    Console.WriteLine("=== Adding Application Services ===");
    builder.Services.AddApplication();
    Console.WriteLine("✅ Application services registered");

    Console.WriteLine("=== Configuring Controllers ===");
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        });
    Console.WriteLine("✅ Controllers configured");

    Console.WriteLine("=== Configuring API Versioning ===");
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });
    Console.WriteLine("✅ API versioning configured");

    Console.WriteLine("=== Configuring Swagger ===");
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
        {
            options.IncludeXmlComments(xmlPath);
            Console.WriteLine("✅ XML documentation included");
        }
        else
        {
            Console.WriteLine("ℹ️  XML documentation file not found");
        }
    });
    Console.WriteLine("✅ Swagger configured");

    Console.WriteLine("=== Configuring Health Checks ===");
    builder.Services.AddHealthChecks();
    Console.WriteLine("✅ Health checks configured");

    Console.WriteLine("✅ ALL SERVICES REGISTERED SUCCESSFULLY");

    // ---------------------
    // Build Application
    // ---------------------
    Console.WriteLine("=== Building Application ===");
    var app = builder.Build();
    Console.WriteLine("✅ Application built successfully");

    // ---------------------
    // Database migrations (AUTO-CREATION)
    // ---------------------
    Console.WriteLine("=== DATABASE SETUP (AUTO-CREATION) ===");
    
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        Console.WriteLine("=== Testing database connection ===");
        
        // Essayer de se connecter et créer la DB si elle n'existe pas
        var retryCount = 0;
        const int maxRetries = 3;

        while (retryCount < maxRetries)
        {
            try
            {
                Console.WriteLine($"🔄 Database connection attempt {retryCount + 1}/{maxRetries}");
                
                // Cette méthode va créer la DB si elle n'existe pas
                await db.Database.MigrateAsync();
                Console.WriteLine("✅ Database migrated/created successfully");
                
                // Vérifier si des données de seed sont nécessaires
                Console.WriteLine("=== Checking for seed data ===");
                await Infrastructure.Seeds.UserSeeder.SeedUsersAsync(db);
                Console.WriteLine("✅ Seed data applied successfully");
                
                break; // Succès, sortir de la boucle
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "3D000") // Database doesn't exist
            {
                retryCount++;
                Console.WriteLine($"❌ Database doesn't exist: {ex.MessageText}");
                
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("💥 Cannot create database automatically");
                    Console.WriteLine("ℹ️  Please create the database manually: CREATE DATABASE Ecommerce;");
                    Console.WriteLine("ℹ️  Application will start without database");
                    break;
                }
                
                Console.WriteLine("⏳ Retrying...");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"❌ Database error: {ex.Message}");
                
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("💥 All database connection attempts failed");
                    Console.WriteLine("ℹ️  Application will start without database");
                    break;
                }
                
                Console.WriteLine("⏳ Retrying...");
                await Task.Delay(2000);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Database setup error: {ex.Message}");
        Console.WriteLine("ℹ️  Application will start without database initialization");
        // Ne pas throw - laisser l'application démarrer
    }

    Console.WriteLine("✅ DATABASE SETUP COMPLETED");

    // ---------------------
    // Middleware pipeline
    // ---------------------
    Console.WriteLine("=== CONFIGURING MIDDLEWARE PIPELINE ===");

    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("=== Development environment detected ===");
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.EnablePersistAuthorization();
            options.DisplayRequestDuration();
        });
        Console.WriteLine("✅ Swagger configured for development");
    }

    Console.WriteLine("=== Configuring HTTPS Redirection ===");
    app.UseHttpsRedirection();
    Console.WriteLine("✅ HTTPS redirection configured");

    if (!app.Environment.IsDevelopment())
    {
        Console.WriteLine("=== Configuring HSTS for production ===");
        app.UseHsts();
        Console.WriteLine("✅ HSTS configured");
    }

    Console.WriteLine("=== Configuring Routing ===");
    app.UseRouting();
    Console.WriteLine("✅ Routing configured");

    Console.WriteLine("=== Adding Exception Handling Middleware ===");
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    Console.WriteLine("✅ Exception middleware configured");

    Console.WriteLine("=== Mapping Health Check endpoint ===");
    app.MapHealthChecks("/health");
    Console.WriteLine("✅ Health checks mapped");

    Console.WriteLine("=== Mapping default route ===");
    app.MapGet("/", () => Results.Redirect("/swagger"));
    Console.WriteLine("✅ Default route mapped");

    Console.WriteLine("=== Configuring Authorization ===");
    app.UseAuthorization();
    Console.WriteLine("✅ Authorization configured");

    Console.WriteLine("=== Mapping Controllers ===");
    app.MapControllers();
    Console.WriteLine("✅ Controllers mapped");

    Console.WriteLine("✅ ALL MIDDLEWARE CONFIGURED SUCCESSFULLY");

    // ---------------------
    // Run app
    // ---------------------
    Console.WriteLine("=== 🚀 STARTING APPLICATION ===");
    Console.WriteLine($"=== Environment: {app.Environment.EnvironmentName} ===");
    Console.WriteLine("=== Application is now running... ===");
    
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"💥 CRITICAL ERROR: {ex}");
    Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
    throw;
}