# ADR-004: Database Design

**Date:** 2024-01-15  
**Status:** Accepted  

## Context
ERP data is relational: employees belong to departments, orders reference suppliers and items.

## Decision
Use PostgreSQL with code-first EF Core migrations.

### Schema patterns
| Pattern | Use Case |
|---------|----------|
| Soft deletes | User-editable records |
| UUID PKs | Distributed-friendly |
| JSONB | Extensible fields |
| Index on FKs | Query perf |
| Partition by tenant | Multi-tenancy ready |

## Consequences

### Pro
- ACID transactions protect financial data
- JSONB handles semi-structured config
- UUIDs avoid ID collision on merge

### Con
- No schema flexibility
- Migrations must be versioned

## Notes
- Run `dotnet ef migrations add` before merge
- Avoid `SELECT *` — project only needed columns
- Batch inserts for bulk operations
