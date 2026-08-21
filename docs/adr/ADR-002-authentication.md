# ADR-002: Authentication Strategy

**Date:** 2024-01-15  
**Status:** Accepted  

## Context
Enterprise apps need secure, auditable authentication with refresh token rotation and brute-force protection.

## Decision

### Tokens
| Token | Expiry | Storage |
|-------|---------|---------|
| Access Token (JWT) | 15 minutes | Memory |
| Refresh Token | 7 days | httpOnly cookie |
| Token Hash (SHA-256) | — | Redis blacklist |

### Security Controls
- **Brute-force lockout:** 5 attempts → 15-minute cooldown
- **Refresh rotation:** New refresh token on every login
- **Token blacklist:** Redis set for instant logout
- **bcrypt cost factor:** 12

## Consequences

### Pro
- httpOnly prevents XSS token theft  
- Refresh rotation limits token lifetime
- Redis blacklist enables immediate logout

### Con
- Extra Redis calls on every auth check  
- Session state depends on Redis availability

## Alternatives Considered

| Option | Why Not |
|--------|---------|
| Sessions | Doesn't scale horizontally |
| Simple JWT | No logout/rotate control |
| OAuth provider | Adds external dependency |

## Notes
- Add 2FA (TOTP) if compliance requires it
- Audit every auth event
