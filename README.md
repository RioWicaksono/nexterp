# 🏢 NEXTERP - Enterprise Resource Planning System

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql)](https://postgresql.org)
[![Next.js](https://img.shields.io/badge/Next.js-14.2-black?style=for-the-badge&logo=next.js)](https://nextjs.org)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)](https://docker.com)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript)](https://typescriptlang.org)

**Full-featured ERP system with Clean Architecture (.NET 8 + Next.js 14)**

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [Architecture](#-architecture) • [Security](#-security)

</div>

---

## ✨ Features

### 10 Complete Business Modules

| Module | Description |
|--------|-------------|
| 📦 **Inventory** | Stock management, multi-warehouse, batch/serial tracking |
| 💰 **Accounting** | Chart of accounts, double-entry, financial reports |
| 🛒 **Sales** | Customers, orders, invoices, payments |
| 📝 **Purchasing** | Suppliers, purchase orders, goods receipt |
| 👥 **HRM** | Employees, departments, attendance, leave |
| 📊 **Projects** | Project planning, task tracking, Gantt charts |
| 🏢 **Assets** | Fixed assets, depreciation, maintenance |
| ✅ **Quality** | Inspections, NCR management, CAPA |
| 📊 **Analytics** | Real-time dashboards, KPI tracking |

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

Edit `.env` file with your secure values:

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

### 4. Access Application

| Service | URL |
|---------|-----|
| Frontend | <http://localhost:3000> |
| API/Swagger | <http://localhost:5000/swagger> |
| pgAdmin | <http://localhost:5050> |

### 5. Local Development

```bash
# Backend
dotnet run --project ERP.API

# Frontend (separate terminal)
cd ERP.WebUI && npm install && npm run dev
```

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| `README_DEV.md` | Developer setup, architecture, API reference |
| `README_CLIENT.md` | Feature overview, benefits, pricing |
| `docs/API_DOCUMENTATION.md` | Complete API reference |
| `CLAUDE.md` | Claude Code project instructions |

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
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  DOMAIN LAYER                                            │
│  Entities │ Value Objects │ Domain Events │ Enums         │
│  ⚠️ NO external dependencies - pure business logic       │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER                                   │
│  Entity Framework Core │ JWT Services │ Redis Cache     │
└─────────────────────────────────────────────────────────┘
```

### Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 8, C# 12, MediatR (CQRS) |
| Database | PostgreSQL 16, Entity Framework Core 8 |
| Frontend | Next.js 14, TypeScript 5, TailwindCSS |
| Cache | Redis 7 |
| Container | Docker, Docker Compose |
| Auth | JWT Bearer, BCrypt |
| Logging | Serilog (structured JSON) |
| Testing | xUnit, Jest, Playwright |

---

## 🔒 Security

### Implemented Security Features

- ✅ **JWT Authentication** with httpOnly cookies
- ✅ **Role-Based Access Control** (RBAC)
- ✅ **Multi-Tenancy** with organization isolation
- ✅ **Input Validation** with FluentValidation
- ✅ **SQL Injection Prevention** via EF Core parameterized queries
- ✅ **XSS Protection** with proper output encoding
- ✅ **CORS** with explicit origin whitelist
- ✅ **Secure Password Hashing** with BCrypt (cost factor 12)
- ✅ **HSTS Headers** for production
- ✅ **Docker Security** hardening (non-root, read-only, resource limits)

---

## 📂 Project Structure

```
nexterp/
├── ERP.API/              # REST API (Controllers, Middleware)
├── ERP.Application/       # Use Cases (Commands, Queries, DTOs)
├── ERP.Domain/           # Domain Entities & Business Logic
├── ERP.Infrastructure/   # Data Access (EF Core, Services)
├── ERP.WebUI/            # Next.js Frontend
├── docker/               # Docker configurations
├── docs/                 # Additional documentation
├── docker-compose.yml     # Container orchestration
└── .env.example          # Environment template
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd ERP.WebUI && npm test
```

---

## 📄 License

MIT License - Free for personal and commercial use.

---

<div align="center">

**Built with ❤️ by [Care Technologies](https://caretechnologies.com)**

*Transforming businesses with powerful, intuitive software*

</div>
