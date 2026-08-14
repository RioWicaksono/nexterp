using ERP.Domain.Hrm.Enums;
using ERP.Domain.Hrm.Services;

namespace ERP.Application.Hrm.DTOs;

/// <summary>
/// Payroll record DTO.
/// </summary>
public record PayrollDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeNumber { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal BasicSalary { get; init; }
    public decimal TotalAllowances { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal GrossSalary => BasicSalary + TotalAllowances;
    public decimal NetSalary => GrossSalary - TotalDeductions;
    public decimal PPh21Deduction { get; init; }
    public decimal BpjsKerjaDeduction { get; init; }
    public decimal BpjsKesehatanDeduction { get; init; }
    public decimal ThrAmount { get; init; }
    public PayrollStatus Status { get; init; }
    public DateTime? PaymentDate { get; init; }
    public string? Notes { get; init; }
    public List<PayrollDetailDto> Details { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Payroll detail DTO.
/// </summary>
public record PayrollDetailDto
{
    public Guid Id { get; init; }
    public string ComponentCode { get; init; } = string.Empty;
    public string ComponentName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public bool IsEarning { get; init; }
}

/// <summary>
/// Payroll calculation preview DTO.
/// </summary>
public record PayrollPreviewDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public decimal BasicSalary { get; init; }
    public decimal GrossSalary { get; init; }
    public decimal NetSalary { get; init; }
    public TaxCalculationDto Tax { get; init; } = new();
    public ThrCalculationDto? Thr { get; init; }
    public BpjsCalculationDto Bpjs { get; init; } = new();
    public List<PayrollComponentDto> Components { get; init; } = new();
}

/// <summary>
/// Tax calculation DTO.
/// </summary>
public record TaxCalculationDto
{
    public decimal AnnualTax { get; init; }
    public decimal MonthlyTax { get; init; }
    public decimal TaxableIncome { get; init; }
    public decimal PtkpAmount { get; init; }
    public string TaxStatus { get; init; } = string.Empty;
}

/// <summary>
/// THR calculation DTO.
/// </summary>
public record ThrCalculationDto
{
    public decimal BasicSalary { get; init; }
    public int MonthsOfService { get; init; }
    public decimal ThrAmount { get; init; }
    public decimal ProrateRatio { get; init; }
}

/// <summary>
/// BPJS calculation DTO.
/// </summary>
public record BpjsCalculationDto
{
    public decimal BpjsKetenagakerjaan { get; init; }
    public decimal BpjsKesehatan { get; init; }
    public decimal TotalContribution { get; init; }
}

/// <summary>
/// Payroll component DTO.
/// </summary>
public record PayrollComponentDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public bool IsEarning { get; init; }
    public string Category { get; init; } = string.Empty;
}

/// <summary>
/// Payslip DTO for employee.
/// </summary>
public record PayslipDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string CompanyAddress { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeNumber { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public int PayPeriodMonth { get; init; }
    public int PayPeriodYear { get; init; }
    public DateTime PayDate { get; init; }

    // Earnings
    public decimal BasicSalary { get; init; }
    public List<PayrollComponentDto> Allowances { get; init; } = new();
    public decimal TotalAllowances { get; init; }
    public decimal GrossSalary { get; init; }

    // Deductions
    public List<PayrollComponentDto> Deductions { get; init; } = new();
    public decimal TotalDeductions { get; init; }

    // Net
    public decimal NetSalary { get; init; }

    // Tax info
    public TaxCalculationDto Tax { get; init; } = new();
}

/// <summary>
/// Payroll summary for period.
/// </summary>
public record PayrollSummaryDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalEmployees { get; init; }
    public decimal TotalBasicSalary { get; init; }
    public decimal TotalAllowances { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal TotalGrossSalary { get; init; }
    public decimal TotalNetSalary { get; init; }
    public decimal TotalThr { get; init; }
    public decimal TotalPPh21 { get; init; }
    public PayrollStatus Status { get; init; }
}
