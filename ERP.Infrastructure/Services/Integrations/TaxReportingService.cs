using ERP.Application.Common.Integrations;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services.Integrations;

/// <summary>
/// Stub implementation of tax reporting service.
/// In production, this would integrate with DJP e-Billing/e-Filing API.
/// </summary>
public class TaxReportingService : ITaxReportingService
{
    private readonly ILogger<TaxReportingService> _logger;

    public TaxReportingService(ILogger<TaxReportingService> logger)
    {
        _logger = logger;
    }

    public Task<TaxSubmissionResult> SubmitPPh21ReturnAsync(PPh21Submission submission, CancellationToken ct = default)
    {
        _logger.LogInformation("Submitting PPh 21 return for {Year}/{Month} with {Count} employees",
            submission.TaxYear, submission.TaxMonth, submission.Employees.Count);

        var refNum = $"SPT-{submission.TaxYear}{submission.TaxMonth:D2}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        return Task.FromResult(new TaxSubmissionResult(
            Success: true,
            SubmissionId: Guid.NewGuid().ToString(),
            ReferenceNumber: refNum,
            SubmissionDate: DateTime.UtcNow,
            ErrorMessage: null));
    }

    public Task<TaxSubmissionResult> SubmitPpnReturnAsync(PpnSubmission submission, CancellationToken ct = default)
    {
        _logger.LogInformation("Submitting PPN return for {Year}/{Month}", submission.TaxYear, submission.TaxMonth);

        var refNum = $"SPT-PPN-{submission.TaxYear}{submission.TaxMonth:D2}";
        return Task.FromResult(new TaxSubmissionResult(
            Success: true,
            SubmissionId: Guid.NewGuid().ToString(),
            ReferenceNumber: refNum,
            SubmissionDate: DateTime.UtcNow,
            ErrorMessage: null));
    }

    public Task<BillingCodeResult> RequestBillingCodeAsync(BillingCodeRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Requesting billing code for {TaxType} amount {Amount}", request.TaxType, request.Amount);

        var random = new Random();
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
        var billingCode = $"0{random.Next(100000, 999999)}{dateStr}01";

        return Task.FromResult(new BillingCodeResult(
            Success: true,
            BillingCode: billingCode,
            ExpiryDate: DateTime.UtcNow.AddDays(30),
            ErrorMessage: null));
    }

    public Task<NpwpValidationResult> ValidateNpwpAsync(string npwp, CancellationToken ct = default)
    {
        _logger.LogInformation("Validating NPWP: {Npwp}", npwp);

        var cleanNpwp = npwp.Replace(".", "").Replace("-", "");
        var isValid = cleanNpwp.Length == 15 && cleanNpwp.All(char.IsDigit);

        return Task.FromResult(new NpwpValidationResult(
            IsValid: isValid,
            Npwp: isValid ? FormatNpwp(cleanNpwp) : null,
            Name: isValid ? "PT Contoh Indonesia" : null,
            Address: isValid ? "Jl. Contoh No. 123, Jakarta" : null,
            ErrorMessage: isValid ? null : "NPWP format is invalid"));
    }

    public Task<byte[]> DownloadTaxCertificateAsync(string documentId, CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading tax certificate: {DocumentId}", documentId);
        return Task.FromResult(Array.Empty<byte>());
    }

    private static string FormatNpwp(string npwp)
    {
        if (npwp.Length != 15) return npwp;
        return npwp[..2] + "." + npwp[2..4] + "." + npwp[4..6] + "." + npwp[6..9] + "-" + npwp[9..12] + "." + npwp[12..15];
    }
}
