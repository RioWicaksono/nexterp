using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ModuleSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetDepreciations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    DepreciationAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AccumulatedDepreciation = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDepreciations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetMaintenances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMaintenances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AssetType = table.Column<string>(type: "text", nullable: false),
                    ParentAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "numeric", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Module = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Account_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountCode = table.Column<string>(type: "text", nullable: true),
                    Account_Name = table.Column<string>(type: "text", nullable: true),
                    Account_Description = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: true),
                    Class = table.Column<int>(type: "integer", nullable: true),
                    Account_CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Account_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    AllowDirectPosting = table.Column<bool>(type: "boolean", nullable: true),
                    IsBankAccount = table.Column<bool>(type: "boolean", nullable: true),
                    IsCashAccount = table.Column<bool>(type: "boolean", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "numeric", nullable: true),
                    OpeningBalanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: true),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    AccountId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    JournalEntry_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryNumber = table.Column<string>(type: "text", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    IsAutoEntry = table.Column<bool>(type: "boolean", nullable: true),
                    ReversedEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    JournalLine_Description = table.Column<string>(type: "text", nullable: true),
                    DebitAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    CreditAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JournalLine_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Organization_Name = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    TaxId = table.Column<string>(type: "text", nullable: true),
                    Organization_Phone = table.Column<string>(type: "text", nullable: true),
                    Organization_Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Organization_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    LicenseExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Role_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Role_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: true),
                    RolePermission_RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Permission = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsSuperAdmin = table.Column<bool>(type: "boolean", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginIp = table.Column<string>(type: "text", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationSetting_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettingKey = table.Column<string>(type: "text", nullable: true),
                    SettingValue = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    OrganizationSetting_Description = table.Column<string>(type: "text", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: true),
                    LicenseTier_Code = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    LicenseTier_Description = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    DefaultMaxUsers = table.Column<int>(type: "integer", nullable: true),
                    LicenseTier_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    ModuleDefinition_Code = table.Column<string>(type: "text", nullable: true),
                    ModuleDefinition_DisplayName = table.Column<string>(type: "text", nullable: true),
                    ModuleDefinition_Description = table.Column<string>(type: "text", nullable: true),
                    ModuleDefinition_Category = table.Column<int>(type: "integer", nullable: true),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: true),
                    ModuleDefinition_SortOrder = table.Column<int>(type: "integer", nullable: true),
                    ModuleDefinition_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModulePermission_Permission = table.Column<string>(type: "text", nullable: true),
                    ModulePermission_Description = table.Column<string>(type: "text", nullable: true),
                    ModuleDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationLicense_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicenseTierId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxUsers = table.Column<int>(type: "integer", nullable: true),
                    IsAutoRenew = table.Column<bool>(type: "boolean", nullable: true),
                    BillingEmail = table.Column<string>(type: "text", nullable: true),
                    OrganizationModule_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationModule_ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedBy = table.Column<string>(type: "text", nullable: true),
                    OrganizationModule_Notes = table.Column<string>(type: "text", nullable: true),
                    OrganizationModule_ModuleDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Attendance_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attendance_Status = table.Column<int>(type: "integer", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedHours = table.Column<TimeSpan>(type: "interval", nullable: true),
                    OvertimeHours = table.Column<decimal>(type: "numeric", nullable: true),
                    Attendance_Notes = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Department_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Department_Name = table.Column<string>(type: "text", nullable: true),
                    Department_Code = table.Column<string>(type: "text", nullable: true),
                    Department_Description = table.Column<string>(type: "text", nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Department_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Employee_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeNumber = table.Column<string>(type: "text", nullable: true),
                    Employee_UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Employee_FirstName = table.Column<string>(type: "text", nullable: true),
                    Employee_LastName = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    MaritalStatus = table.Column<int>(type: "integer", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    Employee_DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmploymentType = table.Column<int>(type: "integer", nullable: true),
                    Employee_Status = table.Column<int>(type: "integer", nullable: true),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TerminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PersonalEmail = table.Column<string>(type: "text", nullable: true),
                    Employee_Phone = table.Column<string>(type: "text", nullable: true),
                    Mobile = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "text", nullable: true),
                    Employee_Address = table.Column<string>(type: "text", nullable: true),
                    Employee_City = table.Column<string>(type: "text", nullable: true),
                    Employee_Country = table.Column<string>(type: "text", nullable: true),
                    Employee_PostalCode = table.Column<string>(type: "text", nullable: true),
                    Employee_BankName = table.Column<string>(type: "text", nullable: true),
                    Employee_BankAccountNumber = table.Column<string>(type: "text", nullable: true),
                    BankAccountName = table.Column<string>(type: "text", nullable: true),
                    Employee_TaxId = table.Column<string>(type: "text", nullable: true),
                    LeaveBalance_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveBalance_EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveType = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    TotalDays = table.Column<decimal>(type: "numeric", nullable: true),
                    UsedDays = table.Column<decimal>(type: "numeric", nullable: true),
                    PendingDays = table.Column<decimal>(type: "numeric", nullable: true),
                    CarryForward = table.Column<decimal>(type: "numeric", nullable: true),
                    LeaveRequest_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveRequest_EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveRequest_LeaveType = table.Column<int>(type: "integer", nullable: true),
                    LeaveRequest_StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeaveRequest_EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HalfDay = table.Column<decimal>(type: "numeric", nullable: true),
                    LeaveRequest_Status = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    LeaveRequest_ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LeaveRequest_ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Position_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Position_DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Position_Title = table.Column<string>(type: "text", nullable: true),
                    Position_Description = table.Column<string>(type: "text", nullable: true),
                    Grade = table.Column<int>(type: "integer", nullable: true),
                    MinSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    Position_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    StockItem_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockItem_Name = table.Column<string>(type: "text", nullable: true),
                    StockItem_Code = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    StockItem_Description = table.Column<string>(type: "text", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReorderLevel = table.Column<decimal>(type: "numeric", nullable: true),
                    MinimumStock = table.Column<decimal>(type: "numeric", nullable: true),
                    MaximumStock = table.Column<decimal>(type: "numeric", nullable: true),
                    StandardCost = table.Column<decimal>(type: "numeric", nullable: true),
                    StandardPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    ValuationMethod = table.Column<int>(type: "integer", nullable: true),
                    StockItem_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    TrackSerials = table.Column<bool>(type: "boolean", nullable: true),
                    TrackBatch = table.Column<bool>(type: "boolean", nullable: true),
                    ExpiryDays = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric", nullable: true),
                    Length = table.Column<decimal>(type: "numeric", nullable: true),
                    Width = table.Column<decimal>(type: "numeric", nullable: true),
                    Height = table.Column<decimal>(type: "numeric", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockItemWarehouse_WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    AverageCost = table.Column<decimal>(type: "numeric", nullable: true),
                    ReservedQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    LastStockIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastStockOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StockTransaction_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionNumber = table.Column<string>(type: "text", nullable: true),
                    StockTransaction_Type = table.Column<int>(type: "integer", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StockTransaction_StockItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockTransaction_WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceWarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockTransaction_Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    StockTransaction_ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    StockTransaction_ReferenceType = table.Column<string>(type: "text", nullable: true),
                    StockTransaction_ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    BatchNumber = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StockTransaction_Notes = table.Column<string>(type: "text", nullable: true),
                    StockTransaction_Status = table.Column<int>(type: "integer", nullable: true),
                    UnitOfMeasure_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitOfMeasure_Name = table.Column<string>(type: "text", nullable: true),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    Abbreviation = table.Column<string>(type: "text", nullable: true),
                    UnitOfMeasure_Type = table.Column<int>(type: "integer", nullable: true),
                    FactorToBase = table.Column<decimal>(type: "numeric", nullable: true),
                    BaseUomId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitOfMeasure_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Warehouse_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Warehouse_Name = table.Column<string>(type: "text", nullable: true),
                    Warehouse_Code = table.Column<string>(type: "text", nullable: true),
                    Warehouse_Description = table.Column<string>(type: "text", nullable: true),
                    Warehouse_Address = table.Column<string>(type: "text", nullable: true),
                    Warehouse_City = table.Column<string>(type: "text", nullable: true),
                    Warehouse_Country = table.Column<string>(type: "text", nullable: true),
                    Warehouse_Phone = table.Column<string>(type: "text", nullable: true),
                    Warehouse_Email = table.Column<string>(type: "text", nullable: true),
                    Warehouse_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true),
                    AllowsNegativeStock = table.Column<bool>(type: "boolean", nullable: true),
                    Project_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Project_Name = table.Column<string>(type: "text", nullable: true),
                    Project_Code = table.Column<string>(type: "text", nullable: true),
                    Project_Description = table.Column<string>(type: "text", nullable: true),
                    Project_Status = table.Column<int>(type: "integer", nullable: true),
                    Project_StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Project_EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Budget = table.Column<decimal>(type: "numeric", nullable: true),
                    ProjectManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: true),
                    ProjectTask_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectTask_ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectTask_Title = table.Column<string>(type: "text", nullable: true),
                    ProjectTask_Description = table.Column<string>(type: "text", nullable: true),
                    ProjectTask_Status = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: true),
                    ProjectTask_StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectTask_DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "numeric", nullable: true),
                    ActualHours = table.Column<decimal>(type: "numeric", nullable: true),
                    Progress = table.Column<decimal>(type: "numeric", nullable: true),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrder_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNumber = table.Column<string>(type: "text", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrder_Status = table.Column<int>(type: "integer", nullable: true),
                    PaymentTermId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PurchaseOrder_TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    BillingAddress = table.Column<string>(type: "text", nullable: true),
                    ShippingAddress = table.Column<string>(type: "text", nullable: true),
                    PurchaseOrder_Notes = table.Column<string>(type: "text", nullable: true),
                    PurchaseOrder_WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderLine_StockItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderLine_Description = table.Column<string>(type: "text", nullable: true),
                    PurchaseOrderLine_Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    PurchaseOrderLine_DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: true),
                    PurchaseOrderLine_TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    LineTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    PurchaseOrderLine_UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: true),
                    Supplier_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierCode = table.Column<string>(type: "text", nullable: true),
                    SupplierName = table.Column<string>(type: "text", nullable: true),
                    Supplier_Type = table.Column<int>(type: "integer", nullable: true),
                    Supplier_TaxId = table.Column<string>(type: "text", nullable: true),
                    Supplier_Email = table.Column<string>(type: "text", nullable: true),
                    Supplier_Phone = table.Column<string>(type: "text", nullable: true),
                    Supplier_Mobile = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    Supplier_BillingAddress = table.Column<string>(type: "text", nullable: true),
                    BillingCity = table.Column<string>(type: "text", nullable: true),
                    BillingCountry = table.Column<string>(type: "text", nullable: true),
                    BillingPostalCode = table.Column<string>(type: "text", nullable: true),
                    Supplier_ShippingAddress = table.Column<string>(type: "text", nullable: true),
                    ShippingCity = table.Column<string>(type: "text", nullable: true),
                    ShippingCountry = table.Column<string>(type: "text", nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "text", nullable: true),
                    Supplier_PaymentTermId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "numeric", nullable: true),
                    OutstandingAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Supplier_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Supplier_Notes = table.Column<string>(type: "text", nullable: true),
                    Supplier_BankName = table.Column<string>(type: "text", nullable: true),
                    Supplier_BankAccountNumber = table.Column<string>(type: "text", nullable: true),
                    Supplier_BankAccountName = table.Column<string>(type: "text", nullable: true),
                    Customer_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerCode = table.Column<string>(type: "text", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    Customer_Type = table.Column<int>(type: "integer", nullable: true),
                    Customer_TaxId = table.Column<string>(type: "text", nullable: true),
                    Customer_Email = table.Column<string>(type: "text", nullable: true),
                    Customer_Phone = table.Column<string>(type: "text", nullable: true),
                    Customer_Mobile = table.Column<string>(type: "text", nullable: true),
                    Customer_Website = table.Column<string>(type: "text", nullable: true),
                    Customer_BillingAddress = table.Column<string>(type: "text", nullable: true),
                    Customer_BillingCity = table.Column<string>(type: "text", nullable: true),
                    Customer_BillingCountry = table.Column<string>(type: "text", nullable: true),
                    Customer_BillingPostalCode = table.Column<string>(type: "text", nullable: true),
                    Customer_ShippingAddress = table.Column<string>(type: "text", nullable: true),
                    Customer_ShippingCity = table.Column<string>(type: "text", nullable: true),
                    Customer_ShippingCountry = table.Column<string>(type: "text", nullable: true),
                    Customer_ShippingPostalCode = table.Column<string>(type: "text", nullable: true),
                    PriceListId = table.Column<Guid>(type: "uuid", nullable: true),
                    Customer_PaymentTermId = table.Column<Guid>(type: "uuid", nullable: true),
                    Customer_CreditLimit = table.Column<decimal>(type: "numeric", nullable: true),
                    Customer_OutstandingAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Customer_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Customer_Notes = table.Column<string>(type: "text", nullable: true),
                    SalesInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentDetail_Reference = table.Column<string>(type: "text", nullable: true),
                    PaymentDetail_Notes = table.Column<string>(type: "text", nullable: true),
                    SalesInvoice_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesInvoice_DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesInvoice_Status = table.Column<int>(type: "integer", nullable: true),
                    SalesInvoice_Type = table.Column<int>(type: "integer", nullable: true),
                    SalesInvoice_PriceListId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesInvoice_PaymentTermId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesInvoice_Subtotal = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoice_TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoice_DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoice_TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoice_OutstandingAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoice_BillingAddress = table.Column<string>(type: "text", nullable: true),
                    SalesInvoice_Notes = table.Column<string>(type: "text", nullable: true),
                    Terms = table.Column<string>(type: "text", nullable: true),
                    SalesPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesInvoiceLine_SalesInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesInvoiceLine_StockItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesInvoiceLine_Description = table.Column<string>(type: "text", nullable: true),
                    SalesInvoiceLine_Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_UnitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_DiscountPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_TaxRate = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_Subtotal = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_LineTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesInvoiceLine_UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrder_OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrder_OrderNumber = table.Column<string>(type: "text", nullable: true),
                    SalesOrder_OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesOrder_CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrder_Status = table.Column<int>(type: "integer", nullable: true),
                    SalesOrder_PriceListId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrder_PaymentTermId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrder_Subtotal = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrder_TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrder_DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrder_TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrder_BillingAddress = table.Column<string>(type: "text", nullable: true),
                    SalesOrder_ShippingAddress = table.Column<string>(type: "text", nullable: true),
                    SalesOrder_Notes = table.Column<string>(type: "text", nullable: true),
                    SalesOrder_SalesPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrder_WarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrderLine_SalesOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrderLine_StockItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrderLine_Description = table.Column<string>(type: "text", nullable: true),
                    SalesOrderLine_Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    DeliveredQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_UnitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_DiscountPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_TaxRate = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_LineTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    SalesOrderLine_UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_AccountId",
                        column: x => x.AccountId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_AccountId1",
                        column: x => x.AccountId1,
                        principalTable: "BaseEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_Employee_DepartmentId",
                        column: x => x.Employee_DepartmentId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_LeaveRequest_EmployeeId",
                        column: x => x.LeaveRequest_EmployeeId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_LicenseTierId",
                        column: x => x.LicenseTierId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_ModuleDefinitionId",
                        column: x => x.ModuleDefinitionId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_OrganizationModule_ModuleDefinitionId",
                        column: x => x.OrganizationModule_ModuleDefinitionId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_PositionId",
                        column: x => x.PositionId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_ProjectTask_ProjectId",
                        column: x => x.ProjectTask_ProjectId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_RoleId",
                        column: x => x.RoleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_RolePermission_RoleId",
                        column: x => x.RolePermission_RoleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_SalesInvoiceLine_SalesInvoiceId",
                        column: x => x.SalesInvoiceLine_SalesInvoiceId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_SalesOrderLine_SalesOrderId",
                        column: x => x.SalesOrderLine_SalesOrderId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_StockTransaction_WarehouseId",
                        column: x => x.StockTransaction_WarehouseId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_UserId",
                        column: x => x.UserId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_Warehouse_OrganizationId",
                        column: x => x.Warehouse_OrganizationId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardWidgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WidgetType = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    To = table.Column<string>(type: "text", nullable: false),
                    Cc = table.Column<string>(type: "text", nullable: false),
                    Bcc = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionNumber = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Inspector = table.Column<string>(type: "text", nullable: true),
                    Results = table.Column<string>(type: "text", nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NonConformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RootCause = table.Column<string>(type: "text", nullable: false),
                    CorrectiveAction = table.Column<string>(type: "text", nullable: false),
                    PreventiveAction = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonConformances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_AccountId",
                table: "BaseEntity",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_AccountId1",
                table: "BaseEntity",
                column: "AccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_DepartmentId",
                table: "BaseEntity",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Employee_DepartmentId",
                table: "BaseEntity",
                column: "Employee_DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_EmployeeId",
                table: "BaseEntity",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_JournalEntryId",
                table: "BaseEntity",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_LeaveRequest_EmployeeId",
                table: "BaseEntity",
                column: "LeaveRequest_EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_LicenseTierId",
                table: "BaseEntity",
                column: "LicenseTierId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_ModuleDefinitionId",
                table: "BaseEntity",
                column: "ModuleDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_OrganizationId",
                table: "BaseEntity",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_OrganizationModule_ModuleDefinitionId",
                table: "BaseEntity",
                column: "OrganizationModule_ModuleDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_PositionId",
                table: "BaseEntity",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_ProjectTask_ProjectId",
                table: "BaseEntity",
                column: "ProjectTask_ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_ProjectTaskId",
                table: "BaseEntity",
                column: "ProjectTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_PurchaseOrderId",
                table: "BaseEntity",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_RoleId",
                table: "BaseEntity",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_RolePermission_RoleId",
                table: "BaseEntity",
                column: "RolePermission_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_SalesInvoiceId",
                table: "BaseEntity",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_SalesInvoiceLine_SalesInvoiceId",
                table: "BaseEntity",
                column: "SalesInvoiceLine_SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_SalesOrderLine_SalesOrderId",
                table: "BaseEntity",
                column: "SalesOrderLine_SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_StockItemId",
                table: "BaseEntity",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_StockTransaction_WarehouseId",
                table: "BaseEntity",
                column: "StockTransaction_WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_UserId",
                table: "BaseEntity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Warehouse_OrganizationId",
                table: "BaseEntity",
                column: "Warehouse_OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_WarehouseId",
                table: "BaseEntity",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetDepreciations");

            migrationBuilder.DropTable(
                name: "AssetMaintenances");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BaseEntity");

            migrationBuilder.DropTable(
                name: "DashboardWidgets");

            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "NonConformances");

            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
