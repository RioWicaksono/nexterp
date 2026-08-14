namespace ERP.Domain.Hrm.Enums;

/// <summary>
/// Employment types (Indonesian labor law compliant)
/// </summary>
public enum EmploymentType
{
	FullTime = 1,
	PartTime = 2,
	Contract = 3,
	Probation = 4,
	Intern = 5,
	Freelance = 6,
	PKWT = 7,  // Perjanjian Kerja Waktu Tertentu
	PKWTT = 8   // Perjanjian Kerja Waktu Tidak Tertentu
}

/// <summary>
/// Employee status
/// </summary>
public enum EmployeeStatus
{
	Active = 1,
	OnLeave = 2,
	Suspended = 3,
	Terminated = 4,
	Resigned = 5,
	/// <summary>
/// Completed probation period
/// </summary>
	Confirmed = 6
}

/// <summary>
/// Gender
/// </summary>
public enum Gender
{
	Male = 1,
	Female = 2,
	Other = 3
}

/// <summary>
/// Marital status
/// </summary>
public enum MaritalStatus
{
	Single = 1,
	Married = 2,
	Divorced = 3,
	Widowed = 4
}

/// <summary>
/// Leave types (Indonesian regulations compliant)
/// </summary>
public enum LeaveType
{
	/// <summary>
/// Cuti tahunan - Annual leave (min 12 working days per year)
/// </summary>
	Annual = 1,
	/// <summary>
/// Cuti sakit - Sick leave
/// </summary>
	Sick = 2,
	/// <summary>
/// Cuti darurat - Emergency leave
/// </summary>
	Emergency = 3,
	/// <summary>
/// Cuti melahirkan - Maternity leave (3 months paid)
/// </summary>
	Maternity = 4,
	/// <summary>
/// Cuti ayah - Paternity leave
/// </summary>
	Paternity = 5,
	/// <summary>
/// Cuti tanpa cuti - Unpaid leave
/// </summary>
	Unpaid = 6,
	/// <summary>
/// Cuti besar - Long leave (can be accumulated)
/// </summary>
	LongLeave = 7,
	/// <summary>
/// Cuti sakit khusus COVID-19 or pandemic leave
/// </summary>
	Pandemic = 8,
	/// <summary>
/// Other leave types
/// </summary>
	Other = 9
}

/// <summary>
/// Leave request status
/// </summary>
public enum LeaveStatus
{
	Pending = 1,
	Approved = 2,
	Rejected = 3,
	Cancelled = 4
}

/// <summary>
/// Attendance status
/// </summary>
public enum AttendanceStatus
{
	Present = 1,
	Absent = 2,
	Late = 3,
	OnLeave = 4,
	Holiday = 5,
	Remote = 6,       // WFH
	BusinessTrip = 7    // Dinas luar
}

/// <summary>
/// Overtime request status
/// </summary>
public enum OvertimeStatus
{
	Pending = 1,
	Approved = 2,
	Rejected = 3,
	Cancelled = 4
}

/// <summary>
/// Overtime type (Indonesian labor law compliant)
/// </summary>
public enum OvertimeType
{
	/// <summary>
/// Lembur hari kerja biasa
/// </summary>
	WeekdayOvertime = 1,
	/// <summary>
/// Lembur hari libur (Sabtu/Minggu)
/// </summary>
	WeekendOvertime = 2,
	/// <summary>
/// Lembur hari libur nasional
/// </summary>
	HolidayOvertime = 3,
	/// <summary>
/// Lembur lembur hari kerja (max 4 hours/day, 18 hours/week)
/// </summary>
	DailyOvertime = 4
}

/// <summary>
/// Payroll status
/// </summary>
public enum PayrollStatus
{
	Draft = 1,
	/// <summary>
/// Processed but not yet paid
/// </summary>
	Processed = 2,
	/// <summary>
/// Payment in progress
/// </summary>
	PaymentPending = 3,
	/// <summary>
/// Successfully paid
/// </summary>
	Paid = 4,
	/// <summary>
/// Payment failed
/// </summary>
	PaymentFailed = 5
}

/// <summary>
/// Tax filing status
/// </summary>
public enum TaxFilingStatus
{
	Draft = 1,
	/// <summary>
/// Filed to tax office
/// </summary>
	Filed = 2,
	/// <summary>
/// Accepted by tax office
/// </summary>
	Accepted = 3,
	/// <summary>
/// Rejected, requires correction
/// </summary>
	Rejected = 4
}

/// <summary>
/// BPJSTk classification
/// </summary>
public enum JamsostekClass
{
	/// <summary>
/// Tenaga Kerja Asing
/// </summary>
	TKA = 1,
	/// <summary>
/// Buruh Tetap
/// </summary>
	BuruhTetap = 2,
	/// <summary>
/// Buruh Harian Lepas
/// </summary>
	HarianLepas = 3
}
