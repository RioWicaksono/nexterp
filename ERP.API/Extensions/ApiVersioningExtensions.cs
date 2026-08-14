using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Asp.Versioning;

namespace ERP.API.Extensions;

/// <summary>
/// API versioning configuration following RFC 8594 and Semantic Versioning.
/// Supports URL-based versioning with deprecation lifecycle management.
/// </summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApiVersioningWithExplorer(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default API version when not specified
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // Assume unversioned API as v1.0
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Report API versions in response headers
            options.ReportApiVersions = true;

            // Read version from URL path segment
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),   // /api/v1/...
                new HeaderApiVersionReader("X-Api-Version"));  // X-Api-Version header fallback

        }).AddApiExplorer(options =>
        {
            // Group all versions under a named group in Swagger
            options.GroupNameFormat = "'v'VVV";

            // Substitute version in Swagger UI URL
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
