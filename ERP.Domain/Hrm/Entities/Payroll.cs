using ERP.Domain.Common;
using ERP.Domain.Common.Modules;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.Hrm.Entities;

/// <summary>
/// Payroll record per employee per period.
/// Handles Indonesian salary components: basic salary, allowances, deductions.
/// </summary>
public class Payroll : BaseEntity, ITenantEntity
{
	public Guid OrganizationId { get; private set; }
	public Guid EmployeeId { get; private set; }
	public int Year { get; private set; }
	public int Month { get; private set; }
	public DateTime? PaymentDate { get; private set; }

	// Salary components
	public decimal BasicSalary { get; private set; }
	public decimal TotalAllowances { get; private set; }
	public decimal TotalDeductions { get; private set; }
	public decimal GrossSalary => BasicSalary + TotalAllowances;
	public decimal NetSalary => GrossSalary - TotalDeductions;

	// Tax & mandatory contributions
	public decimal PPh21Deduction { get; private set; }
	public decimal BpjsKerjaDeduction { get; private set; }
	public decimal BpjsKesehatanDeduction { get; private set; }
	public decimal Thr { get; private set; }  // Tunjangan Hari Raya
	public decimal OngkirDeduction { get; private set; }
	public PayrollStatus Status { get; private set; } = PayrollStatus.Draft;
	public string? Notes { get; private set; }

	// Navigation
	private readonly Employee _employee = null!;
	public Employee Employee => _employee;

	private readonly List<PayrollDetail> _details = new();
	public IReadOnlyCollection<PayrollDetail> Details => _details.AsReadOnly();

	private Payroll() { }

	public static Payroll Create(
		Guid organizationId,
		Guid employeeId,
		int year,
		int month,
		decimal basicSalary,
		decimal totalAllowances = 0,
		decimal totalDeductions = 0)
	{
		return new Payroll
		{
			OrganizationId = organizationId,
			EmployeeId = employeeId,
			Year = year,
			Month = month,
			BasicSalary = basicSalary,
			TotalAllowances = totalAllowances,
			TotalDeductions = totalDeductions,
			Status = PayrollStatus.Draft
		};
	}

	public void SetMandatoryContributions(
		decimal pph21,
		decimal bpjsKetenagakerjaan,
		decimal bpjsKesehatan,
		decimal thr = 0)
	{
		PPh21Deduction = pph21;
		BpjsKerjaDeduction = bpjsKetenagakerjaan;
		BpjsKesehatanDeduction = bpjsKesehatan;
		Thr = thr;
	}

	public void MarkAsPaid(DateTime paymentDate)
	{
		if (Status != PayrollStatus.Processed)
			throw new InvalidOperationException("Payroll must be processed before payment");
		Status = PayrollStatus.PaymentPending;
	}

	public void MarkAsProcessed()
	{
		if (Status != PayrollStatus.Draft)
			throw new InvalidOperationException("Only draft payroll can be marked as processed");
		Status = PayrollStatus.Processed;
	}

	public void SetDetails(List<PayrollDetail> details)
	{
		_details.AddRange(details);
	}
}

/// <summary>
/// Detail component for payroll (allowance, deduction, deduction type)
/// </summary>
public class PayrollDetail : BaseEntity
{
	public Guid PayrollId { get; private set; }
	public string ComponentCode { get; private set; } = string.Empty;
	public string ComponentName { get; private set; } = string.Empty;
	public decimal Amount { get; private set; }
	public bool IsEarning { get; private set; }  // true = earning, false = deduction

	private PayrollDetail() { }

	public PayrollDetail(Guid payrollId, string code, string name, decimal amount, bool isEarning)
	{
		PayrollId = payrollId;
		ComponentCode = code;
		ComponentName = name;
		Amount = amount;
		IsEarning = isEarning;
	}
}
