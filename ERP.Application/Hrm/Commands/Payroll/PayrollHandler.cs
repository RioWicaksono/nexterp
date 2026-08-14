using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Hrm.DTOs;
using ERP.Application.Hrm.Queries.Payroll;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;
using ERP.Domain.Hrm.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Hrm.Commands.Payroll;

/// <summary>
/// Handler for payroll commands and queries.
/// </summary>
public class PayrollHandler :
    IRequestHandler<CalculatePayrollPreviewCommand, PayrollPreviewDto>,
    IRequestHandler<CreatePayrollCommand, Guid>,
    IRequestHandler<CreateBatchPayrollCommand, BatchPayrollResult>,
    IRequestHandler<ApprovePayrollCommand, bool>,
    IRequestHandler<MarkPayrollPaidCommand, bool>,
    IRequestHandler<DeletePayrollCommand, bool>,
    IRequestHandler<GetPayrollListQuery, PaginatedList<PayrollListItemDto>>,
    IRequestHandler<GetPayrollByIdQuery, PayrollDto?>,
    IRequestHandler<GetPayrollSummaryQuery, PayrollSummaryDto?>,
    IRequestHandler<GetPayslipQuery, PayslipDto?>,
    IRequestHandler<GetEmployeePayrollHistoryQuery, PaginatedList<PayrollDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly PayrollCalculationService _payrollService;

    public PayrollHandler(IApplicationDbContext context, PayrollCalculationService payrollService)
    {
        _context = context;
        _payrollService = payrollService;
    }

    public async Task<PayrollPreviewDto> Handle(CalculatePayrollPreviewCommand request, CancellationToken ct)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.OrganizationId == request.OrganizationId, ct);

        if (employee == null)
            throw new InvalidOperationException("Employee not found");

        var allowances = new List<PayrollInputComponent>
        {
            new("TUNJ", "Tunjangan Transport", 500000),
            new("TUNM", "Tunjangan Makan", 300000)
        };

        var result = _payrollService.CalculatePayroll(
            employee.BasicSalary,
            allowances,
            new List<PayrollInputComponent>(),
            request.TaxStatus,
            employee.HireDate,
            new DateTime(request.Year, request.Month, 1).AddMonths(1).AddDays(-1),
            request.IncludeThr);

        return new PayrollPreviewDto
        {
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            BasicSalary = result.BasicSalary,
            GrossSalary = result.GrossSalary,
            NetSalary = result.NetSalary,
            Tax = new TaxCalculationDto
            {
                AnnualTax = result.Tax.AnnualTax,
                MonthlyTax = result.Tax.MonthlyTax,
                TaxableIncome = result.Tax.TaxableIncome,
                PtkpAmount = result.Tax.PtkpAmount,
                TaxStatus = result.Tax.TaxStatus.ToString()
            },
            Thr = result.Thr != null ? new ThrCalculationDto
            {
                BasicSalary = result.Thr.BasicSalary,
                MonthsOfService = result.Thr.MonthsOfService,
                ThrAmount = result.Thr.ThrAmount,
                ProrateRatio = result.Thr.ProrateRatio
            } : null,
            Bpjs = new BpjsCalculationDto
            {
                BpjsKetenagakerjaan = result.Bpjs.BpjsKetenagakerjaan,
                BpjsKesehatan = result.Bpjs.BpjsKesehatan,
                TotalContribution = result.Bpjs.TotalWorkerContribution
            },
            Components = result.ComponentBreakdown.Select(c => new PayrollComponentDto
            {
                Code = c.Code,
                Name = c.Name,
                Amount = c.Amount,
                IsEarning = c.IsEarning,
                Category = c.Category
            }).ToList()
        };
    }

    public async Task<Guid> Handle(CreatePayrollCommand request, CancellationToken ct)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.OrganizationId == request.OrganizationId, ct)
            ?? throw new InvalidOperationException("Employee not found");

        var existingPayroll = await _context.Payrolls
            .AnyAsync(p => p.EmployeeId == request.EmployeeId && p.Year == request.Year && p.Month == request.Month, ct);

        if (existingPayroll)
            throw new InvalidOperationException($"Payroll already exists for {request.Month}/{request.Year}");

        var allowances = request.Allowances.Select(a => new PayrollInputComponent(a.Code, a.Name, a.Amount)).ToList();
        var deductions = request.Deductions.Select(d => new PayrollInputComponent(d.Code, d.Name, d.Amount)).ToList();

        var result = _payrollService.CalculatePayroll(
            request.BasicSalary,
            allowances,
            deductions,
            request.TaxStatus,
            employee.HireDate,
            new DateTime(request.Year, request.Month, 1).AddMonths(1).AddDays(-1),
            request.IncludeThr);

        var payroll = ERP.Domain.Hrm.Entities.Payroll.Create(
            request.OrganizationId,
            request.EmployeeId,
            request.Year,
            request.Month,
            request.BasicSalary,
            result.TotalAllowances,
            result.TotalDeductions);

        payroll.SetMandatoryContributions(
            result.Tax.MonthlyTax,
            result.Bpjs.BpjsKetenagakerjaan,
            result.Bpjs.BpjsKesehatan,
            result.Thr?.ThrAmount ?? 0);

        var details = result.ComponentBreakdown
            .Where(c => c.Amount > 0)
            .Select(c => new ERP.Domain.Hrm.Entities.PayrollDetail(payroll.Id, c.Code, c.Name, c.Amount, c.IsEarning))
            .ToList();

        payroll.SetDetails(details);

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync(ct);

        return payroll.Id;
    }

    public async Task<BatchPayrollResult> Handle(CreateBatchPayrollCommand request, CancellationToken ct)
    {
        var employees = await _context.Employees
            .Where(e => e.OrganizationId == request.OrganizationId && e.Status == EmployeeStatus.Active)
            .Where(e => request.DepartmentId == null || e.DepartmentId == request.DepartmentId)
            .ToListAsync(ct);

        var created = 0;
        var skipped = 0;
        var errors = new List<string>();

        foreach (var employee in employees)
        {
            try
            {
                var existingPayroll = await _context.Payrolls
                    .AnyAsync(p => p.EmployeeId == employee.Id && p.Year == request.Year && p.Month == request.Month, ct);

                if (existingPayroll)
                {
                    skipped++;
                    continue;
                }

                var allowances = new List<PayrollInputComponent>
                {
                    new("TUNJ", "Tunjangan Transport", 500000),
                    new("TUNM", "Tunjangan Makan", 300000)
                };

                var result = _payrollService.CalculatePayroll(
                    employee.BasicSalary,
                    allowances,
                    new List<PayrollInputComponent>(),
                    request.DefaultTaxStatus,
                    employee.HireDate,
                    new DateTime(request.Year, request.Month, 1).AddMonths(1).AddDays(-1),
                    request.IncludeThr);

                var payroll = ERP.Domain.Hrm.Entities.Payroll.Create(
                    request.OrganizationId,
                    employee.Id,
                    request.Year,
                    request.Month,
                    employee.BasicSalary,
                    result.TotalAllowances,
                    result.TotalDeductions);

                payroll.SetMandatoryContributions(
                    result.Tax.MonthlyTax,
                    result.Bpjs.BpjsKetenagakerjaan,
                    result.Bpjs.BpjsKesehatan,
                    result.Thr?.ThrAmount ?? 0);

                _context.Payrolls.Add(payroll);
                created++;
            }
            catch (Exception ex)
            {
                errors.Add($"Employee {employee.EmployeeNumber}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(ct);

        return new BatchPayrollResult(employees.Count, created, skipped, errors);
    }

    public async Task<bool> Handle(ApprovePayrollCommand request, CancellationToken ct)
    {
        var payroll = await _context.Payrolls
            .FirstOrDefaultAsync(p => p.Id == request.PayrollId && p.OrganizationId == request.OrganizationId, ct)
            ?? throw new InvalidOperationException("Payroll not found");

        payroll.MarkAsProcessed();
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Handle(MarkPayrollPaidCommand request, CancellationToken ct)
    {
        var payroll = await _context.Payrolls
            .FirstOrDefaultAsync(p => p.Id == request.PayrollId && p.OrganizationId == request.OrganizationId, ct)
            ?? throw new InvalidOperationException("Payroll not found");

        payroll.MarkAsPaid(request.PaymentDate);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Handle(DeletePayrollCommand request, CancellationToken ct)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.PayrollId && p.OrganizationId == request.OrganizationId, ct)
            ?? throw new InvalidOperationException("Payroll not found");

        if (payroll.Status != PayrollStatus.Draft)
            throw new InvalidOperationException("Only draft payroll can be deleted");

        _context.Payrolls.Remove(payroll);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PaginatedList<PayrollListItemDto>> Handle(GetPayrollListQuery request, CancellationToken ct)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Where(p => p.OrganizationId == request.OrganizationId)
            .Where(p => request.Year == null || p.Year == request.Year)
            .Where(p => request.Month == null || p.Month == request.Month)
            .Where(p => request.EmployeeId == null || p.EmployeeId == request.EmployeeId)
            .Where(p => request.Status == null || p.Status == request.Status);

        var count = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PayrollListItemDto
            {
                Id = p.Id,
                EmployeeNumber = p.Employee.EmployeeNumber,
                EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                DepartmentName = p.Employee.Department != null ? p.Employee.Department.Name : "",
                Year = p.Year,
                Month = p.Month,
                NetSalary = p.NetSalary,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            })
            .ToListAsync(ct);

        return new PaginatedList<PayrollListItemDto>(items, count, request.Page, request.PageSize);
    }

    public async Task<PayrollDto?> Handle(GetPayrollByIdQuery request, CancellationToken ct)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Include(p => p.Details)
            .Where(p => p.Id == request.PayrollId && p.OrganizationId == request.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (payroll == null)
            return null;

        return MapToPayrollDto(payroll);
    }

    public async Task<PayrollSummaryDto?> Handle(GetPayrollSummaryQuery request, CancellationToken ct)
    {
        var payrolls = await _context.Payrolls
            .Where(p => p.OrganizationId == request.OrganizationId && p.Year == request.Year && p.Month == request.Month)
            .ToListAsync(ct);

        if (!payrolls.Any())
            return null;

        return new PayrollSummaryDto
        {
            Year = request.Year,
            Month = request.Month,
            TotalEmployees = payrolls.Count,
            TotalBasicSalary = payrolls.Sum(p => p.BasicSalary),
            TotalAllowances = payrolls.Sum(p => p.TotalAllowances),
            TotalDeductions = payrolls.Sum(p => p.TotalDeductions),
            TotalGrossSalary = payrolls.Sum(p => p.GrossSalary),
            TotalNetSalary = payrolls.Sum(p => p.NetSalary),
            TotalThr = payrolls.Sum(p => p.Thr),
            TotalPPh21 = payrolls.Sum(p => p.PPh21Deduction),
            Status = payrolls.First().Status
        };
    }

    public async Task<PayslipDto?> Handle(GetPayslipQuery request, CancellationToken ct)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
                .ThenInclude(e => e.Department)
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.PayrollId && p.OrganizationId == request.OrganizationId, ct);

        if (payroll == null)
            return null;

        var allowances = payroll.Details.Where(d => d.IsEarning).ToList();
        var deductions = payroll.Details.Where(d => !d.IsEarning).ToList();

        return new PayslipDto
        {
            CompanyName = "NEXTERP Corp",
            EmployeeName = $"{payroll.Employee.FirstName} {payroll.Employee.LastName}".Trim(),
            EmployeeNumber = payroll.Employee.EmployeeNumber,
            Department = payroll.Employee.Department?.Name ?? "",
            Position = "",
            PayPeriodMonth = payroll.Month,
            PayPeriodYear = payroll.Year,
            PayDate = payroll.PaymentDate ?? DateTime.UtcNow,
            BasicSalary = payroll.BasicSalary,
            Allowances = allowances.Select(a => new PayrollComponentDto
            {
                Code = a.ComponentCode,
                Name = a.ComponentName,
                Amount = a.Amount,
                IsEarning = true,
                Category = "ALLOWANCE"
            }).ToList(),
            TotalAllowances = payroll.TotalAllowances,
            GrossSalary = payroll.GrossSalary,
            Deductions = deductions.Select(d => new PayrollComponentDto
            {
                Code = d.ComponentCode,
                Name = d.ComponentName,
                Amount = d.Amount,
                IsEarning = false,
                Category = "DEDUCTION"
            }).ToList(),
            TotalDeductions = payroll.TotalDeductions,
            NetSalary = payroll.NetSalary,
            Tax = new TaxCalculationDto
            {
                MonthlyTax = payroll.PPh21Deduction
            }
        };
    }

    public async Task<PaginatedList<PayrollDto>> Handle(GetEmployeePayrollHistoryQuery request, CancellationToken ct)
    {
        var query = _context.Payrolls
            .Include(p => p.Details)
            .Where(p => p.OrganizationId == request.OrganizationId && p.EmployeeId == request.EmployeeId);

        var count = await query.CountAsync(ct);

        var payrolls = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = payrolls.Select(MapToPayrollDto).ToList();

        return new PaginatedList<PayrollDto>(items, count, request.Page, request.PageSize);
    }

    private static PayrollDto MapToPayrollDto(ERP.Domain.Hrm.Entities.Payroll payroll)
    {
        return new PayrollDto
        {
            Id = payroll.Id,
            OrganizationId = payroll.OrganizationId,
            EmployeeId = payroll.EmployeeId,
            EmployeeNumber = payroll.Employee?.EmployeeNumber ?? "",
            EmployeeName = payroll.Employee != null
                ? $"{payroll.Employee.FirstName} {payroll.Employee.LastName}".Trim()
                : "",
            Year = payroll.Year,
            Month = payroll.Month,
            BasicSalary = payroll.BasicSalary,
            TotalAllowances = payroll.TotalAllowances,
            TotalDeductions = payroll.TotalDeductions,
            PPh21Deduction = payroll.PPh21Deduction,
            BpjsKerjaDeduction = payroll.BpjsKerjaDeduction,
            BpjsKesehatanDeduction = payroll.BpjsKesehatanDeduction,
            ThrAmount = payroll.Thr,
            Status = payroll.Status,
            PaymentDate = payroll.PaymentDate,
            Notes = payroll.Notes,
            Details = payroll.Details.Select(d => new PayrollDetailDto
            {
                Id = d.Id,
                ComponentCode = d.ComponentCode,
                ComponentName = d.ComponentName,
                Amount = d.Amount,
                IsEarning = d.IsEarning
            }).ToList(),
            CreatedAt = payroll.CreatedAt
        };
    }
}
