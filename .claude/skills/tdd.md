# NEXTERP TDD WORKFLOW

Project-specific TDD (Test-Driven Development) guidelines for NEXTERP ERP.

---

## TDD CYCLE

### Red-Green-Refactor
```
1. RED:   Write failing test first
2. GREEN: Write minimal code to pass
3. REFACTOR: Clean up code while keeping tests green
```

### For Each Feature
1. Write unit test defining expected behavior
2. Implement minimal code to satisfy test
3. Refactor for cleanliness
4. Repeat until feature complete

---

## DOMAIN LAYER TDD

### Entity Tests
```csharp
[Fact]
public void Employee_SetInactiveStatus_ThrowsWhenHasActiveAssignments()
{
    // Arrange
    var employee = CreateEmployeeWithActiveAssignments();
    
    // Act & Assert
    var action = () => employee.SetStatus(EmployeeStatus.Inactive);
    action.Should().Throw<DomainException>();
}
```

### Value Object Tests
```csharp
[Fact]
public void Email_CreateWithInvalidFormat_ThrowsException()
{
    // Arrange & Act & Assert
    var action = () => Email.Create("invalid-email");
    action.Should().Throw<InvalidEmailException>();
}
```

### Business Rule Tests
```csharp
[Fact]
public void LeaveRequest_ExceedsBalance_ThrowsException()
{
    // Arrange
    var employee = CreateEmployeeWithLeaveBalance(5);
    var request = new LeaveRequest(employee.Id, leaveTypeId, 10);
    
    // Act & Assert
    request.IsValid().Should().BeFalse();
    request.Errors.Should().Contain("Insufficient leave balance");
}
```

---

## APPLICATION LAYER TDD

### Command Tests
```csharp
[Fact]
public async Task CreateEmployeeCommand_ValidData_ReturnsSuccess()
{
    // Arrange
    var command = new CreateEmployeeCommand(
        FirstName: "John",
        LastName: "Doe",
        Email: "john.doe@company.com",
        DepartmentId: departmentId
    );
    
    _mockRepo.Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Employee e, CancellationToken _) => e);
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
}
```

### Validator Tests
```csharp
[Fact]
public void CreateEmployeeCommand_EmptyEmail_FailsValidation()
{
    // Arrange
    var command = new CreateEmployeeCommand { Email = "" };
    var validator = new CreateEmployeeCommandValidator();
    
    // Act
    var result = validator.Validate(command);
    
    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "Email");
}
```

### License Validation Tests
```csharp
[Fact]
public async Task LicenseValidationBehavior_SuperAdmin_BypassesCheck()
{
    // Arrange
    var request = new RequiresModuleRequest { ModuleCode = "HRM" };
    _mockCurrentUser.Setup(c => c.IsSuperAdmin).Returns(true);
    
    // Act
    var result = await _handler.Handle(request, CancellationToken.None);
    
    // Assert
    result.Success.Should().BeTrue();
    _mockLicenseService.Verify(s => s.IsLicenseValidAsync(It.IsAny<Guid>()), Times.Never);
}
```

---

## TESTING PATTERNS

### Arrange-Act-Assert (AAA)
```csharp
[Fact]
public void TestName_Scenario_ExpectedResult()
{
    // Arrange: Set up test data and dependencies
    var service = new ServiceUnderTest(CreateMockDependencies());
    
    // Act: Execute the method being tested
    var result = service.MethodUnderTest();
    
    // Assert: Verify the expected outcome
    result.Should().Be(expectedValue);
}
```

### Given-When-Then (GWT)
```csharp
[Fact]
public void Given_ValidLicense_When_AccessingModule_Then_AccessGranted()
{
    // Given: Valid license exists
    SetupValidLicense(organizationId, "HRM");
    
    // When: User accesses HRM module
    var result = _service.ValidateModuleAccess(organizationId, "HRM");
    
    // Then: Access is granted
    result.HasAccess.Should().BeTrue();
}
```

---

## MOCKING STRATEGY

### Repository Mocks
```csharp
_mockEmployeeRepository
    .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Employee e, CancellationToken _) => 
    {
        e.Id = Guid.NewGuid();
        return e;
    });
```

### Service Mocks
```csharp
_mockLicenseService
    .Setup(s => s.IsLicenseValidAsync(It.IsAny<Guid>()))
    .ReturnsAsync(true);

_mockLicenseService
    .Setup(s => s.HasModuleAccessAsync(It.IsAny<Guid>(), It.IsAny<string>()))
    .ReturnsAsync((Guid orgId, string module) => module != "BANNED_MODULE");
```

### Null Object Pattern
```csharp
_mockAuditService = new Mock<ILicenseAuditService>();
_mockAuditService.Setup(s => s.LogValidationAttemptAsync(
    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>()))
    .Returns(Task.CompletedTask);
```

---

## COVERAGE REQUIREMENTS

| Component | Minimum Coverage |
|-----------|------------------|
| Domain Entities | 90% |
| Domain Services | 85% |
| Value Objects | 100% |
| Application Handlers | 80% |
| Validators | 90% |
| License Behaviors | 100% |

---

## RUNNING TESTS

### Backend
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test ERP.Application.UnitTests

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreateEmployeeCommandTests"
```

### Frontend
```bash
# Run Jest tests
npm test

# Run with coverage
npm run test:coverage

# Run specific test
npm test -- --testPathPattern="EmployeeForm"
```

---

**Auto-loaded for:** TDD workflow in NEXTERP
