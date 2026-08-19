using System.IO;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ERP.API.Extensions;

/// <summary>
/// Swagger configuration with multi-version support and enhanced documentation.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // API Info
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "NEXTERP ERP API",
                Description = @"
## Enterprise Resource Planning API

### Authentication
All endpoints (except `/api/v1/auth/login` and `/api/v1/auth/register`) require JWT Bearer authentication.

**Headers:**
```
Authorization: Bearer <your_access_token>
```

### Rate Limiting
- Anonymous: 100 requests/minute
- Authenticated: 1000 requests/minute
- Login endpoint: 5 attempts/5 minutes (brute force protection)

### Response Format
All responses follow a consistent format:
```json
{
  ""isSuccess"": true/false,
  ""value"": { ... },
  ""error"": ""error message""
}
```

### Health Endpoints
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe (checks DB, Redis)

### Error Codes
| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Invalid or expired token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error |
",
                Contact = new OpenApiContact
                {
                    Name = "NEXTERP Support",
                    Email = "support@nexterp.com"
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary"
                }
            });

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Apply version metadata to all operations
            options.OperationFilter<SwaggerVersionOperationFilter>();

            // Include XML comments if available
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Group endpoints by tag
            options.TagActionsBy(api =>
            {
                var controllerName = api.GroupName ?? api.ActionDescriptor.RouteValues["controller"];
                return new[] { controllerName ?? "Other" };
            });

            // Order tags
            options.OrderActionsBy(api => api.RelativePath);
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerWithVersioning(this IApplicationBuilder app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "📋 NEXTERP API v1.0 (Stable)");

            options.DocumentTitle = "NEXTERP ERP API Documentation";
            options.DefaultModelsExpandDepth(2);
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableTryItOutByDefault();

            // Custom CSS for better styling
            options.InjectStylesheet("/swagger-ui/custom.css");
        });

        return app;
    }
}

/// <summary>
/// Injects API version metadata into every Swagger operation.
/// </summary>
public class SwaggerVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add deprecation notice if the action is deprecated
        var actionDescriptor = context.ApiDescription.ActionDescriptor as ActionDescriptor;
        if (actionDescriptor?.EndpointMetadata?.OfType<ObsoleteAttribute>().Any() == true)
        {
            operation.Deprecated = true;
        }

        // Add operation summary from XML docs
        if (string.IsNullOrEmpty(operation.Summary))
        {
            var controllerAction = context.ApiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (controllerAction != null)
            {
                var methodInfo = controllerAction.MethodInfo;
                var xmlDocs = methodInfo?.GetCustomAttributes<System.ComponentModel.DescriptionAttribute>()
                    .FirstOrDefault();
                if (xmlDocs != null)
                {
                    operation.Summary = xmlDocs.Description;
                }
            }
        }

        // Add security requirements to all operations
        var anonEndpoints = new[]
        {
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/auth/refresh"
        };

        var path = context.ApiDescription.RelativePath ?? "";
        if (!anonEndpoints.Any(e => path.Contains(e.Replace("/api/v1/", ""))))
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
}
