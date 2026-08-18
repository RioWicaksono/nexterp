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

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IModuleAccessService, ModuleAccessService>();
builder.Services.AddScoped<ERP.Application.Analytics.Services.INotificationService, ERP.Infrastructure.Services.NotificationService>();
builder.Services.AddScoped<IWorkflowService, ERP.Infrastructure.Services.WorkflowService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Domain Services
builder.Services.AddScoped<ERP.Domain.Hrm.Services.PayrollCalculationService>();

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

// Fix missing columns from legacy schema (skip migrations since tables already exist)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

    try
    {
        // Skip migrations - database already has tables
        // Just add any missing columns
        logger.LogInformation("Ensuring database schema has all required columns...");
        await dbContext.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""RefreshTokenHash"" text;
            ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""RefreshTokenExpiry"" timestamp with time zone;
        ");
        logger.LogInformation("Database schema fixes applied successfully");

        // Ensure demo organization exists
        var demoOrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        logger.LogInformation("Ensuring demo data exists...");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Organizations"" (""Id"", ""Name"", ""Code"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES ('{demoOrgId}', 'Nexterp Demo Corp', 'NEXTERP', TRUE, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ");

        // Ensure admin role exists
        var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000101");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Roles"" (""Id"", ""OrganizationId"", ""Name"", ""Description"", ""IsActive"", ""IsSystemRole"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES ('{adminRoleId}', '{demoOrgId}', 'Admin', 'System Administrator', TRUE, TRUE, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ");

        // Ensure demo user exists with correct password
        var demoUserId = Guid.Parse("00000000-0000-0000-0000-000000000100");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Users"" (""Id"", ""OrganizationId"", ""Username"", ""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Phone"", ""IsActive"", ""IsSuperAdmin"", ""FailedLoginAttempts"", ""LockedUntil"", ""LastLoginAt"", ""LastLoginIp"", ""RefreshTokenHash"", ""RefreshTokenExpiry"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES ('{demoUserId}', '{demoOrgId}', 'admin', 'admin@nexterp.com', '{passwordHash}', 'System', 'Administrator', NULL, TRUE, TRUE, 0, NULL, NULL, NULL, NULL, NULL, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO UPDATE SET ""PasswordHash"" = EXCLUDED.""PasswordHash"";
        ");

        // Assign Admin role to demo user
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""RoleId"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            SELECT '{Guid.NewGuid()}', '{demoUserId}', '{adminRoleId}', FALSE, NOW(), NOW()
            WHERE NOT EXISTS (SELECT 1 FROM ""UserRoles"" WHERE ""UserId"" = '{demoUserId}' AND ""RoleId"" = '{adminRoleId}');
        ");

        // Seed Departments
        var engineeringDeptId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        var hrDeptId = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var financeDeptId = Guid.Parse("00000000-0000-0000-0000-000000000012");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES
            ('{engineeringDeptId}', '{demoOrgId}', 'Engineering', 'ENG', 'Engineering Department', TRUE, FALSE, NOW(), NOW()),
            ('{hrDeptId}', '{demoOrgId}', 'Human Resources', 'HR', 'Human Resources Department', TRUE, FALSE, NOW(), NOW()),
            ('{financeDeptId}', '{demoOrgId}', 'Finance', 'FIN', 'Finance Department', TRUE, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ");

        // Seed Positions
        var engineerPosId = Guid.Parse("00000000-0000-0000-0000-000000000020");
        var hrPosId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var managerPosId = Guid.Parse("00000000-0000-0000-0000-000000000022");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES
            ('{engineerPosId}', '{demoOrgId}', 'Software Engineer', 'SE', 'Software Engineer Position', TRUE, FALSE, NOW(), NOW()),
            ('{hrPosId}', '{demoOrgId}', 'HR Manager', 'HRM', 'HR Manager Position', TRUE, FALSE, NOW(), NOW()),
            ('{managerPosId}', '{demoOrgId}', 'Department Manager', 'MGR', 'Manager Position', TRUE, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ");

        // Seed Warehouses
        var mainWhId = Guid.Parse("00000000-0000-0000-0000-000000000030");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Warehouses"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""Address"", ""City"", ""Country"", ""Phone"", ""Email"", ""IsActive"", ""IsDefault"", ""AllowsNegativeStock"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES
            ('{mainWhId}', '{demoOrgId}', 'Main Warehouse', 'WH001', 'Main storage warehouse', '123 Industrial Ave', 'Jakarta', 'Indonesia', '+6221123456', 'warehouse@nexterp.com', TRUE, TRUE, FALSE, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ");

        // Seed License Tiers
        var starterTierId = Guid.Parse("00000000-0000-0000-0000-000000000200");
        var proTierId = Guid.Parse("00000000-0000-0000-0000-000000000201");
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""LicenseTiers"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""SortOrder"", ""MonthlyPrice"", ""DefaultMaxUsers"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            VALUES
            ('{starterTierId}', 'STARTER', 'Starter', 'For small teams', 1, 99.00, 5, TRUE, FALSE, NOW(), NOW()),
            ('{proTierId}', 'PROFESSIONAL', 'Professional', 'For growing businesses', 2, 299.00, 25, TRUE, FALSE, NOW(), NOW())
            ON CONFLICT (""Id"") DO NOTHING;
        ");

        // Seed Organization License
        await dbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""OrganizationLicenses"" (""Id"", ""OrganizationId"", ""LicenseTierId"", ""StartDate"", ""EndDate"", ""MaxUsers"", ""IsAutoRenew"", ""BillingEmail"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
            SELECT '{Guid.NewGuid()}', '{demoOrgId}', '{proTierId}', NOW(), NOW() + INTERVAL '1 year', 100, TRUE, 'billing@nexterp.com', FALSE, NOW(), NOW()
            WHERE NOT EXISTS (SELECT 1 FROM ""OrganizationLicenses"" WHERE ""OrganizationId"" = '{demoOrgId}');
        ");

        logger.LogInformation("Demo data ensured successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Schema fix or seeding failed. Continuing anyway...");
    }
}

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
