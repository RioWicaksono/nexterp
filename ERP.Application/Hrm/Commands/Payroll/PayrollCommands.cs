using ERP.Domain.Hrm.Enums;
using ERP.Domain.Hrm.Services;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ERP.Application.Hrm.Commands.Payroll;

/// <summary>
/// Calculate payroll preview for an employee.
/// </summary>
public record CalculatePayrollPreviewCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    int Year,
    int Month,
    bool IncludeThr = false,
    TaxStatus TaxStatus = TaxStatus.TK0
) : IRequest<PayrollPreviewDto>;

/// <summary>
/// Create payroll for a single employee.
/// </summary>
public record CreatePayrollCommand(
    Guid OrganizationId,
    Guid EmployeeId,
    int Year,
    int Month,
    decimal BasicSalary,
    List<PayrollComponentInput> Allowances,
    List<PayrollComponentInput> Deductions,
    TaxStatus TaxStatus,
    bool IncludeThr,
    string? Notes = null
) : IRequest<Guid>;

/// <summary>
/// Create payroll batch for all employees in a department.
/// </summary>
public record CreateBatchPayrollCommand(
    Guid OrganizationId,
    Guid? DepartmentId,
    int Year,
    int Month,
    TaxStatus DefaultTaxStatus,
    bool IncludeThr,
    bool IncludeOvertime,
    string? Notes = null
) : IRequest<BatchPayrollResult>;

/// <summary>
/// Approve payroll for payment.
/// </summary>
public record ApprovePayrollCommand(
    Guid OrganizationId,
    Guid PayrollId,
    string ApprovedBy
) : IRequest<bool>;

/// <summary>
/// Mark payroll as paid.
/// </summary>
public record MarkPayrollPaidCommand(
    Guid OrganizationId,
    Guid PayrollId,
    DateTime PaymentDate,
    string PaidBy
) : IRequest<bool>;

/// <summary>
/// Delete payroll draft.
/// </summary>
public record DeletePayrollCommand(
    Guid OrganizationId,
    Guid PayrollId,
    string DeletedBy
) : IRequest<bool>;

/// <summary>
/// Payroll component input.
/// </summary>
public record PayrollComponentInput(string Code, string Name, decimal Amount);

/// <summary>
/// Batch payroll creation result.
/// </summary>
public record BatchPayrollResult(
    int TotalEmployees,
    int CreatedCount,
    int SkippedCount,
    List<string> Errors
);

/// <summary>
/// Validator for CreatePayrollCommand.
/// </summary>
public class CreatePayrollCommandValidator : AbstractValidator<CreatePayrollCommand>
{
    public CreatePayrollCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Year must be valid");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage("Basic salary must be greater than 0");

        RuleFor(x => x.TaxStatus)
            .IsInEnum().WithMessage("Invalid tax status");
    }
}

/// <summary>
/// Validator for CalculatePayrollPreviewCommand.
/// </summary>
public class CalculatePayrollPreviewCommandValidator : AbstractValidator<CalculatePayrollPreviewCommand>
{
    public CalculatePayrollPreviewCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Year must be valid");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");
    }
}
