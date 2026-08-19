using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.API.Controllers.Base;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Reports;
using Asp.Versioning;

namespace ERP.API.Controllers.Common;

/// <summary>
/// Report generation and export endpoints.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IReportService reportService, ICurrentUserService currentUser)
    {
        _reportService = reportService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Generate a report based on type and date range.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
    {
        var result = await _reportService.GenerateReportAsync(request);
        return Success(result);
    }

    /// <summary>
    /// Get attendance report.
    /// </summary>
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? employeeId = null)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var result = await _reportService.GenerateAttendanceReportAsync(
            organizationId, startDate, endDate, employeeId);

        return Success(new
        {
            Metadata = new
            {
                Title = "Attendance Report",
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            },
            Rows = result
        });
    }

    /// <summary>
    /// Get leave report.
    /// </summary>
    [HttpGet("leave")]
    public async Task<IActionResult> GetLeaveReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? employeeId = null)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var result = await _reportService.GenerateLeaveReportAsync(
            organizationId, startDate, endDate, employeeId);

        return Success(result);
    }

    /// <summary>
    /// Get payroll report.
    /// </summary>
    [HttpGet("payroll/{year:int}/{month:int}")]
    public async Task<IActionResult> GetPayrollReport([FromRoute] int year, [FromRoute] int month)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        if (month < 1 || month > 12)
            return Error("Month must be between 1 and 12");

        var result = await _reportService.GeneratePayrollReportAsync(organizationId, year, month);
        return Success(result);
    }

    /// <summary>
    /// Get general ledger report.
    /// </summary>
    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var result = await _reportService.GenerateGeneralLedgerAsync(organizationId, startDate, endDate);
        return Success(result);
    }

    /// <summary>
    /// Get trial balance report.
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime asOfDate)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var result = await _reportService.GenerateTrialBalanceAsync(organizationId, asOfDate);
        return Success(result);
    }

    /// <summary>
    /// Get profit and loss report.
    /// </summary>
    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLoss(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var result = await _reportService.GenerateProfitLossAsync(organizationId, startDate, endDate);
        return Success(result);
    }

    /// <summary>
    /// Export report to CSV.
    /// </summary>
    [HttpPost("export/csv")]
    public async Task<IActionResult> ExportToCsv([FromBody] GenerateReportRequest request)
    {
        var report = await _reportService.GenerateReportAsync(request);
        var bytes = await _reportService.ExportReportAsync(report, ReportExportFormat.Csv);

        return File(bytes, "text/csv", $"{request.Type}.csv");
    }

    /// <summary>
    /// Export report to HTML.
    /// </summary>
    [HttpPost("export/html")]
    public async Task<IActionResult> ExportToHtml([FromBody] GenerateReportRequest request)
    {
        var report = await _reportService.GenerateReportAsync(request);
        var bytes = await _reportService.ExportReportAsync(report, ReportExportFormat.Html);

        return File(bytes, "text/html", $"{request.Type}.html");
    }

    /// <summary>
    /// Export payroll report.
    /// </summary>
    [HttpGet("payroll/{year:int}/{month:int}/export/{format}")]
    public async Task<IActionResult> ExportPayrollReport(
        [FromRoute] int year,
        [FromRoute] int month,
        [FromRoute] string format)
    {
        var organizationId = _currentUser.OrganizationId ?? Guid.Empty;
        if (organizationId == Guid.Empty)
            return Error("Organization not found", 401);

        var report = await _reportService.GeneratePayrollReportAsync(organizationId, year, month);
        var exportFormat = ParseExportFormat(format);
        var bytes = await _reportService.ExportPayrollReportAsync(report, exportFormat);
        var contentType = GetContentType(exportFormat);

        return File(bytes, contentType, $"Payroll_{year}_{month}.{GetExtension(exportFormat)}");
    }

    private static ReportExportFormat ParseExportFormat(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "csv" => ReportExportFormat.Csv,
            "html" => ReportExportFormat.Html,
            "excel" or "xlsx" => ReportExportFormat.Excel,
            "pdf" => ReportExportFormat.Pdf,
            _ => ReportExportFormat.Csv
        };
    }

    private static string GetContentType(ReportExportFormat format)
    {
        return format switch
        {
            ReportExportFormat.Csv => "text/csv",
            ReportExportFormat.Html => "text/html",
            ReportExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ReportExportFormat.Pdf => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private static string GetExtension(ReportExportFormat format)
    {
        return format switch
        {
            ReportExportFormat.Csv => "csv",
            ReportExportFormat.Html => "html",
            ReportExportFormat.Excel => "xlsx",
            ReportExportFormat.Pdf => "pdf",
            _ => "bin"
        };
    }
}
