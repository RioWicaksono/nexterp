# ADR-001: Technology Stack

**Date:** 2024-01-15  
**Status:** Accepted  

## Context
We need to build a full-stack ERP application that serves enterprise users with complex workflows, data-heavy UIs, and real-time updates.

## Decision
We will use:

| Layer | Choice | Rationale |
|-------|--------|----------|
| Frontend | Next.js 16 + React 19 | SSR, file-based routing, good DX |
| Language | TypeScript strict | Catch errors at compile time |
| Styling | Tailwind CSS v4 | Fast iteration, responsive design |
| State | Zustand | Lightweight, persist middleware built-in |
| Backend | .NET 8 + MediatR | Strong typing, CQRS-ready |
| Database | PostgreSQL | ACID compliance, JSONB support |
| Cache | Redis | Sub-millisecond reads |
| Auth | JWT + Refresh Tokens | Stateless auth with rotation |
| Container | Docker Compose | Local dev reproducibility |
| Hosting | Vercel (FE) + Railway (BE) | Next.js native + managed .NET hosting |

## Consequences

### Pro
- TypeScript end-to-end reduces runtime errors  
- Next.js handles SEO, SSR, API routes
- Zustand is simpler than Redux

### Con
- Two separate deployments to maintain
- Redis session state needs Redis service

## Alternatives Considered

| Option | Why Not |
|--------|----------|
| Express.js | No SSR, less opinionated |
| NestJS | Over-engineered for our scale |
| Redux Toolkit | Zustand is simpler |
| MongoDB | PostgreSQL relational integrity preferred |

## Notes
- Revisit in 6 months if team grows > 10 engineers
