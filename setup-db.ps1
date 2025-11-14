# =====================
# setup-db.ps1
# Initialisation de la base de données E-Shop
# =====================

$ErrorActionPreference = "Stop"

Write-Host "1: Restauration des packages NuGet..."
dotnet restore

Write-Host "2: Creation de la migration Initial (si inexistante)..."
dotnet ef migrations add Initial -p src/Infrastructure -s src/API -o Migrations -v

Write-Host "3: Application des migrations sur la base de donnees..."
dotnet ef database update -p src/Infrastructure -s src/API -v

Write-Host "Base de donnees initialisee avec succes !"
Write-Host "Lancez l'API avec : dotnet run --project src/API"