# NEXTERP TESTING GUIDELINES

Project-specific testing standards for NEXTERP ERP system.

---

## TEST STRUCTURE

### Project Organization
```
ERP.Domain.UnitTests/           → Domain layer tests (325 tests)
ERP.Application.UnitTests/      → Application layer tests (272 tests)
ERP.API.ContractTests/          → API contract tests
ERP.WebUI/                      → Jest + Playwright for frontend
```

### Test Naming Convention
```
[Method]_[Scenario]_[ExpectedResult]
Examples:
- CreateEmployee_WithValidData_ReturnsSuccess
- ValidateLicense_WhenExpired_ReturnsFalse
- HasModuleAccess_WhenNotLicensed_ReturnsFalse
```

---

## UNIT TESTING (xUnit + Moq + FluentAssertions)

### Domain Layer Tests
- Entity behavior validation
- Value object immutability
- Business rule enforcement
- Domain event emission

### Application Layer Tests
- Command/Query handlers
- Validator behavior
- License validation logic
- DTO mapping

### Example Structure
```csharp
public class CreateEmployeeCommandTests
{
    private readonly IMock<IEmployeeRepository> _mockRepo;
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandTests()
    {
        _mockRepo = new Mock<IEmployeeRepository>();
        _handler = new CreateEmployeeCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsEmployeeId()
    {
        // Arrange
        var command = new CreateEmployeeCommand { ... };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }
}
```

---

## TEST COVERAGE TARGETS

| Layer | Target | Critical Paths |
|-------|--------|---------------|
| Domain | 90%+ | Entity invariants, business rules |
| Application | 80%+ | Handlers, validators, behaviors |
| Infrastructure | 70%+ | Repository implementations |
| API | 60%+ | Controller endpoints |

---

## LICENSE VALIDATION TESTS

### Required Test Cases
```
✓ ValidateLicense_WhenValid_ReturnsTrue
✓ ValidateLicense_WhenExpired_ReturnsFalse
✓ ValidateLicense_WhenTampered_ReturnsFalse
✓ HasModuleAccess_WhenLicensed_ReturnsTrue
✓ HasModuleAccess_WhenNotLicensed_ReturnsFalse
✓ SuperAdmin_BypassesLicenseCheck
✓ OrganizationNotFound_ReturnsUnauthorized
✓ LicenseIntegrity_HashMismatch_DetectsTampering
```

### Mock Strategy
```csharp
// Mock ILicenseService
_mockLicenseService.Setup(s => s.IsLicenseValidAsync(It.IsAny<Guid>()))
    .ReturnsAsync(true);

_mockLicenseService.Setup(s => s.HasModuleAccessAsync(It.IsAny<Guid>(), It.IsAny<string>()))
    .ReturnsAsync(true);

// Mock ICurrentUserService
_mockCurrentUser.Setup(c => c.OrganizationId).Returns(orgId);
_mockCurrentUser.Setup(c => c.IsSuperAdmin).Returns(false);
```

---

## INTEGRATION TESTING

### Testcontainers
Use Testcontainers for PostgreSQL:
```csharp
var container = new PostgreSQLBuilder()
    .WithImage("postgres:16-alpine")
    .Build();
await container.StartAsync();
```

### API Contract Tests
- Validate response format
- Check HTTP status codes
- Verify pagination metadata

---

## FRONTEND TESTING (Jest + Playwright)

### Unit Tests
```bash
npm test                    # Run Jest tests
npm run test:coverage       # With coverage report
```

### E2E Tests
```bash
npm run test:e2e           # Run Playwright tests
```

### Test Structure
```
ERP.WebUI/
├── src/
│   ├── __tests__/         # Unit tests
│   └── components/        # Component tests
└── e2e/                   # Playwright tests
```

---

## CONTINUOUS TESTING

### CI Pipeline
```yaml
test:
  - dotnet test --configuration Release
  - npm test -- --coverage
  - npm run test:e2e
```

### Pre-commit Hooks
- Run unit tests before commit
- Check linting errors
- Validate TypeScript types

---

**Auto-loaded for:** Testing tasks in NEXTERP
