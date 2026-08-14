using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ERP.API.Extensions;

/// <summary>
/// Swagger configuration with multi-version support.
/// Displays version lifecycle status (Stable, Deprecated, Experimental).
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Add JWT bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerWithVersioning(this IApplicationBuilder app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            // Show both v1 and v2
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "📋 NEXTERP API v1.0 (Stable)");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "✨ NEXTERP API v2.0 (Latest)");

            options.DocumentTitle = "NEXTERP ERP API Documentation";
            options.DefaultModelsExpandDepth(2);
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();

            // Add version selector
            options.ConfigObject.AdditionalItems["syntaxHighlight"] = new
            {
                activated = true
            };
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

        // Check if version is specified via route
        var relativePath = context.ApiDescription.RelativePath ?? "";
        if (relativePath.Contains("v{version}"))
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "version",
                In = ParameterLocation.Path,
                Required = true,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "API version (1 or 2)"
            });
        }
    }
}
