# NEXTERP ERP - Development Setup

## Quick Start with Docker Compose

### Prerequisites
- Docker & Docker Compose installed
- 4GB+ RAM available

### Start Development Environment

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### Services

| Service | URL | Credentials |
|---------|-----|------------|
| Frontend (Next.js) | http://localhost:3000 | - |
| Backend API (.NET) | http://localhost:5000 | - |
| PostgreSQL | localhost:5432 | nexterp / nexterp_dev_password |
| Redis | localhost:6379 | - |
| pgAdmin | http://localhost:5050 | admin@nexterp.local / admin123 |
| Redis Commander | http://localhost:8081 | - |

### Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

Key variables:
- `JWT_SECRET_KEY` - JWT signing key (min 32 chars)
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection
- `Redis__ConnectionString` - Redis connection

### Database Commands

```bash
# Run migrations
docker-compose exec api dotnet ef database update

# Open psql shell
docker-compose exec db psql -U nexterp -d nexterp_dev
```

## E2E Testing with Playwright

### Setup

```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install --with-deps
```

### Run Tests

```bash
# Run all tests
npm run test

# Run with UI
npm run test:ui

# Run specific test file
npm run test:auth

# Run in headed mode (see browser)
npm run test:headed

# Run API tests only
npm run test:api
```

### Test Structure

```
tests/
├── auth.spec.ts      # Login/logout tests
├── dashboard.spec.ts  # Dashboard and navigation tests
├── api.spec.ts       # API integration tests
└── ...
```

### Configuration

Edit `playwright.config.ts` to customize:
- Base URL
- Test timeout
- Screenshot settings
- CI mode

### Writing Tests

```typescript
import { test, expect } from '@playwright/test';

test('my test', async ({ page }) => {
  await page.goto('/login');
  await expect(page.locator('h1')).toContainText('Login');
});
```

### CI/CD

For CI environments, tests run against the production build:

```bash
# Build and test
npm run build
npm run test:ci
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+K` | Open Command Palette |
| `Ctrl+1-7` | Navigate to sections |
| `Ctrl+N` | Create new item |
| `Ctrl+S` | Save |
| `Ctrl+R` | Refresh |
| `Escape` | Close modal |

## Troubleshooting

### Port Already in Use

```bash
# Find and kill process on port
lsof -i :3000
kill -9 <PID>
```

### Database Connection Issues

```bash
# Restart database
docker-compose restart db

# Check database logs
docker-compose logs db
```

### Clean Start

```bash
# Remove all containers and volumes
docker-compose down -v

# Rebuild and start
docker-compose up --build -d
```
# retry
