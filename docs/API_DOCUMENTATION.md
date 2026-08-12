# NEXTERP API Documentation

## Overview

NEXTERP REST API provides programmatic access to the NEXTERP ERP system. The API follows RESTful principles and uses JSON for request/response payloads.

## Base URL

| Environment | URL |
|-------------|-----|
| Development | <http://localhost:5000/api> |
| Production | <https://api.nexterp.com/api> |

## Authentication

All API endpoints (except `/api/v1/auth/*`) require JWT Bearer token authentication.

```
Authorization: Bearer <your_jwt_token>
```

### Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "YourSecurePassword123!"
}
```

### Response Format

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
      "isSuperAdmin": false
    }
  }
}
```

## Common Headers

| Header | Value | Description |
|--------|-------|-------------|
| Content-Type | application/json | Required for POST/PUT/PATCH |
| Authorization | Bearer {token} | JWT Bearer token |

## Response Format

### Success Response

```json
{
  "success": true,
  "data": { ... }
}
```

### Error Response

```json
{
  "success": false,
  "error": "Error message"
}
```

### Pagination

```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 100,
    "totalPages": 5
  }
}
```

---

## Inventory Endpoints

### Warehouses

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/warehouses` | List all warehouses |
| GET | `/api/v1/warehouses/{id}` | Get warehouse by ID |
| POST | `/api/v1/warehouses` | Create warehouse |

### Stock Items

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/stock-items` | List all stock items |
| GET | `/api/v1/stock-items/{id}` | Get stock item by ID |
| POST | `/api/v1/stock-items` | Create stock item |

---

## Sales Endpoints

### Customers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/customers` | List all customers |
| GET | `/api/v1/customers/{id}` | Get customer by ID |
| POST | `/api/v1/customers` | Create customer |

### Sales Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/sales-orders` | List all sales orders |
| GET | `/api/v1/sales-orders/{id}` | Get sales order by ID |
| POST | `/api/v1/sales-orders` | Create sales order |

---

## HRM Endpoints

### Employees

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/employees` | List all employees |
| GET | `/api/v1/employees/{id}` | Get employee by ID |
| POST | `/api/v1/employees` | Create employee |

### Attendance

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/attendances` | List attendance records |
| POST | `/api/v1/attendances/check-in` | Check in |
| POST | `/api/v1/attendances/check-out` | Check out |

### Leave Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/leave-requests` | List leave requests |
| GET | `/api/v1/leave-requests/{id}` | Get leave request by ID |
| POST | `/api/v1/leave-requests` | Create leave request |
| POST | `/api/v1/leave-requests/{id}/approve` | Approve request |
| POST | `/api/v1/leave-requests/{id}/reject` | Reject request |

---

## Accounting Endpoints

### Journal Entries

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/journal-entries` | List journal entries |
| GET | `/api/v1/journal-entries/{id}` | Get journal entry by ID |
| POST | `/api/v1/journal-entries` | Create journal entry |

---

## Asset Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/assets` | List all assets |
| GET | `/api/assets/{id}` | Get asset by ID |
| POST | `/api/assets` | Create asset |
| POST | `/api/assets/maintenance` | Create maintenance record |

---

## Quality Endpoints

### Inspections

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/inspections` | List inspections |
| POST | `/api/inspections` | Create inspection |

### Non-Conformances

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/non-conformances` | List NCRs |
| POST | `/api/non-conformances` | Create NCR |
| PUT | `/api/non-conformances/{id}/resolve` | Resolve NCR |

---

## Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| VALIDATION_ERROR | 400 | Invalid request data |
| UNAUTHORIZED | 401 | Invalid or missing token |
| FORBIDDEN | 403 | Insufficient permissions |
| NOT_FOUND | 404 | Resource not found |
| INTERNAL_ERROR | 500 | Server error |

---

## Rate Limiting

API requests are limited to prevent abuse:

| Tier | Limit |
|------|-------|
| Anonymous | 60 requests/minute |
| Authenticated | 300 requests/minute |

Rate limit headers in response:

```
X-RateLimit-Limit: 300
X-RateLimit-Remaining: 299
X-RateLimit-Reset: 1642532400
```

---

## API Versioning

Current version: **v1**

```
/api/v1/endpoint
```
