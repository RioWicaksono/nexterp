# NEXTERP PERFORMANCE GUIDELINES

Project-specific performance optimization for NEXTERP ERP system.

---

## CACHING STRATEGY

### Multi-Level Caching
```
L1: In-memory cache (per instance)
L2: Redis cache (distributed)
L3: CDN (static assets)
```

### Redis Implementation
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
```

### Cache Keys Pattern
```
nexterp:{org}:{module}:{resource}:{id}
Examples:
nexterp:org123:inventory:warehouses:all
nexterp:org123:inventory:stockitems:sku-001
nexterp:org123:hrm:employees:dept-sales
```

### Cache Invalidation
```csharp
// Event-based invalidation
public async Task OnEmployeeUpdated(EmployeeUpdatedEvent evt)
{
    await _cache.RemoveByPatternAsync($"nexterp:{evt.OrganizationId}:hrm:employees:*");
    await _cache.RemoveAsync($"nexterp:{evt.OrganizationId}:hrm:departments:{evt.DepartmentId}");
}
```

---

## DATABASE OPTIMIZATION

### Query Performance
- Use `.AsNoTracking()` for read-only queries
- Implement pagination for all list endpoints
- Use indexed columns for filtering/sorting
- Avoid N+1 queries with Include/Eager loading

### Connection Pooling
```json
{
  "ConnectionStrings": {
    "DefaultConnection": {
      "MaxPoolSize": 100,
      "MinPoolSize": 10,
      "ConnectionIdleLifetime": 300
    }
  }
}
```

### Query Timeouts
```csharp
// Global timeout: 30 seconds
options.CommandTimeout = 30;

// Per-query timeout
await _context.Database
    .ExecuteSqlRawAsync("SET LOCAL statement_timeout = '30s'");
```

---

## API PERFORMANCE

### Response Compression
```csharp
// Program.cs
app.UseResponseCompression();
```

### Batch Operations
```csharp
// Instead of N requests
// Allow batch endpoint
POST /api/v1/employees/batch
{
  "operations": [
    { "action": "create", "data": {...} },
    { "action": "update", "id": "...", "data": {...} }
  ]
}
```

### Field Selection
```
GET /api/v1/employees?fields=id,firstName,lastName,email
```

---

## RATE LIMITING

### Redis-Backed Sliding Window
```csharp
public class RateLimitService
{
    public async Task<RateLimitResult> CheckRateLimitAsync(
        string clientId, 
        int maxRequests, 
        TimeSpan window)
    {
        var key = $"ratelimit:{clientId}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - (long)window.TotalSeconds;
        
        // Remove old entries
        await _redis.ZRemRangeByScoreAsync(key, 0, windowStart);
        
        // Count current entries
        var count = await _redis.ZCardAsync(key);
        
        if (count >= maxRequests)
        {
            return RateLimitResult.Exceeded;
        }
        
        // Add new entry
        await _redis.ZAddAsync(key, (now, now.ToString()));
        await _redis.KeyExpireAsync(key, window);
        
        return RateLimitResult.Allowed;
    }
}
```

---

## FRONTEND PERFORMANCE

### Bundle Optimization
```javascript
// next.config.js
module.exports = {
  compiler: {
    removeConsole: process.env.NODE_ENV === 'production',
  },
  experimental: {
    optimizeCss: true,
  },
};
```

### Image Optimization
```typescript
<Image
  src="/warehouse.jpg"
  alt="Warehouse"
  width={800}
  height={600}
  placeholder="blur"
  blurDataURL={generateBlurPlaceholder()}
/>
```

### Code Splitting
```typescript
// Dynamic imports for heavy components
const DataTable = dynamic(() => import('@/components/tables/DataTable'), {
  loading: () => <TableSkeleton rows={10} />,
  ssr: false,
});
```

---

## MEMORY MANAGEMENT

### Resource Cleanup
```csharp
public class EmployeeService : IDisposable
{
    private bool _disposed;
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _httpClient?.Dispose();
        _logger?.LogInformation("EmployeeService disposed");
        
        _disposed = true;
    }
}
```

### Async Stream Processing
```csharp
// Process large datasets without loading all in memory
await foreach (var batch in _repository.GetEmployeesBatched(batchSize: 1000))
{
    await ProcessBatch(batch);
}
```

---

## MONITORING & OBSERVABILITY

### Health Endpoints
```
GET /health          → Overall health
GET /health/ready    → Readiness check
GET /health/live     → Liveness check
```

### Metrics to Track
- Response time (p50, p95, p99)
- Error rate
- Cache hit ratio
- Database query time
- Active connections

---

**Auto-loaded for:** Performance tasks in NEXTERP
