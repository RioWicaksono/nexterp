using Xunit;
using ERP.Domain.Common;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for HRM domain entities
/// </summary>
public class TestHrmEntity
{
    #region Employee Tests

    [Fact]
    public void Employee_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var employeeNumber = "EMP001";
        var firstName = "John";
        var lastName = "Doe";
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);
        var hireDate = DateTime.UtcNow.AddMonths(-6);

        // Act
        var employee = Employee.Create(
            organizationId: orgId,
            userId: userId,
            employeeNumber: employeeNumber,
            firstName: firstName,
            lastName: lastName,
            dateOfBirth: dateOfBirth,
            gender: Gender.Male,
            departmentId: departmentId,
            positionId: positionId,
            employmentType: EmploymentType.FullTime,
            hireDate: hireDate,
            personalEmail: "john.doe@email.com",
            phone: "+6281234567890");

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(orgId, employee.OrganizationId);
        Assert.Equal(userId, employee.UserId);
        Assert.Equal(employeeNumber, employee.EmployeeNumber);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal("John Doe", employee.FullName);
        Assert.Equal(dateOfBirth, employee.DateOfBirth);
        Assert.Equal(Gender.Male, employee.Gender);
        Assert.Equal(departmentId, employee.DepartmentId);
        Assert.Equal(positionId, employee.PositionId);
        Assert.Equal(EmploymentType.FullTime, employee.EmploymentType);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
    }

    [Fact]
    public void Employee_Create_WithEmptyEmployeeNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Employee.Create(
                organizationId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                employeeNumber: "",
                firstName: "John",
                dateOfBirth: dateOfBirth,
                gender: Gender.Male,
                departmentId: Guid.NewGuid(),
                positionId: Guid.NewGuid(),
                employmentType: EmploymentType.FullTime,
                hireDate: DateTime.UtcNow));

        Assert.Contains("Employee number is required", exception.Message);
    }

    [Fact]
    public void Employee_Create_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Employee.Create(
                organizationId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                employeeNumber: "EMP001",
                firstName: "",
                dateOfBirth: dateOfBirth,
                gender: Gender.Male,
                departmentId: Guid.NewGuid(),
                positionId: Guid.NewGuid(),
                employmentType: EmploymentType.FullTime,
                hireDate: DateTime.UtcNow));

        Assert.Contains("First name is required", exception.Message);
    }

    [Fact]
    public void Employee_Create_Under18YearsOld_ShouldThrowArgumentException()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-17); // Only 17 years old

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Employee.Create(
                organizationId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                employeeNumber: "EMP001",
                firstName: "Young",
                dateOfBirth: dateOfBirth,
                gender: Gender.Male,
                departmentId: Guid.NewGuid(),
                positionId: Guid.NewGuid(),
                employmentType: EmploymentType.FullTime,
                hireDate: DateTime.UtcNow));

        Assert.Contains("Employee must be at least 18 years old", exception.Message);
    }

    [Fact]
    public void Employee_Create_Exactly18YearsOld_ShouldCreateSuccessfully()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-18); // Exactly 18 years old

        // Act
        var employee = Employee.Create(
            organizationId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            employeeNumber: "EMP001",
            firstName: "New",
            dateOfBirth: dateOfBirth,
            gender: Gender.Male,
            departmentId: Guid.NewGuid(),
            positionId: Guid.NewGuid(),
            employmentType: EmploymentType.Intern,
            hireDate: DateTime.UtcNow);

        // Assert
        Assert.NotNull(employee);
    }

    [Fact]
    public void Employee_Create_ShouldTrimAndLowercaseEmail()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);

        // Act
        var employee = Employee.Create(
            organizationId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            employeeNumber: "EMP001",
            firstName: "John",
            dateOfBirth: dateOfBirth,
            gender: Gender.Male,
            departmentId: Guid.NewGuid(),
            positionId: Guid.NewGuid(),
            employmentType: EmploymentType.FullTime,
            hireDate: DateTime.UtcNow,
            personalEmail: "JOHN@EMAIL.COM");

        // Assert
        Assert.Equal("john@email.com", employee.PersonalEmail);
    }

    [Fact]
    public void Employee_Terminate_ShouldSetStatusAndTerminationDate()
    {
        // Arrange
        var employee = CreateValidEmployee();
        var terminationDate = DateTime.UtcNow;

        // Act
        employee.Terminate(terminationDate, "Mutual agreement");

        // Assert
        Assert.Equal(terminationDate, employee.TerminationDate);
        Assert.Equal(EmployeeStatus.Terminated, employee.Status);
    }

    [Fact]
    public void Employee_Resign_ShouldSetStatusToResigned()
    {
        // Arrange
        var employee = CreateValidEmployee();
        var resignationDate = DateTime.UtcNow;

        // Act
        employee.Resign(resignationDate);

        // Assert
        Assert.Equal(resignationDate, employee.TerminationDate);
        Assert.Equal(EmployeeStatus.Resigned, employee.Status);
    }

    [Fact]
    public void Employee_Confirm_WhenNotConfirmed_ShouldSetConfirmationDate()
    {
        // Arrange
        var employee = CreateValidEmployee();

        // Act
        employee.Confirm();

        // Assert
        Assert.NotNull(employee.ConfirmationDate);
        Assert.Equal(EmploymentType.FullTime, employee.EmploymentType);
    }

    [Fact]
    public void Employee_Confirm_WhenAlreadyConfirmed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employee = CreateValidEmployee();
        employee.Confirm();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => employee.Confirm());
        Assert.Contains("Employee is already confirmed", exception.Message);
    }

    [Theory]
    [InlineData(EmploymentType.FullTime)]
    [InlineData(EmploymentType.PartTime)]
    [InlineData(EmploymentType.Contract)]
    [InlineData(EmploymentType.Probation)]
    [InlineData(EmploymentType.Intern)]
    [InlineData(EmploymentType.Freelance)]
    public void Employee_Create_WithDifferentEmploymentTypes_ShouldSetCorrectType(EmploymentType type)
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);

        // Act
        var employee = Employee.Create(
            organizationId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            employeeNumber: "EMP001",
            firstName: "Test",
            dateOfBirth: dateOfBirth,
            gender: Gender.Male,
            departmentId: Guid.NewGuid(),
            positionId: Guid.NewGuid(),
            employmentType: type,
            hireDate: DateTime.UtcNow);

        // Assert
        Assert.Equal(type, employee.EmploymentType);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.Other)]
    public void Employee_Create_WithDifferentGenders_ShouldSetCorrectGender(Gender gender)
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-25);

        // Act
        var employee = Employee.Create(
            organizationId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            employeeNumber: "EMP001",
            firstName: "Test",
            dateOfBirth: dateOfBirth,
            gender: gender,
            departmentId: Guid.NewGuid(),
            positionId: Guid.NewGuid(),
            employmentType: EmploymentType.FullTime,
            hireDate: DateTime.UtcNow);

        // Assert
        Assert.Equal(gender, employee.Gender);
    }

    #endregion

    #region Attendance Tests

    [Fact]
    public void Attendance_CheckIn_WithValidTime_ShouldSetCheckInTimeAndStatus()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);

        // Use fixed time before 9 AM to ensure Present status
        var today = DateTime.UtcNow.Date;
        var checkInTime = today.AddHours(8).AddMinutes(30); // 8:30 AM - well before 9 AM cutoff

        // Act
        attendance.CheckIn(checkInTime);

        // Assert
        Assert.Equal(checkInTime, attendance.CheckInTime);
        Assert.Equal(AttendanceStatus.Present, attendance.Status);
    }

    [Fact]
    public void Attendance_CheckIn_After9AM_ShouldSetStatusToLate()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);

        var lateCheckIn = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 9, 30, 0);

        // Act
        attendance.CheckIn(lateCheckIn);

        // Assert
        Assert.Equal(AttendanceStatus.Late, attendance.Status);
    }

    [Fact]
    public void Attendance_CheckIn_WhenAlreadyCheckedIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present,
            checkInTime: DateTime.UtcNow.AddHours(-1));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attendance.CheckIn(DateTime.UtcNow));
        Assert.Contains("Already checked in", exception.Message);
    }

    [Fact]
    public void Attendance_CheckOut_WithValidTime_ShouldSetCheckOutTime()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.AddHours(-8);
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present,
            checkInTime: checkIn);

        var checkOut = DateTime.UtcNow;

        // Act
        attendance.CheckOut(checkOut);

        // Assert
        Assert.Equal(checkOut, attendance.CheckOutTime);
        Assert.Equal(AttendanceStatus.Present, attendance.Status);
        Assert.NotNull(attendance.WorkingHours);
        Assert.True(attendance.WorkingHours.Value.TotalHours > 0);
    }

    [Fact]
    public void Attendance_CheckOut_WhenNotCheckedIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attendance.CheckOut(DateTime.UtcNow));
        Assert.Contains("Not checked in yet", exception.Message);
    }

    [Fact]
    public void Attendance_CheckOut_BeforeCheckIn_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var checkIn = DateTime.UtcNow;
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present,
            checkInTime: checkIn);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attendance.CheckOut(checkIn.AddHours(-1)));
        Assert.Contains("Check out time cannot be before check in time", exception.Message);
    }

    [Fact]
    public void Attendance_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        // Act
        var attendance = Attendance.Create(
            organizationId: orgId,
            employeeId: employeeId,
            date: date,
            status: AttendanceStatus.Present,
            checkInTime: DateTime.UtcNow.AddHours(-8),
            checkOutTime: DateTime.UtcNow,
            notes: "Regular day");

        // Assert
        Assert.NotNull(attendance);
        Assert.Equal(orgId, attendance.OrganizationId);
        Assert.Equal(employeeId, attendance.EmployeeId);
        Assert.Equal(date, attendance.Date);
        Assert.Equal(AttendanceStatus.Present, attendance.Status);
        Assert.NotNull(attendance.CheckInTime);
        Assert.NotNull(attendance.CheckOutTime);
        Assert.NotNull(attendance.WorkingHours);
    }

    [Fact]
    public void Attendance_SetOvertime_WithNegativeHours_ShouldThrowArgumentException()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            attendance.SetOvertime(-2m));
        Assert.Contains("Overtime hours cannot be negative", exception.Message);
    }

    [Fact]
    public void Attendance_SetOvertime_WithValidHours_ShouldSetOvertime()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);

        // Act
        attendance.SetOvertime(3.5m);

        // Assert
        Assert.Equal(3.5m, attendance.OvertimeHours);
    }

    [Fact]
    public void Attendance_Approve_WhenNotApproved_ShouldSetApprovalInfo()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);
        var approvedBy = Guid.NewGuid();

        // Act
        attendance.Approve(approvedBy);

        // Assert
        Assert.True(attendance.IsApproved);
        Assert.Equal(approvedBy, attendance.ApprovedBy);
        Assert.NotNull(attendance.ApprovedAt);
    }

    [Fact]
    public void Attendance_Approve_WhenAlreadyApproved_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var attendance = Attendance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            date: DateTime.UtcNow.Date,
            status: AttendanceStatus.Present);
        attendance.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attendance.Approve(Guid.NewGuid()));
        Assert.Contains("Already approved", exception.Message);
    }

    #endregion

    #region LeaveRequest Tests

    [Fact]
    public void LeaveRequest_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.Date.AddDays(7);
        var endDate = DateTime.UtcNow.Date.AddDays(14);

        // Act
        var leaveRequest = LeaveRequest.Create(
            organizationId: orgId,
            employeeId: employeeId,
            leaveType: LeaveType.Annual,
            startDate: startDate,
            endDate: endDate,
            reason: "Family vacation");

        // Assert
        Assert.NotNull(leaveRequest);
        Assert.Equal(orgId, leaveRequest.OrganizationId);
        Assert.Equal(employeeId, leaveRequest.EmployeeId);
        Assert.Equal(LeaveType.Annual, leaveRequest.LeaveType);
        Assert.Equal(startDate, leaveRequest.StartDate);
        Assert.Equal(endDate, leaveRequest.EndDate);
        Assert.Equal(8, leaveRequest.TotalDays);
        Assert.Equal(LeaveStatus.Pending, leaveRequest.Status);
        Assert.True(leaveRequest.IsPending);
        Assert.False(leaveRequest.IsApproved);
    }

    [Fact]
    public void LeaveRequest_Create_EndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date.AddDays(14);
        var endDate = DateTime.UtcNow.Date.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            LeaveRequest.Create(
                organizationId: Guid.NewGuid(),
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Annual,
                startDate: startDate,
                endDate: endDate));

        Assert.Contains("End date cannot be before start date", exception.Message);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(-1)]
    [InlineData(1.5)]
    public void LeaveRequest_Create_WithInvalidHalfDay_ShouldThrowArgumentException(decimal invalidHalfDay)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            LeaveRequest.Create(
                organizationId: Guid.NewGuid(),
                employeeId: Guid.NewGuid(),
                leaveType: LeaveType.Annual,
                startDate: DateTime.UtcNow.Date,
                endDate: DateTime.UtcNow.Date.AddDays(5),
                halfDay: invalidHalfDay));

        Assert.Contains("Half day must be between 0 and 1", exception.Message);
    }

    [Fact]
    public void LeaveRequest_Create_WithHalfDay_ShouldCalculateCorrectTotalDays()
    {
        // Arrange - 5 days with 0.5 half day = 4.5 total leave days
        var startDate = DateTime.UtcNow.Date;
        var endDate = DateTime.UtcNow.Date.AddDays(4);

        // Act
        var leaveRequest = LeaveRequest.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Sick,
            startDate: startDate,
            endDate: endDate,
            halfDay: 0.5m);

        // Assert
        Assert.Equal(5, leaveRequest.TotalDays);
        Assert.Equal(4.5m, leaveRequest.TotalLeaveDays);
    }

    [Fact]
    public void LeaveRequest_Approve_WhenPending_ShouldSetApprovalInfo()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();
        var approvedBy = Guid.NewGuid();

        // Act
        leaveRequest.Approve(approvedBy);

        // Assert
        Assert.True(leaveRequest.IsApproved);
        Assert.Equal(LeaveStatus.Approved, leaveRequest.Status);
        Assert.Equal(approvedBy, leaveRequest.ApprovedBy);
        Assert.NotNull(leaveRequest.ApprovedAt);
    }

    [Fact]
    public void LeaveRequest_Approve_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();
        leaveRequest.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.Approve(Guid.NewGuid()));
        Assert.Contains("Can only approve pending requests", exception.Message);
    }

    [Fact]
    public void LeaveRequest_Reject_WhenPending_ShouldSetRejectionInfo()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();
        var rejectedBy = Guid.NewGuid();

        // Act
        leaveRequest.Reject(rejectedBy, "Insufficient leave balance");

        // Assert
        Assert.True(leaveRequest.IsRejected);
        Assert.Equal(LeaveStatus.Rejected, leaveRequest.Status);
        Assert.Equal(rejectedBy, leaveRequest.RejectedBy);
        Assert.NotNull(leaveRequest.RejectedAt);
        Assert.Equal("Insufficient leave balance", leaveRequest.RejectionReason);
    }

    [Fact]
    public void LeaveRequest_Cancel_WhenApproved_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();
        leaveRequest.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.Cancel());
        Assert.Contains("Cannot cancel approved leave", exception.Message);
    }

    [Fact]
    public void LeaveRequest_Cancel_WhenPending_ShouldSetStatusToCancelled()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();

        // Act
        leaveRequest.Cancel();

        // Assert
        Assert.Equal(LeaveStatus.Cancelled, leaveRequest.Status);
        Assert.False(leaveRequest.IsActive);
    }

    [Fact]
    public void LeaveRequest_UpdateDates_WhenPending_ShouldUpdateDates()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();
        var newStartDate = DateTime.UtcNow.Date.AddDays(14);
        var newEndDate = DateTime.UtcNow.Date.AddDays(21);

        // Act
        leaveRequest.UpdateDates(newStartDate, newEndDate);

        // Assert
        Assert.Equal(newStartDate, leaveRequest.StartDate);
        Assert.Equal(newEndDate, leaveRequest.EndDate);
        Assert.Equal(8, leaveRequest.TotalDays);
    }

    [Fact]
    public void LeaveRequest_UpdateDates_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();
        leaveRequest.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            leaveRequest.UpdateDates(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(7)));
        Assert.Contains("Can only update pending requests", exception.Message);
    }

    [Fact]
    public void LeaveRequest_UpdateDates_EndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var leaveRequest = CreateValidLeaveRequest();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            leaveRequest.UpdateDates(DateTime.UtcNow.Date.AddDays(14), DateTime.UtcNow.Date.AddDays(7)));
        Assert.Contains("End date cannot be before start date", exception.Message);
    }

    [Theory]
    [InlineData(LeaveType.Annual)]
    [InlineData(LeaveType.Sick)]
    [InlineData(LeaveType.Emergency)]
    [InlineData(LeaveType.Maternity)]
    [InlineData(LeaveType.Paternity)]
    [InlineData(LeaveType.Unpaid)]
    [InlineData(LeaveType.Other)]
    public void LeaveRequest_Create_WithDifferentLeaveTypes_ShouldSetCorrectType(LeaveType leaveType)
    {
        // Act
        var leaveRequest = LeaveRequest.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: leaveType,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(3));

        // Assert
        Assert.Equal(leaveType, leaveRequest.LeaveType);
    }

    #endregion

    #region LeaveBalance Tests

    [Fact]
    public void LeaveBalance_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        // Act
        var balance = LeaveBalance.Create(
            organizationId: orgId,
            employeeId: employeeId,
            leaveType: LeaveType.Annual,
            year: 2024,
            totalDays: 12,
            carryForward: 5);

        // Assert
        Assert.NotNull(balance);
        Assert.Equal(orgId, balance.OrganizationId);
        Assert.Equal(employeeId, balance.EmployeeId);
        Assert.Equal(LeaveType.Annual, balance.LeaveType);
        Assert.Equal(2024, balance.Year);
        Assert.Equal(12m, balance.TotalDays);
        Assert.Equal(5m, balance.CarryForward);
        Assert.Equal(0, balance.UsedDays);
        Assert.Equal(0, balance.PendingDays);
        Assert.Equal(12m, balance.Balance); // TotalDays - UsedDays - PendingDays (CarryForward is separate)
    }

    [Fact]
    public void LeaveBalance_UseDays_WithInsufficientBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Annual,
            year: 2024,
            totalDays: 5,
            carryForward: 0);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            balance.UseDays(10m));
        Assert.Contains("Insufficient leave balance", exception.Message);
    }

    [Fact]
    public void LeaveBalance_UseDays_WithValidAmount_ShouldDecreaseBalance()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Annual,
            year: 2024,
            totalDays: 12);

        // Act
        balance.UseDays(5m);

        // Assert
        Assert.Equal(5m, balance.UsedDays);
        Assert.Equal(7m, balance.Balance); // 12 - 5 = 7
    }

    [Fact]
    public void LeaveBalance_UseDays_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Annual,
            year: 2024,
            totalDays: 12);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            balance.UseDays(-2m));
        Assert.Contains("Days must be positive", exception.Message);
    }

    [Fact]
    public void LeaveBalance_CalculateBalance_ShouldIncludePendingDays()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Annual,
            year: 2024,
            totalDays: 12);

        balance.SetPendingDays(3m);
        balance.UseDays(2m);

        // Assert - Balance = 12 - 2 - 3 = 7
        Assert.Equal(7m, balance.Balance);
    }

    #endregion

    #region Helper Methods

    private static Employee CreateValidEmployee()
    {
        return Employee.Create(
            organizationId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            employeeNumber: "EMP001",
            firstName: "John",
            lastName: "Doe",
            dateOfBirth: DateTime.UtcNow.AddYears(-25),
            gender: Gender.Male,
            departmentId: Guid.NewGuid(),
            positionId: Guid.NewGuid(),
            employmentType: EmploymentType.FullTime,
            hireDate: DateTime.UtcNow.AddMonths(-6));
    }

    private static LeaveRequest CreateValidLeaveRequest()
    {
        return LeaveRequest.Create(
            organizationId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Annual,
            startDate: DateTime.UtcNow.Date.AddDays(7),
            endDate: DateTime.UtcNow.Date.AddDays(14),
            reason: "Vacation");
    }

    #endregion
}
