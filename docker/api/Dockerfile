# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better caching
COPY ERP.slnx ./
COPY ERP.Domain/ERP.Domain.csproj ERP.Domain/
COPY ERP.Application/ERP.Application.csproj ERP.Application/
COPY ERP.Infrastructure/ERP.Infrastructure.csproj ERP.Infrastructure/
COPY ERP.API/ERP.API.csproj ERP.API/

# Restore dependencies
RUN dotnet restore ERP.API/ERP.API.csproj

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/ERP.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 5000
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ERP.API.dll"]
