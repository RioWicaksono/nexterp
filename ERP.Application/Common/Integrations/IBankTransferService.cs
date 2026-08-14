namespace ERP.Application.Common.Integrations;

/// <summary>
/// Bank transfer/disbursement service interface.
/// </summary>
public interface IBankTransferService
{
    /// <summary>
    /// Submit bulk salary disbursement to bank.
    /// </summary>
    Task<DisbursementResult> SubmitBulkDisbursementAsync(BulkDisbursementRequest request, CancellationToken ct = default);

    /// <summary>
    /// Get disbursement status.
    /// </summary>
    Task<DisbursementStatus> GetDisbursementStatusAsync(string batchId, CancellationToken ct = default);

    /// <summary>
    /// Validate bank account (account number + bank code).
    /// </summary>
    Task<BankAccountValidationResult> ValidateBankAccountAsync(string bankCode, string accountNumber, CancellationToken ct = default);

    /// <summary>
    /// Get bank code list.
    /// </summary>
    Task<List<BankInfo>> GetAvailableBanksAsync(CancellationToken ct = default);
}

/// <summary>
/// Bulk disbursement request.
/// </summary>
public record BulkDisbursementRequest(
    Guid OrganizationId,
    string BatchId,
    string BankCode,
    string SourceAccountNumber,
    List<DisbursementItem> Items,
    string? Description = null);

/// <summary>
/// Individual disbursement item.
/// </summary>
public record DisbursementItem(
    string BeneficiaryAccountNumber,
    string BeneficiaryName,
    decimal Amount,
    string? Reference = null,
    string? Email = null);

/// <summary>
/// Disbursement result.
/// </summary>
public record DisbursementResult(
    bool Success,
    string? BatchId,
    string? TransactionId,
    DateTime? TransactionDate,
    int TotalRecords,
    decimal TotalAmount,
    string? ErrorMessage);

/// <summary>
/// Disbursement status.
/// </summary>
public record DisbursementStatus(
    string BatchId,
    string Status,
    DateTime SubmittedAt,
    DateTime? ProcessedAt,
    int TotalRecords,
    int SuccessCount,
    int FailedCount,
    List<DisbursementItemStatus> Items);

/// <summary>
/// Individual disbursement item status.
/// </summary>
public record DisbursementItemStatus(
    string BeneficiaryAccountNumber,
    string Status,
    string? ErrorMessage,
    DateTime? ProcessedAt);

/// <summary>
/// Bank account validation result.
/// </summary>
public record BankAccountValidationResult(
    bool IsValid,
    string? BankCode,
    string? AccountNumber,
    string? AccountHolderName,
    string? ErrorMessage);

/// <summary>
/// Bank information.
/// </summary>
public record BankInfo(
    string Code,
    string Name,
    string? LogoUrl,
    bool SupportsRealTimeTransfer,
    decimal MinAmount,
    decimal MaxAmount);
