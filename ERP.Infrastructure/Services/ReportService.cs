using System.Globalization;
using System.Text;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Report service implementation.
/// </summary>
public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<ReportService> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReportResult> GenerateReportAsync(GenerateReportRequest request, CancellationToken ct = default)
    {
        return request.Type switch
        {
            ReportType.EmployeeList => await GenerateEmployeeListReportAsync(request, ct),
            ReportType.AttendanceSummary => await GenerateAttendanceSummaryReportAsync(request, ct),
            ReportType.LeaveReport => await GenerateLeaveReportAsync(
                request.OrganizationId ?? Guid.Empty,
                request.StartDate,
                request.EndDate,
                request.Parameters?.GetValueOrDefault("EmployeeId") as Guid?,
                ct),
            ReportType.PayrollReport => new ReportResult(
                CreateMetadata("Payroll Report", request),
                new List<Dictionary<string, object?>>(),
                new List<string>()),
            ReportType.GeneralLedger => new ReportResult(
                CreateMetadata("General Ledger", request),
                new List<Dictionary<string, object?>>(),
                new List<string>()),
            ReportType.TrialBalance => new ReportResult(
                CreateMetadata("Trial Balance", request),
                new List<Dictionary<string, object?>>(),
                new List<string>()),
            ReportType.ProfitLoss => new ReportResult(
                CreateMetadata("Profit & Loss", request),
                new List<Dictionary<string, object?>>(),
                new List<string>()),
            _ => throw new ArgumentException($"Unknown report type: {request.Type}")
        };
    }

    public async Task<List<AttendanceReportRow>> GenerateAttendanceReportAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        Guid? employeeId = null,
        CancellationToken ct = default)
    {
        var query = _context.Attendances
            .Where(a => a.Date >= startDate && a.Date <= endDate);

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        var attendances = await query.ToListAsync(ct);

        var groupedByEmployee = attendances.GroupBy(a => a.EmployeeId);
        var results = new List<AttendanceReportRow>();

        foreach (var group in groupedByEmployee)
        {
            var employee = await _context.Employees.FindAsync(new object[] { group.Key }, ct);
            if (employee == null) continue;

            var records = group.ToList();
            var presentDays = records.Count(a => a.Status == Domain.Hrm.Enums.AttendanceStatus.Present);
            var absentDays = records.Count(a => a.Status == Domain.Hrm.Enums.AttendanceStatus.Absent);
            var totalHours = records.Sum(a => a.WorkingHours.HasValue ? (decimal)a.WorkingHours.Value.TotalHours : 0);
            var overtimeHours = records.Sum(a => a.OvertimeHours ?? 0);

            results.Add(new AttendanceReportRow
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeNik = employee.EmployeeNumber,
                Department = employee.Department?.Name ?? "N/A",
                TotalDays = records.Count,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LateDays = records.Count(a => a.Status == Domain.Hrm.Enums.AttendanceStatus.Late),
                TotalWorkingHours = totalHours,
                OvertimeHours = overtimeHours
            });
        }

        return results;
    }

    public async Task<ReportResult> GenerateLeaveReportAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        Guid? employeeId = null,
        CancellationToken ct = default)
    {
        var query = _context.LeaveRequests
            .Where(l => l.StartDate >= startDate && l.StartDate <= endDate);

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);

        var leaves = await query
            .Include(l => l.Employee)
            .ThenInclude(e => e!.Department)
            .ToListAsync(ct);

        var rows = leaves.Select(l => new Dictionary<string, object?>
        {
            ["EmployeeId"] = l.EmployeeId,
            ["EmployeeName"] = l.Employee?.FullName ?? "Unknown",
            ["EmployeeNik"] = l.Employee?.EmployeeNumber ?? "N/A",
            ["Department"] = l.Employee?.Department?.Name ?? "N/A",
            ["LeaveType"] = l.LeaveType.ToString(),
            ["StartDate"] = l.StartDate.ToString("yyyy-MM-dd"),
            ["EndDate"] = l.EndDate.ToString("yyyy-MM-dd"),
            ["TotalDays"] = l.TotalDays,
            ["Status"] = l.Status.ToString(),
            ["Reason"] = l.Reason ?? string.Empty
        }).ToList();

        return new ReportResult(
            new ReportMetadata
            {
                Title = "Leave Report",
                Description = $"Leave requests from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                GeneratedAt = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedBy = _currentUser.UserId?.ToString() ?? "System"
            },
            rows,
            new List<string> { "EmployeeId", "EmployeeName", "EmployeeNik", "Department", "LeaveType", "StartDate", "EndDate", "TotalDays", "Status", "Reason" });
    }

    public async Task<PayrollReportResult> GeneratePayrollReportAsync(
        Guid organizationId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var payrolls = await _context.Payrolls
            .Where(p => p.Month == month && p.Year == year)
            .Include(p => p.Employee)
            .ThenInclude(e => e!.Department)
            .ToListAsync(ct);

        var rows = payrolls.Select(p => new PayrollReportRow
        {
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee?.FullName ?? "Unknown",
            EmployeeNik = p.Employee?.EmployeeNumber ?? "N/A",
            Department = p.Employee?.Department?.Name ?? "N/A",
            BasicSalary = p.BasicSalary,
            Allowances = p.TotalAllowances,
            GrossSalary = p.GrossSalary,
            TotalDeductions = p.TotalDeductions,
            NetSalary = p.NetSalary,
            Pph21 = p.PPh21Deduction,
            Jht = p.BpjsKerjaDeduction,
            Jp = 0, // JP is part of BpjsKerjaDeduction in this model
            BpjsKesehatan = p.BpjsKesehatanDeduction
        }).ToList();

        return new PayrollReportResult(
            new ReportMetadata
            {
                Title = $"Payroll Report - {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}",
                Description = $"Monthly payroll for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}",
                GeneratedAt = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedBy = _currentUser.UserId?.ToString() ?? "System"
            },
            rows,
            rows.Sum(r => r.GrossSalary),
            rows.Sum(r => r.TotalDeductions),
            rows.Sum(r => r.NetSalary));
    }

    public async Task<FinancialReportResult> GenerateGeneralLedgerAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        var journals = await _context.JournalEntries
            .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .ToListAsync(ct);

        var grouped = journals.SelectMany(j => j.Lines)
            .GroupBy(l => l.Account?.AccountCode ?? l.AccountId.ToString());

        var rows = new List<FinancialReportRow>();
        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (var group in grouped)
        {
            var debit = group.Sum(l => l.DebitAmount);
            var credit = group.Sum(l => l.CreditAmount);
            var balance = debit - credit;

            rows.Add(new FinancialReportRow
            {
                AccountCode = group.Key,
                AccountName = group.First().Account?.Name ?? "Unknown",
                AccountType = group.First().Account?.Class.ToString() ?? "General",
                Debit = debit,
                Credit = credit,
                Balance = balance
            });

            totalDebit += debit;
            totalCredit += credit;
        }

        return new FinancialReportResult(
            new ReportMetadata
            {
                Title = "General Ledger",
                Description = $"Journal entries from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                GeneratedAt = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedBy = _currentUser.UserId?.ToString() ?? "System"
            },
            rows,
            totalDebit,
            totalCredit,
            totalDebit - totalCredit);
    }

    public async Task<FinancialReportResult> GenerateTrialBalanceAsync(
        Guid organizationId,
        DateTime asOfDate,
        CancellationToken ct = default)
    {
        return await GenerateGeneralLedgerAsync(organizationId, DateTime.MinValue, asOfDate, ct);
    }

    public async Task<FinancialReportResult> GenerateProfitLossAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        return await GenerateGeneralLedgerAsync(organizationId, startDate, endDate, ct);
    }

    public Task<byte[]> ExportReportAsync(ReportResult report, ReportExportFormat format, CancellationToken ct = default)
    {
        return format switch
        {
            ReportExportFormat.Csv => ExportToCsvAsync(report),
            ReportExportFormat.Html => ExportToHtmlAsync(report),
            ReportExportFormat.Excel => ExportToExcelAsync(report),
            ReportExportFormat.Pdf => ExportToPdfAsync(report),
            _ => throw new ArgumentException($"Unknown format: {format}")
        };
    }

    public Task<byte[]> ExportPayrollReportAsync(PayrollReportResult report, ReportExportFormat format, CancellationToken ct = default)
    {
        return format switch
        {
            ReportExportFormat.Csv => ExportPayrollToCsvAsync(report),
            ReportExportFormat.Html => ExportPayrollToHtmlAsync(report),
            ReportExportFormat.Excel => ExportPayrollToExcelAsync(report),
            ReportExportFormat.Pdf => ExportPayrollToPdfAsync(report),
            _ => throw new ArgumentException($"Unknown format: {format}")
        };
    }

    public Task<byte[]> ExportFinancialReportAsync(FinancialReportResult report, ReportExportFormat format, CancellationToken ct = default)
    {
        return format switch
        {
            ReportExportFormat.Csv => ExportFinancialToCsvAsync(report),
            ReportExportFormat.Html => ExportFinancialToHtmlAsync(report),
            ReportExportFormat.Excel => ExportFinancialToExcelAsync(report),
            ReportExportFormat.Pdf => ExportFinancialToPdfAsync(report),
            _ => throw new ArgumentException($"Unknown format: {format}")
        };
    }

    private async Task<ReportResult> GenerateEmployeeListReportAsync(GenerateReportRequest request, CancellationToken ct)
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .ToListAsync(ct);

        var rows = employees.Select(e => new Dictionary<string, object?>
        {
            ["EmployeeNumber"] = e.EmployeeNumber,
            ["Name"] = e.FullName,
            ["Email"] = e.PersonalEmail ?? string.Empty,
            ["Phone"] = e.Phone ?? string.Empty,
            ["Department"] = e.Department?.Name ?? "N/A",
            ["Position"] = e.Position?.Title ?? "N/A",
            ["HireDate"] = e.HireDate.ToString("yyyy-MM-dd"),
            ["Status"] = e.Status.ToString()
        }).ToList();

        return new ReportResult(
            CreateMetadata("Employee List", request),
            rows,
            new List<string> { "EmployeeNumber", "Name", "Email", "Phone", "Department", "Position", "HireDate", "Status" });
    }

    private async Task<ReportResult> GenerateAttendanceSummaryReportAsync(GenerateReportRequest request, CancellationToken ct)
    {
        var attendances = await GenerateAttendanceReportAsync(
            request.OrganizationId ?? Guid.Empty,
            request.StartDate,
            request.EndDate,
            request.Parameters?.GetValueOrDefault("EmployeeId") as Guid?,
            ct);

        var rows = attendances.Select(a => new Dictionary<string, object?>
        {
            ["NIK"] = a.EmployeeNik,
            ["Name"] = a.EmployeeName,
            ["Department"] = a.Department,
            ["TotalDays"] = a.TotalDays,
            ["Present"] = a.PresentDays,
            ["Absent"] = a.AbsentDays,
            ["Late"] = a.LateDays,
            ["WorkingHours"] = a.TotalWorkingHours,
            ["Overtime"] = a.OvertimeHours
        }).ToList();

        return new ReportResult(
            CreateMetadata("Attendance Summary", request),
            rows,
            new List<string> { "NIK", "Name", "Department", "TotalDays", "Present", "Absent", "Late", "WorkingHours", "Overtime" });
    }

    private ReportMetadata CreateMetadata(string title, GenerateReportRequest request)
    {
        return new ReportMetadata
        {
            Title = title,
            Description = $"{title} from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}",
            GeneratedAt = DateTime.UtcNow,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            GeneratedBy = _currentUser.UserId?.ToString() ?? "System"
        };
    }

    private Task<byte[]> ExportToCsvAsync(ReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", report.ColumnNames));

        foreach (var row in report.Rows)
        {
            var values = report.ColumnNames.Select(col =>
            {
                var val = row.GetValueOrDefault(col);
                if (val is decimal d) return d.ToString("F2", CultureInfo.InvariantCulture);
                if (val is DateTime dt) return dt.ToString("yyyy-MM-dd");
                return $"\"{val?.ToString()?.Replace("\"", "\"\"") ?? ""}\"";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> ExportToHtmlAsync(ReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine($"<title>{report.Metadata.Title}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { color: #333; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        sb.AppendLine("th { background-color: #4a90d9; color: white; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
        sb.AppendLine(".meta { color: #666; margin-bottom: 20px; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>{report.Metadata.Title}</h1>");
        sb.AppendLine($"<p class=\"meta\">Generated: {report.Metadata.GeneratedAt:yyyy-MM-dd HH:mm} | Period: {report.Metadata.StartDate:yyyy-MM-dd} to {report.Metadata.EndDate:yyyy-MM-dd}</p>");
        sb.AppendLine("<table><thead><tr>");
        sb.AppendLine(string.Join("", report.ColumnNames.Select(c => $"<th>{c}</th>")));
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var row in report.Rows)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine(string.Join("", report.ColumnNames.Select(c =>
            {
                var val = row.GetValueOrDefault(c);
                return $"<td>{val?.ToString() ?? ""}</td>";
            })));
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> ExportToExcelAsync(ReportResult report)
    {
        return ExportToCsvAsync(report);
    }

    private Task<byte[]> ExportToPdfAsync(ReportResult report)
    {
        return ExportToHtmlAsync(report);
    }

    private Task<byte[]> ExportPayrollToCsvAsync(PayrollReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("NIK,Name,Department,BasicSalary,Allowances,GrossSalary,Deductions,NetSalary,PPh21,JHT,JP,BPJS");

        foreach (var row in report.Rows)
        {
            sb.AppendLine($"{row.EmployeeNik},{row.EmployeeName},{row.Department}," +
                $"{row.BasicSalary:F2},{row.Allowances:F2},{row.GrossSalary:F2}," +
                $"{row.TotalDeductions:F2},{row.NetSalary:F2}," +
                $"{row.Pph21:F2},{row.Jht:F2},{row.Jp:F2},{row.BpjsKesehatan:F2}");
        }

        sb.AppendLine($",,,,,,,,,");
        sb.AppendLine($",,TOTALS,{report.TotalGross:F2},,{report.TotalGross:F2}," +
            $"{report.TotalDeductions:F2},{report.TotalNet:F2},,,,,");

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> ExportPayrollToHtmlAsync(PayrollReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine($"<title>{report.Metadata.Title}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { color: #333; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 6px; text-align: right; }");
        sb.AppendLine("th { background-color: #4a90d9; color: white; text-align: center; }");
        sb.AppendLine("th:first-child, td:first-child { text-align: left; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
        sb.AppendLine("tfoot td { font-weight: bold; background-color: #e0e0e0; }");
        sb.AppendLine(".meta { color: #666; margin-bottom: 20px; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>{report.Metadata.Title}</h1>");
        sb.AppendLine($"<p class=\"meta\">Generated: {report.Metadata.GeneratedAt:yyyy-MM-dd HH:mm} | Period: {report.Metadata.StartDate:yyyy-MM-dd} to {report.Metadata.EndDate:yyyy-MM-dd}</p>");
        sb.AppendLine("<table><thead><tr>");
        sb.AppendLine("<th>NIK</th><th>Name</th><th>Department</th><th>Basic</th><th>Allowances</th><th>Gross</th><th>Deductions</th><th>Net</th><th>PPh21</th><th>JHT</th><th>JP</th><th>BPJS</th>");
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var row in report.Rows)
        {
            sb.AppendLine($"<tr><td>{row.EmployeeNik}</td><td>{row.EmployeeName}</td><td>{row.Department}</td>" +
                $"<td>{row.BasicSalary:N0}</td><td>{row.Allowances:N0}</td><td>{row.GrossSalary:N0}</td>" +
                $"<td>{row.TotalDeductions:N0}</td><td>{row.NetSalary:N0}</td>" +
                $"<td>{row.Pph21:N0}</td><td>{row.Jht:N0}</td><td>{row.Jp:N0}</td><td>{row.BpjsKesehatan:N0}</td></tr>");
        }

        sb.AppendLine("</tbody><tfoot><tr>");
        sb.AppendLine($"<td colspan=\"3\">TOTALS</td><td></td><td></td><td>{report.TotalGross:N0}</td>" +
            $"<td>{report.TotalDeductions:N0}</td><td>{report.TotalNet:N0}</td><td></td><td></td><td></td><td></td>");
        sb.AppendLine("</tr></tfoot></table></body></html>");
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> ExportPayrollToExcelAsync(PayrollReportResult report)
    {
        return ExportPayrollToCsvAsync(report);
    }

    private Task<byte[]> ExportPayrollToPdfAsync(PayrollReportResult report)
    {
        return ExportPayrollToHtmlAsync(report);
    }

    private Task<byte[]> ExportFinancialToCsvAsync(FinancialReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AccountCode,AccountName,AccountType,Debit,Credit,Balance");
        foreach (var row in report.Rows)
        {
            sb.AppendLine($"{row.AccountCode},{row.AccountName},{row.AccountType}," +
                $"{row.Debit:F2},{row.Credit:F2},{row.Balance:F2}");
        }
        sb.AppendLine($",,,,{report.TotalDebit:F2},{report.TotalCredit:F2},");
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> ExportFinancialToHtmlAsync(FinancialReportResult report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine($"<title>{report.Metadata.Title}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { color: #333; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }");
        sb.AppendLine("th { background-color: #4a90d9; color: white; text-align: center; }");
        sb.AppendLine("th:first-child, td:first-child { text-align: left; }");
        sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
        sb.AppendLine(".meta { color: #666; margin-bottom: 20px; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>{report.Metadata.Title}</h1>");
        sb.AppendLine($"<p class=\"meta\">Generated: {report.Metadata.GeneratedAt:yyyy-MM-dd HH:mm}</p>");
        sb.AppendLine("<table><thead><tr>");
        sb.AppendLine("<th>Account</th><th>Name</th><th>Type</th><th>Debit</th><th>Credit</th><th>Balance</th>");
        sb.AppendLine("</tr></thead><tbody>");
        foreach (var row in report.Rows)
        {
            sb.AppendLine($"<tr><td>{row.AccountCode}</td><td>{row.AccountName}</td><td>{row.AccountType}</td>" +
                $"<td>{row.Debit:N2}</td><td>{row.Credit:N2}</td><td>{row.Balance:N2}</td></tr>");
        }
        sb.AppendLine("</tbody></table></body></html>");
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> ExportFinancialToExcelAsync(FinancialReportResult report)
    {
        return ExportFinancialToCsvAsync(report);
    }

    private Task<byte[]> ExportFinancialToPdfAsync(FinancialReportResult report)
    {
        return ExportFinancialToHtmlAsync(report);
    }
}
