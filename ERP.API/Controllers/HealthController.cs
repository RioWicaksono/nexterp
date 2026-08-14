using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers;

/// <summary>
/// Health check endpoints for Kubernetes probes.
/// </summary>
[ApiController]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ERPDbContext _dbContext;

    public HealthController(
        HealthCheckService healthCheckService,
        ERPDbContext dbContext)
    {
        _healthCheckService = healthCheckService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Liveness probe - is the application running?
    /// </summary>
    [HttpGet("health/live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe - is the application ready to serve traffic?
    /// </summary>
    [HttpGet("health/ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(ct);

        var response = new
        {
            status = report.Status.ToString().ToLower(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString().ToLower(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        };

        if (report.Status == HealthStatus.Healthy)
            return Ok(response);

        return StatusCode(503, response);
    }

    /// <summary>
    /// Detailed health check with database connectivity test.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        var results = new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow,
            ["version"] = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0"
        };

        // Check database
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(ct);
            results["database"] = new { status = canConnect ? "healthy" : "unhealthy" };
        }
        catch (Exception ex)
        {
            results["database"] = new { status = "unhealthy", error = ex.Message };
        }

        // Run all health checks
        var report = await _healthCheckService.CheckHealthAsync(ct);
        results["checks"] = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString().ToLower(),
            description = e.Value.Description
        });

        var isHealthy = report.Status == HealthStatus.Healthy;

        if (isHealthy)
            return Ok(results);

        return StatusCode(503, results);
    }
}
