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
using ERP.Application.Common.Configuration;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Behaviors;
using ERP.Application.Common.Integrations;
using ERP.Application.Common.Documents;
using ERP.Application.Common.Modules;
using ERP.Application.Common.Licensing;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Services;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Data.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with JSON structured logging for production
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "NEXTERP-API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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

// Configure JWT Authentication - Production security settings
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
    // Access token: 15 minutes (production standard - reduced from 60 for security)
    AccessTokenExpirationMinutes = int.Parse(builder.Configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15"),
    // Refresh token: 7 days with rotation enabled
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

// Brute force protection for login
builder.Services.AddScoped<ILoginRateLimitService, LoginRateLimitService>();

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

        // For production, allow specific Vercel and Railway domains
        var additionalOrigins = new[]
        {
            "https://nextjs-frontend-ivory.vercel.app",
            "https://nextjs-frontend-ok8i1ckcj-rio-wicaksonos-projects.vercel.app",
            "https://rio-wicaksonos-projects.vercel.app",
            "https://api-production-ab1b.up.railway.app",
            "http://localhost:3000",
            "http://localhost:3001",
            "http://localhost:5000"
        };

        var allOrigins = allowedOrigins.Concat(additionalOrigins).ToArray();

        // SECURITY: Remove fallback to allow-all origins - always require explicit origins
        if (allOrigins.Length > 0)
        {
            policy.WithOrigins(allOrigins);
        }
        // else: No additional origins configured - CORS will only use AllowedOrigins from config

        policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page-Count", "X-Correlation-ID");
    });
});

var app = builder.Build();

// Fix missing columns from legacy schema (skip migrations since tables already exist)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    // Use configurable demo password from env var with fallback for local dev
    // BCrypt cost factor 12 for production security
    var demoPasswordHash = BCrypt.Net.BCrypt.HashPassword(
        Environment.GetEnvironmentVariable("DEMO_PASSWORD") ?? "DevPassword2024!", 12);

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
            VALUES ('{demoUserId}', '{demoOrgId}', 'admin', 'admin@nexterp.com', '{demoPasswordHash}', 'System', 'Administrator', NULL, TRUE, TRUE, 0, NULL, NULL, NULL, NULL, NULL, FALSE, NOW(), NOW())
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

        // Seed Role Permissions for Admin role (all permissions)
        var adminPermissions = new[]
        {
            "admin.users.read", "admin.users.create", "admin.users.update", "admin.users.delete",
            "admin.roles.read", "admin.roles.create", "admin.roles.update", "admin.roles.delete",
            "admin.modules.read", "admin.modules.manage", "admin.settings.read", "admin.settings.update",
            "hrm.employees.read", "hrm.employees.create", "hrm.employees.update", "hrm.employees.delete",
            "hrm.departments.read", "hrm.departments.create", "hrm.departments.update", "hrm.departments.delete",
            "hrm.attendances.read", "hrm.attendances.create", "hrm.attendances.update",
            "hrm.leave.read", "hrm.leave.approve", "hrm.payroll.read", "hrm.payroll.process",
            "hrm.reports.read",
            "inventory.items.read", "inventory.items.create", "inventory.items.update", "inventory.items.delete",
            "inventory.stock.read", "inventory.stock.adjust", "inventory.warehouses.read", "inventory.warehouses.manage",
            "inventory.reports.read",
            "sales.orders.read", "sales.orders.create", "sales.orders.update", "sales.orders.delete",
            "sales.invoices.read", "sales.invoices.create", "sales.invoices.update",
            "sales.customers.read", "sales.customers.manage", "sales.reports.read",
            "purchasing.orders.read", "purchasing.orders.create", "purchasing.orders.update", "purchasing.orders.delete",
            "purchasing.suppliers.read", "purchasing.suppliers.manage", "purchasing.reports.read",
            "accounting.accounts.read", "accounting.accounts.create", "accounting.accounts.update",
            "accounting.journals.read", "accounting.journals.create", "accounting.journals.post",
            "accounting.reports.read", "accounting.reports.financial",
            "projects.read", "projects.create", "projects.update", "projects.delete",
            "projects.tasks.read", "projects.tasks.manage", "projects.reports.read",
            "assets.read", "assets.create", "assets.update", "assets.delete",
            "assets.maintenance.read", "assets.maintenance.schedule", "assets.depreciation.read",
            "quality.inspections.read", "quality.inspections.create", "quality.inspections.update",
            "quality.nc.read", "quality.nc.create", "quality.nc.resolve",
            "analytics.dashboard.read", "analytics.reports.read", "analytics.exports.read"
        };

        foreach (var permission in adminPermissions)
        {
            await dbContext.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""RolePermissions"" (""Id"", ""RoleId"", ""Permission"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                SELECT '{Guid.NewGuid()}', '{adminRoleId}', '{permission}', FALSE, NOW(), NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""RolePermissions"" WHERE ""RoleId"" = '{adminRoleId}' AND ""Permission"" = '{permission}');
            ");
        }

        logger.LogInformation("Demo data ensured successfully");

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

// Global exception handler - must be early in pipeline
app.UseGlobalExceptionHandler();

app.UseCors();

// Structured request logging with correlation ID
app.UseSerilogRequestLogging();

// Rate limiting middleware
app.UseRateLimiting();

// Add Correlation ID to all requests
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    await next();
});

// Prometheus metrics
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("service", context => "nexterp-api");
});

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();

// Enhanced health check endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Just checks if app is running
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true // Checks all dependencies (DB, Redis)
});
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
