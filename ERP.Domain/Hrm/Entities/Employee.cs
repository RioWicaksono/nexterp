using ERP.Domain.Common;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.Hrm.Entities;

/// <summary>
/// Employee entity - extends User with HR data
/// </summary>
public class Employee : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string EmployeeNumber { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }

    // Personal Information
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; }
    public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public MaritalStatus MaritalStatus { get; private set; }
    public string? PhotoUrl { get; private set; }

    // Employment Information
    public Guid DepartmentId { get; private set; }
    public Guid PositionId { get; private set; }
    public EmploymentType EmploymentType { get; private set; }
    public EmployeeStatus Status { get; private set; } = EmployeeStatus.Active;
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public DateTime? ConfirmationDate { get; private set; }

    // Contact Information
    public string? PersonalEmail { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? EmergencyContactRelation { get; private set; }

    // Address
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }

    // Banking Information (for payroll)
    public string? BankName { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? BankAccountName { get; private set; }
    public string? TaxId { get; private set; }

    // Navigation properties
    private readonly Department? _department;
    public Department? Department => _department;

    private readonly Position? _position;
    public Position? Position => _position;

    private readonly List<Attendance> _attendances = new();
    public IReadOnlyCollection<Attendance> Attendances => _attendances.AsReadOnly();

    private readonly List<LeaveRequest> _leaveRequests = new();
    public IReadOnlyCollection<LeaveRequest> LeaveRequests => _leaveRequests.AsReadOnly();

    // Calculated properties
    public int YearsOfService => (DateTime.UtcNow - HireDate).Days / 365;

    // Factory method
    public static Employee Create(
        Guid organizationId,
        Guid userId,
        string employeeNumber,
        string firstName,
        DateTime dateOfBirth,
        Gender gender,
        Guid departmentId,
        Guid positionId,
        EmploymentType employmentType,
        DateTime hireDate,
        string? lastName = null,
        MaritalStatus maritalStatus = MaritalStatus.Single,
        string? personalEmail = null,
        string? phone = null,
        string? mobile = null)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Employee number is required", nameof(employeeNumber));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (dateOfBirth > DateTime.UtcNow.AddYears(-18))
            throw new ArgumentException("Employee must be at least 18 years old", nameof(dateOfBirth));

        return new Employee
        {
            OrganizationId = organizationId,
            UserId = userId,
            EmployeeNumber = employeeNumber.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName?.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,
            MaritalStatus = maritalStatus,
            DepartmentId = departmentId,
            PositionId = positionId,
            EmploymentType = employmentType,
            HireDate = hireDate,
            PersonalEmail = personalEmail?.Trim().ToLowerInvariant(),
            Phone = phone?.Trim(),
            Mobile = mobile?.Trim()
        };
    }

    public void UpdatePersonalInfo(
        string? firstName = null,
        string? lastName = null,
        DateTime? dateOfBirth = null,
        Gender? gender = null,
        MaritalStatus? maritalStatus = null)
    {
        FirstName = firstName?.Trim() ?? FirstName;
        LastName = lastName?.Trim() ?? LastName;
        DateOfBirth = dateOfBirth ?? DateOfBirth;
        Gender = gender ?? Gender;
        MaritalStatus = maritalStatus ?? MaritalStatus;
        UpdateTimestamp();
    }

    public void UpdateContactInfo(
        string? phone = null,
        string? mobile = null,
        string? email = null)
    {
        Phone = phone?.Trim() ?? Phone;
        Mobile = mobile?.Trim() ?? Mobile;
        PersonalEmail = email?.Trim().ToLowerInvariant() ?? PersonalEmail;
        UpdateTimestamp();
    }

    public void UpdateEmergencyContact(
        string? contactName = null,
        string? contactPhone = null,
        string? contactRelation = null)
    {
        EmergencyContactName = contactName?.Trim();
        EmergencyContactPhone = contactPhone?.Trim();
        EmergencyContactRelation = contactRelation?.Trim();
        UpdateTimestamp();
    }

    public void UpdateAddress(
        string? address = null,
        string? city = null,
        string? country = null,
        string? postalCode = null)
    {
        Address = address?.Trim();
        City = city?.Trim();
        Country = country?.Trim();
        PostalCode = postalCode?.Trim();
        UpdateTimestamp();
    }

    public void UpdateEmployment(
        Guid departmentId,
        Guid positionId,
        EmploymentType employmentType)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        EmploymentType = employmentType;
        UpdateTimestamp();
    }

    public void UpdateBankingInfo(
        string? bankName = null,
        string? accountNumber = null,
        string? accountName = null,
        string? taxId = null)
    {
        BankName = bankName?.Trim();
        BankAccountNumber = accountNumber?.Trim();
        BankAccountName = accountName?.Trim();
        TaxId = taxId?.Trim();
        UpdateTimestamp();
    }

    public void SetStatus(EmployeeStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }

    public void Confirm()
    {
        if (ConfirmationDate.HasValue)
            throw new InvalidOperationException("Employee is already confirmed");

        ConfirmationDate = DateTime.UtcNow;
        EmploymentType = EmploymentType.FullTime;
        UpdateTimestamp();
    }

    public void Terminate(DateTime terminationDate, string? reason = null)
    {
        TerminationDate = terminationDate;
        Status = EmployeeStatus.Terminated;
        UpdateTimestamp();
    }

    public void Resign(DateTime resignationDate)
    {
        TerminationDate = resignationDate;
        Status = EmployeeStatus.Resigned;
        UpdateTimestamp();
    }

    public void SetPhoto(string? photoUrl)
    {
        PhotoUrl = photoUrl?.Trim();
        UpdateTimestamp();
    }

    public void Activate() { Status = EmployeeStatus.Active; UpdateTimestamp(); }
    public void Suspend() { Status = EmployeeStatus.Suspended; UpdateTimestamp(); }
    public void SetOnLeave() { Status = EmployeeStatus.OnLeave; UpdateTimestamp(); }
}
