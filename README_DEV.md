# 👨‍💻 NEXTERP - Developer Guide

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql)](https://postgresql.org)
[![Next.js](https://img.shields.io/badge/Next.js-14.2-black?style=for-the-badge&logo=next.js)](https://nextjs.org)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)](https://docker.com)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript)](https://typescriptlang.org)

**Technical Documentation for NEXTERP ERP System**

</div>

---

## 📐 Architecture

### Clean Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                     │
│   Next.js Frontend (Port 3000)  │  REST API (Port 5000) │
└─────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────┐
│                      APPLICATION LAYER                      │
│   Commands/Queries (MediatR)  │  DTOs  │  FluentValidation│
└─────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────┐
│                        DOMAIN LAYER                        │
│   Entities  │  Value Objects  │  Domain Events  │  Enums │
│   ⚠️ NO external dependencies - pure business logic       │
└─────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────┐
│                     INFRASTRUCTURE LAYER                   │
│   Entity Framework Core  │  JWT Services  │  Redis Cache   │
└─────────────────────────────────────────────────────────────┘
```

---

## 🛠 Tech Stack

### Backend

| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET 8 | 8.0 |
| Language | C# 12 | Latest |
| ORM | Entity Framework Core | 8.x |
| Database | PostgreSQL | 16+ |
| CQRS | MediatR | 12.x |
| Validation | FluentValidation | 11.x |
| Auth | JWT Bearer | 7.x |
| Logging | Serilog | 3.x |

### Frontend

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | Next.js | 14.2 |
| Language | TypeScript | 5.x |
| UI | TailwindCSS | 3.4 |
| State | React Context | 18.x |
| Icons | Lucide React | Latest |

### Infrastructure

| Component | Technology | Version |
|-----------|------------|---------|
| Container | Docker Compose | 3.8 |
| Cache | Redis | 7.x |
| Web Server | Kestrel | - |

---

## 🚀 Setup Development Environment

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Node.js 18+
- PostgreSQL 16+ (via Docker)
- Redis 7+ (via Docker)

### Quick Start

```bash
# 1. Clone repository
git clone <repository-url>
cd nexterp

# 2. Copy environment file
cp .env.example .env

# 3. Edit .env with secure values
# Required: POSTGRES_PASSWORD, REDIS_PASSWORD, JWT_SECRET

# 4. Start infrastructure
docker-compose up -d postgres redis

# 5. Restore dependencies
dotnet restore

# 6. Run database migrations
dotnet ef database update --project ERP.Infrastructure --startup-project ERP.API

# 7. Run API
dotnet run --project ERP.API

# 8. Run Frontend (separate terminal)
cd ERP.WebUI
npm install
npm run dev
```

### Full Docker Setup

```bash
# Start all services
docker-compose --profile development up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

---

## 📂 Project Structure

```
nexterp/
├── ERP.API/                      # REST API Layer
│   ├── Controllers/             # API Controllers by domain
│   ├── Extensions/              # Service registration
│   ├── Middleware/              # Custom middleware
│   ├── Filters/                # Action filters
│   └── Program.cs               # Entry point
│
├── ERP.Application/              # Application Layer
│   ├── Common/
│   │   ├── Interfaces/        # Abstractions (IRepository, ICurrentUserService)
│   │   ├── Behaviors/          # MediatR pipeline behaviors
│   │   └── DTOs/              # Shared DTOs
│   └── [Domain]/               # Feature modules
│       ├── Commands/            # Write operations (Create, Update, Delete)
│       ├── Queries/             # Read operations
│       └── DTOs/               # Module-specific DTOs
│
├── ERP.Domain/                   # Domain Layer (Pure C#)
│   ├── Base/                   # Base classes (Entity, AuditableEntity)
│   ├── Common/                 # Shared domain types
│   └── [Domain]/              # Feature domains
│       ├── Entities/           # Domain entities
│       ├── ValueObjects/       # Immutable value types
│       ├── Enums/             # Domain enums
│       └── Events/             # Domain events
│
├── ERP.Infrastructure/          # Infrastructure Layer
│   ├── Data/                   # DbContext, Configurations
│   ├── Services/               # JWT, CurrentUser services
│   └── Repositories/           # Repository implementations
│
├── ERP.WebUI/                   # Next.js Frontend
│   ├── src/
│   │   ├── app/               # App Router pages
│   │   ├── components/        # React components
│   │   ├── lib/               # Utilities, API clients
│   │   └── types/             # TypeScript types
│   └── ...
│
├── docker/                      # Docker configurations
│   ├── api/Dockerfile
│   ├── db/init.sql
│   └── nginx/nginx.conf
│
├── docker-compose.yml
└── .env.example
```

---

## 📡 API Documentation

### Authentication

All API endpoints require JWT Bearer token authentication (except auth endpoints).

```
Authorization: Bearer <your_jwt_token>
```

#### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "YourSecurePassword123!"
}
```

#### Response

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresAt": "2024-01-15T12:00:00Z",
    "user": {
      "id": "uuid",
      "organizationId": "uuid",
      "username": "admin",
      "email": "admin@nexterp.com",
      "fullName": "Admin User",
      "isActive": true,
      "isSuperAdmin": true
    }
  }
}
```

### API Endpoints

#### Inventory

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/warehouses` | List warehouses |
| GET | `/api/v1/warehouses/{id}` | Get warehouse by ID |
| POST | `/api/v1/warehouses` | Create warehouse |
| GET | `/api/v1/stock-items` | List stock items |
| GET | `/api/v1/stock-items/{id}` | Get stock item by ID |
| POST | `/api/v1/stock-items` | Create stock item |

#### Sales

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/customers` | List customers |
| GET | `/api/v1/customers/{id}` | Get customer by ID |
| GET | `/api/v1/sales-orders` | List sales orders |
| GET | `/api/v1/sales-orders/{id}` | Get sales order by ID |
| POST | `/api/v1/sales-orders` | Create sales order |

#### HRM

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/employees` | List employees |
| GET | `/api/v1/employees/{id}` | Get employee by ID |
| POST | `/api/v1/employees` | Create employee |
| GET | `/api/v1/attendances` | List attendance records |
| POST | `/api/v1/attendances/check-in` | Check in |
| POST | `/api/v1/attendances/check-out` | Check out |
| GET | `/api/v1/leave-requests` | List leave requests |

#### Accounting

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/journal-entries` | List journal entries |
| GET | `/api/v1/journal-entries/{id}` | Get journal entry by ID |
| POST | `/api/v1/journal-entries` | Create journal entry |

---

## 🔄 CQRS Pattern

### Command Example

```csharp
// Command
public record CreateStockItemCommand(
    string Code,
    string Name,
    decimal StandardCost,
    decimal StandardPrice
) : IRequest<Result<Guid>>;

// Handler
public class CreateStockItemHandler
    : IRequestHandler<CreateStockItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _ctx;
    private readonly ICurrentUserService _user;

    public CreateStockItemHandler(
        IApplicationDbContext ctx,
        ICurrentUserService user)
    {
        _ctx = ctx;
        _user = user;
    }

    public async Task<Result<Guid>> Handle(
        CreateStockItemCommand request,
        CancellationToken ct)
    {
        var item = StockItem.Create(
            _user.OrganizationId!.Value,
            request.Code,
            request.Name,
            request.StandardCost,
            request.StandardPrice);

        _ctx.StockItems.Add(item);
        await _ctx.SaveChangesAsync(ct);

        return Result<Guid>.Success(item.Id);
    }
}
```

### Query Example

```csharp
// Query
public record GetStockItemsQuery : IRequest<Result<List<StockItemDto>>>;

// Handler
public class GetStockItemsHandler
    : IRequestHandler<GetStockItemsQuery, Result<List<StockItemDto>>>
{
    private readonly IApplicationDbContext _ctx;
    private readonly ICurrentUserService _user;

    public async Task<Result<List<StockItemDto>>> Handle(
        GetStockItemsQuery request,
        CancellationToken ct)
    {
        var items = await _ctx.StockItems
            .AsNoTracking()
            .Where(x => x.OrganizationId == _user.OrganizationId)
            .ProjectTo<StockItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return Result<List<StockItemDto>>.Success(items);
    }
}
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test ERP.Domain.UnitTests

# Frontend tests
cd ERP.WebUI && npm test
```

---

## 🐳 Docker Configuration

### Services

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: nexterp_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "127.0.0.1:5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
    deploy:
      resources:
        limits:
          memory: 1G

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD}
    ports:
      - "127.0.0.1:6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
    deploy:
      resources:
        limits:
          memory: 256M

  api:
    build:
      context: .
      dockerfile: ERP.API/Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: Host=postgres;...
      Jwt__Secret: ${JWT_SECRET}
    ports:
      - "5000:5000"
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:5000/health"]
```

---

## 🔒 Security Implementation

### JWT Token Structure

```json
{
  "sub": "user-uuid",
  "org": "organization-uuid",
  "username": "admin",
  "roles": ["Admin"],
  "iat": 1699900000,
  "exp": 1699903600
}
```

### Password Hashing

```csharp
// Using BCrypt with cost factor 12
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
```

### Multi-Tenancy

All queries must include OrganizationId filter:

```csharp
var items = await _ctx.StockItems
    .AsNoTracking()
    .Where(x => x.OrganizationId == _user.OrganizationId)
    .ToListAsync(ct);
```

---

## 📈 Performance Considerations

| Area | Strategy |
|------|----------|
| Database | Indexed foreign keys, query optimization |
| Caching | Redis for session and hot data |
| API | Pagination, filtering, cursor-based |
| Frontend | Lazy loading, code splitting |

---

## 🤝 Contributing

```bash
# 1. Fork & Clone
git clone <fork-url>
cd nexterp

# 2. Create branch
git checkout -b feature/your-feature

# 3. Commit (Conventional Commits)
git commit -m "feat: add new feature"

# 4. Push & PR
git push origin feature/your-feature
```

---

## 📚 Additional Resources

| Resource | Link |
|----------|------|
| API Docs | <http://localhost:5000/swagger> |
| Frontend | <http://localhost:3000> |

---

## 📞 Support

- **Issues**: GitHub Issues
- **Email**: dev-support@caretechnologies.com

---

<div align="center">

**Built with ❤️ using Clean Architecture**

*© 2026 NEXTERP by Care Technologies*

</div>
