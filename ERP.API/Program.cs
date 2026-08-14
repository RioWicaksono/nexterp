using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using StackExchange.Redis;
using Serilog;
using ERP.API.Controllers;
using ERP.API.Extensions;
using ERP.API.Middleware;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Integrations;
using ERP.Application.Common.Documents;
using ERP.Application.Common.Modules;
using ERP.Application.Common.Licensing;
using ERP.Application.Common.Behaviors;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Services;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Data.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with JSON structured logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure Swagger with API versioning
builder.Services.AddSwaggerWithVersioning();

// Configure API Versioning
builder.Services.AddApiVersioningWithExplorer();

// Configure JWT Authentication - Secret key is REQUIRED, no fallback
var jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException(
        "JWT SecretKey must be configured. Set 'Jwt:SecretKey' in configuration or JWT_SECRET environment variable.");
}

var jwtSettings = new JwtSettings
{
    SecretKey = jwtSecret,
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "ERP.System",
    Audience = builder.Configuration["Jwt:Audience"] ?? "ERP.Client",
    AccessTokenExpirationMinutes = int.Parse(builder.Configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60"),
    RefreshTokenExpirationDays = int.Parse(builder.Configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")
};

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Request.Headers.Authorization.ToString()))
            {
                context.Token = context.Request.Cookies["nexterp_token"];
            }
            return Task.CompletedTask;
        }
    };
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "postgresql",
        tags: new[] { "db", "postgresql" })
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379",
        name: "redis",
        tags: new[] { "cache", "redis" })
    .AddCheck("custom", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin", "SuperAdmin"));
});

// Add DbContext
builder.Services.AddDbContext<ERPDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsql =>
    {
        npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
        npgsql.CommandTimeout(30);
    });
});

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379");
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IApplicationDbContext>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Add Application DbContext
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ERPDbContext>());

// Add Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IModuleAccessService, ModuleAccessService>();
builder.Services.AddScoped<ERP.Application.Analytics.Services.INotificationService, ERP.Infrastructure.Services.NotificationService>();
builder.Services.AddScoped<IWorkflowService, ERP.Infrastructure.Services.WorkflowService>();
builder.Services.AddScoped<IReportService, ReportService>();

// External Integration Services
builder.Services.AddScoped<ITaxReportingService, ERP.Infrastructure.Services.Integrations.TaxReportingService>();
builder.Services.AddScoped<IBankTransferService, ERP.Infrastructure.Services.Integrations.BankTransferService>();
builder.Services.AddScoped<INotificationGateway, ERP.Infrastructure.Services.Integrations.NotificationGateway>();
builder.Services.AddScoped<IDocumentTemplateService, ERP.Infrastructure.Services.Documents.DocumentTemplateService>();
builder.Services.AddScoped<IModuleManager, ModuleManager>();

// Licensing Services
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<ILicenseCheckService, LicenseCheckService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddSingleton<ILicenseIntegrityService, LicenseIntegrityService>();
builder.Services.AddScoped<ILicenseAuditService>(sp =>
{
    var auditLogger = new SerilogAuditLogger(sp.GetRequiredService<ILogger<LicenseAuditService>>());
    return new LicenseAuditService(auditLogger);
});

// License Validation Pipeline
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LicenseValidationBehavior<,>));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Total-Count", "X-Page-Count");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithVersioning();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseSerilogRequestLogging();

// Add Prometheus metrics
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("service", context => "nexterp-api");
});

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints BEFORE UseRouting
app.MapControllers();
app.MapHealthChecks("/health/ready");
app.MapMetrics();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

Log.Information("Starting NEXTERP API on port {Port}", port);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
