# NEXTERP Testing Strategy

This document outlines the comprehensive testing strategy for the NEXTERP ERP system.

## 🧪 Test Pyramid

```
                        ┌─────────────┐
                        │    E2E      │  ← Playwright
                        ├─────────────┤
                        │ Contract    │  ← Pact
                        ├─────────────┤
                        │ Integration │  ← xUnit (API)
                        ├─────────────┤
                        │   Unit      │  ← xUnit + Jest
                        └─────────────┘
```

## 📁 Test Project Structure

```
d:\RW\PROJECT RW\ERP\
├── ERP.Domain.UnitTests/           → Domain entity tests (xUnit)
├── ERP.Application.UnitTests/      → Application layer tests (xUnit + Moq)
├── ERP.API.ContractTests/          → Pact contract tests
├── ERP.Infrastructure.UnitTests/  → (future) Infrastructure tests
└── ERP.WebUI/
    ├── src/**/*.test.{ts,tsx}      → Jest unit tests
    ├── src/**/*.spec.{ts,tsx}      → Component tests
    └── e2e/                        → Playwright E2E tests
        ├── app.spec.ts             → Landing page & auth
        └── critical-flows.spec.ts   → Critical user journeys
```

## 🎯 Coverage Targets

| Layer | Type | Target | Current |
|-------|------|--------|---------|
| Domain | Unit | 90% | - |
| Application | Unit | 80% | - |
| Infrastructure | Unit | 70% | - |
| API | Integration | 60% | - |
| Frontend | Unit | 50% | - |
| E2E | End-to-End | Critical paths | ✅ |

## 🚀 Quick Start

### Backend Tests

```bash
# Navigate to solution
cd d:\RW\PROJECT RW\ERP

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific project
dotnet test ERP.Domain.UnitTests

# Run with verbose output
dotnet test -v n
```

### Frontend Tests

```bash
cd ERP.WebUI

# Install dependencies
npm install

# Run unit tests
npm test

# Run with coverage
npm run test:coverage

# Run in watch mode
npm run test:watch

# Run E2E tests
npm run test:e2e

# Run E2E with UI
npm run test:e2e:ui
```

### Mutation Testing

```bash
# Backend (Stryker.NET)
cd d:\RW\PROJECT RW\ERP
dotnet tool install --global dotnet-stryker 2>/dev/null || true
dotnet stryker

# Frontend (jest-stryker)
cd ERP.WebUI
npm install
npm run test:mutation

# View report
open reports/stryker/index.html
```

### Contract Testing

```bash
# Prerequisites: Start API
cd ERP.API && dotnet run &

# Run consumer tests (generate pacts)
dotnet test ERP.API.ContractTests --filter "Consumer"

# Run provider tests (verify contract)
dotnet test ERP.API.ContractTests --filter "Provider"

# View generated pacts
ls ERP.API.ContractTests/pacts/
```

## 🔬 Test Types

### 1. Unit Tests

#### Domain Layer (xUnit)
- Entity business rules
- Value object validation
- Domain event emission
- Invariant enforcement

```csharp
[Fact]
public void Order_AddItem_UpdatesTotal()
{
    var order = new Order();
    var item = new OrderItem(Product.Create("SKU-001", "Test Product", 100m), 2);

    order.AddItem(item);

    Assert.Equal(200m, order.TotalAmount);
}
```

#### Application Layer (xUnit + Moq)
- Command/Query handlers
- Validation rules
- Authorization logic
- DTO mapping

```csharp
[Fact]
public async Task CreateOrderCommandHandler_ValidCommand_ReturnsResult()
{
    var mockRepo = new Mock<IOrderRepository>();
    mockRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), default))
            .ReturnsAsync((Order o, CancellationToken _) => o);

    var handler = new CreateOrderCommandHandler(mockRepo.Object);
    var command = new CreateOrderCommand(/* ... */);

    var result = await handler.Handle(command);

    Assert.True(result.IsSuccess);
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), default), Times.Once);
}
```

#### Frontend Layer (Jest)
- Hook logic
- Utility functions
- State management
- API clients

```typescript
describe('useAuth', () => {
  it('returns user after login', async () => {
    const { result } = renderHook(() => useAuth());

    await act(async () => {
      await result.current.login('admin@test.com', 'password');
    });

    expect(result.current.user).toBeDefined();
    expect(result.current.isAuthenticated).toBe(true);
  });
});
```

### 2. Integration Tests

#### API Integration (xUnit + TestServer)
- Full HTTP request/response
- Database operations
- Authentication flow
- Error handling

```csharp
[Fact]
public async Task AuthController_Login_ValidCredentials_ReturnsToken()
{
    var app = new WebApplicationFactory<Program>();
    var client = app.CreateClient();

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
        Email = "admin@nexterp.com",
        Password = "Admin@123!"
    });

    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsJsonAsync<LoginResponse>();
    Assert.NotNull(content.Token);
}
```

### 3. Contract Tests (Pact)

#### Consumer Tests
Define expected API behavior from consumer perspective.

```csharp
[Fact]
public async Task Login_ReturnsExpectedStructure()
{
    _pact
        .UponReceiving("valid login request")
            .WithRequest(HttpMethod.Post, "/api/auth/login")
            .WithBody(new { Email = "test@test.com", Password = "pass" })
        .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithBody(new {
                Token = Match.Type("jwt..."),
                User = Match.Type(new { Id = "", Email = "" })
            });

    await _pact.VerifyAsync(async ctx => {
        var response = await ctx.MakeRequest();
        Assert.Equal(200, (int)response.StatusCode);
    });
}
```

### 4. E2E Tests (Playwright)

#### Critical User Flows
- Authentication (login, logout, session)
- Dashboard navigation
- CRUD operations
- Error handling
- Performance benchmarks

```typescript
test('complete order creation flow', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill('admin@nexterp.com');
    await page.getByLabel('Password').fill('password');
    await page.getByRole('button', { name: 'Sign In' }).click();

    await expect(page).toHaveURL('/dashboard', { timeout: 10000 });

    await page.goto('/sales/orders/new');
    await page.getByLabel('Customer').selectOption('Acme Corp');
    await page.getByRole('button', { name: 'Add Item' }).click();

    // ... complete order
    await page.getByRole('button', { name: 'Submit Order' }).click();
    await expect(page.getByText('Order created successfully')).toBeVisible();
});
```

### 5. Mutation Tests

#### Stryker.NET (Backend)
Mutates code to verify tests actually catch bugs.

```bash
# Run mutation tests
dotnet stryker

# View report
open reports/stryker/index.html
```

#### jest-stryker (Frontend)
```bash
npm run test:mutation
open reports/stryker/index.html
```

## 📊 CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Tests

on: [push, pull_request]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet test --verbosity normal --collect:"XPlat Code Coverage"
      - uses: codecov/codecov-action@v3

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
      - run: npm ci
      - run: npm test -- --coverage
      - run: npm run test:mutation
        continue-on-error: true  # Mutation tests are informational

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
      - run: npm ci
      - run: npx playwright install --with-deps
      - run: npm run test:e2e
        env:
          API_BASE_URL: http://localhost:5000
```

## 🎯 Running Tests in Development

### Watch Mode (Development)

```bash
# Backend - Watch for changes
dotnet watch test

# Frontend - Watch for changes
npm run test:watch
```

### Specific Test Files

```bash
# Backend
dotnet test ERP.Domain.UnitTests --filter "FullyQualifiedName~UserTests"
dotnet test --filter "Name~Login"

# Frontend
npm test -- --testPathPattern="useAuth"
npm test -- --testNamePattern="returns user"
```

### Debug Mode

```bash
# Backend - Attach debugger
dotnet test --no-build --logger "console;verbosity=detailed"

# Frontend
npm test -- --watch --detectOpenHandles
```

## 📈 Coverage Reports

### Backend

```bash
# Generate HTML report
dotnet test --collect:"XPlat Code Coverage"

# View report (after test run)
open coverage/index.html
```

### Frontend

```bash
npm run test:coverage
open coverage/lcov-report/index.html
```

## 🐛 Debugging Failed Tests

### Backend

1. Check test output for stack trace
2. Run with verbose: `dotnet test -v n`
3. Attach debugger to test process
4. Check test logs in `TestResults/`

### Frontend

1. Run specific test: `npm test -- --testNamePattern="test name"`
2. Check console output
3. Use `--verbose` flag
4. Check test setup file (`jest.setup.js`)

### E2E

1. Run with UI: `npm run test:e2e:ui`
2. Use `test.skip()` to skip failing tests temporarily
3. Check `playwright/.auth/` for auth state
4. Use `page.screenshot()` for debugging

## 📚 Resources

- [xUnit Documentation](https://xunit.net/)
- [Playwright Documentation](https://playwright.dev/)
- [Jest Documentation](https://jestjs.io/)
- [Stryker Mutation Testing](https://stryker-mutator.io/)
- [Pact Contract Testing](https://docs.pact.io/)
- [Testing Library](https://testing-library.com/)

## 📋 Checklist

Before submitting PR:

- [ ] All unit tests pass
- [ ] Coverage meets target (Domain 90%, App 80%, etc.)
- [ ] E2E tests pass for affected flows
- [ ] No console errors in browser
- [ ] Mutation tests show good coverage (>70%)
- [ ] Contract tests pass (if API changed)

## 🤝 Contributing

When adding new features:

1. Write unit tests first (TDD)
2. Add integration tests for API
3. Update E2E tests for user flows
4. Run mutation tests to verify test quality
5. Update this document if testing strategy changes
