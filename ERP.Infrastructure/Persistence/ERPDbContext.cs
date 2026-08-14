using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Base;
using ERP.Domain.Common.Configuration;
using ERP.Domain.Common.Modules;
using ERP.Domain.Inventory.Entities;
using ERP.Domain.Accounting.Entities;
using ERP.Domain.Sales.Entities;
using ERP.Domain.Purchasing.Entities;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Projects.Entities;
using ERP.Domain.Analytics.Entities;
using ERP.Domain.Assets.Entities;
using ERP.Domain.Quality.Entities;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// ERP Database Context - main data access layer
/// </summary>
public class ERPDbContext : DbContext, IApplicationDbContext
{
	private readonly ITenantContext? _tenantContext;

	// Constructor for normal DI
	public ERPDbContext(DbContextOptions<ERPDbContext> options) : base(options)
	{
	}

	// Constructor for design-time (migrations)
	public ERPDbContext(DbContextOptions<ERPDbContext> options, ITenantContext? tenantContext) : base(options)
	{
		_tenantContext = tenantContext;
	}

	// Base entities
	public DbSet<Organization> Organizations => Set<Organization>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Role> Roles => Set<Role>();
	public DbSet<UserRole> UserRoles => Set<UserRole>();
	public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

	// Inventory entities
	public DbSet<Warehouse> Warehouses => Set<Warehouse>();
	public DbSet<StockItem> StockItems => Set<StockItem>();
	public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
	public DbSet<StockItemWarehouse> StockItemWarehouses => Set<StockItemWarehouse>();
	public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

	// Accounting entities
	public DbSet<Account> Accounts => Set<Account>();
	public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
	public DbSet<JournalLine> JournalLines => Set<JournalLine>();

	// Sales entities
	public DbSet<Customer> Customers => Set<Customer>();
	public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
	public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
	public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
	public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();
	public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();

	// Purchasing entities
	public DbSet<Supplier> Suppliers => Set<Supplier>();
	public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
	public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

	// HRM entities
	public DbSet<Department> Departments => Set<Department>();
	public DbSet<Position> Positions => Set<Position>();
	public DbSet<Employee> Employees => Set<Employee>();
	public DbSet<Attendance> Attendances => Set<Attendance>();
	public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
	public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
	public DbSet<LeaveEntitlement> LeaveEntitlements => Set<LeaveEntitlement>();
	public DbSet<OvertimeRequest> OvertimeRequests => Set<OvertimeRequest>();
	public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
	public DbSet<Shift> Shifts => Set<Shift>();
	public DbSet<Holiday> Holidays => Set<Holiday>();
	public DbSet<Payroll> Payrolls => Set<Payroll>();
	public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

	// Project Management entities
	public DbSet<Project> Projects => Set<Project>();
	public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

	// Analytics entities
	public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
	public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
	public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
	public DbSet<Notification> Notifications => Set<Notification>();

	// Asset Management entities
	public DbSet<Asset> Assets => Set<Asset>();
	public DbSet<AssetDepreciation> AssetDepreciations => Set<AssetDepreciation>();
	public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();

	// Quality Management entities
	public DbSet<Inspection> Inspections => Set<Inspection>();
	public DbSet<NonConformance> NonConformances => Set<NonConformance>();

	// Module & Licensing entities
	public DbSet<ModuleDefinition> Modules => Set<ModuleDefinition>();
	public DbSet<OrganizationModule> OrganizationModules => Set<OrganizationModule>();
	public DbSet<LicenseTier> LicenseTiers => Set<LicenseTier>();
	public DbSet<OrganizationLicense> OrganizationLicenses => Set<OrganizationLicense>();
	public DbSet<ModulePermission> ModulePermissions => Set<ModulePermission>();

	// Organization Settings
	public DbSet<OrganizationSetting> OrganizationSettings => Set<OrganizationSetting>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// Apply global query filters
		ApplyGlobalFilters(modelBuilder);
	}

	// Note: Timestamp updates (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) are handled
	// by AuditingInterceptor to ensure consistency with user context.
	// Soft delete is also handled by AuditingInterceptor.

	private void ApplyGlobalFilters(ModelBuilder modelBuilder)
	{
		// Global filter for soft delete - applied to root BaseEntity (TPT hierarchy)
		// Note: Tenant filter (OrganizationId) is NOT applied via query filter because
		// TPT inheritance stores discriminator columns per entity (Employee_OrganizationId, etc.)
		// instead of a shared OrganizationId on BaseEntity. Tenant isolation is handled by:
		//   1. TenantEntityInterceptor: auto-sets OrganizationId on new entities
		//   2. Explicit OrganizationId filters in each repository/service query
		modelBuilder.Entity<Domain.Common.BaseEntity>().HasQueryFilter(e => !e.IsDeleted);
	}
}
