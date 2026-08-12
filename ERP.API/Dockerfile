# NEXTERP Backend - Dockerfile for Railway
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (relative to ERP.API folder)
COPY ["ERP.API.csproj", "."]
COPY ["../../ERP.Application/ERP.Application.csproj", "../ERP.Application/"]
COPY ["../../ERP.Domain/ERP.Domain.csproj", "../ERP.Domain/"]
COPY ["../../ERP.Infrastructure/ERP.Infrastructure.csproj", "../ERP.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "./ERP.API.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src"
RUN dotnet build "ERP.API/ERP.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ERP.API/ERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create uploads directory
RUN mkdir -p /app/uploads

# Copy published app
COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose ports
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "ERP.API.dll"]
