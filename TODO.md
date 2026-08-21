# NEXTERP ERP - Project TODO & Documentation

> Last Updated: 2026-08-20

---

## 🚀 AUTO-RUN COMMANDS

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

### 4. Storybook
```bash
cd "d:\RW\PROJECT RW\ERP\nextjs-frontend"
npm run storybook
```

### 5. E2E Tests
```bash
cd "d:\RW\PROJECT RW\ERP\nextjs-frontend"
npm run test
```

### 6. Build & Deploy
```bash
cd "d:\RW\PROJECT RW\ERP"
git add -A && git commit -m "message" && git push
```

---

## 🎯 AI TASK QUEUE

### Phase 6: User Experience Enhancements ✅ COMPLETED

#### Task 6.1: Global Search Enhancement ⭐ READY
```
Goal: Improve global search with fuzzy search, recent searches, and category filters

Files to create:
- nextjs-frontend/src/components/SearchModal.tsx (enhance CommandPalette)
- nextjs-frontend/src/hooks/useGlobalSearch.ts
- nextjs-frontend/src/stores/searchHistoryStore.ts
- nextjs-frontend/src/lib/search.ts (fuzzy search utilities)

Features:
- [x] Fuzzy search with Fuse.js
- [x] Recent searches (localStorage)
- [x] Search by category (Employees, Inventory, etc.)
- [x] Keyboard navigation
- [x] Highlight matched terms
```

#### Task 6.2: Notification Bell & Badge ⭐ READY
```
Goal: Add notification bell with unread count badge and dropdown

Files to create:
- nextjs-frontend/src/components/NotificationBell.tsx
- nextjs-frontend/src/stores/notificationStore.ts
- nextjs-frontend/src/hooks/useNotifications.ts
- nextjs-frontend/src/components/NotificationDropdown.tsx

Features:
- [x] Unread count badge
- [x] Notification dropdown list
- [x] Mark as read functionality
- [x] Notification types (info, warning, error, success)
- [x] Real-time updates (polling every 30s)
```

#### Task 6.3: Dark Mode Toggle ⭐ READY
```
Goal: Add permanent dark mode toggle with system preference detection

Files to create:
- nextjs-frontend/src/components/ThemeToggle.tsx
- nextjs-frontend/src/lib/theme-utils.ts
- Update: nextjs-frontend/src/app/layout.tsx

Features:
- [x] System preference detection (prefers-color-scheme)
- [x] Manual toggle button
- [x] Persist preference to localStorage
- [x] Smooth transition animation
- [x] Dark mode for all components
```

#### Task 6.4: Bulk Actions for Tables ⭐ READY
```
Goal: Add bulk select, bulk delete, bulk update for all tables

Files to create:
- nextjs-frontend/src/components/DataTableWithBulkActions.tsx
- nextjs-frontend/src/hooks/useBulkActions.ts

Features:
- [x] Checkbox selection column
- [x] Select all / Deselect all
- [x] Bulk delete confirmation
- [x] Bulk status toggle
- [x] Selected count indicator
```

#### Task 6.5: Inline Edit for Tables ⭐ READY
```
Goal: Enable inline editing for quick field updates

Files to create:
- nextjs-frontend/src/components/InlineEdit.tsx
- nextjs-frontend/src/hooks/useInlineEdit.ts

Features:
- [x] Double-click to edit
- [x] Enter to save, Escape to cancel
- [x] Optimistic update
- [x] Validation feedback
- [x] Edit history tracking
```

#### Task 6.6: Export Enhancements ⭐ READY
```
Goal: Add Excel export and column selection for exports

Files to create:
- nextjs-frontend/src/components/ExportDialog.tsx
- nextjs-frontend/src/lib/export.ts

Features:
- [x] Export to CSV
- [x] Export to Excel (.xlsx)
- [x] Column selection checkboxes
- [x] Date range filter
- [x] Custom filename
```

---

### Phase 7: Additional Module Pages ⭐ READY

#### Task 7.1: Employee Profile Page ⭐ READY
```
Goal: Create detailed employee profile page

Files to create:
- nextjs-frontend/src/app/dashboard/hrm/[id]/page.tsx
- nextjs-frontend/src/components/EmployeeProfile.tsx
- nextjs-frontend/src/components/EmployeeTimeline.tsx

Features:
- [x] Personal information section
- [x] Employment history
- [x] Documents list
- [x] Leave balance
- [x] Performance ratings
- [x] Edit profile modal
```

#### Task 7.2: Stock Adjustment & Transfer ⭐ READY
```
Goal: Add stock adjustment and warehouse transfer features

Files to create:
- nextjs-frontend/src/components/StockAdjustment.tsx
- nextjs-frontend/src/components/StockTransfer.tsx
- nextjs-frontend/src/app/dashboard/inventory/adjustments/page.tsx
- nextjs-frontend/src/app/dashboard/inventory/transfers/page.tsx

Features:
- [x] Stock count adjustment
- [x] Adjustment reasons (damaged, lost, found)
- [x] Warehouse-to-warehouse transfer
- [x] Transfer approval workflow
- [x] Adjustment history log
```

#### Task 7.3: PO Tracking Timeline ⭐ READY
```
Goal: Visual timeline for purchase order status tracking

Files to create:
- nextjs-frontend/src/components/POTimeline.tsx
- nextjs-frontend/src/components/PODetails.tsx

Features:
- [x] Visual status timeline (Draft → Submitted → Approved → Shipped → Received)
- [x] Expected delivery date
- [x] PO item details
- [x] Approval history
- [x] Notes/comments
```

#### Task 7.4: Journal Entry Preview ⭐ READY
```
Goal: Add journal entry preview and validation

Files to create:
- nextjs-frontend/src/components/JournalEntryForm.tsx
- nextjs-frontend/src/components/JournalEntryPreview.tsx

Features:
- [x] Debit/Credit balance validation
- [x] Preview before posting
- [x] Recurring journal templates
- [x] Journal approval workflow
```

#### Task 7.5: Financial Reports ⭐ READY
```
Goal: Add Balance Sheet and Profit & Loss reports

Files to create:
- nextjs-frontend/src/app/dashboard/accounting/reports/balance-sheet/page.tsx
- nextjs-frontend/src/app/dashboard/accounting/reports/profit-loss/page.tsx
- nextjs-frontend/src/components/ReportChart.tsx
- nextjs-frontend/src/components/ReportExport.tsx

Features:
- [x] Balance Sheet with assets, liabilities, equity
- [x] Profit & Loss statement
- [x] Date range selection
- [x] Period comparison
- [x] Export to PDF/Excel
```

#### Task 7.6: Audit Trail Page ⭐ READY
```
Goal: Create audit trail viewer for admin

Files to create:
- nextjs-frontend/src/app/dashboard/audit/page.tsx
- nextjs-frontend/src/components/AuditLogTable.tsx
- nextjs-frontend/src/components/AuditFilters.tsx

Features:
- [x] Searchable audit log
- [x] Filter by user, action, date range
- [x] Filter by entity type
- [x] Export audit log
- [x] Detail view with before/after values
```

#### Task 7.7: User Activity Log ⭐ READY
```
Goal: Track and display user session activities

Files to create:
- nextjs-frontend/src/app/dashboard/activity/page.tsx
- nextjs-frontend/src/components/ActivityTimeline.tsx

Features:
- [x] Last login timestamp
- [x] Pages visited
- [x] Actions performed
- [x] Session duration
- [x] Active now indicator
```

#### Task 7.8: Approval Workflow Visual ⭐ READY
```
Goal: Visual workflow builder for approval processes

Files to create:
- nextjs-frontend/src/app/dashboard/workflows/page.tsx
- nextjs-frontend/src/components/WorkflowBuilder.tsx
- nextjs-frontend/src/components/WorkflowStep.tsx

Features:
- [x] Drag-and-drop workflow steps
- [x] Approver assignment
- [x] Condition-based routing
- [x] Approval threshold (e.g., 2 of 3)
- [x] Deadline configuration
```

---

### Phase 8: Code Quality & Testing ⭐ READY

#### Task 8.1: Fix Error Handling ✅ COMPLETED
```
Goal: Fix all empty catch blocks and improve error handling

Files to check:
- nextjs-frontend/src/app/dashboard/**/*.tsx
- nextjs-frontend/src/hooks/*.ts
- nextjs-frontend/src/lib/*.ts

Changes:
- [x] Add console.error or toast for all catch blocks
- [x] Add error boundary per page
- [x] Centralize error messages
- [x] Add error recovery options
```

#### Task 8.2: API Response Types ⭐ READY
```
Goal: Centralize all API response types

Files to create:
- nextjs-frontend/src/types/api.ts
- nextjs-frontend/src/types/entities.ts

Features:
- [x] ApiResponse<T> generic type
- [x] PaginatedResponse<T> type
- [x] All entity types centralized
- [x] Export from single index.ts
```

#### Task 8.3: Jest Unit Tests ⭐ READY
```
Goal: Add Jest unit tests for hooks and utilities

Files to create:
- nextjs-frontend/src/**/*.test.ts
- nextjs-frontend/src/**/*.test.tsx
- nextjs-frontend/jest.config.js

Test coverage:
- [x] useAutoSave hook
- [x] useDraftStorage functions
- [x] cn() utility
- [x] Export utilities
- [x] Component snapshot tests
```

#### Task 8.4: Storybook Interaction Tests ⭐ READY
```
Goal: Add interaction tests to Storybook stories

Files to update:
- nextjs-frontend/src/components/**/*.stories.tsx

Interactions to test:
- [x] Button click handlers
- [x] Form validation
- [x] Loading states
- [x] Error states
- [x] User interactions
```

#### Task 8.5: Component Library Refactor ⭐ READY
```
Goal: Create reusable component library

Files to create:
- nextjs-frontend/src/components/ui/Button.tsx
- nextjs-frontend/src/components/ui/Input.tsx
- nextjs-frontend/src/components/ui/Select.tsx
- nextjs-frontend/src/components/ui/Dialog.tsx
- nextjs-frontend/src/components/ui/Card.tsx
- nextjs-frontend/src/components/ui/Badge.tsx

Features:
- [x] Consistent design tokens
- [x] Variant props (primary, secondary, danger)
- [x] Size variants (sm, md, lg)
- [x] Disabled states
- [x] Loading states
```

---

### Phase 9: DevOps & Infrastructure ⭐ READY

#### Task 9.1: Frontend Dockerfile ⭐ READY
```
Goal: Create production-ready Dockerfile for frontend

Files to create:
- nextjs-frontend/Dockerfile

Features:
- [x] Multi-stage build (builder → runner)
- [x] Node 22 Alpine
- [x] Non-root user
- [x] Health check
- [x] .dockerignore
```

#### Task 9.2: GitHub Actions CI/CD ⭐ READY
```
Goal: Add CI/CD pipeline for frontend

Files to create:
- .github/workflows/ci.yml
- .github/workflows/deploy.yml

Pipeline stages:
- [x] Lint check
- [x] TypeScript check
- [x] Unit tests
- [x] Build
- [x] E2E tests
- [x] Deploy to preview (PR)
- [x] Deploy to production (main)
```

#### Task 9.3: Pre-commit Hooks ⭐ READY
```
Goal: Add lint-staged for pre-commit validation

Files to create:
- .lintstagedrc

Features:
- [x] TypeScript check
- [x] ESLint fix
- [x] Prettier format
- [x] Commit message validation
```

#### Task 9.4: Sentry Integration ⭐ READY
```
Goal: Add error tracking with Sentry

Files to create:
- nextjs-frontend/src/lib/sentry.ts
- nextjs-frontend/sentry.client.config.ts
- nextjs-frontend/sentry.server.config.ts

Features:
- [x] Client-side error tracking
- [x] Server-side error tracking (if applicable)
- [x] Source maps upload
- [x] Performance monitoring
- [x] User context
```

---

### Phase 10: Documentation ⭐ READY

#### Task 10.1: Postman Collection ⭐ READY
```
Goal: Create comprehensive Postman collection for API testing

Files to create:
- docs/NEXTERP-API.postman_collection.json

Coverage:
- [x] All API endpoints
- [x] Auth flows (login, refresh, logout)
- [x] CRUD operations for all entities
- [x] Environment variables
- [x] Example responses
```

#### Task 10.2: Architecture Decision Records ⭐ READY
```
Goal: Document architectural decisions

Files to create:
- docs/adr/ADR-001-tech-stack.md
- docs/adr/ADR-002-authentication.md
- docs/adr/ADR-003-state-management.md
- docs/adr/ADR-004-database-design.md

Content:
- [x] Context and decision
- [x] Consequences
- [x] Alternatives considered
```

#### Task 10.3: CONTRIBUTING.md ⭐ READY
```
Goal: Create contribution guidelines

Files to create:
- CONTRIBUTING.md

Content:
- [x] Setup instructions
- [x] Coding standards
- [x] Git workflow
- [x] Testing requirements
- [x] Pull request template
```

#### Task 10.4: CHANGELOG.md ⭐ READY
```
Goal: Create standardized changelog

Files to create:
- CHANGELOG.md

Format:
- [x] Keep a Changelog standard
- [x] Semantic versioning
- [x] Categorized changes (Added, Changed, Deprecated, Removed, Fixed, Security)
- [x] Auto-generate from commits (conventional-changelog)
```

---

### Phase 11: Mobile & Performance ⭐ READY

#### Task 11.1: Mobile Responsive Audit ⭐ READY
```
Goal: Fix mobile responsiveness issues

Pages to audit:
- All dashboard pages
- Login page
- Modal dialogs
- Tables
- Navigation

Fixes:
- [x] Sidebar collapse on mobile
- [x] Table horizontal scroll
- [x] Modal full-screen on mobile
- [x] Touch-friendly buttons
- [x] Viewport meta tags
```

#### Task 11.2: Performance Optimization ⭐ READY
```
Goal: Optimize Core Web Vitals

Files to check/update:
- nextjs-frontend/src/app/layout.tsx
- nextjs-frontend/src/app/page.tsx
- nextjs-frontend/src/components/**

Optimizations:
- [x] Image optimization with next/image
- [x] Dynamic imports for heavy components
- [x] Bundle size analysis
- [x] Remove unused dependencies
- [x] Add loading skeletons
```

---

### Phase 12: Notifications & Communication ⚠️ COMPLEX

#### Task 12.1: Email Notification Templates ⭐ READY
```
Goal: Create email notification templates

Backend files to create:
- ERP.Application/Notifications/EmailTemplates/
- ERP.Application/Notifications/IEmailService.cs
- ERP.Infrastructure/Services/SmtpEmailService.cs

Templates:
- [x] Welcome email
- [x] Password reset
- [x] PO approval request
- [x] PO status update
- [x] Low stock alert
- [x] Weekly summary
```

#### Task 12.2: SMS Notifications ⭐ READY
```
Goal: Add SMS notification support

Backend files to create:
- ERP.Application/Notifications/ISmsService.cs
- ERP.Infrastructure/Services/TwilioSmsService.cs

Features:
- [x] Twilio integration
- [x] SMS templates
- [x] Delivery status tracking
- [x] Rate limiting
```

#### Task 12.3: WebSocket Notifications (SignalR) ⚠️ COMPLEX
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

⚠️ Requires backend changes - Need Confirmation
```

---

## 📋 QUICK WINS (Week 1) ✅ COMPLETED

| # | Task | Priority | Status |
|---|------|----------|--------|
| 1 | Fix `catch {}` empty blocks | HIGH | ✅ DONE |
| 2 | Add Export CSV button to tables | HIGH | ✅ DONE |
| 3 | Consistent skeleton loaders | MEDIUM | ✅ DONE |
| 4 | Add unread notification badge | HIGH | ✅ DONE |
| 5 | Dark mode toggle | MEDIUM | ✅ DONE |

---

## 📋 QUICK WINS (Week 2) ✅ COMPLETED

| # | Task | Priority | Status |
|---|------|----------|--------|
| 1 | User activity log page | MEDIUM | ✅ DONE |
| 2 | Inline edit for tables | MEDIUM | ✅ DONE |
| 3 | Bulk actions for tables | HIGH | ✅ DONE |
| 4 | Audit trail page | HIGH | ✅ DONE |
| 5 | Jest unit tests for hooks | HIGH | ✅ DONE |

---

## 📋 NEXT SPRINT

| # | Task | Priority | Status |
|---|------|----------|--------|
| 1 | Financial reports (Balance Sheet, P&L) | HIGH | ⭐ READY |
| 2 | Approval workflow visual | HIGH | ⭐ READY |
| 3 | Sentry integration | HIGH | ⭐ READY |
| 4 | Mobile responsive audit | MEDIUM | ⭐ READY |
| 5 | Postman collection | MEDIUM | ⭐ READY |
| 6 | Docker frontend | HIGH | ⭐ READY |
| 7 | GitHub Actions CI/CD | HIGH | ⭐ READY |

---

## ✅ COMPLETED FEATURES

### Phase 1-5 (2026-08-20)

- [x] Dashboard Draggable Widgets with @dnd-kit
- [x] Auto-Save Draft to localStorage
- [x] MSW Mock API for development
- [x] Storybook component documentation
- [x] Phase 1 Security (CORS, JWT, Rate Limiting, etc.)
- [x] Phase 2-4 TypeScript, Accessibility, Error Boundary

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
| Storybook | ✅ Ready | localhost:6006 |

---

## 🔗 COMMITS HISTORY

```
d52cf23 - feat: add Storybook component documentation
8e3028a - feat: implement MSW mock API for development
32af134 - feat: implement auto-save form drafts to localStorage
69a8886 - feat: implement draggable dashboard widgets with @dnd-kit
5ab43a8 - docs: update TODO.md - all major features complete
1d8895a - feat: implement low priority features
edd3551 - feat: implement remaining production features
dbca178 - feat: implement High to Low features
90ebbdf - feat: Phase 1 Critical Security
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
| Dashboard Widgets | `nextjs-frontend/src/components/dashboard/` |
| Auto-Save | `nextjs-frontend/src/hooks/useAutoSave.ts` |
| MSW Mock | `nextjs-frontend/src/mocks/` |
| Storybook | `nextjs-frontend/.storybook/` |

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
NEXT_PUBLIC_MSW_ENABLED=false

Backend (Railway env vars):
JwtSettings__SecretKey=<generate-32-chars>
JwtSettings__AccessTokenExpirationMinutes=15
JwtSettings__RefreshTokenExpirationDays=7
ConnectionStrings__DefaultConnection=<postgres-url>
Redis__ConnectionString=<redis-url>
```

---

*Last updated: 2026-08-20*
