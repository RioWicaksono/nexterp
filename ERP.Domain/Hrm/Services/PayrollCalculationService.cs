using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.Hrm.Services;

/// <summary>
/// Result of PPh 21 (income tax) calculation.
/// </summary>
public record TaxCalculationResult(
    decimal GrossAnnualIncome,
    decimal TotalDeductions,
    decimal NetAnnualIncome,
    decimal TaxableIncome,
    decimal AnnualTax,
    decimal MonthlyTax,
    decimal PtkpAmount,
    TaxStatus TaxStatus);

/// <summary>
/// Result of THR (Tunjangan Hari Raya) calculation.
/// </summary>
public record ThrCalculationResult(
    decimal BasicSalary,
    int MonthsOfService,
    decimal YearsOfService,
    decimal ThrAmount,
    decimal ProrateRatio);

/// <summary>
/// Result of BPJS contribution calculation.
/// </summary>
public record BpjsCalculationResult(
    decimal BpjsKetenagakerjaan,
    decimal BpjsKesehatan,
    decimal TotalWorkerContribution,
    decimal BpjsKetenagakerjaanSalaryBase,
    decimal BpjsKesehatanSalaryBase);

/// <summary>
/// Complete payroll calculation result.
/// </summary>
public record PayrollCalculationResult(
    decimal BasicSalary,
    decimal GrossSalary,
    decimal NetSalary,
    TaxCalculationResult Tax,
    ThrCalculationResult? Thr,
    BpjsCalculationResult Bpjs,
    decimal TotalAllowances,
    decimal TotalDeductions,
    List<PayrollComponentBreakdown> ComponentBreakdown);

/// <summary>
/// Breakdown of individual payroll component.
/// </summary>
public record PayrollComponentBreakdown(
    string Code,
    string Name,
    decimal Amount,
    bool IsEarning,
    string Category);

/// <summary>
/// Domain service for Indonesian payroll calculations.
/// Implements UU Ketenagakerjaan and Tax Regulation compliance.
/// </summary>
public class PayrollCalculationService
{
    /// <summary>
    /// Calculate annual PPh 21 (income tax) using PTKP method.
    /// Indonesian tax calculation per Dirjen Pajak Regulation.
    /// </summary>
    public TaxCalculationResult CalculatePPh21(
        decimal monthlyGrossSalary,
        decimal annualAllowances,
        int monthsEmployed,
        TaxStatus taxStatus,
        decimal previousYearTax = 0)
    {
        // Get PTKP based on tax status
        var ptkp = GetPtkpAmount(taxStatus);

        // Annualize gross income
        var grossAnnualIncome = (monthlyGrossSalary * monthsEmployed) + annualAllowances;

        // Calculate deductions
        var biayaJabatan = Math.Min(grossAnnualIncome * PayrollConstants.BiayaJabatanRate,
                                     PayrollConstants.BiayaJabatanMax);
        var totalDeductions = biayaJabatan + ptkp;

        // Calculate taxable income
        var netAnnualIncome = grossAnnualIncome - biayaJabatan;
        var taxableIncome = Math.Max(0, netAnnualIncome - ptkp);

        // Calculate progressive tax
        var annualTax = CalculateProgressiveTax(taxableIncome);

        // Monthly tax
        var monthlyTax = Math.Round(annualTax / 12, 0);

        return new TaxCalculationResult(
            grossAnnualIncome,
            totalDeductions,
            netAnnualIncome,
            taxableIncome,
            annualTax,
            monthlyTax,
            ptkp,
            taxStatus);
    }

    /// <summary>
    /// Calculate THR (Tunjangan Hari Raya) based on months of service.
    /// </summary>
    public ThrCalculationResult CalculateThr(
        decimal basicSalary,
        DateTime hireDate,
        DateTime calculationDate)
    {
        var monthsOfService = CalculateMonthsOfService(hireDate, calculationDate);

        if (monthsOfService < PayrollConstants.ThrMinimumMonthsService)
        {
            // Not eligible for THR yet - return 0
            return new ThrCalculationResult(
                basicSalary,
                monthsOfService,
                monthsOfService / 12m,
                0,
                0);
        }

        decimal thrAmount;
        decimal prorateRatio;

        if (monthsOfService >= 12)
        {
            // Full THR (1 month salary)
            thrAmount = basicSalary;
            prorateRatio = 1.0m;
        }
        else
        {
            // Partial THR (months / 12)
            prorateRatio = (decimal)monthsOfService / 12m;
            thrAmount = Math.Round(basicSalary * prorateRatio, 0);
        }

        return new ThrCalculationResult(
            basicSalary,
            monthsOfService,
            monthsOfService / 12m,
            thrAmount,
            prorateRatio);
    }

    /// <summary>
    /// Calculate THR with specific months of service.
    /// </summary>
    public ThrCalculationResult CalculateThrByMonths(
        decimal basicSalary,
        int monthsOfService)
    {
        if (monthsOfService < PayrollConstants.ThrMinimumMonthsService)
        {
            return new ThrCalculationResult(
                basicSalary,
                monthsOfService,
                monthsOfService / 12m,
                0,
                0);
        }

        decimal thrAmount;
        decimal prorateRatio;

        if (monthsOfService >= 12)
        {
            thrAmount = basicSalary;
            prorateRatio = 1.0m;
        }
        else
        {
            prorateRatio = (decimal)monthsOfService / 12m;
            thrAmount = Math.Round(basicSalary * prorateRatio, 0);
        }

        return new ThrCalculationResult(
            basicSalary,
            monthsOfService,
            monthsOfService / 12m,
            thrAmount,
            prorateRatio);
    }

    /// <summary>
    /// Calculate BPJS Kesehatan contribution.
    /// </summary>
    public decimal CalculateBpjsKesehatan(decimal monthlySalary)
    {
        var salaryBase = Math.Min(monthlySalary, PayrollConstants.BpjsKesehatanMaxMonthly);
        return Math.Round(salaryBase * PayrollConstants.BpjsKesehatanWorkerRate, 0);
    }

    /// <summary>
    /// Calculate BPJS Ketenagakerjaan (JHT, JKK, JKM) contributions.
    /// </summary>
    public BpjsCalculationResult CalculateBpjsKetenagakerjaan(decimal monthlySalary)
    {
        var salaryBase = Math.Min(monthlySalary, PayrollConstants.BpjsKetenagakerjaanMaxMonthly);

        // Total employer + worker = 9% (worker: 2%, employer: 3.7% JHT + 0.3% JKK + 0.3% JKM + 2% JP)
        var workerContribution = Math.Round(salaryBase * PayrollConstants.BpjsKetenagakerjaanWorkerRate, 0);

        return new BpjsCalculationResult(
            workerContribution,
            0,  // BpjsKesehatan calculated separately
            workerContribution,
            salaryBase,
            0);
    }

    /// <summary>
    /// Calculate complete payroll including all components.
    /// </summary>
    public PayrollCalculationResult CalculatePayroll(
        decimal basicSalary,
        List<PayrollInputComponent> allowances,
        List<PayrollInputComponent> deductions,
        TaxStatus taxStatus,
        DateTime hireDate,
        DateTime calculationDate,
        bool includeThr = false)
    {
        var breakdown = new List<PayrollComponentBreakdown>();

        // Add basic salary
        breakdown.Add(new PayrollComponentBreakdown(
            "BASIC", "Gaji Pokok", basicSalary, true, "SALARY"));

        decimal totalAllowances = 0;

        // Add allowances
        foreach (var allowance in allowances)
        {
            breakdown.Add(new PayrollComponentBreakdown(
                allowance.Code, allowance.Name, allowance.Amount, true, "ALLOWANCE"));
            totalAllowances += allowance.Amount;
        }

        // Calculate gross
        var grossSalary = basicSalary + totalAllowances;

        // Calculate THR if requested
        ThrCalculationResult? thrResult = null;
        decimal thrAmount = 0;
        if (includeThr)
        {
            thrResult = CalculateThr(basicSalary, hireDate, calculationDate);
            thrAmount = thrResult.ThrAmount;
            if (thrAmount > 0)
            {
                breakdown.Add(new PayrollComponentBreakdown(
                    "THR", "Tunjangan Hari Raya", thrAmount, true, "THR"));
                totalAllowances += thrAmount;
            }
        }

        // Calculate BPJS Kesehatan
        var bpjsKesehatan = CalculateBpjsKesehatan(basicSalary);
        breakdown.Add(new PayrollComponentBreakdown(
            "BPJSKS", "BPJS Kesehatan", bpjsKesehatan, false, "BPJS"));

        // Calculate BPJS Ketenagakerjaan
        var bpjsKetenagakerjaan = CalculateBpjsKetenagakerjaan(basicSalary);
        breakdown.Add(new PayrollComponentBreakdown(
            "BPJSKT", "BPJS Ketenagakerjaan", bpjsKetenagakerjaan.BpjsKetenagakerjaan, false, "BPJS"));

        // Calculate PPh 21
        var taxResult = CalculatePPh21(
            basicSalary + totalAllowances,
            0,  // annualAllowances
            12, // monthsEmployed (assume full year)
            taxStatus);

        breakdown.Add(new PayrollComponentBreakdown(
            "PPH21", "PPh 21", taxResult.MonthlyTax, false, "TAX"));

        // Add deductions
        decimal totalDeductions = bpjsKesehatan + bpjsKetenagakerjaan.BpjsKetenagakerjaan + taxResult.MonthlyTax;
        foreach (var deduction in deductions)
        {
            breakdown.Add(new PayrollComponentBreakdown(
                deduction.Code, deduction.Name, deduction.Amount, false, "DEDUCTION"));
            totalDeductions += deduction.Amount;
        }

        // Calculate net
        var netSalary = grossSalary - totalDeductions;

        return new PayrollCalculationResult(
            basicSalary,
            grossSalary,
            netSalary,
            taxResult,
            thrResult,
            new BpjsCalculationResult(
                bpjsKetenagakerjaan.BpjsKetenagakerjaan,
                bpjsKesehatan,
                bpjsKetenagakerjaan.BpjsKetenagakerjaan + bpjsKesehatan,
                bpjsKetenagakerjaan.BpjsKetenagakerjaanSalaryBase,
                0),
            totalAllowances,
            totalDeductions,
            breakdown);
    }

    /// <summary>
    /// Get PTKP amount based on tax status.
    /// </summary>
    private static decimal GetPtkpAmount(TaxStatus status) => status switch
    {
        TaxStatus.TK0 => PayrollConstants.PtkpTk0,
        TaxStatus.TK1 => PayrollConstants.PtkpTk1,
        TaxStatus.TK2 => PayrollConstants.PtkpTk0 + PayrollConstants.AdditionalPtkpPerFamily * 2,
        TaxStatus.TK3 => PayrollConstants.PtkpTk0 + PayrollConstants.AdditionalPtkpPerFamily * 3,
        TaxStatus.K0 => PayrollConstants.PtkpK0,
        TaxStatus.K1 => PayrollConstants.PtkpK1,
        TaxStatus.K2 => PayrollConstants.PtkpK2,
        TaxStatus.K3 => PayrollConstants.PtkpK3,
        _ => PayrollConstants.PtkpTk0
    };

    /// <summary>
    /// Calculate progressive tax based on tax brackets.
    /// </summary>
    private static decimal CalculateProgressiveTax(decimal taxableIncome)
    {
        if (taxableIncome <= 0) return 0;

        decimal tax = 0;
        var remainingIncome = taxableIncome;
        decimal previousThreshold = 0;

        foreach (var (threshold, rate, _) in PayrollConstants.TaxBrackets2024)
        {
            if (remainingIncome <= 0) break;

            var bracketIncome = Math.Min(remainingIncome, threshold - previousThreshold);
            if (bracketIncome > 0)
            {
                tax += bracketIncome * rate;
                remainingIncome -= bracketIncome;
            }

            previousThreshold = threshold;
        }

        return tax;
    }

    /// <summary>
    /// Calculate months of service between two dates.
    /// </summary>
    private static int CalculateMonthsOfService(DateTime hireDate, DateTime endDate)
    {
        var years = endDate.Year - hireDate.Year;
        var months = endDate.Month - hireDate.Month;

        var totalMonths = (years * 12) + months;

        // Add partial month if end day > hire day
        if (endDate.Day > hireDate.Day)
            totalMonths++;

        return Math.Max(0, totalMonths);
    }
}

/// <summary>
/// Input component for payroll calculation.
/// </summary>
public record PayrollInputComponent(string Code, string Name, decimal Amount);
