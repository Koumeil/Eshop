# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution and project files
COPY ["CleanArchitecture.sln", "."]
COPY ["Directory.Build.props", "."]  # ⬅️ COPIEZ LE FICHIER ICI AVANT RESTORE
COPY ["src/API/API.csproj", "src/API/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Tests/Tests.csproj", "src/Tests/"]

# Restore dependencies (avec Directory.Build.props disponible)
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build --configuration Release --no-restore /p:GenerateAssemblyInfo=false
RUN dotnet test --configuration Release --no-build --verbosity normal

# Publish
RUN dotnet publish "src/API/API.csproj" \
    --configuration Release \
    --no-build \
    --output /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd --system appuser && \
    useradd --system --gid appuser --home-dir /app --shell /bin/bash appuser && \
    chown appuser:appuser /app
USER appuser

COPY --from=build --chown=appuser:appuser /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "API.dll"]