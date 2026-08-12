using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ERP.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Basic liveness check - returns 200 if the application is running.
    /// Use for Kubernetes liveness probe.
    /// </summary>
    [HttpGet("/live")]
    public IActionResult Live()
    {
        var response = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Description = "Application is running and responding"
        };

        _logger.LogDebug("Liveness check passed at {Timestamp}", response.Timestamp);

        return Ok(response);
    }

    /// <summary>
    /// Readiness check - verifies all dependencies (DB, Redis) are accessible.
    /// Use for Kubernetes readiness probe and load balancer health checks.
    /// </summary>
    [HttpGet("/ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(e => new HealthCheckResult
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration.TotalMilliseconds,
                Description = e.Value.Description,
                Exception = e.Value.Exception?.Message,
                Data = new Dictionary<string, object>(e.Value.Data).Count > 0 ? new Dictionary<string, object>(e.Value.Data) : null
            }).ToList()
        };

        // Log the readiness check result
        if (report.Status == HealthStatus.Healthy)
        {
            _logger.LogInformation(
                "Readiness check passed. All {Count} components healthy. Duration: {Duration}ms",
                response.Checks.Count,
                response.TotalDuration);
        }
        else
        {
            var unhealthyComponents = response.Checks
                .Where(c => c.Status != "Healthy")
                .Select(c => $"{c.Name}({c.Status})")
                .ToList();

            _logger.LogWarning(
                "Readiness check failed. Unhealthy components: {Components}. Duration: {Duration}ms",
                string.Join(", ", unhealthyComponents),
                response.TotalDuration);
        }

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(503, response);
    }

    /// <summary>
    /// Detailed health check with full component breakdown.
    /// Use for admin dashboards and monitoring systems.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new DetailedHealthResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Components = report.Entries.Select(e => new ComponentHealth
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration.TotalMilliseconds,
                Description = e.Value.Description,
                Tags = e.Value.Tags.ToList(),
                Data = new Dictionary<string, object>(e.Value.Data)
            }).ToList()
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(503, response);
    }
}

#region Response Models

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Description { get; set; }
}

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double TotalDuration { get; set; }
    public List<HealthCheckResult> Checks { get; set; } = new();
}

public class HealthCheckResult
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Duration { get; set; }
    public string? Description { get; set; }
    public string? Exception { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class DetailedHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double TotalDuration { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public List<ComponentHealth> Components { get; set; } = new();
}

public class ComponentHealth
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Duration { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
}

#endregion

#region Health Check Configuration

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddErpHealthChecks(
        this IHealthChecksBuilder builder,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var redisConnectionString = configuration.GetConnectionString("Redis");

        // Database health check (supports both SQLite and PostgreSQL)
        if (!string.IsNullOrEmpty(connectionString))
        {
            if (connectionString.Contains("sqlite", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddSqlite(connectionString, name: "sqlite", tags: new[] { "db", "sqlite", "ready" });
            }
            else
            {
                builder.AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db", "postgresql", "ready" });
            }
        }

        // Redis health check (optional - graceful degradation if not configured)
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            builder.AddRedis(
                redisConnectionString,
                name: "redis",
                tags: new[] { "cache", "redis", "ready" },
                failureStatus: HealthStatus.Degraded);
        }

        return builder;
    }
}

#endregion
