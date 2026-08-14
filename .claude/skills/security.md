# NEXTERP SECURITY GUIDELINES

Project-specific security standards for NEXTERP ERP system.

---

## AUTHENTICATION & AUTHORIZATION

### JWT Configuration
- **Algorithm:** HS256 (minimum)
- **Access Token TTL:** 60 minutes
- **Refresh Token TTL:** 7 days
- **Token Validation:** Timing-safe comparison

### Multi-Tenancy Security
- Organization ID required in all requests
- Tenant isolation via global query filters
- Cross-tenant access strictly prohibited

### Role Hierarchy
```
SuperAdmin > Admin > Manager > Staff
```

---

## MODULE-BASED LICENSING

### License Tiers
| Tier | Modules | Target |
|------|---------|--------|
| STARTER | SALES, INVENTORY | 5-20 employees |
| PROFESSIONAL | + HRM, PURCHASING | 20-100 employees |
| ENTERPRISE | + All modules | 100-500 employees |

### Protection Layers
```
Frontend → Controller Filter → MediatR Pipeline → Service → Database
   │              │                  │                  │            │
   │         [RequireLicense]  LicenseValidation    Hash Check   License Table
   │              │                  Behavior            │            │
   └── Menu      └── 403 Forbidden ──→ Exception ───→ Audit Log
```

---

## INPUT VALIDATION

### FluentValidation Rules
- All DTOs must have corresponding Validator
- Validate on both API and Application layer
- Return structured error responses

### SQL Injection Prevention
- Use Entity Framework Core parameterized queries
- No raw SQL unless absolutely necessary
- Linq queries only

### XSS Prevention
- Sanitize all user input
- React handles output escaping
- CSP headers configured in middleware

---

## PASSWORD & CREDENTIALS

### Password Policy
- Minimum 8 characters
- BCrypt hashing (cost factor 12)
- Timing-safe comparison for verification

### Environment Variables
```bash
JWT_SECRET=minimum_32_characters
POSTGRES_PASSWORD=secure_password
REDIS_PASSWORD=secure_redis_password
```

---

## RATE LIMITING

### Redis-Backed Sliding Window
- Auth endpoints: 5 attempts/minute
- API endpoints: 100 requests/minute
- Admin endpoints: 50 requests/minute

### Response Headers
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1620000000
```

---

## AUDIT LOGGING

### Security Events to Log
- Login attempts (success/failure)
- License validation attempts
- Module access denials
- Tampering detection events
- Admin actions

### Log Format
```json
{
  "timestamp": "2024-01-01T00:00:00Z",
  "event": "LICENSE_VALIDATION",
  "organizationId": "guid",
  "module": "HRM",
  "success": true,
  "ipAddress": "x.x.x.x"
}
```

---

## LICENSE INTEGRITY

### Tamper-Proof System
1. SHA256 hash of license data
2. HMAC signature verification
3. Audit trail for all validation attempts
4. Detect tampering → immediate lockout

### Validation Flow
```
1. Extract organization from JWT
2. Verify license hash integrity
3. Check license expiration
4. Validate module access
5. Log validation attempt
6. Allow or deny access
```

---

## SECURITY HEADERS

Middleware sets these headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: SAMEORIGIN`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy: ...`

---

## CORS CONFIGURATION

```csharp
cors:
  allowedOrigins:
    - http://localhost:3000 (development)
    - https://nexterp.app (production)
  allowedMethods: GET, POST, PUT, PATCH, DELETE, OPTIONS
  allowedHeaders: Content-Type, Authorization, X-Idempotency-Key
```

---

**Auto-loaded for:** Security-related tasks in NEXTERP
