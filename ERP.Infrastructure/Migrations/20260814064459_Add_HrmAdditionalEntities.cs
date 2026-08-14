using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_HrmAdditionalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "BaseEntity",
                newName: "Shift_Name");

            migrationBuilder.AddColumn<DateTime>(
                name: "AllocationDate",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedHours",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasicSalary",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BpjsKerjaDeduction",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BpjsKesehatanDeduction",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakEnd",
                table: "BaseEntity",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakStart",
                table: "BaseEntity",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarryForwardDays",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComponentCode",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComponentName",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeDocument_EmployeeId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeDocument_Notes",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeDocument_OrganizationId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "BaseEntity",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HolidayType",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Holiday_Date",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Holiday_Name",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Holiday_OrganizationId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Hours",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoAllocated",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEarning",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNightShift",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaveBalance_Year",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveEntitlement_EmployeeId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveEntitlement_LeaveType",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveEntitlement_Notes",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveEntitlement_OrganizationId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LeaveEntitlement_PendingDays",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LeaveEntitlement_TotalDays",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LeaveEntitlement_UsedDays",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaveEntitlement_Year",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OngkirDeduction",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OvertimeRequest_ApprovedAt",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OvertimeRequest_EmployeeId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OvertimeRequest_OrganizationId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OvertimeRequest_Reason",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeRequest_Status",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeType",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PPh21Deduction",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentDetail_Amount",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDetail_PaymentDate",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayrollId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Payroll_EmployeeId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Payroll_Notes",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Payroll_OrganizationId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Payroll_Status",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Payroll_Year",
                table: "BaseEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shift_Code",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shift_Description",
                table: "BaseEntity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Shift_EndTime",
                table: "BaseEntity",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Shift_IsActive",
                table: "BaseEntity",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Shift_OrganizationId",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Shift_StartTime",
                table: "BaseEntity",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "BaseEntity",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StockTransaction_ExpiryDate",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Thr",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAllowances",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "BaseEntity",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedBy",
                table: "BaseEntity",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkDate",
                table: "BaseEntity",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_PayrollId",
                table: "BaseEntity",
                column: "PayrollId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEntity_BaseEntity_PayrollId",
                table: "BaseEntity",
                column: "PayrollId",
                principalTable: "BaseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEntity_BaseEntity_PayrollId",
                table: "BaseEntity");

            migrationBuilder.DropIndex(
                name: "IX_BaseEntity_PayrollId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "AllocationDate",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ApprovedHours",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BasicSalary",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BpjsKerjaDeduction",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BpjsKesehatanDeduction",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BreakEnd",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "BreakStart",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "CarryForwardDays",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ComponentCode",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "ComponentName",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "EmployeeDocument_EmployeeId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "EmployeeDocument_Notes",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "EmployeeDocument_OrganizationId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "HolidayType",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Holiday_Date",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Holiday_Name",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Holiday_OrganizationId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IsAutoAllocated",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IsEarning",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IsNightShift",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "IssuedBy",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveBalance_Year",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_EmployeeId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_LeaveType",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_Notes",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_OrganizationId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_PendingDays",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_TotalDays",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_UsedDays",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "LeaveEntitlement_Year",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OngkirDeduction",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OvertimeRequest_ApprovedAt",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OvertimeRequest_EmployeeId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OvertimeRequest_OrganizationId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OvertimeRequest_Reason",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OvertimeRequest_Status",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "OvertimeType",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "PPh21Deduction",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "PaymentDetail_Amount",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "PaymentDetail_PaymentDate",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "PayrollId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Payroll_EmployeeId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Payroll_Notes",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Payroll_OrganizationId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Payroll_Status",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Payroll_Year",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Shift_Code",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Shift_Description",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Shift_EndTime",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Shift_IsActive",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Shift_OrganizationId",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Shift_StartTime",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "StockTransaction_ExpiryDate",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "Thr",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "TotalAllowances",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "VerifiedBy",
                table: "BaseEntity");

            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "BaseEntity");

            migrationBuilder.RenameColumn(
                name: "Shift_Name",
                table: "BaseEntity",
                newName: "RefreshToken");
        }
    }
}
