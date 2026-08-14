using ERP.Application.Common.Integrations;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of bank transfer service.
/// In production, this would integrate with bank's bulk transfer API.
/// </summary>
public class BankTransferService : IBankTransferService
{
    private readonly ILogger<BankTransferService> _logger;

    public BankTransferService(ILogger<BankTransferService> logger)
    {
        _logger = logger;
    }

    public Task<DisbursementResult> SubmitBulkDisbursementAsync(BulkDisbursementRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Submitting bulk disbursement {BatchId} to {Bank} with {Count} items, total {Amount}",
            request.BatchId, request.BankCode, request.Items.Count, request.Items.Sum(i => i.Amount));

        return Task.FromResult(new DisbursementResult(
            Success: true,
            BatchId: request.BatchId,
            TransactionId: $"TX{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
            TransactionDate: DateTime.UtcNow,
            TotalRecords: request.Items.Count,
            TotalAmount: request.Items.Sum(i => i.Amount),
            ErrorMessage: null));
    }

    public Task<DisbursementStatus> GetDisbursementStatusAsync(string batchId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting disbursement status for batch {BatchId}", batchId);

        return Task.FromResult(new DisbursementStatus(
            BatchId: batchId,
            Status: "PROCESSED",
            SubmittedAt: DateTime.UtcNow.AddHours(-2),
            ProcessedAt: DateTime.UtcNow.AddHours(-1),
            TotalRecords: 10,
            SuccessCount: 9,
            FailedCount: 1,
            Items: new List<DisbursementItemStatus>
            {
                new("1234567890", "SUCCESS", null, DateTime.UtcNow.AddHours(-1)),
                new("0987654321", "FAILED", "Invalid account number", null)
            }));
    }

    public Task<BankAccountValidationResult> ValidateBankAccountAsync(string bankCode, string accountNumber, CancellationToken ct = default)
    {
        _logger.LogInformation("Validating bank account {BankCode}/{AccountNumber}", bankCode, accountNumber);

        // Basic validation
        var isValid = accountNumber.Length >= 8 && accountNumber.All(char.IsDigit);

        return Task.FromResult(new BankAccountValidationResult(
            IsValid: isValid,
            BankCode: bankCode,
            AccountNumber: accountNumber,
            AccountHolderName: isValid ? "John Doe" : null,
            ErrorMessage: isValid ? null : "Invalid account number format"));
    }

    public Task<List<BankInfo>> GetAvailableBanksAsync(CancellationToken ct = default)
    {
        var banks = new List<BankInfo>
        {
            new("002", "Bank BRI", null, true, 10000, 100_000_000_000),
            new("008", "Bank Mandiri", null, true, 10000, 100_000_000_000),
            new("009", "Bank BNP", null, true, 10000, 100_000_000_000),
            new("014", "Bank BCA", null, true, 10000, 100_000_000_000),
            new("200", "Bank BTN", null, false, 50000, 500_000_000_000),
            new("300", "Bank BII", null, false, 25000, 250_000_000_000),
            new("426", "Bank Permata", null, true, 10000, 100_000_000_000),
            new("427", "Bank BCA Syariah", null, true, 10000, 50_000_000_000),
            new("451", "Bank Mandiri Syariah", null, true, 10000, 50_000_000_000),
            new(" SYARIAH", "Bank BRI Syariah", null, true, 10000, 50_000_000_000)
        };

        return Task.FromResult(banks);
    }
}
