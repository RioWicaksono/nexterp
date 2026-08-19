# NEXTERP API - Development Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better caching
COPY ["*.sln", "./"]
COPY ["ERP.API/*.csproj", "ERP.API/"]
COPY ["ERP.Application/*.csproj", "ERP.Application/"]
COPY ["ERP.Domain/*.csproj", "ERP.Domain/"]
COPY ["ERP.Infrastructure/*.csproj", "ERP.Infrastructure/"]

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build
WORKDIR "/src/ERP.API"
RUN dotnet build -c Release -o /app/build

# Run tests
# RUN dotnet test --no-build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health/live || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ERP.API.dll"]
