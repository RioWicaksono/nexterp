using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Data;

/// <summary>
/// Seeds the database with initial demo data
/// </summary>
public static class DatabaseSeeder
{
    public static readonly Guid DemoOrganizationId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    // Departments
    public static readonly Guid EngineeringDeptId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    public static readonly Guid HrDeptId = Guid.Parse("00000000-0000-0000-0000-000000000011");
    public static readonly Guid FinanceDeptId = Guid.Parse("00000000-0000-0000-0000-000000000012");
    public static readonly Guid SalesDeptId = Guid.Parse("00000000-0000-0000-0000-000000000013");
    public static readonly Guid WarehouseDeptId = Guid.Parse("00000000-0000-0000-0000-000000000014");

    // Positions
    public static readonly Guid EngineeringPositionId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    public static readonly Guid HrPositionId = Guid.Parse("00000000-0000-0000-0000-000000000021");
    public static readonly Guid ManagerPositionId = Guid.Parse("00000000-0000-0000-0000-000000000022");

    // Warehouses
    public static readonly Guid MainWarehouseId = Guid.Parse("00000000-0000-0000-0000-000000000030");
    public static readonly Guid SecondaryWarehouseId = Guid.Parse("00000000-0000-0000-0000-000000000031");

    // Demo User ID
    public static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000100");
    public static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000101");

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()!.CreateLogger("DatabaseSeeder");

        await context.Database.EnsureCreatedAsync();

        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        // ============ ORGANIZATION (MUST BE FIRST - FK dependency) ============
        if (!await context.Organizations.AnyAsync())
        {
            logger.LogInformation("Seeding organization...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Organizations"" (""Id"", ""Name"", ""Code"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                VALUES ('{DemoOrganizationId}', 'Nexterp Demo Corp', 'NEXTERP', TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ ROLES ============
        if (!await context.Roles.AnyAsync())
        {
            logger.LogInformation("Seeding roles...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Roles"" (""Id"", ""OrganizationId"", ""Name"", ""Description"", ""IsActive"", ""IsSystemRole"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{AdminRoleId}', '{DemoOrganizationId}', 'Admin', 'System Administrator', TRUE, TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ USERS ============
        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("Seeding demo user...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Users"" (""Id"", ""OrganizationId"", ""Username"", ""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Phone"", ""IsActive"", ""IsSuperAdmin"", ""FailedLoginAttempts"", ""LockedUntil"", ""LastLoginAt"", ""LastLoginIp"", ""RefreshToken"", ""RefreshTokenExpiry"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{DemoUserId}', '{DemoOrganizationId}', 'admin', 'admin@nexterp.com', '{passwordHash}', 'System', 'Administrator', NULL, TRUE, TRUE, 0, NULL, NULL, NULL, NULL, NULL, FALSE, '{now}', '{now}')");

            // Assign Admin role
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""RoleId"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoUserId}', '{AdminRoleId}', FALSE, '{now}', '{now}')");
        }

        // ============ DEPARTMENTS ============
        if (!await context.Departments.AnyAsync())
        {
            logger.LogInformation("Seeding departments...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{EngineeringDeptId}', '{DemoOrganizationId}', 'Engineering', 'ENG', 'Software Engineering Department', TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{HrDeptId}', '{DemoOrganizationId}', 'Human Resources', 'HR', 'HR Management Department', TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{FinanceDeptId}', '{DemoOrganizationId}', 'Finance', 'FIN', 'Finance & Accounting Department', TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{SalesDeptId}', '{DemoOrganizationId}', 'Sales', 'SLS', 'Sales & Marketing Department', TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ POSITIONS ============
        if (!await context.Positions.AnyAsync())
        {
            logger.LogInformation("Seeding positions...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""DepartmentId"", ""Title"", ""Description"", ""Grade"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{EngineeringPositionId}', '{DemoOrganizationId}', '{EngineeringDeptId}', 'Software Engineer', 'Entry-level software developer', 1, TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""DepartmentId"", ""Title"", ""Description"", ""Grade"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{HrPositionId}', '{DemoOrganizationId}', '{HrDeptId}', 'HR Staff', 'Human Resources Officer', 1, TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""DepartmentId"", ""Title"", ""Description"", ""Grade"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{ManagerPositionId}', '{DemoOrganizationId}', '{EngineeringDeptId}', 'Engineering Manager', 'Engineering Team Lead', 5, TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ WAREHOUSES ============
        if (!await context.Warehouses.AnyAsync())
        {
            logger.LogInformation("Seeding warehouses...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Warehouses"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""Address"", ""City"", ""Country"", ""Phone"", ""Email"", ""IsActive"", ""IsDefault"", ""AllowsNegativeStock"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{MainWarehouseId}', '{DemoOrganizationId}', 'Main Warehouse', 'WH001', 'Primary storage facility', 'Jl. Sudirman No. 1', 'Jakarta', 'Indonesia', '+6221-555-0001', 'warehouse@nexterp.com', TRUE, TRUE, FALSE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Warehouses"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""Address"", ""City"", ""Country"", ""Phone"", ""Email"", ""IsActive"", ""IsDefault"", ""AllowsNegativeStock"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{SecondaryWarehouseId}', '{DemoOrganizationId}', 'Secondary Warehouse', 'WH002', 'Backup storage facility', 'Jl. Gatot Subroto No. 50', 'Surabaya', 'Indonesia', '+6231-555-0002', 'warehouse2@nexterp.com', TRUE, FALSE, TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ CUSTOMERS ============
        if (!await context.Customers.AnyAsync())
        {
            logger.LogInformation("Seeding customers...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Customers"" (""Id"", ""OrganizationId"", ""CustomerCode"", ""CustomerName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoOrganizationId}', 'CUST001', 'PT Maju Bersama', 2, 'contact@majubersama.co.id', '+6221-888-0001', 0, TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Customers"" (""Id"", ""OrganizationId"", ""CustomerCode"", ""CustomerName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoOrganizationId}', 'CUST002', 'CV Sejahtera Utama', 2, 'info@sejahtera.co.id', '+6221-888-0002', 0, TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Customers"" (""Id"", ""OrganizationId"", ""CustomerCode"", ""CustomerName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoOrganizationId}', 'CUST003', 'Toko Elektronik Jaya', 1, 'jaya@electronics.com', '+6281-234-5678', 0, TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ SUPPLIERS ============
        if (!await context.Suppliers.AnyAsync())
        {
            logger.LogInformation("Seeding suppliers...");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Suppliers"" (""Id"", ""OrganizationId"", ""SupplierCode"", ""SupplierName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoOrganizationId}', 'SUP001', 'PT Sumber Prima', 2, 'sales@sumberprima.co.id', '+6221-555-1001', 0, TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Suppliers"" (""Id"", ""OrganizationId"", ""SupplierCode"", ""SupplierName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoOrganizationId}', 'SUP002', 'CV Elektronik Grosir', 2, 'order@elegrosir.com', '+6221-555-1002', 0, TRUE, FALSE, '{now}', '{now}')");
            await context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Suppliers"" (""Id"", ""OrganizationId"", ""SupplierCode"", ""SupplierName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ('{Guid.NewGuid()}', '{DemoOrganizationId}', 'SUP003', 'Toko Parts Automotive', 1, 'parts@automotive.com', '+6281-333-4444', 0, TRUE, FALSE, '{now}', '{now}')");
        }

        // ============ STOCK ITEMS (skipped - requires UnitOfMeasure) ============
        // StockItems have complex dependencies, can be added via API later

        // ============ ACCOUNTS (Chart of Accounts - skipped for now) ============
        // Accounts have complex relationships, can be added via API

        logger.LogInformation("Demo data seeding complete!");
    }
}
