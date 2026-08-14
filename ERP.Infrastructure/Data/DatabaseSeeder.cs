#pragma warning disable EF1002 // Warning for raw SQL in seeder - all values are hardcoded constants
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

    // License Tiers
    public static readonly Guid StarterTierId = Guid.Parse("00000000-0000-0000-0000-000000000200");
    public static readonly Guid ProfessionalTierId = Guid.Parse("00000000-0000-0000-0000-000000000201");
    public static readonly Guid EnterpriseTierId = Guid.Parse("00000000-0000-0000-0000-000000000202");

    // Module Definitions
    public static readonly Guid SalesModuleId = Guid.Parse("00000000-0000-0000-0001-000000000001");
    public static readonly Guid InventoryModuleId = Guid.Parse("00000000-0000-0000-0001-000000000002");
    public static readonly Guid PurchasingModuleId = Guid.Parse("00000000-0000-0000-0001-000000000003");
    public static readonly Guid AccountingModuleId = Guid.Parse("00000000-0000-0000-0001-000000000004");
    public static readonly Guid HrmModuleId = Guid.Parse("00000000-0000-0000-0001-000000000005");
    public static readonly Guid ProjectsModuleId = Guid.Parse("00000000-0000-0000-0001-000000000006");
    public static readonly Guid QualityModuleId = Guid.Parse("00000000-0000-0000-0001-000000000007");
    public static readonly Guid AnalyticsModuleId = Guid.Parse("00000000-0000-0000-0001-000000000008");
    public static readonly Guid AssetsModuleId = Guid.Parse("00000000-0000-0000-0001-000000000009");

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()!.CreateLogger("DatabaseSeeder");

        await context.Database.MigrateAsync();

        var now = DateTime.UtcNow;
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        // ============ ORGANIZATION (MUST BE FIRST - FK dependency) ============
        if (!await context.Organizations.AnyAsync())
        {
            logger.LogInformation("Seeding organization...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Organizations"" (""Id"", ""Name"", ""Code"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"")
                VALUES ({DemoOrganizationId}, 'Nexterp Demo Corp', 'NEXTERP', TRUE, FALSE, {now}, {now})");
        }

        // ============ ROLES ============
        if (!await context.Roles.AnyAsync())
        {
            logger.LogInformation("Seeding roles...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Roles"" (""Id"", ""OrganizationId"", ""Name"", ""Description"", ""IsActive"", ""IsSystemRole"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({AdminRoleId}, {DemoOrganizationId}, 'Admin', 'System Administrator', TRUE, TRUE, FALSE, {now}, {now})");
        }

        // ============ USERS ============
        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("Seeding demo user...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Users"" (""Id"", ""OrganizationId"", ""Username"", ""Email"", ""PasswordHash"", ""FirstName"", ""LastName"", ""Phone"", ""IsActive"", ""IsSuperAdmin"", ""FailedLoginAttempts"", ""LockedUntil"", ""LastLoginAt"", ""LastLoginIp"", ""RefreshTokenHash"", ""RefreshTokenExpiry"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({DemoUserId}, {DemoOrganizationId}, 'admin', 'admin@nexterp.com', {passwordHash}, 'System', 'Administrator', NULL, TRUE, TRUE, 0, NULL, NULL, NULL, NULL, NULL, FALSE, {now}, {now})");

            // Assign Admin role
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""RoleId"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoUserId}, {AdminRoleId}, FALSE, {now}, {now})");
        }

        // ============ DEPARTMENTS ============
        if (!await context.Departments.AnyAsync())
        {
            logger.LogInformation("Seeding departments...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({EngineeringDeptId}, {DemoOrganizationId}, 'Engineering', 'ENG', 'Software Engineering Department', TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({HrDeptId}, {DemoOrganizationId}, 'Human Resources', 'HR', 'HR Management Department', TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({FinanceDeptId}, {DemoOrganizationId}, 'Finance', 'FIN', 'Finance & Accounting Department', TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Departments"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({SalesDeptId}, {DemoOrganizationId}, 'Sales', 'SLS', 'Sales & Marketing Department', TRUE, FALSE, {now}, {now})");
        }

        // ============ POSITIONS ============
        if (!await context.Positions.AnyAsync())
        {
            logger.LogInformation("Seeding positions...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""DepartmentId"", ""Title"", ""Description"", ""Grade"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({EngineeringPositionId}, {DemoOrganizationId}, {EngineeringDeptId}, 'Software Engineer', 'Entry-level software developer', 1, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""DepartmentId"", ""Title"", ""Description"", ""Grade"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({HrPositionId}, {DemoOrganizationId}, {HrDeptId}, 'HR Staff', 'Human Resources Officer', 1, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Positions"" (""Id"", ""OrganizationId"", ""DepartmentId"", ""Title"", ""Description"", ""Grade"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({ManagerPositionId}, {DemoOrganizationId}, {EngineeringDeptId}, 'Engineering Manager', 'Engineering Team Lead', 5, TRUE, FALSE, {now}, {now})");
        }

        // ============ WAREHOUSES ============
        if (!await context.Warehouses.AnyAsync())
        {
            logger.LogInformation("Seeding warehouses...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Warehouses"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""Address"", ""City"", ""Country"", ""Phone"", ""Email"", ""IsActive"", ""IsDefault"", ""AllowsNegativeStock"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({MainWarehouseId}, {DemoOrganizationId}, 'Main Warehouse', 'WH001', 'Primary storage facility', 'Jl. Sudirman No. 1', 'Jakarta', 'Indonesia', '+6221-555-0001', 'warehouse@nexterp.com', TRUE, TRUE, FALSE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Warehouses"" (""Id"", ""OrganizationId"", ""Name"", ""Code"", ""Description"", ""Address"", ""City"", ""Country"", ""Phone"", ""Email"", ""IsActive"", ""IsDefault"", ""AllowsNegativeStock"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({SecondaryWarehouseId}, {DemoOrganizationId}, 'Secondary Warehouse', 'WH002', 'Backup storage facility', 'Jl. Gatot Subroto No. 50', 'Surabaya', 'Indonesia', '+6231-555-0002', 'warehouse2@nexterp.com', TRUE, FALSE, TRUE, FALSE, {now}, {now})");
        }

        // ============ CUSTOMERS ============
        if (!await context.Customers.AnyAsync())
        {
            logger.LogInformation("Seeding customers...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Customers"" (""Id"", ""OrganizationId"", ""CustomerCode"", ""CustomerName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'CUST001', 'PT Maju Bersama', 2, 'contact@majubersama.co.id', '+6221-888-0001', 0, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Customers"" (""Id"", ""OrganizationId"", ""CustomerCode"", ""CustomerName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'CUST002', 'CV Sejahtera Utama', 2, 'info@sejahtera.co.id', '+6221-888-0002', 0, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Customers"" (""Id"", ""OrganizationId"", ""CustomerCode"", ""CustomerName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'CUST003', 'Toko Elektronik Jaya', 1, 'jaya@electronics.com', '+6281-234-5678', 0, TRUE, FALSE, {now}, {now})");
        }

        // ============ SUPPLIERS ============
        if (!await context.Suppliers.AnyAsync())
        {
            logger.LogInformation("Seeding suppliers...");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Suppliers"" (""Id"", ""OrganizationId"", ""SupplierCode"", ""SupplierName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'SUP001', 'PT Sumber Prima', 2, 'sales@sumberprima.co.id', '+6221-555-1001', 0, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Suppliers"" (""Id"", ""OrganizationId"", ""SupplierCode"", ""SupplierName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'SUP002', 'CV Elektronik Grosir', 2, 'order@elegrosir.com', '+6221-555-1002', 0, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Suppliers"" (""Id"", ""OrganizationId"", ""SupplierCode"", ""SupplierName"", ""Type"", ""Email"", ""Phone"", ""OutstandingAmount"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'SUP003', 'Toko Parts Automotive', 1, 'parts@automotive.com', '+6281-333-4444', 0, TRUE, FALSE, {now}, {now})");
        }

        // ============ STOCK ITEMS (skipped - requires UnitOfMeasure) ============
        // StockItems have complex dependencies, can be added via API later

        // ============ ACCOUNTS (Chart of Accounts - skipped for now) ============
        // Accounts have complex relationships, can be added via API

        // ============ LICENSE TIERS ============
        if (!await context.LicenseTiers.AnyAsync())
        {
            logger.LogInformation("Seeding license tiers...");

            // Starter Tier
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""LicenseTiers"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""MonthlyPrice"", ""DefaultMaxUsers"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({StarterTierId}, 'STARTER', 'Starter', 'Basic ERP package with core modules', 500000, 10, 1, TRUE, FALSE, {now}, {now})");

            // Professional Tier
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""LicenseTiers"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""MonthlyPrice"", ""DefaultMaxUsers"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({ProfessionalTierId}, 'PROFESSIONAL', 'Professional', 'Full ERP with HRM and Accounting', 1500000, 50, 2, TRUE, FALSE, {now}, {now})");

            // Enterprise Tier
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""LicenseTiers"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""MonthlyPrice"", ""DefaultMaxUsers"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({EnterpriseTierId}, 'ENTERPRISE', 'Enterprise', 'Complete ERP with all modules', 3000000, 200, 3, TRUE, FALSE, {now}, {now})");
        }

        // ============ MODULE DEFINITIONS ============
        if (!await context.Modules.AnyAsync())
        {
            logger.LogInformation("Seeding module definitions...");

            // Core Modules (included in all tiers)
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({SalesModuleId}, 'SALES', 'Sales Management', 'Customer management, quotes, orders, and invoices', 0, FALSE, 1, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({InventoryModuleId}, 'INVENTORY', 'Inventory Management', 'Stock management, warehouses, batch tracking', 0, FALSE, 2, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({PurchasingModuleId}, 'PURCHASING', 'Purchasing', 'Supplier management, purchase orders, goods receipt', 0, FALSE, 3, TRUE, FALSE, {now}, {now})");

            // Professional Modules
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({AccountingModuleId}, 'ACCOUNTING', 'Accounting', 'Chart of accounts, journals, financial reports', 1, TRUE, 4, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({HrmModuleId}, 'HRM', 'Human Resource Management', 'Employee management, attendance, leave, payroll', 1, TRUE, 5, TRUE, FALSE, {now}, {now})");

            // Enterprise Modules
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({ProjectsModuleId}, 'PROJECTS', 'Project Management', 'Project planning, task tracking, Gantt charts', 2, TRUE, 6, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({QualityModuleId}, 'QUALITY', 'Quality Management', 'Inspections, NCR, CAPA management', 2, TRUE, 7, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({AnalyticsModuleId}, 'ANALYTICS', 'Analytics & Reporting', 'Real-time dashboards, KPI tracking', 2, TRUE, 8, TRUE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""Modules"" (""Id"", ""Code"", ""DisplayName"", ""Description"", ""Category"", ""IsPremium"", ""SortOrder"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({AssetsModuleId}, 'ASSETS', 'Asset Management', 'Fixed assets, depreciation, maintenance tracking', 2, TRUE, 9, TRUE, FALSE, {now}, {now})");
        }

        // ============ MODULE PERMISSIONS ============
        if (!await context.ModulePermissions.AnyAsync())
        {
            logger.LogInformation("Seeding module permissions...");

            // Sales module permissions
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {SalesModuleId}, 'SALES.READ', 'View sales data', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {SalesModuleId}, 'SALES.CREATE', 'Create sales orders/quotes', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {SalesModuleId}, 'SALES.UPDATE', 'Update sales orders', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {SalesModuleId}, 'SALES.DELETE', 'Delete sales orders', FALSE, {now}, {now})");

            // HRM module permissions
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {HrmModuleId}, 'HRM.READ', 'View HR data', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {HrmModuleId}, 'HRM.EMPLOYEE.CREATE', 'Add new employees', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {HrmModuleId}, 'HRM.EMPLOYEE.UPDATE', 'Update employee data', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {HrmModuleId}, 'HRM.ATTENDANCE.MANAGE', 'Manage attendance records', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {HrmModuleId}, 'HRM.LEAVE.APPROVE', 'Approve leave requests', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {HrmModuleId}, 'HRM.PAYROLL.VIEW', 'View payroll data', FALSE, {now}, {now})");

            // Inventory module permissions
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {InventoryModuleId}, 'INV.READ', 'View inventory data', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {InventoryModuleId}, 'INV.STOCK.IN', 'Record stock in transactions', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""ModulePermissions"" (""Id"", ""ModuleId"", ""Permission"", ""Description"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {InventoryModuleId}, 'INV.STOCK.OUT', 'Record stock out transactions', FALSE, {now}, {now})");
        }

        // ============ ORGANIZATION LICENSE (Demo gets Enterprise) ============
        if (!await context.OrganizationLicenses.AnyAsync())
        {
            logger.LogInformation("Seeding demo organization license...");

            // Demo organization gets Enterprise license (valid for 1 year)
            var licenseEndDate = now.AddYears(1);
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationLicenses"" (""Id"", ""OrganizationId"", ""LicenseTierId"", ""StartDate"", ""EndDate"", ""MaxUsers"", ""BillingEmail"", ""IsAutoRenew"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {EnterpriseTierId}, {now}, {licenseEndDate}, 50, 'billing@nexterp.com', FALSE, FALSE, {now}, {now})");
        }

        // ============ ORGANIZATION MODULES (Demo gets all modules) ============
        if (!await context.OrganizationModules.AnyAsync())
        {
            logger.LogInformation("Seeding demo organization modules...");

            // All modules activated for demo (no expiry - null)
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {SalesModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {InventoryModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {PurchasingModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {AccountingModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {HrmModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {ProjectsModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {QualityModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {AnalyticsModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationModules"" (""Id"", ""OrganizationId"", ""ModuleId"", ""ActivatedAt"", ""ActivatedBy"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, {AssetsModuleId}, {now}, 'SYSTEM', FALSE, {now}, {now})");
        }

        // ============ DEFAULT ORGANIZATION SETTINGS ============
        if (!await context.OrganizationSettings.AnyAsync())
        {
            logger.LogInformation("Seeding default organization settings...");

            // HR Settings (Indonesian labor law defaults)
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'HR.OVERTIME.MAX_DAILY_HOURS', '4', 'HR', FALSE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'HR.OVERTIME.MAX_WEEKLY_HOURS', '18', 'HR', FALSE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'HR.LEAVE.ANNUAL_DEFAULT_DAYS', '12', 'HR', FALSE, FALSE, {now}, {now})");

            // Accounting Settings
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'ACC.DEFAULT_TERM_DAYS', '30', 'ACCOUNTING', FALSE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'ACC.DEFAULT_TAX_RATE', '11', 'ACCOUNTING', FALSE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'ACC.DEFAULT_CURRENCY', 'IDR', 'ACCOUNTING', FALSE, FALSE, {now}, {now})");

            // General Settings
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'GENERAL.TIMEZONE', 'Asia/Jakarta', 'GENERAL', FALSE, FALSE, {now}, {now})");
            await context.Database.ExecuteSqlAsync($@"
                INSERT INTO ""OrganizationSettings"" (""Id"", ""OrganizationId"", ""SettingKey"", ""SettingValue"", ""Category"", ""IsEncrypted"", ""IsDeleted"", ""CreatedAt"", ""UpdatedAt"") VALUES
                ({Guid.NewGuid()}, {DemoOrganizationId}, 'GENERAL.DATE_FORMAT', 'dd/MM/yyyy', 'GENERAL', FALSE, FALSE, {now}, {now})");
        }

        logger.LogInformation("Demo data seeding complete!");
    }
}
#pragma warning restore EF1002
