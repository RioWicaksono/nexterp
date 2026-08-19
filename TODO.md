# NEXTERP ERP - Project TODO & Documentation

> Last Updated: 2026-08-19

---

## ✅ Completed Fixes

### Phase 1 - Security (Done)
- [x] CORS allow-all removed → scoped to explicit origins
- [x] Hardcoded credentials → env var pattern (`DEMO_PASSWORD`)
- [x] CORS policy in `ERP.API/Program.cs`

### Phase 2 - TypeScript Safety (Done)
- [x] Removed `as any` casts → proper typed conditionals
- [x] Fixed `err: any` → `err: unknown` with Error handling
- [x] Fixed dynamic Tailwind classes (`bg-${card.color}` → `bgClass`)
- [x] Fixed JSX structure bugs (login page)
- [x] Removed unused imports
- [x] TypeScript: **0 errors**

### Phase 3 - Accessibility (Done)
- [x] Added `aria-label` to icon buttons (Edit, Delete, Close)
- [x] Added `aria-pressed` to toggle switches
- [x] Pages: HRM, Accounting, Projects, Purchasing, Inventory, Roles, Modules, Settings

### Phase 4 - Error Boundary (Done)
- [x] Created `ErrorBoundary` component
- [x] Added to root `layout.tsx`

---

## 🔴 Critical - Must Fix for Production

### 1. Backend API Security
```
Priority: CRITICAL
├── Rate limiting di login endpoint (brute force protection)
├── JWT token security hardening:
│   ├── Access token: 15-30 menit (bukan 1 jam)
│   ├── Refresh token rotation
│   ├── Token blacklist/revocation
│   └── Consider httpOnly cookie instead of localStorage
├── Input validation (FluentValidation di semua endpoint)
├── API versioning strategy (/api/v1, /api/v2)
└── Uniform error response format
```

### 2. Database Security
```
Priority: CRITICAL
├── Row-Level Security (tenant_id filter di setiap query)
├── Soft delete pattern (deleted_at, is_deleted)
├── Audit logging untuk data sensitif
└── Verify BCrypt cost factor (minimal 12)
```

### 3. Observability
```
Priority: HIGH
├── Structured logging (JSON format)
├── Correlation ID tracking
├── Health check endpoints:
│   ├── GET /health (basic)
│   ├── GET /ready (dependencies check)
│   └── GET /live (liveness)
└── Metrics export (Prometheus)
```

---

## 🟡 High Priority - Production Ready

### 4. RBAC Implementation
```
├── Permission-based access control
├── API endpoint authorization middleware
├── Row-level data permission
└── Session management (concurrent session limit)
```

### 5. Global Error Handling
```
├── Global exception handler
├── No stack trace exposure di production
├── Circuit breaker pattern
└── Retry policy untuk external calls
```

### 6. API Performance
```
├── Database indexing audit
├── Redis caching untuk frequent queries
├── Pagination optimization
└── API response compression
```

---

## 🟠 Medium Priority - Enhancements

### 7. Frontend UX
```
├── Batch delete operations
├── Bulk import/export (Excel/CSV)
├── Keyboard shortcuts (⌘K untuk quick search)
├── Dashboard customization (draggable widgets)
└── Auto-save draft functionality
```

### 8. Features (User Requested)
```
├── Approval workflow (PO harus di-approve manager)
├── Real-time notifications (WebSocket)
├── Export laporan (Excel/PDF)
├── Mobile responsive improvements
└── Dark mode refinements
```

---

## 🟢 Low Priority - Nice to Have

### 9. Developer Experience
```
├── Swagger/OpenAPI documentation
├── Mock data untuk development
├── E2E testing (Playwright/Cypress)
├── Storybook untuk components
└── Docker Compose untuk local dev
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
