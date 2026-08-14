namespace ERP.Application.Common.Documents;

/// <summary>
/// Document template service interface.
/// </summary>
public interface IDocumentTemplateService
{
    /// <summary>
    /// Generate payslip document.
    /// </summary>
    Task<byte[]> GeneratePayslipAsync(PayslipTemplateData data, CancellationToken ct = default);

    /// <summary>
    /// Generate leave approval letter.
    /// </summary>
    Task<byte[]> GenerateLeaveApprovalLetterAsync(LeaveLetterData data, CancellationToken ct = default);

    /// <summary>
    /// Generate employment contract.
    /// </summary>
    Task<byte[]> GenerateEmploymentContractAsync(EmploymentContractData data, CancellationToken ct = default);

    /// <summary>
    /// Generate certificate of employment (SKK).
    /// </summary>
    Task<byte[]> GenerateCertificateOfEmploymentAsync(CertificateOfEmploymentData data, CancellationToken ct = default);

    /// <summary>
    /// Generate tax certificate (SPT/1721).
    /// </summary>
    Task<byte[]> GenerateTaxCertificateAsync(TaxCertificateData data, CancellationToken ct = default);

    /// <summary>
    /// Generate custom document from template.
    /// </summary>
    Task<byte[]> GenerateFromTemplateAsync(string templateId, Dictionary<string, object> data, CancellationToken ct = default);
}

/// <summary>
/// Payslip template data.
/// </summary>
public record PayslipTemplateData(
    string EmployeeName,
    string EmployeeNik,
    string Department,
    string Position,
    int Year,
    int Month,
    decimal BasicSalary,
    decimal TotalEarnings,
    decimal TotalDeductions,
    decimal NetSalary,
    List<PayslipItem> Earnings,
    List<PayslipItem> Deductions,
    DateTime PaymentDate,
    string BankAccount,
    string CompanyName,
    string CompanyAddress);

/// <summary>
/// Payslip item.
/// </summary>
public record PayslipItem(string Code, string Name, decimal Amount);

/// <summary>
/// Leave letter data.
/// </summary>
public record LeaveLetterData(
    string EmployeeName,
    string EmployeeNik,
    string Department,
    string LeaveType,
    DateTime StartDate,
    DateTime EndDate,
    int TotalDays,
    string Reason,
    DateTime ApprovedDate,
    string ApprovedByName,
    string CompanyName,
    string CompanyAddress);

/// <summary>
/// Employment contract data.
/// </summary>
public record EmploymentContractData(
    string CompanyName,
    string CompanyAddress,
    string CompanyNpwp,
    string EmployeeName,
    string EmployeeNik,
    string EmployeeAddress,
    string EmployeeNpwp,
    string Position,
    DateTime ContractStartDate,
    DateTime ContractEndDate,
    string EmploymentType,
    decimal BasicSalary,
    string WorkSchedule,
    DateTime SignedDate);

/// <summary>
/// Certificate of employment data.
/// </summary>
public record CertificateOfEmploymentData(
    string CompanyName,
    string CompanyAddress,
    string CompanyLetterhead,
    string EmployeeName,
    string EmployeeNik,
    string Department,
    string Position,
    DateTime JoinDate,
    DateTime? EndDate,
    DateTime IssueDate,
    string SignatoryName,
    string SignatoryPosition);

/// <summary>
/// Tax certificate data.
/// </summary>
public record TaxCertificateData(
    string CompanyName,
    string CompanyNpwp,
    string CompanyAddress,
    string EmployeeName,
    string EmployeeNik,
    string EmployeeNpwp,
    int TaxYear,
    decimal GrossIncome,
    decimal TotalDeductions,
    decimal Ptkp,
    decimal TaxableIncome,
    decimal Pph21Annual,
    decimal Pph21Monthly,
    DateTime IssueDate,
    string TaxOffice);
