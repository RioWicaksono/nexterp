# NEXTERP CLEAN ARCHITECTURE

Project-specific architecture guidelines for NEXTERP ERP system.

---

## LAYER DEPENDENCIES

```
┌─────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER (ERP.API + ERP.WebUI)               │
│  Controllers, Middleware, API endpoints                 │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  APPLICATION LAYER (ERP.Application)                   │
│  Use Cases, CQRS, MediatR, DTOs, Validators           │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  DOMAIN LAYER (ERP.Domain)                             │
│  Entities, Value Objects, Domain Events, Enums          │
└─────────────────────────────────────────────────────────┘
                            │
                            ▲
┌─────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER (ERP.Infrastructure)              │
│  EF Core, Repositories, External Services              │
└─────────────────────────────────────────────────────────┘
```

**Rule:** Dependencies only point inward. Domain has NO external dependencies.

---

## DOMAIN LAYER (Pure C#)

### Entity Structure
```csharp
public abstract class BaseEntity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### AuditableEntity
```csharp
public abstract class AuditableEntity : BaseEntity<TId>
{
    public Guid OrganizationId { get; private set; }
    public bool IsActive { get; private set; } = true;
}
```

---

## APPLICATION LAYER

### CQRS Structure
```
ERP.Application/
├── [Domain]/
│   ├── Commands/
│   │   ├── Create[Entity]Command.cs
│   │   ├── Create[Entity]CommandValidator.cs
│   │   ├── Update[Entity]Command.cs
│   │   ├── Update[Entity]CommandValidator.cs
│   │   └── Delete[Entity]Command.cs
│   ├── Queries/
│   │   ├── Get[Entity]ByIdQuery.cs
│   │   ├── Get[Entity]ListQuery.cs
│   │   └── [Entity]ListQueryValidator.cs
│   ├── DTOs/
│   └── Interfaces/
├── Common/
│   ├── Behaviors/          → Pipeline behaviors
│   ├── Licensing/          → License services
│   ├── Modules/            → Module configuration
│   └── Exceptions/         → Custom exceptions
```

### MediatR Pipeline
```csharp
// Pipeline order:
1. LoggingBehavior
2. PerformanceBehavior
3. UnhandledExceptionBehavior
4. ValidationBehavior
5. LicenseValidationBehavior
6. AuthorizationBehavior
7. YourCommand/QueryHandler
```

---

## INFRASTRUCTURE LAYER

### Repository Pattern
```csharp
public interface IRepository<TEntity> where TEntity : BaseEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}

public interface I[Entity]Repository : IRepository<[Entity]>
{
    Task<IReadOnlyList<[Entity]>> GetByOrganizationAsync(Guid organizationId);
    Task<[Entity]?> GetByCodeAsync(string code);
}
```

---

## MULTI-TENANCY

### Tenant Isolation
```csharp
// Global Query Filter in DbContext
model.Entity<AuditableEntity>().HasQueryFilter(e => 
    e.OrganizationId == _currentUser.OrganizationId || 
    _currentUser.IsSuperAdmin);
```

### TenantContext Propagation
```csharp
// From JWT to all layers
public interface ICurrentUserService
{
    Guid OrganizationId { get; }
    Guid UserId { get; }
    bool IsSuperAdmin { get; }
}
```

---

## MODULE SYSTEM

### Module Configuration
```csharp
public static class ModuleCodes
{
    public const string INVENTORY = "INVENTORY";
    public const string SALES = "SALES";
    public const string PURCHASING = "PURCHASING";
    public const string HRM = "HRM";
    public const string PROJECTS = "PROJECTS";
    public const string ASSETS = "ASSETS";
    public const string QUALITY = "QUALITY";
    public const string ACCOUNTING = "ACCOUNTING";
    public const string ANALYTICS = "ANALYTICS";
}
```

### Tier Mapping
```csharp
public static class LicenseTiers
{
    public static readonly string[] STARTER = { SALES, INVENTORY };
    public static readonly string[] PROFESSIONAL = { SALES, INVENTORY, HRM, PURCHASING };
    public static readonly string[] ENTERPRISE = { /* all modules */ };
}
```

---

## EVENTS & MESSAGING

### Domain Events
```csharp
public record EmployeeCreatedEvent(
    Guid EmployeeId,
    Guid OrganizationId,
    string EmployeeName,
    DateTime CreatedAt
) : DomainEvent;
```

### Domain Event Dispatcher
```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync<T>(T domainEvent) where T : DomainEvent;
}
```

---

**Auto-loaded for:** Architecture decisions in NEXTERP
