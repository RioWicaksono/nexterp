# NEXTERP API DESIGN GUIDELINES

Project-specific API design standards for NEXTERP ERP system.

---

## API VERSIONING & STRUCTURE

### URL Structure
```
/api/v1/{resource}
    /api/v1/{resource}/{id}
    /api/v1/{resource}/{id}/{sub-resource}
```

### NEXTERP Domain Prefixes
| Domain | Prefix | Module Code |
|--------|--------|-------------|
| Inventory | `/api/v1/warehouses`, `/api/v1/stock-items` | INVENTORY |
| Sales | `/api/v1/customers`, `/api/v1/orders`, `/api/v1/invoices` | SALES |
| Purchasing | `/api/v1/suppliers`, `/api/v1/purchase-orders` | PURCHASING |
| HRM | `/api/v1/employees`, `/api/v1/attendances` | HRM |
| Projects | `/api/v1/projects`, `/api/v1/tasks` | PROJECTS |
| Assets | `/api/v1/assets` | ASSETS |
| Quality | `/api/v1/inspections`, `/api/v1/non-conformances` | QUALITY |
| Accounting | `/api/v1/journal-entries`, `/api/v1/accounts` | ACCOUNTING |
| Admin | `/api/v1/admin/*` | SuperAdmin only |

---

## RESPONSE FORMAT

### Success Response
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "success": false,
  "error": "Error message",
  "code": "ERROR_CODE",
  "details": [ ... ]
}
```

### Paginated Response
```json
{
  "success": true,
  "data": [ ... ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 100,
    "totalPages": 5
  }
}
```

---

## HTTP STATUS CODES

| Code | Usage | When to Use |
|------|-------|-------------|
| 200 | OK | GET, PUT, PATCH success |
| 201 | Created | POST new resource |
| 204 | No Content | DELETE success |
| 400 | Bad Request | Validation error |
| 401 | Unauthorized | Missing/invalid auth |
| 403 | Forbidden | License invalid or module disabled |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Duplicate resource |
| 422 | Unprocessable | Business rule violation |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Error | Unexpected server error |

---

## MODULE PROTECTION

All endpoints must be protected with module access validation:
- Use `[RequiresModule("MODULE_CODE")]` attribute on Commands/Queries
- Controller-level filter validates before MediatR pipeline
- SuperAdmin bypasses all module checks

### Module Codes
```csharp
[RequiresModule("INVENTORY")]
[RequiresModule("SALES")]
[RequiresModule("PURCHASING")]
[RequiresModule("HRM")]
[RequiresModule("PROJECTS")]
[RequiresModule("ASSETS")]
[RequiresModule("QUALITY")]
[RequiresModule("ACCOUNTING")]
[RequiresModule("ANALYTICS")]
```

---

## PAGINATION & FILTERING

### Standard Pagination
```
GET /api/v1/employees?page=1&pageSize=20
```

### Filtering
```
GET /api/v1/employees?departmentId={id}&status=Active
GET /api/v1/stock-items?warehouseId={id}&category={category}
```

### Sorting
```
GET /api/v1/employees?sortBy=hireDate&sortOrder=desc
```

---

## AUTHENTICATION

### JWT Bearer Token
```
Authorization: Bearer <jwt_token>
```

### Token Claims
- `sub`: User ID
- `email`: User email
- `uid`: Unique identifier
- `unm`: Username
- `org`: Organization ID
- `sadm`: Is SuperAdmin (boolean)
- `per`: Permissions array
- `jti`: JWT ID (for token validation)

---

## IDEMPOTENCY

For mutating operations, implement idempotency using `X-Idempotency-Key` header.

---

**Auto-loaded for:** API design tasks in NEXTERP
