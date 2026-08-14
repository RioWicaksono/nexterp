# NEXTERP Skills Manifest

This file lists all project-specific skills that auto-load when working on NEXTERP.

## Available Skills

| Skill | File | Auto-Load | Description |
|-------|------|-----------|-------------|
| `/api-design` | api-design.md | ✅ | REST API patterns, versioning, response formats |
| `/security` | security.md | ✅ | Authentication, licensing, input validation |
| `/testing` | testing.md | ✅ | Unit tests, integration tests, coverage |
| `/frontend` | frontend.md | ✅ | Next.js patterns, React components |
| `/tdd` | tdd.md | ✅ | Test-driven development workflow |
| `/performance` | performance.md | ✅ | Caching, database optimization |
| `/architecture` | architecture.md | ✅ | Clean Architecture, CQRS, DDD patterns |

## How to Use

Skills auto-load when:
1. You invoke them explicitly: `/api-design`
2. Working on relevant code (context detection)

## Skill Content

Each skill contains NEXTERP-specific:
- Tech stack patterns
- Configuration examples
- Code snippets
- Best practices
- Project conventions

## Adding New Skills

Create new `.md` files in this directory:
```
.claude/skills/
├── api-design.md
├── architecture.md
├── frontend.md
├── performance.md
├── security.md
├── tdd.md
├── testing.md
└── MANIFEST.md (this file)
```

## Version

Updated: 2026-08-14
