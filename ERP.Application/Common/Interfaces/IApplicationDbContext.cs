using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Base;
using ERP.Domain.Inventory.Entities;
using ERP.Domain.Accounting.Entities;
using ERP.Domain.Sales.Entities;
using ERP.Domain.Purchasing.Entities;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Projects.Entities;
using ERP.Domain.Analytics.Entities;
using ERP.Domain.Assets.Entities;
using ERP.Domain.Quality.Entities;
using ERP.Domain.Common.Modules;
using ERP.Domain.Common.Configuration;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// </summary>
public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();

    // Base entities
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    // Inventory entities
    DbSet<Warehouse> Warehouses { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<UnitOfMeasure> UnitOfMeasures { get; }
    DbSet<StockItemWarehouse> StockItemWarehouses { get; }
    DbSet<StockTransaction> StockTransactions { get; }

    // Accounting entities
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalLine> JournalLines { get; }

    // Sales entities
    DbSet<Customer> Customers { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderLine> SalesOrderLines { get; }
    DbSet<SalesInvoice> SalesInvoices { get; }
    DbSet<SalesInvoiceLine> SalesInvoiceLines { get; }
    DbSet<PaymentDetail> PaymentDetails { get; }

    // Purchasing entities
    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }

    // HRM entities
    DbSet<Department> Departments { get; }
    DbSet<Position> Positions { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }
    DbSet<LeaveEntitlement> LeaveEntitlements { get; }
    DbSet<OvertimeRequest> OvertimeRequests { get; }
    DbSet<EmployeeDocument> EmployeeDocuments { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<Payroll> Payrolls { get; }
    DbSet<PayrollDetail> PayrollDetails { get; }

    // Project Management entities
    DbSet<Project> Projects { get; }
    DbSet<ProjectTask> ProjectTasks { get; }

    // Analytics entities
    DbSet<DashboardWidget> DashboardWidgets { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<EmailLog> EmailLogs { get; }
    DbSet<Notification> Notifications { get; }

    // Asset Management entities
    DbSet<Asset> Assets { get; }
    DbSet<AssetDepreciation> AssetDepreciations { get; }
    DbSet<AssetMaintenance> AssetMaintenances { get; }

    // Quality Management entities
    DbSet<Inspection> Inspections { get; }
    DbSet<NonConformance> NonConformances { get; }

    // Module & Licensing entities
    DbSet<ModuleDefinition> Modules { get; }
    DbSet<OrganizationModule> OrganizationModules { get; }
    DbSet<LicenseTier> LicenseTiers { get; }
    DbSet<OrganizationLicense> OrganizationLicenses { get; }
    DbSet<ModulePermission> ModulePermissions { get; }

    // Organization Settings
    DbSet<OrganizationSetting> OrganizationSettings { get; }
}
