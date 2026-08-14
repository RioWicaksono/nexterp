# 🏢 NEXTERP - Enterprise Resource Planning System

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql)](https://postgresql.org)
[![Next.js](https://img.shields.io/badge/Next.js-14.2-black?style=for-the-badge&logo=next.js)](https://nextjs.org)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)](https://docker.com)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript)](https://typescriptlang.org)

**Full-featured ERP system with Clean Architecture (.NET 8 + Next.js 14)**

[Features](#-features) • [Tech Stack](#-tech-stack) • [Quick Start](#-quick-start) • [Architecture](#-architecture) • [Documentation](#-documentation) • [Security](#-security)

</div>

---

## ✨ Features

### 10 Complete Business Modules

| Module | Code | Description |
|--------|------|-------------|
| 📦 **Inventory** | INVENTORY | Stock management, multi-warehouse, batch/serial tracking |
| 💰 **Accounting** | ACCOUNTING | Chart of accounts, double-entry, financial reports |
| 🛒 **Sales** | SALES | Customers, quotes, orders, invoices, payments |
| 📝 **Purchasing** | PURCHASING | Suppliers, purchase orders, goods receipt |
| 👥 **HRM** | HRM | Employees, departments, attendance, leave, overtime |
| 📊 **Projects** | PROJECTS | Project planning, task tracking, Gantt charts |
| 🏢 **Assets** | ASSETS | Fixed assets, depreciation, maintenance |
| ✅ **Quality** | QUALITY | Inspections, NCR management, CAPA |
| 📊 **Analytics** | ANALYTICS | Real-time dashboards, KPI tracking |

### Module-Based Licensing

| Tier | Modules | Target |
|------|---------|--------|
| **STARTER** | Sales, Inventory | Small businesses (5-20 employees) |
| **PROFESSIONAL** | + HRM, Purchasing | Growing companies (20-100 employees) |
| **ENTERPRISE** | + All modules | Full-suite ERP (100-500 employees) |

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | .NET 8, C# 12, ASP.NET Core Web API |
| **Frontend** | Next.js 14, React 18, TypeScript, TailwindCSS |
| **Database** | PostgreSQL 16, Entity Framework Core 8 |
| **Cache** | Redis 7 |
| **Architecture** | Clean Architecture, CQRS via MediatR, DDD |
| **Container** | Docker, Docker Compose |
| **Auth** | JWT Bearer, BCrypt |
| **Logging** | Serilog (structured JSON) |
| **Testing** | xUnit, Jest, Playwright |

---

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- Node.js 18+

### 1. Clone & Setup
```bash
git clone <repository-url>
cd nexterp

# Copy environment file
cp .env.example .env
```

### 2. Configure Environment
Edit `.env` file:
```env
POSTGRES_PASSWORD=YourSecurePassword123!
REDIS_PASSWORD=YourSecureRedisPassword123!
JWT_SECRET=YourSecureJwtSecretMinimum32Chars!
```

### 3. Start with Docker
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f api
```

### 4. Local Development
```bash
# Backend
dotnet build ERP.slnx
dotnet run --project ERP.API

# Frontend (separate terminal)
cd ERP.WebUI && npm install && npm run dev
```

### 5. Access Application

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| API/Swagger | http://localhost:5000/swagger |
| pgAdmin | http://localhost:5050 |
| Redis Commander | http://localhost:8081 |

---

## 🏗️ Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER                                      │
│  Next.js Frontend (Port 3000) │ REST API (Port 5000)     │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  APPLICATION LAYER                                      │
│  MediatR Commands/Queries │ FluentValidation │ DTOs      │
│  Behaviors: License Validation, Authorization             │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  DOMAIN LAYER                                           │
│  Entities │ Value Objects │ Domain Events │ Enums        │
│  ⚠️ NO external dependencies - pure business logic        │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER                                   │
│  Entity Framework Core │ JWT Services │ Redis Cache      │
└─────────────────────────────────────────────────────────┘
```

### Key Patterns Implemented
- **CQRS Pattern** via MediatR (Commands & Queries are separate)
- **Repository Pattern** for data access abstraction
- **Domain Events** for decoupled communication
- **FluentValidation** for input validation
- **Module-Based Licensing** with `[RequiresModule]` attribute
- **Multi-Tenant Isolation** via ITenantEntity and global query filters
- **Rate Limiting** (Redis-backed sliding window)
- **Tamper-Proof License System** (hash + HMAC verification)

---

## 📁 Project Structure

```
nexterp/
├── ERP.API/                    # REST API
│   ├── Controllers/           # API Controllers
│   ├── Controllers/Admin/     # Admin endpoints
│   ├── Filters/               # License validation filters
│   └── Program.cs             # App configuration
├── ERP.Application/           # Use Cases
│   ├── [Domain]/              # Per-module Commands, Queries, DTOs
│   └── Common/
│       ├── Behaviors/          # MediatR pipeline behaviors
│       ├── Licensing/          # License & integrity services
│       └── Modules/           # Module configuration
├── ERP.Domain/                 # Domain Entities & Logic
├── ERP.Infrastructure/        # Data Access & Services
├── ERP.WebUI/                 # Next.js Frontend
├── ERP.Domain.UnitTests/       # Domain unit tests
├── ERP.Application.UnitTests/  # Application unit tests
├── ERP.API.ContractTests/     # API contract tests
├── docker/                     # Docker configurations
├── scripts/                   # Utility scripts
├── docs/                      # Additional documentation
├── docker-compose.yml          # Container orchestration
└── README.md                  # This file
```

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| `README.md` | This file - Overview & Technical guide |
| `README_DEV.md` | Developer setup, API reference, architecture |
| `README_CLIENT.md` | Feature overview, benefits, pricing (for clients) |
| `docs/` | ADRs, testing strategy, deployment guides |

---

## 🔐 Security

### Implemented Security Features

- ✅ **JWT Authentication** with httpOnly cookies
- ✅ **Role-Based Access Control** (RBAC)
- ✅ **Module-Based Licensing** with 3 tiers (Starter/Professional/Enterprise)
- ✅ **Tamper-Proof License System**:
  - MediatR Pipeline validation
  - Controller-level filters (`[RequireLicense]`)
  - SHA256 hash + HMAC signature verification
  - Audit logging for all validation attempts
- ✅ **Multi-Tenancy** with organization isolation
- ✅ **Input Validation** with FluentValidation
- ✅ **SQL Injection Prevention** via EF Core parameterized queries
- ✅ **Secure Password Hashing** with BCrypt (cost factor 12)
- ✅ **Rate Limiting** (Redis-backed sliding window)
- ✅ **Timing-Safe Token Comparison** for refresh tokens
- ✅ **CORS** with explicit origin whitelist
- ✅ **Docker Security** hardening (non-root, read-only)

### License Protection Layers

```
Frontend → Controller Filter → MediatR Pipeline → Service Layer → Database
   │              │                  │                  │            │
   │         [RequireLicense]  LicenseValidation    Hash Check   License Table
   │              │                  Behavior            │            │
   └── Menu      └── 403 Forbidden ──→ Exception ───→ Audit Log
```

---

## 🧪 Testing

### Backend Tests (xUnit)
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests (Jest + Playwright)
```bash
cd ERP.WebUI && npm test
```

### Test Coverage
- **Domain Unit Tests**: Entity behavior, value objects, invariants
- **Application Unit Tests**: Validators, handlers, license validation (597 tests total)

---

## ⚙️ Configuration

### Environment Variables
```bash
# Database
POSTGRES_PASSWORD=secure_password_here

# Redis
REDIS_PASSWORD=secure_redis_password

# JWT
JWT_SECRET=your_jwt_secret_minimum_32_characters

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
```

### Database
- **Provider:** PostgreSQL 16
- **ORM:** Entity Framework Core 8
- **Connection:** `Host=localhost;Port=5432;Database=erp_db`

### Docker Services
| Service | Port | Description |
|---------|------|-------------|
| postgres | 5432 | PostgreSQL database |
| redis | 6379 | Redis cache |
| api | 5000/5001 | .NET API |
| frontend | 3000 | Next.js app |
| nginx | 80/443 | Reverse proxy (prod) |
| pgadmin | 5050 | Database GUI (dev) |
| redis-commander | 8081 | Redis GUI (dev) |

---

## 📄 License

MIT License - Free for personal and commercial use.

---

## 👤 Author

**Built by Rio Wicaksono** - Full-stack developer specializing in enterprise systems

- 📧 Email: riowicaksono.work@gmail.com
- 💼 LinkedIn: [linkedin.com/in/riowicaksono](https://linkedin.com/in/riowicaksono)

---

<div align="center">

*Transforming businesses with powerful, intuitive software*

**Version 3.0.0** • Last Updated: 2026-08-14

</div>
