namespace ERP.Domain.Hrm.Services;

/// <summary>
/// Indonesian payroll calculation constants and rates.
/// Updated per UU Ketenagakerjaan and Tax Regulation.
/// </summary>
public static class PayrollConstants
{
    // BPJS Ketenagakerjaan rates (worker portion, 2024)
    public const decimal BpjsKetenagakerjaanWorkerRate = 0.02m;  // 2% of monthly salary, max 1 salary
    public const decimal BpjsKetenagakerjaanMaxMonthly = 10_000_000m;  // Max salary base for JKK, JKM, JHT

    // BPJS Kesehatan rates (worker portion, 2024)
    public const decimal BpjsKesehatanWorkerRate = 0.01m;  // 1% of monthly salary
    public const decimal BpjsKesehatanMaxMonthly = 12_000_000m;  // Max salary base

    // THR (Tunjangan Hari Raya) - minimum 1 month salary after 12 months
    public const int ThrMinimumMonthsService = 12;
    public const decimal ThrRatioFullYear = 1.0m;  // 1 month salary for full year
    public const decimal ThrRatioPartialMonth = 1m / 12m;  // 1/12 per month for partial year

    // Tax brackets (PTKP 2024) - for unmarried (TK/0)
    public static readonly (decimal Threshold, decimal Rate, decimal FixedDeduction)[] TaxBrackets2024 =
    {
        (60_000_000m, 0.05m, 0),       // 0 - 60M: 5%
        (250_000_000m, 0.15m, 0),      // 60M - 250M: 15%
        (500_000_000m, 0.25m, 0),      // 250M - 500M: 25%
        (5_000_000_000m, 0.30m, 0),    // 500M - 5B: 30%
        (decimal.MaxValue, 0.35m, 0)   // > 5B: 35%
    };

    // PTKP (Penghasilan Tidak Kena Pajak) 2024
    public const decimal PtkpTk0 = 54_000_000m;        // Tidak Kawin tanpa tanggungan
    public const decimal PtkpTk1 = 58_500_000m;        // TK1
    public const decimal PtkpK0 = 58_500_000m;         // Kawin tanpa tanggungan
    public const decimal PtkpK1 = 63_000_000m;        // K1
    public const decimal PtkpK2 = 67_500_000m;        // K2
    public const decimal PtkpK3 = 72_000_000m;        // K3
    public const decimal AdditionalPtkpPerFamily = 4_500_000m;

    // Deduction rates for annualized calculation
    public const decimal BiayaJabatanRate = 0.05m;    // 5% of gross income
    public const decimal BiayaJabatanMax = 6_000_000m;  // Max 500k/month or 6M/year

    // Minimum salary (UMR) - default, should be configured per region
    public const decimal DefaultMinimumSalary = 5_000_000m;
}

/// <summary>
/// PPh 21 tax status based on marital status and number of dependents.
/// </summary>
public enum TaxStatus
{
    TK0,  // Tidak Kawin, tanpa tanggungan
    TK1,  // Tidak Kawin, 1 tanggungan
    TK2,  // Tidak Kawin, 2 tanggungan
    TK3,  // Tidak Kawin, 3 tanggungan
    K0,   // Kawin, tanpa tanggungan
    K1,   // Kawin, 1 tanggungan
    K2,   // Kawin, 2 tanggungan
    K3    // Kawin, 3+ tanggungan
}
