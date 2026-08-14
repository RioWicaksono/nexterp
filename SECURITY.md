# NEXTERP Security Documentation

**Version:** 1.0.0
**Last Updated:** 2026-08-14

---

## 🔐 Security Architecture

### Authentication
- **JWT Bearer Tokens** with short-lived access tokens (60 min default)
- **Refresh Tokens** with SHA-256 hashing for storage
- **BCrypt** for password hashing (work factor 12)
- **Timing-Safe Comparison** for refresh token validation

### Authorization
- **Role-Based Access Control (RBAC)**
- **Module-Based Licensing** via `[RequiresModule]` attribute
- **Multi-Tenant Isolation** with global query filters
- **SuperAdmin Role** bypasses module authorization

### API Security
- **Rate Limiting:**
  - Redis-backed sliding window (production)
  - In-memory fallback (development)
  - Configurable per-endpoint limits
- **CORS Policy:**
  - Explicit allowed origins (no wildcards)
  - Explicit HTTP methods (GET, POST, PUT, PATCH, DELETE, OPTIONS)
  - Explicit allowed headers
  - Credentials support with preflight caching
- **Input Validation:**
  - FluentValidation on all Commands/Queries
  - SQL injection prevention via EF Core parameterized queries
  - XSS prevention via JSON serialization

---

## 🛡️ Security Headers

The API implements security headers via middleware:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy` for feature restrictions

---

## 🔒 Data Protection

### Sensitive Data
- **Banking Information** (Employee bank accounts): Documented as requiring encryption
- **Refresh Tokens**: SHA-256 hashed before storage
- **Passwords**: Never stored, only BCrypt hashes
- **PII Fields**: Marked with SECURITY comments in entities

### Tenant Isolation
- **ITenantEntity Interface**: All tenant-scoped entities implement this
- **Global Query Filters**: Automatic tenant filtering via EF Core
- **TenantEntityInterceptor**: Auto-sets OrganizationId on insert
- **AuditingInterceptor**: Tracks CreatedBy/UpdatedBy with user context

---

## 🚨 Rate Limiting Configuration

### Endpoints
| Endpoint Pattern | Limit | Window |
|-----------------|-------|--------|
| `/api/v1/auth/login` | 5 attempts | 15 min |
| `/api/v1/auth/register` | 3 attempts | 15 min |
| `/api/*` (authenticated) | 100 requests | 1 min |
| `/api/*` (anonymous) | 30 requests | 1 min |

### Response Headers
- `X-RateLimit-Limit`: Maximum requests allowed
- `X-RateLimit-Remaining`: Requests remaining in window
- `X-RateLimit-Reset`: Unix timestamp when window resets

---

## 🔑 Environment Variables (Production)

```bash
# Required
JWT_SECRET=your_jwt_secret_minimum_32_characters
POSTGRES_PASSWORD=secure_database_password

# Optional
CORS_ALLOWED_ORIGINS=https://app.example.com,https://admin.example.com
REDIS_CONNECTION=localhost:6379
```

---

## 📋 Security Checklist

### Pre-Deployment
- [ ] JWT secret is at least 32 characters
- [ ] PostgreSQL password is strong and unique
- [ ] CORS origins are explicitly configured (no `*`)
- [ ] Rate limiting is enabled in production
- [ ] Health check endpoints do not expose sensitive data
- [ ] Swagger is disabled in production (`ASPNETCORE_ENVIRONMENT=Production`)

### Code Review
- [ ] No hardcoded credentials or secrets
- [ ] All user inputs are validated
- [ ] Domain methods validate business rules
- [ ] Error messages do not leak sensitive information
- [ ] Audit logs capture relevant security events

---

## 📞 Security Reporting

If you discover a security vulnerability, please report it to the development team immediately. Do NOT create public issues for security concerns.

---

*Last reviewed: 2026-08-14*
