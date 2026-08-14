using Xunit;
using Moq;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using ERP.Application.Common.Behaviors;

namespace ERP.Application.UnitTests.Common.Licensing;

/// <summary>
/// TDD RED Phase: Tests for LicenseValidationBehavior
/// These tests define the expected behavior of license validation.
/// </summary>
public class LicenseValidationBehaviorTests
{
    private readonly Mock<ILicenseCheckService> _licenseCheckServiceMock;
    private readonly Mock<ILogger<LicenseValidationBehavior<TestCommand, TestResponse>>> _loggerMock;
    private readonly LicenseValidationBehavior<TestCommand, TestResponse> _behavior;

    public LicenseValidationBehaviorTests()
    {
        _licenseCheckServiceMock = new Mock<ILicenseCheckService>();
        _loggerMock = new Mock<ILogger<LicenseValidationBehavior<TestCommand, TestResponse>>>();

        _behavior = new LicenseValidationBehavior<TestCommand, TestResponse>(
            _licenseCheckServiceMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_ValidLicense_ShouldProceed()
    {
        // Arrange
        var request = new TestCommand();
        var validationResult = new LicenseValidationResult(true, null, "ACCOUNTING");

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ReturnsAsync(validationResult);
        _licenseCheckServiceMock.Setup(x => x.IsLicenseValidAsync())
            .ReturnsAsync(true);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse { Success = true }));

        // Act
        var result = await _behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RequestWithoutModuleAttribute_ShouldProceedWithoutCheck()
    {
        // Arrange - Request without RequireModuleAttribute
        var request = new TestCommandWithoutModule();

        // Create behavior for the different request type
        var behavior = new LicenseValidationBehavior<TestCommandWithoutModule, TestResponse>(
            _licenseCheckServiceMock.Object);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse { Success = true }));

        // Act
        var result = await behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert - Should proceed without any license checks
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _licenseCheckServiceMock.Verify(
            x => x.ValidateModuleAccessAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Module Access Denied Tests

    [Fact]
    public async Task Handle_ModuleNotEnabled_ShouldThrowLicenseValidationException()
    {
        // Arrange
        var request = new TestCommand();
        var validationResult = new LicenseValidationResult(false, "Module 'ACCOUNTING' is not enabled for this organization", "ACCOUNTING");

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ReturnsAsync(validationResult);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<LicenseValidationException>(
            () => _behavior.Handle(request, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("not enabled");
        exception.Message.Should().Contain("ACCOUNTING");
        exception.ModuleCode.Should().Be("ACCOUNTING");
    }

    [Fact]
    public async Task Handle_LicenseExpired_ShouldThrowLicenseValidationException()
    {
        // Arrange
        var request = new TestCommand();
        var validationResult = new LicenseValidationResult(false, "License is not valid or has expired", null);

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ReturnsAsync(validationResult);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<LicenseValidationException>(
            () => _behavior.Handle(request, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("not valid");
        exception.ModuleCode.Should().Be("UNKNOWN");
    }

    [Fact]
    public async Task Handle_ValidLicenseButInvalidModule_ShouldThrowWithCorrectModuleCode()
    {
        // Arrange
        var request = new TestCommand();
        // Note: The module code in the exception comes from the attribute, not the validation result
        var validationResult = new LicenseValidationResult(false, "Module validation failed", "ACCOUNTING");

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ReturnsAsync(validationResult);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<LicenseValidationException>(
            () => _behavior.Handle(request, nextDelegate, CancellationToken.None));

        // Module code comes from the RequireModule attribute on the request
        exception.ModuleCode.Should().Be("ACCOUNTING");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Handle_ServiceThrows_ShouldBubbleUpException()
    {
        // Arrange
        var request = new TestCommand();

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse()));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _behavior.Handle(request, nextDelegate, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidationReturnsNullMessage_ShouldThrowWithDefaultMessage()
    {
        // Arrange
        var request = new TestCommand();
        var validationResult = new LicenseValidationResult(false, null, "ACCOUNTING");

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ReturnsAsync(validationResult);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse()));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<LicenseValidationException>(
            () => _behavior.Handle(request, nextDelegate, CancellationToken.None));

        exception.Message.Should().Be("Module access denied");
        exception.ModuleCode.Should().Be("ACCOUNTING");
    }

    #endregion

    #region Anti-Bypass Tests

    [Fact]
    public async Task Handle_DirectDatabaseModification_ShouldStillBlock()
    {
        // Arrange - Simulate that database was tampered but our service still returns correct state
        var request = new TestCommand();
        // Even if DB was modified, the service should detect tampering
        var validationResult = new LicenseValidationResult(false, "License validation failed", null);

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("ACCOUNTING", null))
            .ReturnsAsync(validationResult);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse()));

        // Act & Assert
        await Assert.ThrowsAsync<LicenseValidationException>(
            () => _behavior.Handle(request, nextDelegate, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RequestWithFeatureName_ShouldPassFeatureNameToService()
    {
        // Arrange
        var request = new TestCommandWithFeature();
        var validationResult = new LicenseValidationResult(true, null, "HRM");

        _licenseCheckServiceMock.Setup(x => x.ValidateModuleAccessAsync("HRM", "Attendance"))
            .ReturnsAsync(validationResult);

        var behavior = new LicenseValidationBehavior<TestCommandWithFeature, TestResponse>(
            _licenseCheckServiceMock.Object);

        var nextDelegate = new RequestHandlerDelegate<TestResponse>(() => Task.FromResult(new TestResponse { Success = true }));

        // Act
        var result = await behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        _licenseCheckServiceMock.Verify(x => x.ValidateModuleAccessAsync("HRM", "Attendance"), Times.Once);
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Exception Tests

    [Fact]
    public void LicenseValidationException_ShouldContainModuleCode()
    {
        // Arrange & Act
        var exception = new LicenseValidationException("Test error", "SALES");

        // Assert
        exception.Message.Should().Be("Test error");
        exception.ModuleCode.Should().Be("SALES");
    }

    [Fact]
    public void LicenseValidationException_WithNullModuleCode_ShouldDefaultToUnknown()
    {
        // Arrange & Act
        var exception = new LicenseValidationException("Test error", null);

        // Assert
        exception.ModuleCode.Should().Be("UNKNOWN");
    }

    #endregion
}

// Test classes with RequireModule attribute on class level
[RequireModule("ACCOUNTING")]
public class TestCommand : IRequest<TestResponse>
{
    public string? Data { get; set; }
}

[RequireModule("HRM", "Attendance")]
public class TestCommandWithFeature : IRequest<TestResponse>
{
    public string? Data { get; set; }
}

public class TestCommandWithoutModule : IRequest<TestResponse>
{
    public string? Data { get; set; }
}

public class TestResponse
{
    public bool Success { get; set; } = true;
}
