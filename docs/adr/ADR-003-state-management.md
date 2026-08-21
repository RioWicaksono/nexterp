# ADR-003: State Management

**Date:** 2024-01-20  
**Status:** Accepted  

## Context
Frontend state needs shared state for auth, notifications, dashboard layout, and form drafts.

## Decision
Use Zustand with middleware for persistence where needed.

| Concern | Solution |
|---------|-----------|
| Auth user + token | Zustand persist → localStorage |
| Dashboard layout | Zustand persist → localStorage |
| Form drafts | Custom localStorage TTL wrapper |
| Notifications | Zustand + memory |
| Server data | TanStack Query |

## Consequences

### Pro
- Zustand: tiny bundle, no boilerplate
- TanStack Query: caching, pagination, background refetch
- localStorage persists drafts/layouts across refreshes

### Con
- Two state systems (store + query cache)

## Notes
- Keep TanStack Query for all API calls
- Store only UI state
