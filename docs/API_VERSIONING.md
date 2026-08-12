# API Versioning & Deprecation Policy

**Document Version:** 1.0.0  
**Last Updated:** 2026-08-07  
**Owner:** API Team

---

## 1. Versioning Strategy

### Current Version
- **Active Version:** `v1`
- **Base URL:** `/api/v1/`
- **Status:** Stable (Production)

### Version Lifecycle
```
Alpha → Beta → Stable → Deprecated → Sunset → Removed
```

| Stage | Description | Timeline |
|-------|-------------|----------|
| **Alpha** | Experimental, breaking changes possible | Internal only |
| **Beta** | Public testing, feature freeze | 30-60 days |
| **Stable** | Production-ready, backward compatible | 12+ months |
| **Deprecated** | Still functional, will be removed | 90 days notice |
| **Sunset** | Scheduled removal | End of deprecation period |
| **Removed** | No longer available | - |

---

## 2. Current API Endpoints

### Authentication
| Method | Endpoint | Status |
|--------|----------|--------|
| POST | `/api/v1/auth/login` | Stable |
| POST | `/api/v1/auth/register` | Stable |
| POST | `/api/v1/auth/refresh` | Stable |
| POST | `/api/v1/auth/logout` | Stable |

### Inventory
| Method | Endpoint | Status |
|--------|----------|--------|
| GET | `/api/v1/inventory/items` | Stable |
| GET | `/api/v1/inventory/items/{id}` | Stable |
| POST | `/api/v1/inventory/items` | Stable |
| PUT | `/api/v1/inventory/items/{id}` | Stable |
| DELETE | `/api/v1/inventory/items/{id}` | Stable |
| GET | `/api/v1/inventory/warehouses` | Stable |

### Sales
| Method | Endpoint | Status |
|--------|----------|--------|
| GET | `/api/v1/sales/customers` | Stable |
| POST | `/api/v1/sales/customers` | Stable |
| PUT | `/api/v1/sales/customers/{id}` | Stable |
| DELETE | `/api/v1/sales/customers/{id}` | Stable |
| GET | `/api/v1/sales/orders` | Stable |
| POST | `/api/v1/sales/orders` | Stable |

### Accounting
| Method | Endpoint | Status |
|--------|----------|--------|
| GET | `/api/v1/accounting/accounts` | Stable |
| POST | `/api/v1/accounting/accounts` | Stable |
| GET | `/api/v1/accounting/journal-entries` | Stable |
| POST | `/api/v1/accounting/journal-entries` | Stable |

### HRM
| Method | Endpoint | Status |
|--------|----------|--------|
| GET | `/api/v1/hrm/employees` | Stable |
| POST | `/api/v1/hrm/employees` | Stable |
| GET | `/api/v1/hrm/attendance` | Stable |
| POST | `/api/v1/hrm/attendance` | Stable |
| GET | `/api/v1/hrm/leave-requests` | Stable |
| POST | `/api/v1/hrm/leave-requests` | Stable |

---

## 3. Deprecation Policy

### 90-Day Deprecation Notice
When an endpoint is deprecated, the following headers MUST be included in responses:

```http
Deprecation: true
Sunset: Sat, 01 Nov 2026 00:00:00 GMT
Link: <https://api.nexterp.com/api/v2/endpoint>; rel="successor-version"
```

### Example Deprecation Response
```json
{
  "success": false,
  "error": "This endpoint is deprecated and will be removed on 2026-11-01.",
  "deprecation": {
    "since": "2026-08-01",
    "sunset": "2026-11-01",
    "successor": "/api/v2/endpoint"
  }
}
```

---

## 4. v2 API Roadmap

### Planned Changes for v2
| Feature | Description | Target Date |
|---------|-------------|-------------|
| GraphQL API | Alternative to REST | Q4 2026 |
| Bulk Operations | Batch create/update endpoints | Q4 2026 |
| Real-time Webhooks | Event-driven notifications | Q1 2027 |
| Enhanced Pagination | Cursor-based pagination | Q1 2027 |

### Migration Guide (v1 → v2)
```diff
- GET /api/v1/inventory/items?page=1&pageSize=20
+ GET /api/v2/inventory/items?cursor=xxx&limit=20

- POST /api/v1/sales/orders
+ POST /api/v2/sales/orders (with idempotency key)

- Response: { items: [...], total: 100 }
+ Response: { data: [...], pagination: { cursor, hasMore } }
```

---

## 5. Implementation

### Middleware for Deprecation Headers
```csharp
public class DeprecationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _deprecatedEndpoints;

    public DeprecationMiddleware(RequestDelegate next)
    {
        _next = next;
        _deprecatedEndpoints = new HashSet<string>
        {
            "/api/v1/legacy-endpoint"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        if (_deprecatedEndpoints.Contains(path))
        {
            context.Response.Headers.Append("Deprecation", "true");
            context.Response.Headers.Append("Sunset", "Sat, 01 Nov 2026 00:00:00 GMT");
            context.Response.Headers.Append("Link", "<https://api.nexterp.com/api/v2/endpoint>; rel=\"successor-version\"");
        }

        await _next(context);
    }
}
```

---

## 6. Version Negotiation

### Request Headers
```http
Accept: application/json
API-Version: 2026-08-07
```

### Response Headers
```http
API-Version: v1
API-Until: 2026-11-01
```

---

## 7. Error Codes

| Code | Message | Action |
|------|---------|--------|
| `VERSION_UNSUPPORTED` | API version not supported | Upgrade to supported version |
| `VERSION_DEPRECATED` | API version deprecated | Plan migration to new version |
| `VERSION_SUNSET` | API version removed | Immediate upgrade required |

---

## 8. Changelog

### 2026-08-07 - v1.0.0
- Initial stable release
- All core modules available
- Authentication, Authorization
- Inventory, Sales, Accounting, HRM
- Projects, Assets, Quality, Purchasing, Analytics

---

*For questions, contact: api-support@nexterp.com*
