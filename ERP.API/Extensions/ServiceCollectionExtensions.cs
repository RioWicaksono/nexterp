using FluentValidation;
using ERP.Application.Common.Interfaces;

namespace ERP.API.Extensions;

/// <summary>
/// Extension methods for configuring application services
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<IApplicationDbContext>());

        // Add FluentValidation
        services.AddValidatorsFromAssemblyContaining<IApplicationDbContext>();

        return services;
    }
}
