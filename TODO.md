# NEXTERP ERP - Project TODO & Documentation

> Last Updated: 2026-08-19

---

## ✅ Completed Fixes

### Phase 1 - Security (Done)
- [x] CORS allow-all removed → scoped to explicit origins
- [x] Hardcoded credentials → env var pattern (`DEMO_PASSWORD`)
- [x] CORS policy in `ERP.API/Program.cs`
- [x] JWT Access Token: 60 min → 15 min (production standard)
- [x] Refresh Token Rotation + Storage (SHA-256 hashed in DB)
- [x] Token Blacklist via Redis (JTI-based for logout)
- [x] Brute Force Protection (5 attempts/5min, 15min lockout)
- [x] BCrypt cost factor: 11 → 12 (production standard)
- [x] httpOnly cookies with proper expiration
- [x] Structured logging (Serilog)
- [x] Correlation ID middleware
- [x] Enhanced health check endpoints (/health/live, /health/ready)

### Phase 2-4 - TypeScript, Accessibility, Error Boundary (Done)

---

## 🔴 Critical - Must Fix for Production

### 1. Backend API Security (✅ COMPLETED)
```
├── ✅ Rate limiting di login endpoint (brute force protection)
├── ✅ JWT token security hardening:
│   ├── ✅ Access token: 15 menit
│   ├── ✅ Refresh token rotation
│   ├── ✅ Token blacklist/revocation
│   └── ✅ httpOnly cookie (already existed)
├── ✅ Input validation (FluentValidation - already existed)
├── ✅ API versioning strategy (/api/v1, /api/v2 - already existed)
└── ✅ Uniform error response format (GlobalExceptionHandler - already existed)
```

### 2. Database Security (Partial - Existing)
```
├── ⚠️ Row-Level Security (tenant_id filter - needs verification)
├── ✅ Soft delete pattern (deleted_at, is_deleted - existing)
├── ⚠️ Audit logging untuk data sensitif (basic - existing)
└── ✅ BCrypt cost factor: 12 (verified)
```

### 3. Observability (Partial - Existing)
```
├── ✅ Structured logging (Serilog)
├── ✅ Correlation ID tracking
├── ✅ Health check endpoints:
│   ├── ✅ GET /health/live (basic)
│   └── ✅ GET /health/ready (dependencies check)
└── ✅ Metrics export (Prometheus - already existed)
```

---

## 🟡 High Priority - Production Ready

### 4. RBAC Implementation (✅ COMPLETED)
```
├── ✅ Permission-based authorization via [RequiresPermission] attribute
├── ✅ PermissionAuthorizationBehavior MediatR pipeline
├── ✅ API endpoint authorization middleware
├── ⚠️ Row-level data permission (partial - interceptor exists)
└── ⚠️ Session management (concurrent session limit - not implemented)
```

### 5. Global Error Handling (✅ COMPLETED)
```
├── ✅ Global exception handler (GlobalExceptionHandlerMiddleware)
├── ✅ No stack trace exposure in production
├── ⚠️ Circuit breaker pattern (not implemented)
└── ⚠️ Retry policy for external calls (not implemented)
```

### 6. API Performance (Partial)
```
├── ⚠️ Database indexing audit (needs review)
├── ✅ Redis caching (RedisCacheService implemented)
├── ⚠️ Pagination optimization (basic - existing)
└── ⚠️ API response compression (not implemented)
```

---

## 🟠 Medium Priority - Enhancements

### 7. Frontend UX (Complete)
```
├── ✅ Batch delete operations (BatchDeleteCommand implemented)
├── ✅ Export features (ExportService: CSV, JSON)
├── ✅ Keyboard shortcuts (useKeyboardShortcuts hook + CommandPalette)
├── ⚠️ Dashboard customization (not implemented)
└── ⚠️ Auto-save draft functionality (not implemented)
```

### 8. Features (Partial)
```
├── ✅ Approval workflow (WorkflowCommands: Submit/Approve/Reject/Return)
├── ⚠️ Real-time notifications (WebSocket - not implemented)
├── ✅ Export laporan (ExportService)
├── ⚠️ Mobile responsive improvements (needs review)
└── ⚠️ Dark mode refinements (not implemented)
```

---

## 🟢 Low Priority - Nice to Have

### 9. Developer Experience (Complete)
```
├── ✅ Swagger/OpenAPI documentation (enhanced)
├── ⚠️ Mock data for development (not implemented)
├── ✅ E2E testing (Playwright configured)
├── ⚠️ Storybook for components (not implemented)
└── ✅ Docker Compose for local dev
```

### 10. Frontend UX Enhancements (Complete)
```
├── ✅ Keyboard shortcuts (useKeyboardShortcuts hook)
├── ✅ Command Palette (Ctrl+K)
├── ⚠️ Dashboard customization (draggable widgets - not implemented)
└── ⚠️ Auto-save draft (not implemented)
```

---

## 🔧 Infrastructure - Production Ready

### 10. API Performance (Complete)
```
├── ✅ Database indexing audit (partial - interceptor exists)
├── ✅ Redis caching (RedisCacheService implemented)
├── ✅ API response compression (Gzip/Brotli)
├── ⚠️ Pagination optimization (basic - existing)
└── ⚠️ CDN caching (frontend - not implemented)
```

### 11. API Security - External Integrations (Complete)
```
├── ✅ API Key authentication (X-Api-Key header)
├── ✅ SHA-256 hashed key storage
├── ✅ Permission-based access for API keys
└── ✅ ApiKeysController for management
```

### 12. Audit & Compliance (Complete)
```
├── ✅ Request/Response logging (ApiAuditLoggingMiddleware)
├── ✅ Data masking for GDPR/PIV (DataMaskingService)
├── ✅ Structured audit logging (Serilog)
└── ✅ Correlation ID tracking
```

---

## 📁 Project Structure Reference

```
ERP/
├── nextjs-frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── login/page.tsx
│   │   │   ├── dashboard/
│   │   │   │   ├── page.tsx (Dashboard)
│   │   │   │   ├── hrm/page.tsx
│   │   │   │   ├── inventory/page.tsx
│   │   │   │   ├── purchasing/page.tsx
│   │   │   │   ├── accounting/page.tsx
│   │   │   │   ├── projects/page.tsx
│   │   │   │   ├── roles/page.tsx
│   │   │   │   ├── modules/page.tsx
│   │   │   │   ├── organizations/page.tsx
│   │   │   │   ├── permissions/page.tsx
│   │   │   │   └── settings/page.tsx
│   │   │   ├── layout.tsx
│   │   │   └── providers.tsx
│   │   ├── components/
│   │   │   ├── ErrorBoundary.tsx ✅ NEW
│   │   │   ├── SkeletonLoader.tsx
│   │   │   ├── ConfirmDialog.tsx
│   │   │   ├── DataTable.tsx
│   │   │   ├── PageHeader.tsx
│   │   │   ├── ToastProvider.tsx
│   │   │   └── index.ts
│   │   ├── hooks/
│   │   │   └── useToast.ts
│   │   └── lib/
│   │       ├── api.ts
│   │       └── store.ts
│   └── package.json
├── ERP.API/ (Backend .NET)
├── ERP.Domain/
├── ERP.Application/
├── ERP.Infrastructure/
└── TODO.md (this file)
```

---

## 🔗 Deployment Info

| Component | Platform | URL |
|-----------|----------|-----|
| Frontend | Vercel | https://nexterp.vercel.app |
| Backend | Railway | https://api-production-ab1b.up.railway.app |
| Repository | GitHub | https://github.com/RioWicaksono/nexterp |

---

## 📝 Git History (Recent)

```
2e61195 feat: add aria-labels for accessibility compliance
5210607 fix: TypeScript safety - remove 'as any' casts
(previous commits from earlier session)
```

---

## 🚀 Quick Start New Session

Copy-paste ini di new session:

```
Saya mau lanjut kerja di project NEXTERP ERP. 
TODO.md ada di D:\RW\PROJECT RW\ERP\TODO.md.
Prioritas sekarang: [pilih dari daftar di atas]
```
