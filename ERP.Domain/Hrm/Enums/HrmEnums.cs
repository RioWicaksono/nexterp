namespace ERP.Domain.Hrm.Enums;

/// <summary>
/// Employment types
/// </summary>
public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Probation = 4,
    Intern = 5,
    Freelance = 6
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
    Resigned = 5
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
/// Marital Status
/// </summary>
public enum MaritalStatus
{
    Single = 1,
    Married = 2,
    Divorced = 3,
    Widowed = 4
}

/// <summary>
/// Leave types
/// </summary>
public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Emergency = 3,
    Maternity = 4,
    Paternity = 5,
    Unpaid = 6,
    Other = 7
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
    Holiday = 5
}
