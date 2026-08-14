namespace ERP.Application.Common.Integrations;

/// <summary>
/// Tax reporting service interface for Indonesian e-Billing/e-Filing.
/// </summary>
public interface ITaxReportingService
{
    /// <summary>
    /// Submit PPh 21 return to DJP (Direktorat Jenderal Pajak).
    /// </summary>
    Task<TaxSubmissionResult> SubmitPPh21ReturnAsync(PPh21Submission submission, CancellationToken ct = default);

    /// <summary>
    /// Submit PPN return to DJP.
    /// </summary>
    Task<TaxSubmissionResult> SubmitPpnReturnAsync(PpnSubmission submission, CancellationToken ct = default);

    /// <summary>
    /// Request tax payment code (Kode Billing).
    /// </summary>
    Task<BillingCodeResult> RequestBillingCodeAsync(BillingCodeRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validate NPWP (Tax ID) against DJP database.
    /// </summary>
    Task<NpwpValidationResult> ValidateNpwpAsync(string npwp, CancellationToken ct = default);

    /// <summary>
    /// Download tax certificate (SKET/SPT).
    /// </summary>
    Task<byte[]> DownloadTaxCertificateAsync(string documentId, CancellationToken ct = default);
}

/// <summary>
/// PPh 21 submission data.
/// </summary>
public record PPh21Submission(
    int TaxYear,
    int TaxMonth,
    Guid OrganizationId,
    string NpwpOrg,
    List<PPh21EmployeeSubmission> Employees);

/// <summary>
/// Employee PPh 21 submission data.
/// </summary>
public record PPh21EmployeeSubmission(
    string Npwp,
    string Nik,
    string EmployeeName,
    string Address,
    decimal GrossIncome,
    decimal Deductions,
    decimal NetIncome,
    decimal Ptkp,
    decimal TaxableIncome,
    decimal Pph21Due);

/// <summary>
/// PPN submission data.
/// </summary>
public record PpnSubmission(
    int TaxYear,
    int TaxMonth,
    Guid OrganizationId,
    string NpwpOrg,
    decimal PpnCollected,
    decimal PpnCreditable,
    decimal PpnRemitted);

/// <summary>
/// Tax submission result.
/// </summary>
public record TaxSubmissionResult(
    bool Success,
    string? SubmissionId,
    string? ReferenceNumber,
    DateTime? SubmissionDate,
    string? ErrorMessage);

/// <summary>
/// Billing code request.
/// </summary>
public record BillingCodeRequest(
    Guid OrganizationId,
    string NpwpOrg,
    TaxType TaxType,
    decimal Amount,
    string Description,
    DateTime DueDate);

/// <summary>
/// Tax type enumeration.
/// </summary>
public enum TaxType
{
    PPh21,
    PPh22,
    PPh23,
    PPN,
    PPNBM
}

/// <summary>
/// Billing code result.
/// </summary>
public record BillingCodeResult(
    bool Success,
    string? BillingCode,
    DateTime? ExpiryDate,
    string? ErrorMessage);

/// <summary>
/// NPWP validation result.
/// </summary>
public record NpwpValidationResult(
    bool IsValid,
    string? Npwp,
    string? Name,
    string? Address,
    string? ErrorMessage);
