using ERP.Application.Hrm.Commands.Employees;
using ERP.Application.Hrm.Commands.Overtimes;
using ERP.Application.Hrm.Commands.Departments;
using ERP.Application.Hrm.Commands.Leaves;
using ERP.Domain.Hrm.Entities;
using ERP.Domain.Hrm.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace ERP.Application.UnitTests.Hrm;

/// <summary>
/// Unit tests for HRM Command validators
/// </summary>
public class HrmCommandValidatorTests
{
    #region UpdateEmployeeCommandValidator Tests

    [Fact]
    public void UpdateEmployeeCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new UpdateEmployeeCommandValidator();
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Updated",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Gender = "Male",
            MaritalStatus = "Married",
            Phone = "+6281234567890"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateEmployeeCommand_EmptyEmployeeId_ShouldFail()
    {
        // Arrange
        var validator = new UpdateEmployeeCommandValidator();
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = Guid.Empty,
            FirstName = "John"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void UpdateEmployeeCommand_FirstNameExceeds100_ShouldFail()
    {
        // Arrange
        var validator = new UpdateEmployeeCommandValidator();
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = new string('A', 101)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 100 characters");
    }

    [Fact]
    public void UpdateEmployeeCommand_InvalidGender_ShouldFail()
    {
        // Arrange
        var validator = new UpdateEmployeeCommandValidator();
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = "John",
            Gender = "InvalidGender"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void UpdateEmployeeCommand_InvalidEmploymentType_ShouldFail()
    {
        // Arrange
        var validator = new UpdateEmployeeCommandValidator();
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = Guid.NewGuid(),
            EmploymentType = "InvalidType"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType);
    }

    #endregion

    #region UpdateEmployeeStatusCommandValidator Tests

    [Fact]
    public void UpdateEmployeeStatusCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new UpdateEmployeeStatusCommandValidator();
        var command = new UpdateEmployeeStatusCommand
        {
            EmployeeId = Guid.NewGuid(),
            Status = "Active"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateEmployeeStatusCommand_EmptyStatus_ShouldFail()
    {
        // Arrange
        var validator = new UpdateEmployeeStatusCommandValidator();
        var command = new UpdateEmployeeStatusCommand
        {
            EmployeeId = Guid.NewGuid(),
            Status = ""
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status is required");
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("OnLeave")]
    [InlineData("Suspended")]
    [InlineData("Terminated")]
    [InlineData("Resigned")]
    public void UpdateEmployeeStatusCommand_ValidStatusValues_ShouldPass(string status)
    {
        // Arrange
        var validator = new UpdateEmployeeStatusCommandValidator();
        var command = new UpdateEmployeeStatusCommand
        {
            EmployeeId = Guid.NewGuid(),
            Status = status
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    #endregion

    #region DeleteEmployeeCommandValidator Tests

    [Fact]
    public void DeleteEmployeeCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new DeleteEmployeeCommandValidator();
        var command = new DeleteEmployeeCommand
        {
            EmployeeId = Guid.NewGuid(),
            Reason = "Employee left company"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteEmployeeCommand_EmptyEmployeeId_ShouldFail()
    {
        // Arrange
        var validator = new DeleteEmployeeCommandValidator();
        var command = new DeleteEmployeeCommand
        {
            EmployeeId = Guid.Empty
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    #endregion

    #region CreateOvertimeRequestCommandValidator Tests

    [Fact]
    public void CreateOvertimeRequestCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateOvertimeRequestCommandValidator();
        var command = new CreateOvertimeRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            WorkDate = DateTime.UtcNow.Date.AddDays(1),
            StartTime = "09:00",
            EndTime = "12:00",
            OvertimeType = "WeekdayOvertime",
            Reason = "Project deadline"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateOvertimeRequestCommand_InvalidTimeFormat_ShouldFail()
    {
        // Arrange
        var validator = new CreateOvertimeRequestCommandValidator();
        var command = new CreateOvertimeRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            WorkDate = DateTime.UtcNow.Date.AddDays(1),
            StartTime = "25:00", // Invalid hour
            EndTime = "12:00",
            OvertimeType = "WeekdayOvertime"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void CreateOvertimeRequestCommand_InvalidOvertimeType_ShouldFail()
    {
        // Arrange
        var validator = new CreateOvertimeRequestCommandValidator();
        var command = new CreateOvertimeRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            WorkDate = DateTime.UtcNow.Date.AddDays(1),
            StartTime = "09:00",
            EndTime = "12:00",
            OvertimeType = "InvalidType"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OvertimeType);
    }

    [Theory]
    [InlineData("WeekdayOvertime")]
    [InlineData("WeekendOvertime")]
    [InlineData("HolidayOvertime")]
    [InlineData("DailyOvertime")]
    public void CreateOvertimeRequestCommand_ValidOvertimeType_ShouldPass(string overtimeType)
    {
        // Arrange
        var validator = new CreateOvertimeRequestCommandValidator();
        var command = new CreateOvertimeRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            WorkDate = DateTime.UtcNow.Date.AddDays(1),
            StartTime = "09:00",
            EndTime = "12:00",
            OvertimeType = overtimeType
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OvertimeType);
    }

    #endregion

    #region ApproveOvertimeRequestCommandValidator Tests

    [Fact]
    public void ApproveOvertimeRequestCommand_ValidApproval_ShouldPass()
    {
        // Arrange
        var validator = new ApproveOvertimeRequestCommandValidator();
        var command = new ApproveOvertimeRequestCommand
        {
            OvertimeRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = true,
            ApprovedHours = 3
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ApproveOvertimeRequestCommand_ApprovedHoursExceeds4_ShouldFail()
    {
        // Arrange
        var validator = new ApproveOvertimeRequestCommandValidator();
        var command = new ApproveOvertimeRequestCommand
        {
            OvertimeRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = true,
            ApprovedHours = 5 // Max is 4
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApprovedHours)
            .WithErrorMessage("Approved hours must be between 0 and 4");
    }

    #endregion

    #region CreateDepartmentCommandValidator Tests

    [Fact]
    public void CreateDepartmentCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateDepartmentCommandValidator();
        var command = new CreateDepartmentCommand
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Engineering",
            Code = "ENG",
            Description = "Software engineering team"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateDepartmentCommand_EmptyName_ShouldFail()
    {
        // Arrange
        var validator = new CreateDepartmentCommandValidator();
        var command = new CreateDepartmentCommand
        {
            OrganizationId = Guid.NewGuid(),
            Name = "",
            Code = "ENG"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Department name is required");
    }

    [Fact]
    public void CreateDepartmentCommand_NameExceeds100Chars_ShouldFail()
    {
        // Arrange
        var validator = new CreateDepartmentCommandValidator();
        var command = new CreateDepartmentCommand
        {
            OrganizationId = Guid.NewGuid(),
            Name = new string('A', 101),
            Code = "ENG"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateDepartmentCommand_CodeExceeds20Chars_ShouldFail()
    {
        // Arrange
        var validator = new CreateDepartmentCommandValidator();
        var command = new CreateDepartmentCommand
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Engineering",
            Code = new string('E', 21)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    #endregion

    #region CreatePositionCommandValidator Tests

    [Fact]
    public void CreatePositionCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreatePositionCommandValidator();
        var command = new CreatePositionCommand
        {
            OrganizationId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Title = "Senior Software Engineer",
            Grade = 5,
            MinSalary = 10000000,
            MaxSalary = 20000000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePositionCommand_EmptyTitle_ShouldFail()
    {
        // Arrange
        var validator = new CreatePositionCommandValidator();
        var command = new CreatePositionCommand
        {
            OrganizationId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Title = "",
            Grade = 1
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreatePositionCommand_GradeZero_ShouldFail()
    {
        // Arrange
        var validator = new CreatePositionCommandValidator();
        var command = new CreatePositionCommand
        {
            OrganizationId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Title = "Engineer",
            Grade = 0
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Grade);
    }

    [Fact]
    public void CreatePositionCommand_MaxSalaryLessThanMin_ShouldFail()
    {
        // Arrange
        var validator = new CreatePositionCommandValidator();
        var command = new CreatePositionCommand
        {
            OrganizationId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Title = "Engineer",
            Grade = 1,
            MinSalary = 20000000,
            MaxSalary = 10000000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxSalary);
    }

    #endregion

    #region SetLeaveBalanceCommandValidator Tests

    [Fact]
    public void SetLeaveBalanceCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new SetLeaveBalanceCommandValidator();
        var command = new SetLeaveBalanceCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = "Annual",
            Year = 2024,
            TotalDays = 12,
            CarryForwardDays = 3
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SetLeaveBalanceCommand_InvalidLeaveType_ShouldFail()
    {
        // Arrange
        var validator = new SetLeaveBalanceCommandValidator();
        var command = new SetLeaveBalanceCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = "InvalidType",
            Year = 2024,
            TotalDays = 12
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LeaveType);
    }

    [Fact]
    public void SetLeaveBalanceCommand_YearOutOfRange_ShouldFail()
    {
        // Arrange
        var validator = new SetLeaveBalanceCommandValidator();
        var command = new SetLeaveBalanceCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = "Annual",
            Year = 1999,
            TotalDays = 12
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void SetLeaveBalanceCommand_NegativeTotalDays_ShouldFail()
    {
        // Arrange
        var validator = new SetLeaveBalanceCommandValidator();
        var command = new SetLeaveBalanceCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = "Annual",
            Year = 2024,
            TotalDays = -5
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TotalDays);
    }

    #endregion
}
