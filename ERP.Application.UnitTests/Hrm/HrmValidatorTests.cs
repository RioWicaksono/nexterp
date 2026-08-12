using ERP.Application.Hrm.Commands.Employees;
using ERP.Application.Hrm.Commands.Leaves;
using ERP.Domain.Hrm.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace ERP.Application.UnitTests.Hrm;

/// <summary>
/// Unit tests for HRM domain validators
/// </summary>
public class HrmValidatorTests
{
    #region CreateEmployeeCommandValidator Tests

    [Fact]
    public void CreateEmployeeCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Gender = "Male",
            MaritalStatus = "Single",
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            EmploymentType = "FullTime",
            HireDate = DateTime.UtcNow.AddMonths(-6),
            PersonalEmail = "john.doe@personal.com",
            Phone = "+6281234567890",
            Mobile = "+6289876543210"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateEmployeeCommand_EmployeeNumber_WhenEmptyOrNull_ShouldFail(string? number)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = number!,
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeNumber)
            .WithErrorMessage("Employee number is required");
    }

    [Fact]
    public void CreateEmployeeCommand_EmployeeNumber_WhenExceeds50Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = new string('A', 51),
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeNumber)
            .WithErrorMessage("Employee number cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateEmployeeCommand_FirstName_WhenEmptyOrNull_ShouldFail(string? firstName)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = firstName!,
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void CreateEmployeeCommand_FirstName_WhenExceeds100Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = new string('A', 101),
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 100 characters");
    }

    [Fact]
    public void CreateEmployeeCommand_LastName_WhenExceeds100Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = new string('A', 101),
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 100 characters");
    }

    [Fact]
    public void CreateEmployeeCommand_DateOfBirth_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = default,
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Date of birth is required");
    }

    [Fact]
    public void CreateEmployeeCommand_DateOfBirth_WhenTooRecent_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-17), // Only 17 years old
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Employee must be at least 18 years old");
    }

    [Fact]
    public void CreateEmployeeCommand_DateOfBirth_WhenExactly18YearsOld_ShouldFail()
    {
        // Arrange - validator uses LessThan, so exactly 18 fails
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-18).AddDays(1), // Just turned 18
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert - validator requires < 18 years, so exactly 18 fails
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("Employee must be at least 18 years old");
    }

    [Fact]
    public void CreateEmployeeCommand_DateOfBirth_WhenOver18YearsOld_ShouldPass()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-19), // Clearly over 18
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void CreateEmployeeCommand_DepartmentId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.Empty,
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
            .WithErrorMessage("Department is required");
    }

    [Fact]
    public void CreateEmployeeCommand_PositionId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.Empty,
            HireDate = DateTime.UtcNow
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionId)
            .WithErrorMessage("Position is required");
    }

    [Fact]
    public void CreateEmployeeCommand_HireDate_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = default
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HireDate)
            .WithErrorMessage("Hire date is required");
    }

    [Fact]
    public void CreateEmployeeCommand_HireDate_WhenTooFarInFuture_ShouldFail()
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HireDate)
            .WithErrorMessage("Hire date cannot be in the future");
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Other")]
    [InlineData("male")]     // Case insensitive
    [InlineData("FEMALE")]     // Case insensitive
    public void CreateEmployeeCommand_Gender_WhenValidValue_ShouldPass(string gender)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow,
            Gender = gender
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void CreateEmployeeCommand_Gender_WhenInvalidValue_ShouldFail(string gender)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow,
            Gender = gender
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorMessage("Invalid gender. Valid values: Male, Female, Other");
    }

    [Theory]
    [InlineData("Single")]
    [InlineData("Married")]
    [InlineData("Divorced")]
    [InlineData("Widowed")]
    [InlineData("single")]     // Case insensitive
    [InlineData("MARRIED")]     // Case insensitive
    public void CreateEmployeeCommand_MaritalStatus_WhenValidValue_ShouldPass(string status)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow,
            MaritalStatus = status
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Widow")]
    [InlineData("Separated")]
    [InlineData("")]
    public void CreateEmployeeCommand_MaritalStatus_WhenInvalidValue_ShouldFail(string status)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow,
            MaritalStatus = status
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus)
            .WithErrorMessage("Invalid marital status. Valid values: Single, Married, Divorced, Widowed");
    }

    [Theory]
    [InlineData("FullTime")]
    [InlineData("PartTime")]
    [InlineData("Contract")]
    [InlineData("Probation")]
    [InlineData("Intern")]
    [InlineData("Freelance")]
    [InlineData("fulltime")]     // Case insensitive
    [InlineData("CONTRACT")]     // Case insensitive
    public void CreateEmployeeCommand_EmploymentType_WhenValidValue_ShouldPass(string type)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow,
            EmploymentType = type
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EmploymentType);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Permanent")]
    [InlineData("Temporary")]
    [InlineData("")]
    public void CreateEmployeeCommand_EmploymentType_WhenInvalidValue_ShouldFail(string type)
    {
        // Arrange
        var validator = new CreateEmployeeCommandValidator();
        var command = new CreateEmployeeCommand
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            HireDate = DateTime.UtcNow,
            EmploymentType = type
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType)
            .WithErrorMessage("Invalid employment type. Valid values: FullTime, PartTime, Contract, Probation, Intern, Freelance");
    }

    #endregion

    #region CreateLeaveRequestCommandValidator Tests

    [Fact]
    public void CreateLeaveRequestCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(14),
            Reason = "Family vacation",
            HalfDay = 0
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateLeaveRequestCommand_EmployeeId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.Empty,
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(14)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId)
            .WithErrorMessage("Employee is required");
    }

    [Fact]
    public void CreateLeaveRequestCommand_LeaveType_WhenInvalidEnumValue_ShouldFail()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = "InvalidType",
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(14)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LeaveType)
            .WithErrorMessage("Invalid leave type");
    }

    [Theory]
    [InlineData(LeaveType.Annual)]
    [InlineData(LeaveType.Sick)]
    [InlineData(LeaveType.Emergency)]
    [InlineData(LeaveType.Maternity)]
    [InlineData(LeaveType.Paternity)]
    [InlineData(LeaveType.Unpaid)]
    [InlineData(LeaveType.Other)]
    public void CreateLeaveRequestCommand_LeaveType_WhenValidEnumValue_ShouldPass(LeaveType leaveType)
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = leaveType.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(14)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LeaveType);
    }

    [Fact]
    public void CreateLeaveRequestCommand_StartDate_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = default,
            EndDate = DateTime.UtcNow.Date.AddDays(14)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date is required");
    }

    [Fact]
    public void CreateLeaveRequestCommand_StartDate_WhenInPast_ShouldFail()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(-1),
            EndDate = DateTime.UtcNow.Date.AddDays(7)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Start date cannot be in the past");
    }

    [Fact]
    public void CreateLeaveRequestCommand_StartDate_WhenToday_ShouldPass()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(7)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void CreateLeaveRequestCommand_EndDate_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = default
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("End date is required");
    }

    [Fact]
    public void CreateLeaveRequestCommand_EndDate_WhenBeforeStartDate_ShouldFail()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(14),
            EndDate = DateTime.UtcNow.Date.AddDays(7)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("End date cannot be before start date");
    }

    [Fact]
    public void CreateLeaveRequestCommand_EndDate_WhenSameAsStartDate_ShouldPass()
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(7)
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.5)]
    [InlineData(1.5)]
    [InlineData(2)]
    public void CreateLeaveRequestCommand_HalfDay_WhenOutOfRange_ShouldFail(decimal halfDay)
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(14),
            HalfDay = halfDay
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HalfDay)
            .WithErrorMessage("Half day must be between 0 and 1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void CreateLeaveRequestCommand_HalfDay_WhenInRange_ShouldPass(decimal halfDay)
    {
        // Arrange
        var validator = new CreateLeaveRequestCommandValidator();
        var command = new CreateLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual.ToString(),
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(14),
            HalfDay = halfDay
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HalfDay);
    }

    #endregion

    #region ApproveLeaveRequestCommandValidator Tests

    [Fact]
    public void ApproveLeaveRequestCommand_ValidApproval_ShouldPass()
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = true,
            Reason = "Approved for vacation"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ApproveLeaveRequestCommand_LeaveRequestId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.Empty,
            ApproverId = Guid.NewGuid(),
            Approved = true
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LeaveRequestId)
            .WithErrorMessage("Leave request ID is required");
    }

    [Fact]
    public void ApproveLeaveRequestCommand_ApproverId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.NewGuid(),
            ApproverId = Guid.Empty,
            Approved = true
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApproverId)
            .WithErrorMessage("Approver ID is required");
    }

    [Fact]
    public void ApproveLeaveRequestCommand_RejectionWithoutReason_ShouldFail()
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = false,
            Reason = null
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Rejection reason is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ApproveLeaveRequestCommand_RejectionWithEmptyReason_ShouldFail(string? reason)
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = false,
            Reason = reason
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Rejection reason is required");
    }

    [Fact]
    public void ApproveLeaveRequestCommand_RejectionWithReason_ShouldPass()
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = false,
            Reason = "Insufficient leave balance for requested period"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void ApproveLeaveRequestCommand_ApprovalWithoutReason_ShouldPass()
    {
        // Arrange
        var validator = new ApproveLeaveRequestCommandValidator();
        var command = new ApproveLeaveRequestCommand
        {
            LeaveRequestId = Guid.NewGuid(),
            ApproverId = Guid.NewGuid(),
            Approved = true,
            Reason = null
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
