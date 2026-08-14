# NEXTERP Development Roadmap

**Version:** 2.0.0
**Last Updated:** 2026-08-14
**Target:** Indonesian SMB (50-500 employees)

---

## ✅ Completed Features

### License Management System
- [x] **License Service** - Full CRUD operations
- [x] **Admin License Controller** - REST API endpoints
- [x] **Module Configuration** - JSON-based module manifest
- [x] **Tier-Based Module Sync** - Auto-enable modules based on tier

### Tamper-Proof Security System
- [x] **LicenseValidationBehavior** - MediatR pipeline for license validation
- [x] **Controller-Level Filters** - `[RequireLicense]` attribute
- [x] **License Integrity Service** - SHA256 hash + HMAC signature verification
- [x] **Audit Logging Service** - All validation attempts logged
- [x] **11 Unit Tests** - Comprehensive license validation tests

### Admin Dashboard
- [x] **Dashboard API** - `/api/v1/admin/dashboard`
- [x] **Frontend Dashboard** - `/admin` page with real-time data
- [x] **Sidebar Integration** - Admin menu visible for SuperAdmin only
- [x] **License Statistics** - Tier distribution, expiring licenses

### Project Organization
- [x] **Modules folder** moved to `ERP.Application/Common/Modules/`
- [x] **Scripts folder** created with utility scripts
- [x] **Documentation updated** - README.md consolidated

---

## 📋 Master Todo List

### Phase 1: Foundation Stabilization (Week 1-2)

- [ ] **1.1** Add unit tests for HRM Commands/Queries
  - `CreateEmployeeCommand` tests
  - `UpdateEmployeeCommand` tests
  - `CreateLeaveRequestCommand` tests
  - `ApproveLeaveRequestCommand` tests
  - `RecordAttendanceCommand` tests
  - `CreateOvertimeRequestCommand` tests

- [ ] **1.2** Create HRM API Controllers
  - [ ] `EmployeesController.cs` - CRUD + status management
  - [ ] `AttendancesController.cs` - Check-in/out, record
  - [ ] `LeaveRequestsController.cs` - Apply, approve, cancel
  - [ ] `OvertimeRequestsController.cs` - Request, approve
  - [ ] `DepartmentsController.cs` - CRUD
  - [ ] `PositionsController.cs` - CRUD
  - [ ] `LeaveBalancesController.cs` - Allocation, summary

- [ ] **1.3** Run database migration for new HRM entities
  - [ ] `EmployeeDocument`
  - [ ] `Shift`
  - [ ] `Holiday`
  - [ ] `LeaveEntitlement`
  - [ ] Update existing entity configurations

- [ ] **1.4** Add API versioning strategy
  - [ ] Configure URL versioning (`/api/v1/`, `/api/v2/`)
  - [ ] Add `[ApiVersion]` attributes to controllers
  - [ ] Update Swagger documentation

---

### Phase 2: Payroll Engine (Week 3-5) ⭐ HIGH PRIORITY

- [ ] **2.1** Implement PPh 21 Calculation
  - [ ] PTKP (Pendapatan Tidak Kena Pajak) tables
  - [ ] Biaya jabatan calculation (5% of gross, max 6M/year)
  - [ ] PKP (Penghasilan Kena Pajak) tiered calculation
  - [ ] Monthly/annual tax computation
  - [ ] PPh 21 normalization for year-end

- [ ] **2.2** Implement THR (Tunjangan Hari Raya)
  - [ ] THR calculation (1 month salary for ≥1 year service)
  - [ ] Proportional THR for < 1 year service
  - [ ] THR payment schedule (H-7 before Eid)
  - [ ] THR tax treatment (PPH 21)

- [ ] **2.3** Implement BPJSTK / BPJS Kesehatan
  - [ ] JKK (Jaminan Kecelakaan Kerja) calculation
  - [ ] JKM (Jaminan Kematian) calculation
  - [ ] JP (Jaminan Pensiun) calculation
  - [ ] BPJS Kesehatan contribution
  - [ ] Employer/employee split calculation

- [ ] **2.4** Create Payslip generation
  - [ ] Payslip DTO with all components
  - [ ] PDF generation for payslip
  - [ ] Email payslip delivery
  - [ ] Payslip history

- [ ] **2.5** Bank transfer integration
  - [ ] Bank account validation
  - [ ] Transfer file format (flat file BCA/Mandiri/BRI)
  - [ ] Batch transfer processing
  - [ ] Transfer status tracking

---

### Phase 3: HRM Dashboard (Week 4-6)

- [ ] **3.1** Employee Dashboard Widgets
  - [ ] Total headcount by department
  - [ ] Headcount trend (monthly)
  - [ ] New hires vs terminations
  - [ ] Employee status distribution

- [ ] **3.2** Attendance Dashboard
  - [ ] Daily attendance summary
  - [ ] Late arrivals report
  - [ ] Overtime hours by department
  - [ ] Attendance rate percentage

- [ ] **3.3** Leave Dashboard
  - [ ] Leave utilization rate
  - [ ] Pending leave requests
  - [ ] Leave balance summary
  - [ ] Popular leave types chart

- [ ] **3.4** Overtime Dashboard
  - [ ] Overtime costs by department
  - [ ] Overtime hours trend
  - [ ] Compliance with labor law (max 18h/week)

---

### Phase 4: Notification System (Week 5-7)

- [ ] **4.1** Email Notification Service
  - [ ] Configure SMTP/SendGrid/Mailgun
  - [ ] Email templates (leave approval, payslip, etc.)
  - [ ] Queue-based email sending
  - [ ] Email log/history

- [ ] **4.2** In-App Notification
  - [ ] Notification entity (read/unread)
  - [ ] Notification bell icon
  - [ ] Real-time update (SignalR)
  - [ ] Notification preferences

- [ ] **4.3** Trigger Notifications
  - [ ] Leave request submitted
  - [ ] Leave approved/rejected
  - [ ] Overtime approved/rejected
  - [ ] Payslip ready
  - [ ] THR payment
  - [ ] Document expiry reminder

---

### Phase 5: Approval Workflow (Week 6-8)

- [ ] **5.1** Approval Chain Configuration
  - [ ] Organization approval settings
  - [ ] Department-level overrides
  - [ ] Role-based approvers
  - [ ] Backup approver assignment

- [ ] **5.2** Workflow Engine
  - [ ] Sequential approval (Spv → Manager → HR)
  - [ ] Parallel approval (multiple approvers)
  - [ ] Conditional routing
  - [ ] Escalation rules
  - [ ] SLA monitoring

- [ ] **5.3** Delegation System
  - [ ] Delegate approval authority
  - [ ] Auto-return on delegate return
  - [ ] Delegation history

---

### Phase 6: Report Generator (Week 7-9)

- [ ] **6.1** Standard Reports
  - [ ] Employee list (filterable)
  - [ ] Attendance report
  - [ ] Leave report
  - [ ] Payroll report
  - [ ] Overtime report

- [ ] **6.2** Government Reports (Indonesian)
  - [ ] SPT PPh 21 (annual tax filing)
  - [ ] BPJS ketenagakerjaan report
  - [ ] BPS company report
  - [ ] Jamsostek monthly report

- [ ] **6.3** Export Formats
  - [ ] Excel export (EPPlus)
  - [ ] PDF export (QuestPDF)
  - [ ] CSV for data interchange

- [ ] **6.4** Custom Report Builder
  - [ ] Field selection
  - [ ] Filter configuration
  - [ ] Grouping and aggregation
  - [ ] Save as template

---

### Phase 7: External Integrations (Week 8-10)

- [ ] **7.1** DJP Integration (Tax)
  - [ ] e-SPT PPh 21 format
  - [ ] CSV export for DJP upload
  - [ ] Tax calculation validation

- [ ] **7.2** BPJS Integration
  - [ ] V-Claim API (for claims)
  - [ ] Monthly contribution report
  - [ ] Participant data sync

- [ ] **7.3** Absensi Hardware Integration
  - [ ] Fingerprint device API (ZKTeco, Suprema)
  - [ ] Real-time attendance sync
  - [ ] Device management

---

### Phase 8: Mobile Foundation (Week 9-11)

- [ ] **8.1** Mobile API Endpoints
  - [ ] GPS-based check-in/out
  - [ ] Leave request submission
  - [ ] Approval actions
  - [ ] Payslip viewing
  - [ ] Notification push

- [ ] **8.2** Mobile Authentication
  - [ ] Biometric login
  - [ ] PIN fallback
  - [ ] Session management

---

### Phase 9: Document Templates (Week 10-12)

- [ ] **9.1** HR Templates
  - [ ] Surat Pengantar Cuti (Leave Letter)
  - [ ] Surat Peringatan (Warning Letter)
  - [ ] Kontrak Kerja (Employment Contract)
  - [ ] Surat Keterangan Kerja (Work Certificate)
  - [ ] Medical Certificate

- [ ] **9.2** Template Engine
  - [ ] Variable substitution
  - [ ] PDF generation
  - [ ] Digital signature
  - [ ] Template versioning

---

### Phase 10: DevOps & Quality (Ongoing)

- [ ] **10.1** Integration Tests
  - [ ] API endpoint tests
  - [ ] Database transaction tests
  - [ ] Auth flow tests

- [ ] **10.2** Distributed Tracing
  - [ ] OpenTelemetry setup
  - [ ] Jaeger integration
  - [ ] Trace ID propagation

- [ ] **10.3** Automated Backup
  - [ ] PostgreSQL backup schedule
  - [ ] Redis backup
  - [ ] Off-site storage
  - [ ] Restore procedure

- [ ] **10.4** Feature Flags UI
  - [ ] Admin panel for feature flags
  - [ ] A/B testing support
  - [ ] Gradual rollout controls

---

## 🎯 Quick Win Checklist (Can finish in 1-2 days each)

- [ ] Add `YearsOfService` property test
- [ ] Create `LeaveBalance` query
- [ ] Add pagination to attendance list
- [ ] Create attendance summary query
- [ ] Add overtime calculation test
- [ ] Create department tree query

---

## 📊 Progress Tracking

| Phase | Features | Estimated Weeks | Priority |
|-------|----------|-----------------|----------|
| Phase 1 | Foundation | 2 | HIGH |
| Phase 2 | Payroll | 3 | HIGH |
| Phase 3 | Dashboard | 2 | HIGH |
| Phase 4 | Notification | 2 | MEDIUM |
| Phase 5 | Approval | 2 | MEDIUM |
| Phase 6 | Reports | 3 | MEDIUM |
| Phase 7 | Integration | 2 | LOW |
| Phase 8 | Mobile | 2 | LOW |
| Phase 9 | Documents | 2 | LOW |
| Phase 10 | DevOps | Ongoing | HIGH |

**Total Estimated:** ~20 weeks (5 months)

---

*Generated: 2026-08-14*
