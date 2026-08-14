using ERP.Application.Common.Reports;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Report service interface for generating various reports.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generate a report based on type and parameters.
    /// </summary>
    Task<ReportResult> GenerateReportAsync(GenerateReportRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generate attendance report for employees.
    /// </summary>
    Task<List<AttendanceReportRow>> GenerateAttendanceReportAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        Guid? employeeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Generate leave report.
    /// </summary>
    Task<ReportResult> GenerateLeaveReportAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        Guid? employeeId = null,
        CancellationToken ct = default);

    /// <summary>
        /// Generate payroll report.
    /// </summary>
    Task<PayrollReportResult> GeneratePayrollReportAsync(
        Guid organizationId,
        int year,
        int month,
        CancellationToken ct = default);

    /// <summary>
    /// Generate general ledger report.
    /// </summary>
    Task<FinancialReportResult> GenerateGeneralLedgerAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);

    /// <summary>
    /// Generate trial balance report.
    /// </summary>
    Task<FinancialReportResult> GenerateTrialBalanceAsync(
        Guid organizationId,
        DateTime asOfDate,
        CancellationToken ct = default);

    /// <summary>
    /// Generate profit and loss report.
    /// </summary>
    Task<FinancialReportResult> GenerateProfitLossAsync(
        Guid organizationId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);

    /// <summary>
    /// Export report to specified format.
    /// </summary>
    Task<byte[]> ExportReportAsync(
        ReportResult report,
        ExportFormat format,
        CancellationToken ct = default);

    /// <summary>
    /// Export payroll report to specified format.
    /// </summary>
    Task<byte[]> ExportPayrollReportAsync(
        PayrollReportResult report,
        ExportFormat format,
        CancellationToken ct = default);

    /// <summary>
    /// Export financial report to specified format.
    /// </summary>
    Task<byte[]> ExportFinancialReportAsync(
        FinancialReportResult report,
        ExportFormat format,
        CancellationToken ct = default);
}
