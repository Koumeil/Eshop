FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/API/API.csproj", "src/API/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/API/API.csproj"

COPY . .
RUN dotnet publish "src/API/API.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# ✅ Installation of dependencies for SSL
RUN apt-get update && \
    apt-get install -y ca-certificates openssl && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

# ✅ Copy the files of all SEEDS
COPY --from=build /src/src/Infrastructure/Seeds/ ./Infrastructure/Seeds/

# ✅ Copy only script init-ssl.sh
COPY init-ssl.sh .
RUN chmod +x init-ssl.sh

EXPOSE 7002

# ✅ Command
CMD ["sh", "-c", "./init-ssl.sh && dotnet API.dll"]
