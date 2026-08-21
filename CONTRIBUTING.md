# Contributing to NEXTERP ERP

Thank you for your interest in contributing!

## Setup

```bash
# Fork and clone
git clone https://github.com/YOUR_FORK/nexterp.git
cd nexterp

# Install
cd nextjs-frontend && npm install

# Start dev
dotnet build ERP.API && dotnet run --project ERP.API &
npm run dev
```

## Branching

| Branch | Use |
|--------|-----|
| `master` | Production-ready only |
| `develop` | Integration |
| `feature/*` | New work |
| `fix/*` | Bug fixes |

## Commits

Use Conventional Commits:

```
feat: add dashboard export  
fix: resolve login timeout  
docs: update README
```

## Pull Requests

1. New branch from `develop`
2. Pass CI (lint, tests, build)
3. PR description explains **why**, not what
4. Review required

## Testing

```bash
# Frontend
npm run test:e2e

# Backend  
dotnet test
```

## Code Style

- ESLint + Prettier enforced in CI
- C# uses default dotnet format
- No `TODO:` comments — open an issue instead

## Questions

Open a GitHub Discussion.
