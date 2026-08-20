# NEXTERP ERP - Project TODO & Documentation

> Last Updated: 2026-08-19

---

## 🚀 AUTO-RUN COMMANDS (AI Execute)

### 1. Docker Full Stack Start
```bash
cd "d:\RW\PROJECT RW\ERP"
docker-compose up -d
docker-compose logs -f
```

### 2. Backend Development
```bash
cd "d:\RW\PROJECT RW\ERP"
dotnet build ERP.API/ERP.API.csproj
dotnet run --project ERP.API
```

### 3. Frontend Development
```bash
cd "d:\RW\PROJECT RW\ERP\nextjs-frontend"
npm install
npm run dev
```

### 4. E2E Tests
```bash
cd "d:\RW\PROJECT RW\ERP\nextjs-frontend"
npm install
npx playwright install --with-deps
npm run test
```

### 5. TypeScript Check
```bash
cd "d:\RW\PROJECT RW\ERP\nextjs-frontend"
npx tsc --noEmit
```

### 6. Build & Deploy
```bash
# Backend
cd "d:\RW\PROJECT RW\ERP"
git add -A && git commit -m "message" && git push
```

---

## 🎯 AI TASK QUEUE (Execute Automatically)

### Task 1: Dashboard Draggable Widgets ✅ COMPLETED
```
Goal: Create draggable dashboard with customizable widgets

Files to create:
- nextjs-frontend/src/components/dashboard/DraggableGrid.tsx
- nextjs-frontend/src/components/dashboard/WidgetWrapper.tsx
- nextjs-frontend/src/components/dashboard/widgets/StatsCard.tsx
- nextjs-frontend/src/components/dashboard/widgets/ChartWidget.tsx
- nextjs-frontend/src/components/dashboard/widgets/RecentActivity.tsx
- nextjs-frontend/src/stores/dashboardStore.ts

Commands to run:
cd nextjs-frontend && npm install @dnd-kit/core @dnd-kit/sortable
```

### Task 2 - Auto-Save Draft (2026-08-20)
- [x] useDraftStorage.ts - localStorage operations with TTL
- [x] useAutoSave.ts - debounced auto-save hook
- [x] AutoSaveIndicator.tsx - save status UI component
- [x] Integrated with HRM page as example
- [x] Draft restored on modal open, cleared on save

### Task 3: WebSocket Notifications ⚠️ COMPLEX - Need Confirmation
```
Goal: Real-time notifications via SignalR

Backend files to create:
- ERP.Infrastructure/Services/NotificationService.cs
- ERP.API/Hubs/NotificationHub.cs
- ERP.API/Program.cs (add SignalR)

Frontend files to create:
- nextjs-frontend/src/hooks/useWebSocket.ts
- nextjs-frontend/src/components/NotificationBell.tsx
- nextjs-frontend/src/stores/notificationStore.ts

Commands to run:
cd ERP.API && dotnet add package Microsoft.AspNetCore.SignalR
```

### Task 4: Mock Data (MSW) ⭐ READY TO EXECUTE
```
Goal: Mock API responses for development

Files to create:
- nextjs-frontend/src/lib/mockData.ts
- nextjs-frontend/src/mocks/handlers.ts
- nextjs-frontend/src/mocks/browser.ts

Commands to run:
cd nextjs-frontend && npm install msw --save-dev
npx msw init public --save
```

### Task 5: Storybook ⭐ READY TO EXECUTE
```
Goal: Component documentation and playground

Commands to run:
cd nextjs-frontend
npm install @storybook/react @storybook/react-vite @storybook/addon-essentials --save-dev
npx storybook init
npm run storybook
```

---

## ✅ COMPLETED FEATURES

### Task 1 - Dashboard Draggable Widgets (2026-08-20)
- [x] @dnd-kit/core, @dnd-kit/sortable installed
- [x] dashboardStore.ts with Zustand + persistence
- [x] DraggableGrid.tsx with SortableContext
- [x] WidgetWrapper.tsx with drag handle, resize, hide
- [x] StatsCard.tsx widget
- [x] ChartWidget.tsx with Recharts
- [x] RecentActivity.tsx widget
- [x] QuickActions.tsx widget
- [x] Dashboard page integrated with draggable grid
- [x] Lock/Unlock layout toggle
- [x] Reset to default layout

### Phase 1 - Security (DONE)
- [x] CORS allow-all removed → scoped origins
- [x] Hardcoded credentials → env var
- [x] JWT Access Token: 60 min → 15 min
- [x] Refresh Token Rotation + SHA-256 hash
- [x] Token Blacklist via Redis
- [x] Brute Force Protection (5/5min, 15min lockout)
- [x] BCrypt cost factor: 12
- [x] httpOnly cookies
- [x] Structured logging (Serilog)
- [x] Correlation ID middleware
- [x] Health check endpoints

### Phase 2-4 - TypeScript, Accessibility, Error Boundary (DONE)
- [x] Removed `as any` casts
- [x] Added aria-labels
- [x] Created ErrorBoundary component

### High Priority (DONE)
- [x] RBAC: PermissionAuthorizationBehavior
- [x] Redis Caching: RedisCacheService
- [x] Workflow Commands
- [x] Global Error Handling

### Medium Priority (DONE)
- [x] Batch delete operations
- [x] Export Service (CSV, JSON)
- [x] Approval Workflow
- [x] API Response Compression
- [x] API Audit Logging
- [x] API Key Authentication
- [x] Data Masking (GDPR/PIV)

### Low Priority (DONE)
- [x] Keyboard shortcuts
- [x] Command Palette (Ctrl+K)
- [x] Docker Compose
- [x] E2E Testing (Playwright)
- [x] Swagger Documentation
- [x] Dockerfiles

---

## 📊 PROJECT STATUS

| Component | Status | URL |
|-----------|--------|-----|
| Frontend (Next.js) | ✅ Ready | http://localhost:3000 |
| Backend (.NET) | ✅ Ready | http://localhost:5000 |
| Database (PostgreSQL) | ✅ Ready | localhost:5432 |
| Cache (Redis) | ✅ Ready | localhost:6379 |
| Swagger Docs | ✅ Ready | http://localhost:5000/swagger |
| Health Check | ✅ Ready | http://localhost:5000/health/ready |

---

## 🔗 COMMITS HISTORY

```
5ab43a8 - docs: update TODO.md - all major features complete
1d8895a - feat: implement low priority features
edd3551 - feat: implement remaining production features
dbca178 - feat: implement High to Low features
90ebbdf - feat: Phase 1 Critical Security
```

---

## 📝 QUICK REFERENCE

### Demo Credentials
```
Username: admin
Password: DevPassword2024!
```

### Environment Variables
```
Frontend (.env.local):
NEXT_PUBLIC_API_URL=http://localhost:5000

Backend (Railway env vars):
JwtSettings__SecretKey=<generate-32-chars>
JwtSettings__AccessTokenExpirationMinutes=15
JwtSettings__RefreshTokenExpirationDays=7
ConnectionStrings__DefaultConnection=<postgres-url>
Redis__ConnectionString=<redis-url>
```

---

## 📁 KEY FILES

| Feature | File |
|---------|------|
| JWT Security | `ERP.Infrastructure/Services/JwtService.cs` |
| Rate Limiting | `ERP.Infrastructure/Services/LoginRateLimitService.cs` |
| RBAC | `ERP.Application/Common/Behaviors/PermissionAuthorizationBehavior.cs` |
| Caching | `ERP.Infrastructure/Services/RedisCacheService.cs` |
| Export | `ERP.Application/Common/Reports/ExportService.cs` |
| API Key Auth | `ERP.API/Authentication/ApiKeyAuthenticationHandler.cs` |
| Data Masking | `ERP.Application/Common/Security/DataMaskingService.cs` |
| Audit Logging | `ERP.API/Middleware/ApiAuditLoggingMiddleware.cs` |
| Docker | `docker-compose.yml` |
| Tests | `nextjs-frontend/tests/*.spec.ts` |

---

*Last updated: 2026-08-19*
