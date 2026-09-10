## .NET Performance & Optimization

### General Performance

**1. What are the best practices for optimizing .NET applications?**
- Use async/await for I/O operations
- Implement caching strategies (in-memory, distributed)
- Profile and monitor applications regularly
- Optimize database queries and use indexes
- Use dependency injection properly
- Minimize reflection usage
- Optimize LINQ queries
- Implement proper error handling

**2. How do you handle memory management in .NET?**
- Understand garbage collection (GC) generations
- Use `IDisposable` pattern and `using` statements
- Avoid memory leaks by disposing unmanaged resources
- Monitor memory usage with profilers
- Use `ref struct` for stack-allocated data
- Avoid large object heap (LOH) fragmentation
- Use `ArrayPool<T>` for temporary arrays

**3. How does garbage collection work in .NET and how can you optimize it?**
- GC has 3 generations (0, 1, 2) for different object lifetimes
- Gen 0 collections are fast; Gen 2 collections are slower
- Use `GC.Collect()` sparingly (only when necessary)
- Reduce allocations to reduce GC pressure
- Monitor with `dotnet-trace` or Visual Studio profiler
- Consider `GCSettings.IsServerGC` for server applications

---

### Caching & Latency

**4. How can you implement caching in APIs with best practices?**
- **In-Memory Cache:** Use `IMemoryCache` for single-server scenarios
- **Distributed Cache:** Use `IDistributedCache` (Redis) for multiple servers
- Set appropriate cache expiration times (sliding vs absolute)
- Cache invalidation strategies (event-based, time-based)
- Use cache tags for grouping related cache entries
- Monitor cache hit/miss ratios
- Avoid cache stampedes with locks

**5. What are some strategies for reducing latency in .NET applications?**
- Use response compression (gzip, brotli)
- Implement output caching for static responses
- Use CDN for static assets
- Optimize database queries and use query profiling
- Implement connection pooling
- Use async operations to free up threads
- Reduce serialization/deserialization overhead
- Enable response compression in Kestrel

---

### Async & Concurrency

**6. How can you optimize the use of async/await in .NET applications?**
- Use `async/await` for I/O-bound operations (not CPU-bound)
- Use `ConfigureAwait(false)` in libraries to avoid context switching
- Avoid `async void` (except for event handlers)
- Avoid deadlocks: don't block async code with `.Result` or `.Wait()`
- Use `ValueTask<T>` for performance-critical hot paths
- Use `CancellationToken` for cancellation support
- Avoid unnecessary `Task` allocations

**7. How can you adjust DI (Dependency Injection) to improve performance?**
- Use **Singleton** for stateless services (logging, configuration)
- Use **Scoped** for stateful services (DbContext) - one per request
- Use **Transient** sparingly (high allocation overhead)
- Cache resolved dependencies where appropriate
- Use **Keyed Services** (.NET 8+) to reduce factory overhead
- Compile DI container for AOT scenarios
- Use **Source Generated Dependency Injection** (.NET 7+)

---

### Database Optimization

**8. What are some techniques for optimizing database access in .NET applications?**
- Use **NoTracking** queries (`AsNoTracking()`) when not updating
- Implement proper indexing on frequently queried columns
- Use `Select()` to fetch only needed columns (projection)
- Use `Include()` for eager loading; avoid N+1 queries
- Use compiled queries for frequently executed queries
- Implement connection pooling
- Use batch operations for bulk inserts/updates
- Use stored procedures for complex queries
- Monitor query performance with `MiniProfiler` or `SqlProfiler`

**9. How can you optimize LINQ in .NET applications for better performance?**
- Avoid `.ToList()` before filtering (defer filtering to database)
- Use `.FirstOrDefault()` instead of `.ToList().FirstOrDefault()`
- Avoid `LINQ to Objects` on large datasets (use SQL)
- Understand `IEnumerable` vs `IQueryable` (defer execution to DB)
- Use `Select()` for projection instead of loading full entities
- Avoid multiple enumerations of the same query
- Use `.GroupBy()` on database, not in-memory
- Profile LINQ queries with SQL logging

---

### Serialization & Reflection

**10. How can you optimize serialization and deserialization for better performance?**
- Use **System.Text.Json** (faster) instead of Newtonsoft.Json
- Use source-generated serialization (`JsonSourceGenerationOptions`)
- Disable property name case-insensitive option if not needed
- Custom serialization for known types
- Use `JsonSerializerOptions.Default` to reuse settings
- Pre-compile serialization for hot paths
- Use `MessagePack` or `Protobuf` for binary serialization
- Cache serializer instances

**11. How can you optimize reflection usage in .NET applications?**
- Cache reflection metadata (Type, PropertyInfo, MethodInfo)
- Use expression trees for dynamic invocation
- Use source generators to replace reflection at compile time
- Avoid reflection in hot paths
- Use `typeof()` instead of `GetType()` when possible
- Cache delegate results from reflection
- Consider ahead-of-time (AOT) compilation to avoid reflection

---

### Error Handling & Monitoring

**12. How can you implement efficient error handling in .NET applications?**
- Use custom exception types for different error scenarios
- Avoid exceptions for flow control
- Implement global exception handling middleware
- Log errors with contextual information
- Use structured logging (Serilog) for better analysis
- Return appropriate HTTP status codes
- Implement retry logic with exponential backoff for transient errors
- Avoid catching generic `Exception`

**13. What are best practices for logging and monitoring .NET applications?**
- Use **structured logging** (Serilog, NLog)
- Include correlation IDs for request tracing
- Monitor key metrics: CPU, memory, response time, error rate
- Use **Application Insights** or **Prometheus** for metrics
- Implement health checks (`/health` endpoint)
- Use distributed tracing for microservices
- Monitor GC statistics
- Set up alerts for critical issues

---

### Profiling & Analysis

**14. How can you profile and analyze the performance of .NET APIs?**
- Use **Visual Studio Profiler** for CPU and memory profiling
- Use **dotnet-trace** command-line tool for production diagnostics
- Use **MiniProfiler** for request-level profiling
- Monitor with **PerfView** for ETW tracing
- Use **Application Insights** for production monitoring
- Analyze flame graphs for CPU bottlenecks
- Check memory allocations with heap snapshots
- Use `dotnet-counters` for real-time metrics

**15. What are common performance pitfalls in .NET applications and how can you avoid them?**
| Pitfall | Impact | Solution |
|---------|---------|----------|
| Blocking async code (`.Result`) | Deadlocks, thread pool starvation | Use `await`, not `.Result` |
| N+1 queries | Excessive database round trips | Use `Include()`, eager loading |
| String concatenation in loops | Memory allocations | Use `StringBuilder` |
| Untracked DbContext queries | Memory growth | Use `AsNoTracking()` |
| Excessive object allocation | GC pressure | Object pooling, `ValueType` |
| Reflection in hot paths | Performance degradation | Cache metadata, use source generators |
| Exception throwing for flow | Stack traces, overhead | Use nullable types, bool returns |
| Hardcoded configs | Deployment issues | Use `IConfiguration` |

---

## Angular Performance & Optimization

### Build & Loading

**1. How can you reduce the initial load time of Angular applications?**
- **Lazy Loading:** Load feature modules on-demand
- **Code Splitting:** Split chunks by route
- **AOT Compilation:** Compile at build time for faster rendering
- **Tree Shaking:** Remove unused code
- **Preloading:** Preload critical routes in background
- **Service Worker:** Cache assets for offline access
- **Compression:** Enable Gzip/Brotli on server
- **Image Optimization:** Use WebP, compress, lazy load images
- **Bundle Analysis:** Use `webpack-bundle-analyzer`

**2. How can you improve the performance of Angular applications (general)?**
- Optimize change detection strategy
- Implement lazy loading
- Use OnPush change detection
- Minimize dependencies
- Use production build mode
- Implement virtual scrolling for large lists
- Cache HTTP requests
- Optimize animations
- Use Web Workers for CPU-intensive tasks

---

### Change Detection & Updates

**3. What are strategies for optimizing change detection in Angular applications?**
- Use **OnPush** change detection strategy for components
- Use **trackBy** function in `*ngFor` loops
- Detach change detector when not needed (`ChangeDetectorRef.detach()`)
- Use **Signals** (.NET 10+) for fine-grained reactivity
- Minimize property binding calls
- Use `async` pipe with OnPush for auto-unsubscribe
- Break large templates into smaller components
- Use `ChangeDetectionStrategy.OnPush` for presentational components

**4. How can you optimize the use of Angular directives for better performance?**
- Create structural directives that defer rendering
- Use `*ngIf` sparingly (removes from DOM, re-creates on true)
- Use `hidden` binding for temporary visibility
- Implement custom directives with minimal DOM operations
- Use `trackBy` in directives with `*ngFor`
- Avoid heavy logic in directive constructors
- Cache directive values when possible

**5. What are strategies for optimizing Angular pipes for better performance?**
- Use **pure pipes** (default) - output depends only on input
- Implement **pure function** pipes (no side effects)
- Cache pipe output when applicable
- Use `async` pipe instead of manual subscription/unsubscribe
- Avoid expensive operations in pipes (they run on every change detection)
- Create custom pipes for repeated transformations
- Use standalone pipes (.NET 10+) to reduce bundle size

---

### State Management & Services

**6. What are best practices for optimizing Angular services?**
- Use **Singleton** pattern for services (provided in root)
- Implement **lazy-loaded** services for feature modules
- Cache data at service level
- Use `BehaviorSubject` for shared state
- Implement **Smart/Presentational** component pattern
- Unsubscribe from observables to prevent memory leaks
- Use `shareReplay()` to avoid duplicate HTTP calls
- Use `takeUntil()` or `async` pipe for auto-unsubscribe

**7. How can you implement session management in Angular for better performance?**
- Store session data in **localStorage** or **sessionStorage**
- Use **BehaviorSubject** in authentication service for shared state
- Cache auth tokens to avoid redundant API calls
- Implement token refresh strategy
- Use **Interceptors** to attach tokens to outgoing requests
- Implement user role caching
- Clear cache on logout
- Use **HTTP caching headers**

---

### Data Handling & RxJS

**8. What are techniques for optimizing RxJS usage in Angular applications?**
- Use **Higher-order operators** (`switchMap`, `mergeMap`) for request switching
- Implement **unsubscription** with `takeUntil()` or `until-destroyed`
- Use `shareReplay()` to prevent duplicate subscriptions
- Cache observable results with `shareReplay(1)`
- Avoid nested subscriptions (use operators instead)
- Use `debounceTime()` for search/filter inputs
- Use `distinctUntilChanged()` to skip duplicate values
- Combine observables with `combineLatest()` for dependent requests
- Use `forkJoin()` for parallel requests

**9. How can you implement lazy loading in Angular applications?**
- Use **Lazy-loaded routes:** `loadChildren: () => import('./feature/feature.module').then(m => m.FeatureModule)`
- Preload critical routes with **PreloadAllModules** strategy
- Use custom preloading strategy for selective preloading
- Implement route-level lazy loading
- Lazy load heavy libraries
- Use **Code Splitting** for feature modules
- Monitor lazy load performance with DevTools

---

### Forms & Error Handling

**10. How can you optimize Angular forms for better performance?**
- Use **OnPush** change detection on form components
- Use **Reactive Forms** (better performance than Template-driven)
- Batch form updates with `patchValue()` instead of individual updates
- Disable form validation when not needed
- Use `debounceTime()` for expensive validators
- Implement asynchronous validators efficiently
- Cache validator results
- Use `unsubscribe()` on form value changes

**11. How can you implement efficient error handling in Angular applications?**
- Implement **Global Error Handler** (ErrorHandler service)
- Use **HTTP Interceptor** for error responses
- Log errors with stack traces
- Display user-friendly error messages
- Implement **automatic retry** for transient errors
- Monitor error rates with analytics
- Use **Toast notifications** for temporary errors
- Implement error recovery strategies

**12. What are best practices for logging and monitoring Angular applications?**
- Implement **custom logger service**
- Log errors, warnings, and performance metrics
- Use **Google Analytics** or similar for user analytics
- Implement **source map uploads** for error tracking
- Monitor **Core Web Vitals** (LCP, FID, CLS)
- Track **application performance** with `performance.measure()`
- Use **Sentry** or similar for error tracking
- Implement custom instrumentation for business metrics

---

### Build & Compilation

**13. How can you use Angular Ahead-of-Time (AOT) compilation to improve performance?**
- **Compile at build time:** Reduces runtime compilation
- **Faster rendering:** Browser doesn't need to compile templates
- **Smaller bundle:** Compiler-specific code not included
- **Error detection:** Template errors caught at build time
- **Security:** Prevents dynamic code injection
- **Default in Angular 9+:** Automatically enabled in production mode
- **Enable locally:** `ng build --aot` for production-like builds

**14. What are best architectural patterns for building scalable Angular applications?**
| Pattern | Purpose | Use Case |
|---------|---------|----------|
| **Smart/Presentational** | Separation of concerns | Container + UI components |
| **NGRX/State Management** | Centralized state | Complex applications |
| **Facade Pattern** | Simplified API | Hide complex service interactions |
| **Single Responsibility** | Focused components | Reusable, testable components |
| **Feature Module Pattern** | Code organization | Feature-based folder structure |
| **Barrel Exports** | Cleaner imports | Organize `index.ts` exports |
| **HTTP Interceptors** | Cross-cutting concerns | Auth, logging, error handling |
| **Custom Directives** | Reusable behavior | Common DOM manipulations |


---

## Advanced .NET: Service Lifetime Gotchas & Tricky Scenarios

### Service Lifetime Combinations (The Dangerous Patterns)

**1. Why is Singleton depending on Scoped service dangerous? How do you fix it?**

**Problem:**
```csharp
// ❌ DANGEROUS - This WILL cause issues!
services.AddScoped<IUserService, UserService>();  // Scoped (per-request)
services.AddSingleton<ICacheService, CacheService>(provider =>
{
    var userService = provider.GetRequiredService<IUserService>();  // ❌ PROBLEM!
    return new CacheService(userService);
});
```

**Why it's dangerous:**
- Singleton is created **once** for the entire application lifetime
- Scoped service is supposed to be created **per-request/scope**
- Singleton captures the **first request's Scoped instance** and **reuses it forever**
- All future requests use the **same stale Scoped service instance**
- Result: **Data leaks between requests, concurrency issues, memory bloat**

**Solution 1: Use Factory/IServiceProvider**
```csharp
// ✅ CORRECT - Lazy resolve at runtime
services.AddScoped<IUserService, UserService>();
services.AddSingleton<ICacheService>(provider =>
{
    return new CacheService(() => 
    {
        // Resolve fresh UserService for each access
        return provider.CreateScope().ServiceProvider.GetRequiredService<IUserService>();
    });
});
```

**Solution 2: Inject IServiceProvider**
```csharp
// ✅ CORRECT - Resolve Scoped when needed
public class CacheService : ICacheService
{
    private readonly IServiceProvider _serviceProvider;

    public CacheService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RefreshCache()
    {
        // Create new scope, get fresh Scoped service
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        // Use fresh instance...
    }
}
```

**Solution 3: Make it Scoped Instead**
```csharp
// ✅ SIMPLEST - If CacheService doesn't need Singleton behavior
services.AddScoped<IUserService, UserService>();
services.AddScoped<ICacheService, CacheService>();
```

**Key Takeaway:**
| Dependency | Can safely depend on | Cannot depend on |
|---|---|---|
| **Singleton** | Singleton only | Scoped, Transient |
| **Scoped** | Singleton, Scoped | Transient (tricky) |
| **Transient** | Singleton, Scoped, Transient | Any (creates new instances) |

---

**2. Why can Scoped services have issues with Transient dependencies? Real-world example?**

**Problem:**
```csharp
// ⚠️ PROBLEMATIC - Scoped service with Transient dependency
services.AddTransient<IRepository, Repository>();  // New instance each time
services.AddScoped<IUserService, UserService>();   // One per request

// UserService
public class UserService : IUserService
{
    public UserService(IRepository repository)  // Transient injected
    {
        _repository = repository;
    }

    public void UpdateUser(int id) { _repository.Save(); }
    public void DeleteUser(int id) { _repository.Save(); }  // Different instance!
}
```

**Real-world Problem:**
```csharp
// In controller
var userService = serviceProvider.GetRequiredService<IUserService>();
userService.UpdateUser(1);    // Updates with Repository instance #123
userService.DeleteUser(1);    // Deletes with Repository instance #124 (NEW!)

// If Repository maintains state, operations are inconsistent!
// ❌ Update succeeds, but Delete doesn't see updated state
```

**Why it's a problem:**
- Each `GetRequiredService<IRepository>()` creates a **new instance**
- Scoped service gets a **specific Transient instance** at injection time
- But if Transient is resolved multiple times within same scope, **different instances are used**
- Common issue: **Unit of Work pattern with Transient repositories**

**Fix: Use Scoped for repositories**
```csharp
// ✅ CORRECT
services.AddScoped<IRepository, Repository>();  // One per request
services.AddScoped<IUserService, UserService>(); // Shares same Repository

// Now both UpdateUser() and DeleteUser() use the SAME Repository instance
```

**Better Fix: Explicit Unit of Work**
```csharp
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;  // Scoped
    }

    public void UpdateUser(int id) { _unitOfWork.Users.Update(id); }
    public void DeleteUser(int id) { _unitOfWork.Users.Delete(id); }  // SAME instance
    public void Commit() { _unitOfWork.SaveChanges(); }
}

// In controller
var userService = serviceProvider.GetRequiredService<IUserService>();
userService.UpdateUser(1);
userService.DeleteUser(1);
userService.Commit();  // Both changes saved together
```

---

### Background Services & Scope Management

**3. How do Background Services properly access Scoped services like DbContext? (Tricky!)**

**Problem:**
```csharp
// ❌ WRONG - Background service with injected Scoped service
public class ReportGeneratorService : BackgroundService
{
    private readonly IUserService _userService;  // ❌ Scoped - BAD for singleton!

    public ReportGeneratorService(IUserService userService)
    {
        _userService = userService;  // Captured once, will stale!
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var users = await _userService.GetAllUsers();  // ❌ STALE DATA!
            await Task.Delay(10000, stoppingToken);  // Every 10 seconds
        }
    }
}
```

**Why it fails:**
- `BackgroundService` is **Singleton** (created once, runs forever)
- Scoped `IUserService` is captured at **service initialization**
- DbContext from the captured service becomes **stale, disposed, unusable**
- All subsequent calls fail or use **old data**

**Correct Solution 1: Use IServiceScopeFactory**
```csharp
// ✅ CORRECT - Create new scope for each operation
public class ReportGeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportGeneratorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create new scope for each iteration
            using (var scope = _scopeFactory.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var users = await userService.GetAllUsers();  // ✅ Fresh DbContext!
                // Process users...
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
```

**Correct Solution 2: Inject IServiceProvider**
```csharp
// ✅ CORRECT - Alternative approach
public class ReportGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ReportGeneratorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Use within scope...
                var users = await userService.GetAllUsers();
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
```

**Correct Solution 3: Custom Scoped Background Service**
```csharp
// ✅ BEST - Generic base class
public abstract class ScopedBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected ScopedBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                await DoWork(scope.ServiceProvider, stoppingToken);
            }

            await Task.Delay(GetInterval(), stoppingToken);
        }
    }

    protected abstract Task DoWork(IServiceProvider serviceProvider, CancellationToken stoppingToken);
    protected virtual int GetInterval() => 10000;
}

// Usage
public class ReportGeneratorService : ScopedBackgroundService
{
    public ReportGeneratorService(IServiceScopeFactory scopeFactory) 
        : base(scopeFactory) { }

    protected override async Task DoWork(IServiceProvider sp, CancellationToken ct)
    {
        var userService = sp.GetRequiredService<IUserService>();
        var users = await userService.GetAllUsers();
        // Process...
    }
}
```

---

### Multithreading, Concurrency & Database Communication

**4. How do background report generation and multithreading interact with DbContext? (Critical!)**

**Problem: DbContext is NOT thread-safe**
```csharp
// ❌ DANGEROUS - DbContext used across threads
public class ReportService
{
    private readonly AppDbContext _dbContext;

    public async Task GenerateReportAsync()
    {
        var orders = await _dbContext.Orders.ToListAsync();  // Main thread

        // ❌ WRONG - Use same DbContext on different thread
        var tasks = orders.Select(order => Task.Run(() =>
        {
            _dbContext.SaveChanges();  // Different thread! CRASH!
        }));

        await Task.WhenAll(tasks);
    }
}

// Result: "InvalidOperationException: A second operation started before previous operation completed"
```

**Correct Solution 1: One DbContext per thread/task**
```csharp
// ✅ CORRECT - Separate DbContext for each parallel operation
public class ReportService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IServiceProvider serviceProvider, ILogger<ReportService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task GenerateReportAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var orders = await dbContext.Orders.ToListAsync();

            // Process orders in parallel with separate DbContext for each
            var tasks = orders.Select(order => ProcessOrderAsync(order));
            await Task.WhenAll(tasks);
        }
    }

    private async Task ProcessOrderAsync(Order order)
    {
        // ✅ CORRECT - Create new scope and DbContext for each task
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Now safe to use on this thread
            var items = await dbContext.OrderItems
                .Where(i => i.OrderId == order.Id)
                .ToListAsync();

            // Generate report for this order
            var reportPath = GenerateReportFile(order, items);
            _logger.LogInformation($"Report generated: {reportPath}");
        }
    }

    private string GenerateReportFile(Order order, List<OrderItem> items)
    {
        // CPU-intensive work...
        return Path.Combine("Reports", $"report_{order.Id}.pdf");
    }
}
```

**Correct Solution 2: Parallel processing with Partitioner**
```csharp
// ✅ CORRECT - Optimized parallel processing
public async Task GenerateReportAsync()
{
    using (var scope = _serviceProvider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var orders = await dbContext.Orders.ToListAsync();
    }

    // Process in parallel with degree of parallelism
    var partitioner = Partitioner.Create(orders, loadBalance: true);

    Parallel.ForEach(partitioner, new ParallelOptions 
    { 
        MaxDegreeOfParallelism = Environment.ProcessorCount 
    }, 
    order =>
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ProcessOrder(order, dbContext);  // Each thread has own DbContext
        }
    });
}

private void ProcessOrder(Order order, AppDbContext dbContext)
{
    var items = dbContext.OrderItems
        .Where(i => i.OrderId == order.Id)
        .ToList();

    GenerateReportFile(order, items);
}
```

**Correct Solution 3: Background Job with Async operations**
```csharp
// ✅ BEST - Using async all the way, proper scoping
public class ReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<int> _orderChannel;

    public ReportBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _orderChannel = Channel.CreateUnbounded<int>();
    }

    public async Task EnqueueOrderReportAsync(int orderId)
    {
        await _orderChannel.Writer.WriteAsync(orderId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process 4 reports concurrently
        var tasks = Enumerable.Range(0, 4)
            .Select(_ => ProcessReportsAsync(stoppingToken))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task ProcessReportsAsync(CancellationToken stoppingToken)
    {
        await foreach (var orderId in _orderChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await GenerateReportAsync(orderId, dbContext, stoppingToken);
            }
        }
    }

    private async Task GenerateReportAsync(int orderId, AppDbContext dbContext, CancellationToken ct)
    {
        var order = await dbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken: ct);
        var items = await dbContext.OrderItems
            .Where(i => i.OrderId == orderId)
            .ToListAsync(ct);

        // Generate report asynchronously
        await Task.Run(() => GeneratePDF(order, items), ct);
    }

    private void GeneratePDF(Order order, List<OrderItem> items)
    {
        // CPU-intensive work
    }
}
```

**Critical Rules:**
| Scenario | Rule |
|---|---|
| **Single-threaded async** | One DbContext per scope ✅ |
| **Parallel processing** | **One DbContext PER THREAD** - use `Parallel.ForEach` with scope creation |
| **Background service** | **Create new scope per operation** - use `IServiceScopeFactory` |
| **Fire-and-forget tasks** | ❌ NEVER - use scoped background service instead |
| **Capturing DbContext** | ❌ NEVER in Singleton - always create fresh scope |

---

**5. Database Connection Pool exhaustion in multithreaded scenarios - How to detect and fix?**

**Problem:**
```csharp
// ❌ DANGEROUS - Exhausts connection pool if tasks don't complete
Parallel.For(0, 1000, new ParallelOptions { MaxDegreeOfParallelism = 500 },
    i =>
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // If this hangs or is slow, connections remain open
            dbContext.Database.ExecuteSqlAsync($"EXEC sp_SlowReportGeneration @id = {i}");
        }
    });
// ❌ 500 connections waiting, pool maxes out = DEADLOCK!
```

**Detection:**
```csharp
// Check pool status
var connection = new SqlConnection(connectionString);
connection.StatisticsEnabled = true;
connection.Open();

var stats = connection.RetrieveStatistics();
Console.WriteLine($"Pool size: {stats["NumberOfPooledConnections"]}");
Console.WriteLine($"In use: {stats["NumberOfNonPooledConnections"]}");
// If in-use approaches max, connections are leaking!
```

**Fixes:**

1. **Reduce MaxDegreeOfParallelism**
```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = 10  // Not 500!  Only 10 concurrent connections
};
```

2. **Increase connection string pool size**
```xml
<!-- appsettings.json -->
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=MyDb;Max Pool Size=200"
  }
}
```

3. **Implement semaphore to limit concurrent operations**
```csharp
private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(20);

public async Task ProcessAsync(int id)
{
    await _connectionSemaphore.WaitAsync();  // Limit to 20 concurrent
    try
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.ExecuteSqlAsync($"EXEC sp_Report @id = {id}");
        }
    }
    finally
    {
        _connectionSemaphore.Release();
    }
}
```

---

## Advanced Angular: Tricky State Management & Performance Scenarios

**1. Why does Singleton service share mutable state and cause data corruption in concurrent requests?**

**Problem:**
```typescript
// ❌ WRONG - Singleton with mutable state
@Injectable({
  providedIn: 'root'  // Singleton!
})
export class UserStateService {
  private user: User = null;

  async loadUser(id: number): Promise<User> {
    this.user = await this.http.get(`/api/users/${id}`).toPromise();
    return this.user;
  }

  getUser(): User {
    return this.user;
  }
}

// Component 1
async ngOnInit() {
  await this.userService.loadUser(1);  // Loads user 1
  console.log(this.userService.getUser());  // User 1
}

// Component 2
async ngOnInit() {
  await this.userService.loadUser(2);  // OVERWRITES with user 2
  console.log(this.userService.getUser());  // User 2
}

// Component 1's view shows User 2 instead of User 1! ❌ DATA CORRUPTION
```

**Why it's a problem:**
- Both components share **same Singleton instance**
- Asynchronous loads can **race and overwrite each other**
- View displays **wrong data**
- Hard to debug!

**Solution 1: Use Subjects/Observables with proper scoping**
```typescript
// ✅ CORRECT - Using observables for reactive updates
@Injectable({
  providedIn: 'root'
})
export class UserStateService {
  private userSubject = new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();

  loadUser(id: number): Observable<User> {
    return this.http.get<User>(`/api/users/${id}`).pipe(
      tap(user => this.userSubject.next(user))
    );
  }
}

// Component 1
ngOnInit() {
  this.userService.loadUser(1).subscribe(user => {
    this.currentUser = user;  // Local component state
  });

  // OR use async pipe
  user$ = this.userService.user$;
}

// Component 2
ngOnInit() {
  this.userService.loadUser(2).subscribe(user => {
    this.currentUser = user;  // Local component state
  });
}

// Each component has local state, no interference
```

**Solution 2: Store data by ID (Cache pattern)**
```typescript
// ✅ CORRECT - Multiple users cached separately
@Injectable({
  providedIn: 'root'
})
export class UserCacheService {
  private cache = new Map<number, BehaviorSubject<User>>();

  getUser(id: number): Observable<User> {
    if (!this.cache.has(id)) {
      this.cache.set(id, new BehaviorSubject<User>(null));
      this.http.get<User>(`/api/users/${id}`).subscribe(
        user => this.cache.get(id).next(user)
      );
    }
    return this.cache.get(id).asObservable();
  }
}

// Both components get THEIR OWN user
this.user1$ = this.userService.getUser(1);  // User 1's observable
this.user2$ = this.userService.getUser(2);  // User 2's observable
```

---

**2. How do you prevent memory leaks when unsubscribing from Observables in routes?**

**Problem:**
```typescript
// ❌ MEMORY LEAK - Subscription never unsubscribes
export class ProductListComponent implements OnInit {
  products: Product[] = [];

  constructor(private productService: ProductService) {}

  ngOnInit() {
    this.productService.getProducts().subscribe(products => {
      this.products = products;
    });
    // If user navigates away, component is destroyed but subscription continues!
  }
}

// When user visits ProductList 100 times:
// - 100 active subscriptions running
// - Memory keeps growing
// - Eventually OOM (Out of Memory)
```

**Solution 1: Manual Unsubscribe**
```typescript
// ✅ CORRECT - Explicit unsubscribe
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  private subscription: Subscription;

  constructor(private productService: ProductService) {}

  ngOnInit() {
    this.subscription = this.productService.getProducts()
      .subscribe(products => {
        this.products = products;
      });
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();  // Clean up
  }
}
```

**Solution 2: takeUntil pattern**
```typescript
// ✅ CORRECT - Automatic unsubscribe
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  private destroy$ = new Subject<void>();

  constructor(private productService: ProductService) {}

  ngOnInit() {
    this.productService.getProducts()
      .pipe(
        takeUntil(this.destroy$)  // Auto-unsubscribe when $ emits
      )
      .subscribe(products => {
        this.products = products;
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

**Solution 3: async pipe (BEST)**
```typescript
// ✅ BEST - Framework handles unsubscribing
export class ProductListComponent {
  products$ = this.productService.getProducts();  // Just get observable

  constructor(private productService: ProductService) {}

  // Template
  // <div *ngFor="let product of products$ | async">{{ product.name }}</div>
  // Angular automatically unsubscribes when component destroys!
}
```

---

**3. How do RxJS unsubscribes interact with Angular routing? Potential issues?**

**Problem:**
```typescript
// ❌ DANGEROUS - Subscription survives route navigation
export class DashboardComponent implements OnInit {
  constructor(private route: ActivatedRoute, private service: DataService) {}

  ngOnInit() {
    // Auto-unsubscribes when params change
    this.route.params.subscribe(params => {
      this.service.getData(params.id).subscribe(data => {
        // This continues even after route changes!
        console.log(data);
      });
    });
  }
}

// User navigates: /dashboard/1 → /dashboard/2 → other page
// Original subscription for /dashboard/1 still runs!
```

**Correct Solution:**
```typescript
// ✅ CORRECT - Unsubscribe when route changes
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  constructor(private route: ActivatedRoute, private service: DataService) {}

  ngOnInit() {
    this.route.params
      .pipe(
        switchMap(params => this.service.getData(params.id)),
        // switchMap automatically unsubscribes previous and subscribes to new
        takeUntil(this.destroy$)
      )
      .subscribe(data => console.log(data));
  }

  ngOnDestroy() {
    this.destroy$.next();
  }
}
```

---

**4. How do you handle race conditions in Angular with multiple Observables completing at different times?**

**Problem:**
```typescript
// ❌ RACE CONDITION - Operations complete in unpredictable order
export class OrderService {
  async submitOrder(items: CartItem[]): Promise<void> {
    // Save items
    this.http.post('/api/items', items).toPromise();

    // Save delivery address
    this.http.post('/api/delivery', this.address).toPromise();

    // Process payment
    this.http.post('/api/payment', this.payment).toPromise();

    // ❌ All start simultaneously, but if payment fails AFTER items saved,
    // items are in DB but payment failed - corrupted state!
  }
}
```

**Solution 1: Sequential with concatMap**
```typescript
// ✅ CORRECT - Strictly sequential
submitOrder(items: CartItem[]): Observable<void> {
  return this.http.post('/api/items', items).pipe(
    concatMap(() => this.http.post('/api/delivery', this.address)),
    concatMap(() => this.http.post('/api/payment', this.payment)),
    map(() => void 0)
  );
  // Order guaranteed: items → delivery → payment
}
```

**Solution 2: Parallel with guaranteed completion**
```typescript
// ✅ CORRECT - Parallel but wait all before proceeding
submitOrder(items: CartItem[]): Observable<void> {
  return forkJoin([
    this.http.post('/api/items', items),
    this.http.post('/api/delivery', this.address),
    this.http.post('/api/payment', this.payment)
  ]).pipe(
    map(() => void 0),
    catchError(error => {
      // If ANY fails, entire transaction fails
      return throwError(() => new Error('Order submission failed'));
    })
  );
  // All complete before returning; if any fails, all fail
}
```

**Solution 3: Transaction-like with rollback**
```typescript
// ✅ BEST - Database transaction pattern
submitOrder(items: CartItem[]): Observable<OrderResponse> {
  return this.http.post<OrderResponse>('/api/orders', { items, delivery: this.address, payment: this.payment })
    .pipe(
      catchError(error => {
        // API handles transaction - all or nothing
        return throwError(() => new Error('Order failed and rolled back'));
      })
    );
}
```

---

## Summary: Tricky Questions Cheat Sheet

### .NET Tricky Gotchas
1. ❌ **Singleton → Scoped**: Will capture and reuse stale instance
2. ❌ **DbContext across threads**: "Second operation" error
3. ❌ **Background service with Scoped injection**: Stale at initialization
4. ❌ **Parallel with large pool**: Connection exhaustion
5. ⚠️ **Unit of Work with Transient repos**: Inconsistent state

### Angular Tricky Gotchas
1. ❌ **Singleton with mutable state**: Data corruption in concurrent requests
2. ❌ **Unclosed subscriptions**: Memory leaks, growing RAM
3. ❌ **Multiple observables with different completion times**: Race conditions
4. ❌ **Async operations on destroyed components**: Null reference errors
5. ⚠️ **Change detection with impure pipes**: Runs on every digest cycle



### .NET Interview Focus Areas
1. Async/await and concurrency patterns
2. Memory management and garbage collection
3. Entity Framework Core and database optimization
4. Dependency injection and service lifetime
5. Caching strategies
6. Logging and monitoring
7. Performance profiling tools

### Angular Interview Focus Areas
1. Change detection optimization
2. Lazy loading and code splitting
3. RxJS and observable patterns
4. Component architecture (smart/presentational)
5. Performance monitoring
6. AOT compilation
7. State management patterns