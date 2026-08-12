using Xunit;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for Employee entity
/// </summary>
public class EmployeeTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateEmployee()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var employee = Employee.Create(
            orgId, userId, "EMP001", "John", DateTime.UtcNow.AddYears(-25),
            Gender.Male, Guid.NewGuid(), Guid.NewGuid(),
            EmploymentType.FullTime, DateTime.UtcNow.AddMonths(-6));

        // Assert
        Assert.Equal(orgId, employee.OrganizationId);
        Assert.Equal("EMP001", employee.EmployeeNumber);
        Assert.Equal("John", employee.FirstName);
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.False(employee.IsDeleted);
    }

    [Fact]
    public void Create_WithUnder18_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(
                Guid.NewGuid(), Guid.NewGuid(), "EMP001", "Child",
                DateTime.UtcNow.AddYears(-10), Gender.Male,
                Guid.NewGuid(), Guid.NewGuid(),
                EmploymentType.FullTime, DateTime.UtcNow));
    }

    [Fact]
    public void FullName_WithBothNames_ShouldCombineNames()
    {
        // Arrange
        var employee = Employee.Create(
            Guid.NewGuid(), Guid.NewGuid(), "EMP001", "John",
            DateTime.UtcNow.AddYears(-25), Gender.Male,
            Guid.NewGuid(), Guid.NewGuid(),
            EmploymentType.FullTime, DateTime.UtcNow.AddMonths(-6),
            lastName: "Doe");

        // Assert
        Assert.Equal("John Doe", employee.FullName);
    }

    [Fact]
    public void YearsOfService_CalculatesCorrectly()
    {
        // Arrange - employee is 25 years old to avoid validation
        var employee = CreateTestEmployee();

        // Assert
        Assert.True(employee.YearsOfService >= 0 && employee.YearsOfService <= 25);
    }

    [Fact]
    public void SetStatus_ShouldUpdateStatus()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        employee.SetStatus(EmployeeStatus.OnLeave);

        // Assert
        Assert.Equal(EmployeeStatus.OnLeave, employee.Status);
    }

    [Fact]
    public void Confirm_ShouldSetConfirmationDate()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        employee.Confirm();

        // Assert
        Assert.NotNull(employee.ConfirmationDate);
        Assert.Equal(EmploymentType.FullTime, employee.EmploymentType);
    }

    [Fact]
    public void Terminate_ShouldSetTerminationDate()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var terminationDate = DateTime.UtcNow;

        // Act
        employee.Terminate(terminationDate);

        // Assert
        Assert.Equal(terminationDate, employee.TerminationDate);
        Assert.Equal(EmployeeStatus.Terminated, employee.Status);
    }

    [Fact]
    public void UpdateEmployment_ShouldUpdateFields()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var newDeptId = Guid.NewGuid();
        var newPosId = Guid.NewGuid();

        // Act
        employee.UpdateEmployment(newDeptId, newPosId, EmploymentType.Contract);

        // Assert
        Assert.Equal(newDeptId, employee.DepartmentId);
        Assert.Equal(newPosId, employee.PositionId);
        Assert.Equal(EmploymentType.Contract, employee.EmploymentType);
    }

    [Fact]
    public void Activate_ShouldSetActiveStatus()
    {
        // Arrange
        var employee = CreateTestEmployee();
        employee.Suspend();

        // Act
        employee.Activate();

        // Assert
        Assert.Equal(EmployeeStatus.Active, employee.Status);
    }

    [Fact]
    public void Suspend_ShouldSetSuspendedStatus()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        employee.Suspend();

        // Assert
        Assert.Equal(EmployeeStatus.Suspended, employee.Status);
    }

    private static Employee CreateTestEmployee()
    {
        return Employee.Create(
            Guid.NewGuid(), Guid.NewGuid(), "EMP001", "Test",
            DateTime.UtcNow.AddYears(-25), Gender.Male,
            Guid.NewGuid(), Guid.NewGuid(),
            EmploymentType.FullTime, DateTime.UtcNow.AddMonths(-6));
    }
}

/// <summary>
/// Unit tests for LeaveRequest entity
/// </summary>
public class LeaveRequestTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateLeaveRequest()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(7);
        var endDate = DateTime.UtcNow.AddDays(14);

        // Act
        var leave = LeaveRequest.Create(orgId, employeeId, LeaveType.Annual, startDate, endDate, "Vacation");

        // Assert
        Assert.Equal(LeaveStatus.Pending, leave.Status);
        Assert.Equal(8, leave.TotalDays);
        Assert.True(leave.IsPending);
    }

    [Fact]
    public void Create_WithEndBeforeStart_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            LeaveRequest.Create(
                Guid.NewGuid(), Guid.NewGuid(), LeaveType.Annual,
                DateTime.UtcNow.AddDays(14), DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public void Approve_ShouldSetApprovedStatus()
    {
        // Arrange
        var leave = CreateTestLeaveRequest();
        var approverId = Guid.NewGuid();

        // Act
        leave.Approve(approverId);

        // Assert
        Assert.Equal(LeaveStatus.Approved, leave.Status);
        Assert.NotNull(leave.ApprovedAt);
        Assert.Equal(approverId, leave.ApprovedBy);
    }

    [Fact]
    public void Reject_ShouldSetRejectedStatus()
    {
        // Arrange
        var leave = CreateTestLeaveRequest();
        var rejecterId = Guid.NewGuid();

        // Act
        leave.Reject(rejecterId, "Insufficient leave balance");

        // Assert
        Assert.Equal(LeaveStatus.Rejected, leave.Status);
        Assert.NotNull(leave.RejectedAt);
        Assert.Equal("Insufficient leave balance", leave.RejectionReason);
    }

    [Fact]
    public void Cancel_ShouldSetCancelledStatus()
    {
        // Arrange
        var leave = CreateTestLeaveRequest();

        // Act
        leave.Cancel();

        // Assert
        Assert.Equal(LeaveStatus.Cancelled, leave.Status);
    }

    [Fact]
    public void Cancel_ApprovedLeave_ShouldThrowException()
    {
        // Arrange
        var leave = CreateTestLeaveRequest();
        leave.Approve(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => leave.Cancel());
    }

    [Fact]
    public void UpdateDates_WhenPending_ShouldUpdateDates()
    {
        // Arrange
        var leave = CreateTestLeaveRequest();
        var newStart = DateTime.UtcNow.Date.AddDays(14);
        var newEnd = DateTime.UtcNow.Date.AddDays(21);

        // Act
        leave.UpdateDates(newStart, newEnd);

        // Assert
        Assert.Equal(newStart, leave.StartDate);
        Assert.Equal(newEnd, leave.EndDate);
    }

    private static LeaveRequest CreateTestLeaveRequest()
    {
        return LeaveRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), LeaveType.Annual,
            DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(14));
    }
}

/// <summary>
/// Unit tests for LeaveBalance entity
/// </summary>
public class LeaveBalanceTests
{
    [Fact]
    public void Create_ShouldInitializeBalance()
    {
        // Act
        var balance = LeaveBalance.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            LeaveType.Annual, 2024, 12);

        // Assert
        Assert.Equal(12, balance.TotalDays);
        Assert.Equal(0, balance.UsedDays);
        Assert.Equal(12, balance.Balance);
    }

    [Fact]
    public void UseDays_ShouldDeductFromBalance()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            LeaveType.Annual, 2024, 12);

        // Act
        balance.UseDays(3);

        // Assert
        Assert.Equal(3, balance.UsedDays);
        Assert.Equal(9, balance.Balance);
    }

    [Fact]
    public void UseDays_MoreThanBalance_ShouldThrowException()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            LeaveType.Annual, 2024, 5);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => balance.UseDays(10));
    }

    [Fact]
    public void AddAllocation_ShouldIncreaseTotalDays()
    {
        // Arrange
        var balance = LeaveBalance.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            LeaveType.Annual, 2024, 12);
        var initialBalance = balance.TotalDays;

        // Act
        balance.AddAllocation(3);

        // Assert
        Assert.Equal(initialBalance + 3, balance.TotalDays);
    }
}
