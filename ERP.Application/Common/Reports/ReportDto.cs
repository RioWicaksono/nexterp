namespace ERP.Application.Common.Reports;

/// <summary>
/// Report generation request.
/// </summary>
public record GenerateReportRequest(
    ReportType Type,
    DateTime StartDate,
    DateTime EndDate,
    Guid? OrganizationId = null,
    Guid? EmployeeId = null,
    Dictionary<string, object>? Parameters = null);

/// <summary>
/// Report export format.
/// </summary>
public enum ReportExportFormat
{
    Pdf,
    Excel,
    Csv,
    Html
}

/// <summary>
/// Report type enumeration.
/// </summary>
public enum ReportType
{
    EmployeeList,
    AttendanceSummary,
    LeaveReport,
    PayrollReport,
    GeneralLedger,
    TrialBalance,
    ProfitLoss,
    InventorySummary,
    SalesSummary,
    PurchaseSummary
}

/// <summary>
/// Report metadata.
/// </summary>
public record ReportMetadata
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string GeneratedBy { get; init; } = string.Empty;
    public Dictionary<string, object> CustomFields { get; init; } = new();
}

/// <summary>
/// Generic report result.
/// </summary>
public record ReportResult(
    ReportMetadata Metadata,
    List<Dictionary<string, object?>> Rows,
    List<string> ColumnNames,
    List<string>? GroupHeaders = null);

/// <summary>
/// Payroll report specific result.
/// </summary>
public record PayrollReportResult(
    ReportMetadata Metadata,
    List<PayrollReportRow> Rows,
    decimal TotalGross,
    decimal TotalDeductions,
    decimal TotalNet);

/// <summary>
/// Payroll report row.
/// </summary>
public record PayrollReportRow
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeNik { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public decimal BasicSalary { get; init; }
    public decimal Allowances { get; init; }
    public decimal GrossSalary { get; init; }
    public decimal TotalDeductions { get; init; }
    public decimal NetSalary { get; init; }
    public decimal Pph21 { get; init; }
    public decimal Jht { get; init; }
    public decimal Jp { get; init; }
    public decimal BpjsKesehatan { get; init; }
}

/// <summary>
/// Attendance report row.
/// </summary>
public record AttendanceReportRow
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeNik { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public int TotalDays { get; init; }
    public int PresentDays { get; init; }
    public int AbsentDays { get; init; }
    public int LateDays { get; init; }
    public decimal TotalWorkingHours { get; init; }
    public decimal OvertimeHours { get; init; }
}

/// <summary>
/// Financial report result.
/// </summary>
public record FinancialReportResult(
    ReportMetadata Metadata,
    List<FinancialReportRow> Rows,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Balance);

/// <summary>
/// Financial report row.
/// </summary>
public record FinancialReportRow
{
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal Balance { get; init; }
}
