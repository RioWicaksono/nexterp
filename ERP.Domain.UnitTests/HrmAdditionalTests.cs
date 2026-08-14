using Xunit;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for OvertimeRequest entity
/// </summary>
public class OvertimeRequestTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOvertimeRequest()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var workDate = DateTime.UtcNow.Date;
        var startTime = new TimeSpan(17, 0, 0);
        var endTime = new TimeSpan(21, 0, 0);

        // Act
        var overtime = OvertimeRequest.Create(
            orgId, employeeId, workDate, startTime, endTime,
            OvertimeType.WeekdayOvertime, "Project deadline");

        // Assert
        Assert.Equal(OvertimeStatus.Pending, overtime.Status);
        Assert.Equal(4, overtime.Hours);
        Assert.Equal(0, overtime.ApprovedHours);
        Assert.True(overtime.RequestDate <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_WithHoursExceedingMax_ShouldCapToMaxHours()
    {
        // Arrange - 6 hours, but max is 4
        var startTime = new TimeSpan(17, 0, 0);
        var endTime = new TimeSpan(23, 0, 0);

        // Act
        var overtime = OvertimeRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.Date, startTime, endTime,
            OvertimeType.DailyOvertime);

        // Assert - hours should be capped at MaxDailyOvertimeHours (4)
        Assert.Equal(OvertimeRequest.MaxDailyOvertimeHours, overtime.Hours);
    }

    [Fact]
    public void Approve_ShouldSetApprovedStatus()
    {
        // Arrange
        var overtime = CreateTestOvertime();
        var approverId = Guid.NewGuid();
        var approvedHours = 3m;

        // Act
        overtime.Approve(approvedHours, approverId, "Approved for project work");

        // Assert
        Assert.Equal(OvertimeStatus.Approved, overtime.Status);
        Assert.Equal(approvedHours, overtime.ApprovedHours);
        Assert.Equal(approverId, overtime.ApprovedById);
        Assert.NotNull(overtime.ApprovedAt);
    }

    [Fact]
    public void Approve_WithHoursExceedingMax_ShouldCapHours()
    {
        // Arrange
        var overtime = CreateTestOvertime();
        var approverId = Guid.NewGuid();

        // Act
        overtime.Approve(10, approverId); // 10 hours, but max is 4

        // Assert
        Assert.Equal(OvertimeRequest.MaxDailyOvertimeHours, overtime.ApprovedHours);
    }

    [Fact]
    public void Approve_WhenNotPending_ShouldThrowException()
    {
        // Arrange
        var overtime = CreateTestOvertime();
        overtime.Approve(2, Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            overtime.Approve(2, Guid.NewGuid()));
    }

    [Fact]
    public void Reject_ShouldSetRejectedStatus()
    {
        // Arrange
        var overtime = CreateTestOvertime();
        var rejecterId = Guid.NewGuid();

        // Act
        overtime.Reject(rejecterId, "Budget constraints");

        // Assert
        Assert.Equal(OvertimeStatus.Rejected, overtime.Status);
        Assert.Equal(rejecterId, overtime.ApprovedById);
        Assert.Equal("Budget constraints", overtime.ApprovalNotes);
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetCancelledStatus()
    {
        // Arrange
        var overtime = CreateTestOvertime();

        // Act
        overtime.Cancel();

        // Assert
        Assert.Equal(OvertimeStatus.Cancelled, overtime.Status);
    }

    [Fact]
    public void CalculateOvertimePay_WhenApproved_ShouldCalculateCorrectly()
    {
        // Arrange
        var overtime = OvertimeRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.Date,
            new TimeSpan(17, 0, 0),
            new TimeSpan(21, 0, 0),
            OvertimeType.WeekdayOvertime);
        overtime.Approve(4, Guid.NewGuid());

        var hourlyRate = 50000m;
        var multiplier = 1.5m;

        // Act
        var pay = overtime.CalculateOvertimePay(hourlyRate, multiplier);

        // Assert
        Assert.Equal(4 * hourlyRate * multiplier, pay);
    }

    [Fact]
    public void CalculateOvertimePay_WhenNotApproved_ShouldReturnZero()
    {
        // Arrange
        var overtime = CreateTestOvertime();

        // Act
        var pay = overtime.CalculateOvertimePay(50000m);

        // Assert
        Assert.Equal(0, pay);
    }

    private static OvertimeRequest CreateTestOvertime()
    {
        return OvertimeRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.Date,
            new TimeSpan(17, 0, 0),
            new TimeSpan(21, 0, 0),
            OvertimeType.WeekdayOvertime);
    }
}

/// <summary>
/// Unit tests for Department entity
/// </summary>
public class DepartmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDepartment()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act
        var department = Department.Create(orgId, "Engineering", "ENG", "Software team");

        // Assert
        Assert.Equal("Engineering", department.Name);
        Assert.Equal("ENG", department.Code);
        Assert.True(department.IsActive);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Department.Create(Guid.NewGuid(), "", "ENG"));
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var department = Department.Create(
            Guid.NewGuid(), "Engineering", "ENG");

        // Act
        department.Update("Software Engineering", "SENG", "Updated description");

        // Assert
        Assert.Equal("Software Engineering", department.Name);
        Assert.Equal("SENG", department.Code);
        Assert.Equal("Updated description", department.Description);
    }

    [Fact]
    public void SetParentDepartment_ShouldUpdateParent()
    {
        // Arrange
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");
        var parentId = Guid.NewGuid();

        // Act
        department.SetParentDepartment(parentId);

        // Assert
        Assert.Equal(parentId, department.ParentDepartmentId);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");
        department.Deactivate();

        // Act
        department.Activate();

        // Assert
        Assert.True(department.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var department = Department.Create(Guid.NewGuid(), "Engineering", "ENG");

        // Act
        department.Deactivate();

        // Assert
        Assert.False(department.IsActive);
    }
}

/// <summary>
/// Unit tests for Position entity
/// </summary>
public class PositionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePosition()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        // Act
        var position = Position.Create(
            orgId, deptId, "Senior Engineer", "Expert level", 5,
            15000000, 25000000);

        // Assert
        Assert.Equal("Senior Engineer", position.Title);
        Assert.Equal(5, position.Grade);
        Assert.Equal(15000000, position.MinSalary);
        Assert.Equal(25000000, position.MaxSalary);
        Assert.True(position.IsActive);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Position.Create(Guid.NewGuid(), Guid.NewGuid(), ""));
    }

    [Fact]
    public void Create_WithZeroGrade_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Position.Create(Guid.NewGuid(), Guid.NewGuid(), "Engineer", grade: 0));
    }

    [Fact]
    public void SetSalaryRange_WithValidRange_ShouldUpdate()
    {
        // Arrange
        var position = Position.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Engineer", grade: 1);

        // Act
        position.SetSalaryRange(10000000, 20000000);

        // Assert
        Assert.Equal(10000000, position.MinSalary);
        Assert.Equal(20000000, position.MaxSalary);
    }

    [Fact]
    public void SetSalaryRange_WithMinGreaterThanMax_ShouldThrowException()
    {
        // Arrange
        var position = Position.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Engineer", grade: 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            position.SetSalaryRange(20000000, 10000000));
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var position = Position.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Engineer", grade: 1);

        // Act
        position.Update("Senior Engineer", "Lead technical team", 6);

        // Assert
        Assert.Equal("Senior Engineer", position.Title);
        Assert.Equal("Lead technical team", position.Description);
        Assert.Equal(6, position.Grade);
    }
}

/// <summary>
/// Unit tests for Shift entity
/// </summary>
public class ShiftTests
{
    [Fact]
    public void Create_MorningShift_ShouldCreateWithCorrectHours()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var startTime = new TimeSpan(8, 0, 0);
        var endTime = new TimeSpan(17, 0, 0);
        var breakStart = new TimeSpan(12, 0, 0);
        var breakEnd = new TimeSpan(13, 0, 0);

        // Act
        var shift = Shift.Create(
            orgId, "Morning Shift", "PAGI", startTime, endTime,
            breakStart, breakEnd, "Standard morning shift");

        // Assert
        Assert.Equal("Morning Shift", shift.Name);
        Assert.Equal("PAGI", shift.Code);
        Assert.Equal(8, shift.WorkHours);
        Assert.False(shift.IsNightShift);
    }

    [Fact]
    public void Create_NightShift_ShouldDetectAsNightShift()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var startTime = new TimeSpan(22, 0, 0);
        var endTime = new TimeSpan(6, 0, 0);

        // Act
        var shift = Shift.Create(
            orgId, "Night Shift", "MALAM", startTime, endTime);

        // Assert
        Assert.True(shift.IsNightShift);
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var shift = Shift.Create(
            Guid.NewGuid(), "Morning Shift", "PAGI",
            new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));

        // Act
        shift.Update(
            startTime: new TimeSpan(9, 0, 0),
            endTime: new TimeSpan(18, 0, 0),
            description: "Updated shift");

        // Assert
        Assert.Equal(new TimeSpan(9, 0, 0), shift.StartTime);
        Assert.Equal(new TimeSpan(18, 0, 0), shift.EndTime);
        Assert.Equal("Updated shift", shift.Description);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var shift = Shift.Create(
            Guid.NewGuid(), "Morning Shift", "PAGI",
            new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));

        // Act
        shift.Deactivate();

        // Assert
        Assert.False(shift.IsActive);
    }
}

/// <summary>
/// Unit tests for LeaveEntitlement entity
/// </summary>
public class LeaveEntitlementTests
{
    [Fact]
    public void Create_ShouldInitializeCorrectly()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        // Act
        var entitlement = LeaveEntitlement.Create(
            orgId, employeeId, "Annual", 2024, 12,
            isAutoAllocated: true, "Initial allocation");

        // Assert
        Assert.Equal(12, entitlement.TotalDays);
        Assert.Equal(0, entitlement.UsedDays);
        Assert.Equal(12, entitlement.AvailableDays);
        Assert.True(entitlement.IsAutoAllocated);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 12)]
    [InlineData(4, 12)]
    [InlineData(5, 13)]
    [InlineData(10, 18)]
    [InlineData(15, 21)] // Max is 21
    public void CalculateAnnualLeaveDays_ShouldCalculateCorrectly(int yearsOfService, decimal expectedDays)
    {
        // Act
        var days = LeaveEntitlement.CalculateAnnualLeaveDays(yearsOfService);

        // Assert
        Assert.Equal(expectedDays, days);
    }

    [Fact]
    public void UseDays_ShouldDeductFromAvailable()
    {
        // Arrange
        var entitlement = LeaveEntitlement.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Annual", 2024, 12);

        // Act
        entitlement.UseDays(3);

        // Assert
        Assert.Equal(3, entitlement.UsedDays);
        Assert.Equal(9, entitlement.AvailableDays);
    }

    [Fact]
    public void UseDays_MoreThanAvailable_ShouldThrowException()
    {
        // Arrange
        var entitlement = LeaveEntitlement.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Annual", 2024, 5);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => entitlement.UseDays(10));
    }

    [Fact]
    public void ResetYear_ShouldCarryForwardUnusedDays()
    {
        // Arrange
        var entitlement = LeaveEntitlement.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Annual", 2024, 12);
        entitlement.UseDays(6); // 6 days used, 6 remaining

        // Act
        entitlement.ResetYear(2025);

        // Assert
        Assert.Equal(2025, entitlement.Year);
        Assert.Equal(0, entitlement.UsedDays);
        // Carry forward max is 50% of total = 6 days
        Assert.Equal(6, entitlement.CarryForwardDays);
    }
}

/// <summary>
/// Unit tests for EmployeeDocument entity
/// </summary>
public class EmployeeDocumentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDocument()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var issueDate = DateTime.UtcNow.AddYears(-1);
        var expiryDate = DateTime.UtcNow.AddYears(4);

        // Act
        var document = EmployeeDocument.Create(
            orgId, employeeId, EmployeeDocument.DocumentTypes.KTP,
            "ktp_scan.pdf", "/uploads/ktp.pdf",
            issueDate, expiryDate, "NIK123456",
            "Kecamatan Jakarta", "Personal document");

        // Assert
        Assert.Equal(EmployeeDocument.DocumentTypes.KTP, document.DocumentType);
        Assert.False(document.IsVerified);
        Assert.False(document.IsExpired);
    }

    [Fact]
    public void Verify_ShouldSetVerifiedStatus()
    {
        // Arrange
        var document = EmployeeDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            EmployeeDocument.DocumentTypes.NPWP,
            "npwp.pdf", "/uploads/npwp.pdf",
            DateTime.UtcNow.AddYears(-1));
        var verifiedBy = Guid.NewGuid();

        // Act
        document.Verify(verifiedBy);

        // Assert
        Assert.True(document.IsVerified);
        Assert.Equal(verifiedBy, document.VerifiedBy);
        Assert.NotNull(document.VerifiedAt);
    }

    [Fact]
    public void Verify_WhenAlreadyVerified_ShouldThrowException()
    {
        // Arrange
        var document = EmployeeDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            EmployeeDocument.DocumentTypes.NPWP,
            "npwp.pdf", "/uploads/npwp.pdf",
            DateTime.UtcNow.AddYears(-1));
        document.Verify(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => document.Verify(Guid.NewGuid()));
    }

    [Fact]
    public void IsExpired_WhenExpiryDatePassed_ShouldReturnTrue()
    {
        // Arrange
        var document = EmployeeDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            EmployeeDocument.DocumentTypes.PASSPORT,
            "passport.pdf", "/uploads/passport.pdf",
            DateTime.UtcNow.AddYears(-5),
            expiryDate: DateTime.UtcNow.AddDays(-30));

        // Assert
        Assert.True(document.IsExpired);
    }

    [Fact]
    public void IsExpiringSoon_WhenWithin30Days_ShouldReturnTrue()
    {
        // Arrange
        var document = EmployeeDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            EmployeeDocument.DocumentTypes.BPJSKS,
            "bpjs.pdf", "/uploads/bpjs.pdf",
            DateTime.UtcNow.AddYears(-1),
            expiryDate: DateTime.UtcNow.AddDays(20));

        // Assert
        Assert.True(document.IsExpiringSoon);
    }
}

/// <summary>
/// Unit tests for Holiday entity
/// </summary>
public class HolidayTests
{
    [Fact]
    public void Create_NationalHoliday_ShouldCreate()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var holidayDate = new DateTime(2024, 8, 17);

        // Act
        var holiday = Holiday.Create(
            orgId, "Merdeka Day", holidayDate,
            Holiday.HolidayTypes.NATIONAL,
            isRecurring: true);

        // Assert
        Assert.Equal("Merdeka Day", holiday.Name);
        Assert.Equal(2024, holiday.Year);
        Assert.True(holiday.IsRecurring);
    }

    [Fact]
    public void Update_ShouldUpdateFields()
    {
        // Arrange
        var holiday = Holiday.Create(
            Guid.NewGuid(), "Libur", new DateTime(2024, 12, 25),
            Holiday.HolidayTypes.NATIONAL);

        // Act
        holiday.Update("Natal", new DateTime(2024, 12, 25), Holiday.HolidayTypes.RELIGIOUS);

        // Assert
        Assert.Equal("Natal", holiday.Name);
        Assert.Equal(Holiday.HolidayTypes.RELIGIOUS, holiday.HolidayType);
    }
}
