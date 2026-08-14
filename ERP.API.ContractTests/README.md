# Contract Testing with Pact

This project uses **Pact** for consumer-driven contract testing between the NEXTERP Frontend and Backend API.

## Overview

Contract testing ensures that the API provider (ERP.API) and API consumer (Frontend) agree on the API contract, preventing breaking changes from being introduced.

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│                 │         │                 │         │                 │
│  Frontend       │ ──────> │  Pact Broker    │ ──────> │  API Provider   │
│  (Consumer)     │ pacts   │  (Optional)     │ pacts   │  (ERP.API)      │
│                 │         │                 │         │                 │
└─────────────────┘         └─────────────────┘         └─────────────────┘
      │                           │                           │
      │    1. Write consumer      │   2. Publish pacts        │   3. Verify pacts
      │       tests               │      to broker            │      against API
      └───────────────────────────┴───────────────────────────┘
```

## Project Structure

```
ERP.API.ContractTests/
├── Consumer/
│   ├── AuthConsumerContractTests.cs    → Defines auth API contract
│   └── HealthConsumerContractTests.cs  → Defines health check contract
├── Provider/
│   └── ErpApiProviderContractTests.cs  → Verifies API meets contract
├── ErpApiContractTests.cs              → Base configuration
└── README.md
```

## Running Tests

### Prerequisites

```bash
# Start the API
cd ERP.API && dotnet run

# Or use Docker Compose
docker-compose up -d api
```

### Run Consumer Tests (Generate Pacts)

Consumer tests define the expected API behavior from the frontend perspective.

```bash
# Run all consumer tests
dotnet test ERP.API.ContractTests --filter "Consumer"

# Run specific consumer tests
dotnet test ERP.API.ContractTests --filter "AuthConsumerContractTests"

# View generated pact files
ls -la ERP.API.ContractTests/pacts/
```

### Run Provider Tests (Verify Contract)

Provider tests verify the API matches the contract.

```bash
# Set API URL (if not http://localhost:5000)
export API_BASE_URL=http://localhost:5000

# Run all provider tests
dotnet test ERP.API.ContractTests --filter "Provider"

# Run specific provider tests
dotnet test ERP.API.ContractTests --filter "ErpApiProviderContractTests"
```

### Publish to Pact Broker

```bash
# Install Pact CLI
dotnet tool install --global pact-cli

# Set broker URL
export PACT_BROKER_URL=https://your-broker.example.com
export PACT_BROKER_TOKEN=your-token

# Publish pacts
pact-broker publish ERP.API.ContractTests/pacts/ \
  --consumer-app-version 1.0.0 \
  --branch main

# Can-i-deploy (check if safe to deploy)
pact-broker can-i-deploy \
  --pacticipant ERP.API \
  --version 1.0.0 \
  --to-environment production
```

## Pact Broker Setup (Optional)

### Using Docker Compose

```yaml
# docker-compose.pact.yml
services:
  pact-broker:
    image: pactfoundation/pact-broker:2
    ports:
      - "9292:9292"
    environment:
      PACT_BROKER_DATABASE_HOST: postgres
      PACT_BROKER_DATABASE_USERNAME: pact
      PACT_BROKER_DATABASE_PASSWORD: pact
    depends_on:
      - postgres

  postgres:
    image: postgres:14
    environment:
      POSTGRES_DB: pact
      POSTGRES_USER: pact
      POSTGRES_PASSWORD: pact
```

### Start Broker

```bash
docker-compose -f docker-compose.pact.yml up -d
docker-compose -f docker-compose.pact.yml logs -f pact-broker
```

Access at: http://localhost:9292

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `API_BASE_URL` | Running API URL | `http://localhost:5000` |
| `PACT_BROKER_URL` | Pact Broker URL | (not set) |
| `PACT_BROKER_TOKEN` | Broker authentication token | (not set) |

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Contract Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  consumer-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Run consumer tests
        run: dotnet test --filter "Consumer" --logger "trx;LogFileName=consumer.trx"

      - name: Publish pacts
        if: github.ref == 'refs/heads/main'
        env:
          PACT_BROKER_URL: ${{ secrets.PACT_BROKER_URL }}
          PACT_BROKER_TOKEN: ${{ secrets.PACT_BROKER_TOKEN }}
        run: |
          dotnet tool install --global pact-cli
          pact-broker publish ERP.API.ContractTests/pacts/ \
            --consumer-app-version ${{ github.sha }} \
            --branch ${{ github.ref_name }}

  provider-tests:
    runs-on: ubuntu-latest
    needs: consumer-tests
    services:
      api:
        image: nexterp-api:latest
        ports:
          - 5000:5000
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4

      - name: Run provider tests
        env:
          API_BASE_URL: http://localhost:5000
          PACT_BROKER_URL: ${{ secrets.PACT_BROKER_URL }}
          PACT_BROKER_TOKEN: ${{ secrets.PACT_BROKER_TOKEN }}
        run: dotnet test --filter "Provider"
```

## Writing Contract Tests

### Consumer Test Example

```csharp
[Fact]
public async Task GetUser_ReturnsUserData()
{
    _pact
        .UponReceiving("a request for user data")
            .Given("user exists")
            .WithRequest(HttpMethod.Get, "/api/users/123")
            .WithHeaders(new { Authorization = Match.Regex("Bearer .*", "Bearer token") })
        .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithBody(new
            {
                id = Match.Type("123"),
                email = Match.Email("user@example.com"),
                name = Match.MinType("John", 1)
            });

    await _pact.VerifyAsync(async ctx =>
    {
        var response = await ctx.MakeRequest();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    });
}
```

## Best Practices

1. **Run consumer tests first** - They define the contract
2. **Publish pacts on merge** - Share contracts with provider team
3. **Use can-i-deploy** - Block deployment if contract is broken
4. **Tag versions** - Track which version of consumer uses which contract
5. **Keep pacts small** - Test critical paths, not every edge case

## Resources

- [Pact Documentation](https://docs.pact.io/)
- [PactNet GitHub](https://github.com/pact-foundation/pact-net)
- [Pact Broker](https://github.com/pact-foundation/pact_broker)
