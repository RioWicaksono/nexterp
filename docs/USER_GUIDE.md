<!-------------------------------------------------------------------------------
NEXTERP - User Guide Presentation
marp: true
theme: default
paginate: true
------------------------------------------------------------------------------->

<!-- _class: lead -->
<!-- _footer: "NEXTERP User Guide | 2026 Care Technologies" -->
<!-- _transition: fade -->

# NEXTERP

## User Guide

Enterprise Resource Planning System

---

# Agenda

- **Getting Started** - Login, Dashboard, Navigation
- **Core Modules** - Inventory, Sales, Accounting, HRM
- **Additional Modules** - Purchasing, Projects, Assets, Quality
- **Reports & Analytics** - Data insights and exports
- **Best Practices** - Tips for effective usage

---

# Getting Started

## Step 1: Access NEXTERP

Open your browser and navigate to:

```
http://localhost:3000
```

---

## Step 2: Login

Enter your credentials:

| Field | Demo Value |
|-------|------------|
| Username | admin |
| Password | Admin123! |

> **Note:** Change your password after first login!

---

## Step 3: Dashboard Overview

After login, you will see the main dashboard with:

- **KPI Cards** - Revenue, Orders, Customers at a glance
- **Charts** - Visual trends and analytics
- **Recent Activity** - Latest transactions
- **Quick Actions** - Common operations

---

## Step 4: Navigation

Use the **sidebar menu** to access different modules:

| Icon | Module |
|------|--------|
| Dashboard | Main Dashboard |
| Inventory | Stock Management |
| Accounting | Finance & Ledger |
| Sales | Customer Orders |
| Purchasing | Procurement |
| HRM | Human Resources |
| Projects | Project Management |
| Assets | Fixed Assets |
| Quality | QC & Inspections |

---

# Dashboard Deep Dive

## KPI Cards

| Metric | Description |
|--------|-------------|
| Revenue | Total sales revenue |
| Orders | Number of orders |
| Customers | Active customers |
| Alerts | Low stock, overdue invoices |

> Click any KPI card for detailed breakdown

---

## Date Filtering

Customize the dashboard view by date range:

- **Today** - Current day only
- **This Week** - Current week
- **This Month** - Current month (default)
- **Custom Range** - Pick specific dates

---

# Inventory Management

## Warehouse Management

**Features:**
- Create multiple warehouse locations
- Set default warehouse for auto-selection
- Track warehouse-specific stock levels

**Common Operations:**
1. Add new warehouse
2. Set as default (optional)
3. Configure address and contact

---

## Stock Items

**Item Properties:**
- SKU (Stock Keeping Unit)
- Name and Description
- Category
- Unit of Measure
- Standard Cost and Price
- Reorder Level

**Tracking Options:**
- Batch Number
- Serial Number
- Expiration Date

---

## Stock Movements

Track all inventory transactions:

| Type | Description |
|------|-------------|
| IN | Purchase receipt, production output |
| OUT | Sales delivery, usage |
| ADJUSTMENT | Stock count corrections |

---

## Low Stock Alerts

Automatic notifications when stock falls below reorder level:

```
Alert: "Laptop ASUS ROG"
Current: 5 units
Reorder Level: 10 units
```

---

# Sales Management

## Customer Management

**Customer Profile Includes:**
- Contact Information
- Billing/Shipping Address
- Credit Limit
- Payment Terms
- Tax ID

**Customer Types:**
- Individual
- Company/Corporate
- Government

---

## Sales Order Workflow

```
DRAFT -> SUBMITTED -> APPROVED
                       |
                       v
            INVOICED -> PAID
```

---

## Creating a Sales Order

**Step-by-Step:**

1. Select Customer
2. Choose Warehouse
3. Add Line Items (Product + Quantity + Price)
4. Review Totals
5. Submit Order

**Automatic Calculations:**
- Subtotal
- Tax (if applicable)
- Discounts
- Grand Total

---

# Accounting

## Chart of Accounts

**Account Types:**

| Type | Nature | Examples |
|------|--------|----------|
| Asset | Debit increases | Cash, Inventory, Equipment |
| Liability | Credit increases | Accounts Payable, Loans |
| Equity | Credit increases | Capital, Retained Earnings |
| Revenue | Credit increases | Sales, Services |
| Expense | Debit increases | Rent, Salary, Utilities |

---

## Journal Entries

**Double-Entry Bookkeeping:**

Every transaction has:
- **Debit** entry (left side)
- **Credit** entry (right side)

**Rule:** Debits must equal Credits!

---

## Journal Entry Workflow

```
DRAFT -> SUBMITTED -> POSTED
  |          |
  |       Review
Edit       |
            v
       Cannot modify
       (only reverse)
```

---

## Creating a Journal Entry

1. Select **Entry Date**
2. Enter **Reference Number**
3. Add **Description**
4. Add Line Items:
   - Select Account
   - Enter Debit OR Credit amount
5. Verify Debit = Credit
6. Submit for Approval

---

# HRM (Human Resources)

## Employee Management

**Employee Profile:**
- Personal Information
- Contact Details
- Department
- Position
- Employment Type (Full-time/Part-time/Contract)
- Hire Date

---

## Attendance Tracking

**Daily Workflow:**

```
Check In -> Work -> Check Out
   |           |
   +---- Record Hours ----+
```

**Attendance Status:**
- Present
- Absent
- Late
- Half Day
- On Leave

---

## Leave Management

**Leave Request Process:**

1. Employee submits request
2. Manager reviews
3. Approved / Rejected
4. Balance updated

**Leave Types:**
- Annual Leave
- Sick Leave
- Maternity/Paternity
- Unpaid Leave

---

# Additional Modules

## Purchasing

- Supplier Management
- Purchase Orders
- Goods Receipt
- Price Tracking

---

## Projects

- Project Planning
- Task Tracking
- Time Logging
- Gantt Charts

---

## Assets

- Fixed Asset Register
- Depreciation Tracking
- Maintenance Scheduling

---

## Quality

- Quality Inspections
- Non-Conformance Records (NCR)
- Corrective & Preventive Actions (CAPA)

---

# Reports & Analytics

## Report Categories

| Report Type | Contents |
|-------------|----------|
| Sales | Revenue, orders, customer analysis |
| Inventory | Stock levels, movements, valuation |
| Financial | Trial balance, P&L, Balance Sheet |
| HR | Attendance, leave, headcount |

---

## Export Options

Export data in multiple formats:

- **CSV** - For Excel/spreadsheet analysis
- **PDF** - For printing/sharing
- **Print** - Direct printing

---

# Best Practices

## Daily Operations

| Practice | Benefit |
|----------|--------|
| Check Dashboard | Monitor KPIs daily |
| Review Alerts | Address issues promptly |
| Verify Stock | Prevent stockouts |
| Backup Data | Protect business data |

---

## Security Best Practices

| Practice | Action |
|----------|--------|
| Strong Passwords | Use complex, unique passwords |
| Role-Based Access | Assign appropriate permissions |
| Regular Updates | Keep system updated |
| Audit Logs | Review regularly |

---

## Data Integrity

- **Always** use approval workflows
- **Never** share login credentials
- **Reconcile** accounts monthly
- **Document** all corrections

---

# Summary

## Key Takeaways

- Single system for all business operations
- Real-time visibility across modules
- Automated workflows reduce manual work
- Role-based security protects data
- Built-in reports for decision making

---

# Questions?

## Contact Support

| Channel | Information |
|---------|-------------|
| Email | support@nexterp.com |
| Docs | docs.nexterp.com |
| Chat | Available on application |

---

# Thank You!

## Start Using NEXTERP

**http://localhost:3000**

---

*2026 NEXTERP by Care Technologies*
*Empowering Indonesian Enterprises*
