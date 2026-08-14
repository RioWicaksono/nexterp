using System.Diagnostics;
using Prometheus;

namespace ERP.API.Middleware;

/// <summary>
/// Middleware for capturing HTTP request metrics for Prometheus monitoring.
/// Records request duration, count, and status codes by endpoint.
/// </summary>
public class PrometheusMetricsMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly Counter HttpRequestsTotal = Metrics.CreateCounter(
        "erp_http_requests_total",
        "Total number of HTTP requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" }
        });

    private static readonly Histogram HttpRequestDuration = Metrics.CreateHistogram(
        "erp_http_request_duration_seconds",
        "HTTP request duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" },
            Buckets = new[] { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
        });

    private static readonly Gauge ActiveRequests = Metrics.CreateGauge(
        "erp_http_requests_active",
        "Number of currently active HTTP requests",
        new GaugeConfiguration
        {
            LabelNames = new[] { "method", "endpoint" }
        });

    public PrometheusMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        // Skip metrics for health and swagger endpoints
        if (ShouldSkip(path))
        {
            await _next(context);
            return;
        }

        // Normalize endpoint path for better metrics grouping
        var normalizedPath = NormalizePath(path);

        ActiveRequests.WithLabels(method, normalizedPath).Inc();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode.ToString();

            ActiveRequests.WithLabels(method, normalizedPath).Dec();
            HttpRequestsTotal.WithLabels(method, normalizedPath, statusCode).Inc();
            HttpRequestDuration.WithLabels(method, normalizedPath, statusCode).Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }

    private static bool ShouldSkip(string path)
    {
        return path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/metrics", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        // Replace UUIDs and numeric IDs with placeholders for better metric grouping
        var normalized = System.Text.RegularExpressions.Regex.Replace(
            path,
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            "{id}");

        normalized = System.Text.RegularExpressions.Regex.Replace(
            normalized,
            @"/\d+(/|$)",
            "/{id}$1");

        return normalized;
    }
}

/// <summary>
/// Custom business metrics for ERP system monitoring.
/// </summary>
public static class ErpBusinessMetrics
{
    private static readonly Counter UsersCreatedTotal = Metrics.CreateCounter(
        "erp_users_created_total",
        "Total number of users created",
        new CounterConfiguration { LabelNames = new[] { "source" } });

    private static readonly Counter LoginAttemptsTotal = Metrics.CreateCounter(
        "erp_login_attempts_total",
        "Total login attempts",
        new CounterConfiguration { LabelNames = new[] { "result" } });

    private static readonly Counter ApiCallsTotal = Metrics.CreateCounter(
        "erp_api_calls_total",
        "Total API calls by domain",
        new CounterConfiguration { LabelNames = new[] { "domain", "endpoint" } });

    private static readonly Histogram DatabaseQueryDuration = Metrics.CreateHistogram(
        "erp_database_query_duration_seconds",
        "Database query duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "operation" },
            Buckets = new[] { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1 }
        });

    public static void RecordUserCreated(string source = "manual") =>
        UsersCreatedTotal.WithLabels(source).Inc();

    public static void RecordLoginAttempt(bool success) =>
        LoginAttemptsTotal.WithLabels(success ? "success" : "failure").Inc();

    public static void RecordApiCall(string domain, string endpoint) =>
        ApiCallsTotal.WithLabels(domain, endpoint).Inc();

    public static IDisposable MeasureDatabaseQuery(string operation) =>
        new QueryTimer(DatabaseQueryDuration.WithLabels(operation));

    private class QueryTimer : IDisposable
    {
        private readonly Histogram.Child _histogram;
        private readonly Stopwatch _stopwatch;

        public QueryTimer(Histogram.Child histogram)
        {
            _histogram = histogram;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _histogram.Observe(_stopwatch.Elapsed.TotalSeconds);
        }
    }
}

public static class PrometheusMiddlewareExtensions
{
    public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PrometheusMetricsMiddleware>();
    }
}
