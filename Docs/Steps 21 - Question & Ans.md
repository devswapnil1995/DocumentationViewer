## API Error Handling in .NET

We don't want every controller to have:

```csharp
try
{
    ...
}
catch(Exception ex)
{
    ...
}
```

That's repetitive and inconsistent.

---

**Centralized Exception Handling**

Use ASP.NET Core's global exception handling.

Modern .NET provides:

```csharp
app.UseExceptionHandler();
```

You can also implement custom middleware when you need customized behavior.

Architecture:

```text
Request
   ↓
Middleware
   ↓
Controller
   ↓
Service
   ↓
Exception
   ↑
Global Exception Handler
   ↓
Standard Error Response
```

---

**Custom Exception Middleware**

A common interview implementation:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred.");

            await HandleExceptionAsync(
                context,
                ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType =
            "application/problem+json";

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(
            new
            {
                title = "An unexpected error occurred.",
                status = 500
            });
    }
}
```

Register:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

**Better: ProblemDetails**

For production APIs, use a consistent error contract.

Example response:

```json
{
  "type": "https://example.com/errors/order-not-found",
  "title": "Order not found",
  "status": 404,
  "detail": "Order 1001 was not found.",
  "traceId": "00-abc123..."
}
```

ASP.NET Core supports the **Problem Details** standard.

This is preferable to returning random structures like:

```json
{
  "error": "something went wrong"
}
```

from one controller and:

```json
{
  "message": "failed"
}
```

from another.

---

**HTTP Status Codes**

Know these well:

| Situation                       |                   Status |
| ------------------------------- | -----------------------: |
| Successful GET/POST             | `200 OK` / `201 Created` |
| Bad request/validation          |                    `400` |
| Authentication missing/invalid  |                    `401` |
| Authenticated but not permitted |                    `403` |
| Resource doesn't exist          |                    `404` |
| Conflict                        |                    `409` |
| Too many requests               |                    `429` |
| Unexpected server error         |                    `500` |
| Dependency/service unavailable  |                    `503` |

**401 vs 403**

```text
401 → Who are you?
403 → I know who you are, but you're not allowed.
```

---

**Don't expose internal exception details**

Bad:

```json
{
  "error": "SqlException: Server=tcp:prod-db..."
}
```

Never expose:

* Connection strings
* SQL details
* Stack traces
* Internal class names
* Secrets
* Infrastructure information

to the client.

Instead:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "traceId": "abc123"
}
```

Log the detailed exception internally.

---

**Trace ID**

This is particularly important for your **Production Support + App Insights** topics later.

Client receives:

```json
{
  "status": 500,
  "traceId": "abc123"
}
```

Support team searches:

```text
traceId = abc123
```

in Application Insights.

Then they can follow:

```text
Request
   ↓
Controller
   ↓
Service
   ↓
SQL
   ↓
External API
```

This connects API error handling directly to **distributed tracing**.

---

**Business exceptions**

Suppose:

```csharp
throw new OrderNotFoundException(orderId);
```

Your global handler can map it:

```text
OrderNotFoundException
        ↓
404
```

For example:

```csharp
if (exception is OrderNotFoundException)
{
    context.Response.StatusCode = 404;
}
```

Similarly:

```text
ValidationException → 400
Unauthorized → 401/403
ConflictException → 409
```

Don't map every exception manually if your application can use more structured exception handling; the key interview concept is **consistent exception-to-HTTP mapping**.

---

**Senior-level architecture**

For your interview, remember:

```text
                    API Request
                         │
                         ▼
                Global Exception Handler
                         │
                         ▼
                    Controller
                         │
                         ▼
                   Application Service
                         │
              ┌──────────┼──────────┐
              ▼          ▼          ▼
             DB       External    Cache
                       API
              │          │
              └──────────┼──────────┘
                         │
                    Exception
                         │
                         ▼
                Global Handler
                         │
             ┌───────────┴───────────┐
             ▼                       ▼
          ILogger              ProblemDetails
             │                       │
             ▼                       ▼
     App Insights              Client Response
```
---

**Default/Built-in exception types in .NET**

.NET provides many built-in exceptions.

Common ones:

| Exception                     | Typical situation                   |
| ----------------------------- | ----------------------------------- |
| `Exception`                   | Base class                          |
| `ArgumentException`           | Invalid argument                    |
| `ArgumentNullException`       | Argument is null                    |
| `ArgumentOutOfRangeException` | Argument outside valid range        |
| `NullReferenceException`      | Accessing member on null            |
| `InvalidOperationException`   | Operation invalid for current state |
| `FormatException`             | Invalid format                      |
| `DivideByZeroException`       | Division by zero                    |
| `IndexOutOfRangeException`    | Invalid array index                 |
| `KeyNotFoundException`        | Dictionary key doesn't exist        |
| `IOException`                 | I/O failure                         |
| `UnauthorizedAccessException` | Access denied                       |
| `TimeoutException`            | Operation timed out                 |

Most derive from:

```text id="ywmq8n"
System.Exception
     │
     ├── SystemException
     │     ├── ArgumentException
     │     │      ├── ArgumentNullException
     │     │      └── ArgumentOutOfRangeException
     │     │
     │     ├── InvalidOperationException
     │     ├── NullReferenceException
     │     ├── FormatException
     │     └── ...
     │
     └── ApplicationException
```

You don't need to memorize the entire hierarchy. Understand the **inheritance relationship**, because it determines `catch` behavior.

---

**How multiple `catch` blocks execute**

Consider:

```csharp id="n5gk7h"
try
{
    int x = 10;
    int y = 0;

    var result = x / y;
}
catch (NullReferenceException ex)
{
    Console.WriteLine("Null");
}
catch (DivideByZeroException ex)
{
    Console.WriteLine("Divide by zero");
}
catch (Exception ex)
{
    Console.WriteLine("General exception");
}
```

The exception is:

```text
DivideByZeroException
```

So execution is:

```text id="8t9d4w"
try
 ↓
DivideByZeroException occurs
 ↓
catch NullReferenceException?
 ↓
NO
 ↓
catch DivideByZeroException?
 ↓
YES
 ↓
Execute this catch
 ↓
STOP checking remaining catches
```

Output:

```text
Divide by zero
```

It does **not** continue to:

```csharp id="j9g4jq"
catch (Exception ex)
```

once a matching catch is found.

---
**Order of catch blocks matters**

This is **wrong**:

```csharp id="i1w6y8"
try
{
}
catch (Exception ex)
{
}
catch (NullReferenceException ex)
{
}
```

Why?

Because:

```text id="x7t8re"
Exception
   ↑
NullReferenceException
```

The first catch:

```csharp id="e7q5ar"
catch (Exception ex)
```

can already catch `NullReferenceException`.

Therefore the second catch is unreachable.

The compiler will complain.

---

**Correct order**

Always put:

```text id="1f4y7j"
Specific exception
        ↓
More specific exception
        ↓
General Exception
```

Example:

```csharp id="f3x6g4"
try
{
    // code
}
catch (ArgumentNullException ex)
{
}
catch (ArgumentException ex)
{
}
catch (Exception ex)
{
}
```

Think:

```text id="m1w5so"
Most specific
      ↓
      ↓
Most general
```

---

**What happens if no catch matches?**

No matching catch.

The exception travels **up the call stack**.

For example:

```text id="q9w8ed"
Method C
   ↓
Method B
   ↓
Method A
   ↓
Controller
   ↓
Global Exception Middleware
```

If C throws:

```text id="1zj8p6"
InvalidOperationException
```

and nobody catches it:

```text id="r4b7gk"
C
 ↓
B
 ↓
A
 ↓
Middleware
```

The first matching exception handler up the call stack handles it.

This is exactly why our earlier recommendation of **global exception handling middleware** is useful.

---

**What is `throw`?**

Suppose:

```csharp id="p7w1c0"
try
{
    DoSomething();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Something failed");

    throw;
}
```

This:

```csharp id="zh8t3r"
throw;
```

means:

> **Re-throw the same exception without changing its original stack trace.**

This is usually what you want when you catch an exception only to log it and let the global handler deal with it.

---

**What is `throw ex`?**

Now:

```csharp id="jz8z5e"
catch (Exception ex)
{
    _logger.LogError(ex, "Something failed");

    throw ex;
}
```

This also throws the exception, but there is an important difference:

> `throw ex` resets/restarts the stack trace from the current throw location.

That can make debugging harder because you lose the original throw location from the preserved stack trace.


> **Use `throw;` to rethrow the current exception while preserving its original stack trace. Avoid `throw ex;` when rethrowing because it resets the stack trace to the current location.**

---

**See the difference**

Suppose:

```csharp id="9o5k8m"
public void MethodA()
{
    MethodB();
}

public void MethodB()
{
    MethodC();
}

public void MethodC()
{
    throw new Exception("Something failed");
}
```

The original exception occurs here:

```text id="e5obk9"
MethodA
  ↓
MethodB
  ↓
MethodC  ← exception originated here
```

Now:

`throw;`

```csharp id="w3yq9z"
catch (Exception ex)
{
    throw;
}
```

The stack trace preserves the original origin:

```text id="8knp0w"
MethodC
MethodB
MethodA
...
```

`throw ex;`

```csharp id="v5f3pz"
catch (Exception ex)
{
    throw ex;
}
```

The stack trace can now show the rethrow location as the relevant origin:

```text id="bq8xk6"
catch block
MethodA
...
```

The exact displayed formatting varies, but the important point is:

**`throw;` preserves the original stack trace; `throw ex;` does not.**

---

**What if I want to change the exception?**

Suppose:

```csharp id="zq1a5g"
try
{
    await paymentService.ProcessAsync();
}
catch (PaymentProviderException ex)
{
    throw new PaymentFailedException(
        "Payment processing failed.",
        ex);
}
```

This is valid.

You're creating a **new exception** and keeping the original as the `InnerException`.

```text id="kvf3nj"
PaymentFailedException
       │
       └── InnerException
               │
               └── PaymentProviderException
```

This is useful when you want to translate a low-level exception into a meaningful application/domain exception.

---

**`throw;` vs `throw ex` vs `throw new`**

Remember this table:

| Code                             | Meaning                                                           |
| -------------------------------- | ----------------------------------------------------------------- |
| `throw;`                         | Rethrow current exception, preserve stack trace                   |
| `throw ex;`                      | Rethrow exception but loses original stack-trace location         |
| `throw new Exception(...)`       | Create a new exception                                            |
| `throw new MyException(..., ex)` | Create new exception while preserving original as inner exception |

---
**Very important: don't catch just to throw**

Avoid:

```csharp id="t4m6b3"
try
{
    await service.ProcessAsync();
}
catch (Exception ex)
{
    throw;
}
```

This adds no value.

Either:

```csharp id="v7b7j4"
await service.ProcessAsync();
```

or if you're adding useful logging/context:

```csharp id="myaz2v"
try
{
    await service.ProcessAsync();
}
catch (Exception ex)
{
    _logger.LogError(
        ex,
        "Failed to process order {OrderId}",
        orderId);

    throw;
}
```

That's useful because the exception continues to the centralized handler while you've added valuable contextual logging.

---

**`finally` — another interview favorite**

Exception flow also includes:

```csharp id="qak4qj"
try
{
}
catch (Exception ex)
{
}
finally
{
}
```

`finally` normally executes whether an exception occurs or not.

Example:

```csharp id="7y6j7q"
try
{
    Console.WriteLine("Try");
}
catch
{
    Console.WriteLine("Catch");
}
finally
{
    Console.WriteLine("Finally");
}
```

If exception occurs:

```text id="s0n9ih"
Try
 ↓
Catch
 ↓
Finally
```

If no exception:

```text id="e4m3dz"
Try
 ↓
Finally
```

Common use:

```csharp id="y0r1dz"
finally
{
    connection?.Dispose();
}
```

Although in modern C#, prefer `using`/`await using` for disposable resources where applicable.

---

**Complete execution flow**

This is the mental model I'd remember:

```text id="r0x8yo"
             try
              │
              ▼
        Exception occurs
              │
              ▼
       Matching catch?
          /       \
        NO         YES
        │           │
        │           ▼
        │      Execute catch
        │           │
        │           ▼
        │      finally
        │
        ▼
   Search caller's
   catch blocks
        │
        ▼
 Global exception handler
```

And when you have multiple catches:

```text id="r7gj1a"
Specific
   ↓
Less specific
   ↓
Exception
```

> "The closest matching catch block executes first. If it handles the exception without rethrowing, the exception stops there. If it uses throw;, the exception propagates to the caller's catch, and eventually to the global exception handler if nobody handles it. That's why global exception middleware acts as the final safety net.

```
Repository throws
       ↓
Service catch       ← called first
       ↓
    throw;
       ↓
Controller catch    ← called next
       ↓
Doesn't rethrow
       ↓
Global catch         ← NOT called
```
---
---

**What is a Tenant?**

A **tenant is an independent customer/organization using the same application**.

For example, imagine we build an HR SaaS application:

```text
              HR SaaS Application
                       |
        ┌──────────────┼──────────────┐
        ↓              ↓              ↓
     Company A      Company B      Company C
       Tenant         Tenant         Tenant
```

Each company uses the **same application**, but their data must remain separate.

Example:

```text
Company A
Employees:
  John
  Mike

Company B
Employees:
  Rahul
  Priya
```

Company A must **never see Company B's employees**.

That's where **tenant isolation** comes in.

---

**What is Multi-Tenant?**

**Multi-tenancy means one application serves multiple independent customers/organizations while keeping their data and configuration logically isolated.**

Instead of deploying:

```text
Company A → Application A
Company B → Application B
Company C → Application C
```

we can have:

```text
             One Application
                    |
       ┌────────────┼────────────┐
       ↓            ↓            ↓
   Tenant A      Tenant B      Tenant C
```

This is common in **SaaS applications**.

Examples:

* Salesforce
* Microsoft 365
* Slack
* Shopify
* HRMS SaaS
* Accounting SaaS

---

**Why do we need Multi-Tenancy?**

Suppose we have 1,000 companies.

Without multi-tenancy:

```text
1000 customers
      ↓
1000 application deployments
1000 databases
1000 maintenance operations
```

This becomes expensive and difficult to maintain.

With multi-tenancy:

```text
              SaaS Application
                    |
      ┌─────────────┼─────────────┐
      ↓             ↓             ↓
   Tenant A      Tenant B      Tenant C
      ↓             ↓             ↓
    Data A        Data B        Data C
```

We can share infrastructure while keeping customer data isolated.

**Benefits**

* Lower infrastructure cost
* Easier deployment
* Centralized maintenance
* Easier scaling
* One codebase
* Faster onboarding of customers

---

**What is Multi-Tenant Isolation?**

**Multi-tenant isolation means ensuring one tenant cannot access, modify, or accidentally receive another tenant's data.**

This is the most important part.

Imagine:

```text
GET /api/employees
```

User belongs to:

```text
TenantId = 100
```

Database:

```text
Employee
--------------------------------
Id   Name     TenantId
1    John     100
2    Mike     100
3    Rahul    200
4    Priya    200
```

The query must effectively become:

```sql
SELECT *
FROM Employees
WHERE TenantId = 100;
```

NOT:

```sql
SELECT *
FROM Employees;
```

Otherwise Tenant 100 could receive Tenant 200's data.

---

**How do we implement Tenant Isolation?**

There are several approaches.

`Approach 1 — Shared DB + TenantId`

Most common for many SaaS applications.

```text
One Database
     |
     ├── Employees
     │     ├── TenantId = 100
     │     ├── TenantId = 100
     │     ├── TenantId = 200
     │     └── TenantId = 200
     |
     └── Orders
           ├── TenantId = 100
           └── TenantId = 200
```

Every tenant-owned table has:

```text
TenantId
```

Example:

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Guid TenantId { get; set; }
}
```

Then queries always filter by tenant:

```csharp
var employees = await db.Employees
    .Where(x => x.TenantId == tenantId)
    .ToListAsync();
```

**Risk**

A developer may accidentally forget:

```csharp
.Where(x => x.TenantId == tenantId)
```

That's a serious security problem.

**Better approach with EF Core Global Query Filter**

We can enforce tenant filtering centrally.

```csharp
modelBuilder.Entity<Employee>()
    .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
```

Then:

```csharp
db.Employees.ToListAsync();
```

automatically generates the equivalent of:

```sql
SELECT *
FROM Employees
WHERE TenantId = @TenantId;
```

This reduces the chance of developers forgetting the filter.

**Other Isolation Models**

There are three common models:

| Model                      | Isolation                | Cost    | Common use                       |
| -------------------------- | ------------------------ | ------- | -------------------------------- |
| Shared DB + Shared Tables  | Lower physical isolation | Lowest  | Large SaaS                       |
| Separate Schema per Tenant | Medium/High              | Medium  | Some SaaS                        |
| Separate DB per Tenant     | Highest                  | Highest | Enterprise / sensitive workloads |

**Shared database**

```text
Database
 ├── Tenant A data
 ├── Tenant B data
 └── Tenant C data
```

**Separate schema**

```text
Database
 ├── tenant_a schema
 ├── tenant_b schema
 └── tenant_c schema
```

**Separate database**

```text
Tenant A → DB A
Tenant B → DB B
Tenant C → DB C
```

---

**Where does TenantId come from?**

**Important interview question.**

Usually we don't trust:

```http
GET /employees?tenantId=200
```

because a user could change it to another tenant.

Instead, tenant identity can come from authenticated identity/token claims.

Example JWT:

```json
{
  "sub": "user123",
  "tenantId": "100"
}
```

Then:

```text
Request
   ↓
Authentication
   ↓
JWT
   ↓
TenantId = 100
   ↓
TenantContext
   ↓
EF Core Query
   ↓
WHERE TenantId = 100
```

---

**Real-world .NET Architecture**

A clean approach:

```text
Angular
   ↓
ASP.NET Core API
   ↓
Authentication
   ↓
Tenant Resolution
   ↓
TenantContext
   ↓
Application Service
   ↓
EF Core
   ↓
Tenant-filtered DB
```

Example:

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
}
```

Then services don't need to manually pass tenant ID everywhere.

---

**What else must be isolated?**

Don't think only about SQL tables.

Tenant isolation may be required for:

```text
Database
Files / Blob Storage
Cache
Queues
Search indexes
Logs
Configuration
API access
Background jobs
```

For example, Azure Blob:

```text
container/documents/{tenantId}/{documentId}.pdf
```

Cache:

```text
tenant:100:customer:123
```

rather than:

```text
customer:123
```

Otherwise cached data can accidentally leak between tenants.

---
---

## Azure Functions vs App Service

This is a common **Azure interview question**. The easiest way to remember it:

> **App Service = host a continuously running web application/API.**

> **Azure Functions = run small pieces of code in response to events/triggers.**

**Basic difference**

|                  | Azure App Service                    | Azure Functions                                   |
| ---------------- | ------------------------------------ | ------------------------------------------------- |
| Purpose          | Host web apps/APIs                   | Event-driven/serverless workloads                 |
| Execution        | Application runs as a web app        | Function runs when triggered                      |
| Typical use      | ASP.NET Core Web API                 | Timer, Queue, Blob, Service Bus, Event Grid       |
| Infrastructure   | You manage app/service configuration | Azure manages more of the infrastructure          |
| Scaling          | App Service scaling rules/plans      | Function hosting plan handles scaling             |
| Best for         | Long-running APIs/websites           | Background/event-driven processing                |
| HTTP APIs        | ✅ Excellent                          | ✅ Possible                                        |
| Scheduled jobs   | Possible                             | ✅ Excellent                                       |
| Queue processing | Possible                             | ✅ Excellent                                       |
| Serverless       | ❌                                    | ✅                                                 |
| Cold start       | Generally less concern               | Can occur depending on hosting plan/configuration |

---

**App Service**

Suppose we have:

```text
Angular
   ↓
ASP.NET Core Web API
   ↓
App Service
   ↓
SQL Database
```

The App Service hosts your ASP.NET Core application.

Example:

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        return Ok(await _service.GetOrdersAsync());
    }
}
```

This is a typical **App Service workload**.

Use it when you have:

* REST API
* MVC application
* Web application
* continuously available backend
* authentication/authorization
* business APIs

---

**Azure Functions**

Functions are usually **trigger-based**.

For example:

```text
Service Bus Message
        ↓
Azure Function
        ↓
Process Order
```

Or:

```text
Every midnight
      ↓
Timer Trigger
      ↓
Azure Function
      ↓
Generate Report
```

Or:

```text
Blob uploaded
      ↓
Blob/Event Grid Trigger
      ↓
Azure Function
      ↓
Process PDF
```

**Simple decision rule**

```text
Is it a web application/API?
        ↓
     App Service

Is it triggered by an event/schedule?
        ↓
     Azure Function

Need queue/event processing?
        ↓
     Azure Function

Need a continuously running ASP.NET Core API?
        ↓
     App Service

Need complex stateful workflow?
        ↓
 Durable Functions
```
----
----

## Deploy & Rollback in Version Management

## Deploy & Rollback in Version Management

For a senior .NET/Azure interview, think of this as:

> **Deploy = move a tested version to an environment safely.**

> **Rollback = quickly return to the previously known-good version when the new version causes a problem.**

---

**What is version management?**

Every release should have a traceable version.

For example:

```text
v1.0.0
v1.1.0
v1.2.0
```

or build versions:

```text
Build 125
Build 126
Build 127
```

Ideally, we know:

```text
Production
   ↓
Version: v1.2.0
Commit: abc123
Build: 127
```

So if something goes wrong, we know exactly what was deployed.

---
**How would you implement this in Azure DevOps?**

A typical pipeline:

```text
Git
 ↓
Azure DevOps Pipeline
 ↓
Build
 ↓
Test
 ↓
Package
 ↓
Artifact
 ↓
Deploy DEV
 ↓
Deploy QA
 ↓
Approval
 ↓
Deploy PROD
```

Important concept:

**Build once, deploy many times**
Instead of rebuilding separately for every environment:

```text
Build v1.3.0
      ↓
Artifact v1.3.0
   ↙      ↓      ↘
 DEV      QA      PROD
```

This ensures the same artifact is tested and promoted.


**Azure App Service Deployment Slots**

This is a very useful Azure interview topic.

Suppose:

```text
Production Slot
    ↓
v1.2.0
```

Create a **staging slot**:

```text
App Service
 ├── Production → v1.2.0
 └── Staging    → v1.3.0
```

Deploy the new version to staging:

```text
Staging
   ↓
v1.3.0
```

Test it.

If everything looks good:

```text
        SWAP
         ↓

Production → v1.3.0
Staging    → v1.2.0
```

This can reduce downtime and gives you a fast way to switch versions.

---

**Rollback using Deployment Slots**

Suppose after the swap:

```text
Production → v1.3.0 ❌
Staging    → v1.2.0 ✅
```

You can swap back:

```text
        SWAP
         ↓

Production → v1.2.0 ✅
Staging    → v1.3.0
```

This is much faster than rebuilding and redeploying the old version.

---

**Blue-Green Deployment**

Another common strategy.

```text
Blue
 ↓
Current Production
v1.2.0

Green
 ↓
New Version
v1.3.0
```

Test Green.

Then switch traffic:

```text
Before:

Users → Blue → v1.2.0


After:

Users → Green → v1.3.0
```

If there's a serious issue:

```text
Users → Blue → v1.2.0
```

Key idea

> Keep the old environment available so traffic can quickly be switched back.

---

**Canary Deployment**

Instead of sending 100% traffic to the new version:

```text
Users
  |
  ├── 95% → v1.2.0
  |
  └── 5%  → v1.3.0
```

Monitor:

* Error rate
* Response time
* CPU/memory
* Business metrics
* Application Insights

If healthy:

```text
5% → 25% → 50% → 100%
```

If problems occur:

```text
5% → 0%
```

This reduces the blast radius of a bad release.

---

**Rollback is not always just code**

This is an **important senior-level point**.

Suppose:

```text
v1.2.0
   ↓
Database schema

v1.3.0
   ↓
Changed database schema
```

Rolling application code back may not be enough.

For example:

```text
v1.3.0
Added column
Dropped old column
Changed data format
```

If you simply deploy v1.2.0 again, it may not understand the new database schema.

**Solution: backward-compatible database migrations**

Use a pattern like:

```text
Expand
  ↓
Deploy compatible application
  ↓
Migrate data
  ↓
Remove old structure later
```

This is often called **Expand-and-Contract**.

---

> **"I use a CI/CD pipeline where code is built, tested and packaged into an immutable versioned artifact. The same artifact is promoted through DEV, QA and production. For Azure App Service, deployment slots can be used to deploy and validate the new version before swapping it into production. For critical releases, blue-green or canary deployment can reduce risk. If production metrics such as 5xx errors, latency or critical business failures increase after deployment, I would stop the rollout and return to the previous known-good artifact or swap back to the previous slot. Database changes also need backward-compatible migrations because application rollback alone may not safely rollback the database."**

> A deployment slot itself is an Azure App Service feature, not a deployment strategy. It provides separate environments such as staging and production. We can use deployment slots to implement strategies like blue-green deployment and to achieve safer deployments and quick rollback through slot swaps.
---
----

## Application Insights — Tracing & Alerts: Complete Flow

> **Request → Application → Telemetry → Application Insights → Azure Monitor → Alert Rule → Action Group → Notification/Action**

**What is Application Insights?**

**Application Insights (App Insights)** is an Azure Application Performance Monitoring (APM) service used to monitor an application.

It helps us understand:

* Requests
* Exceptions
* Dependencies
* Response time
* Failures
* Logs
* Distributed traces
* Availability
* Performance
* User/application behavior

Architecture:

```text
                    Azure
                     │
             Application Insights
                     │
        ┌────────────┼────────────┐
        ↓            ↓            ↓
     Logs         Metrics       Traces
        │            │            │
        └────────────┼────────────┘
                     ↓
               Azure Monitor
                     ↓
              Alert Rules
                     ↓
               Action Group
                     ↓
        Email / Teams / SMS / Webhook
```

---

**How does tracing start?**

Suppose Angular calls:

```text
GET /api/orders/1001
```

Flow:

```text
Angular
   ↓
ASP.NET Core API
   ↓
OrderService
   ↓
SQL Database
   ↓
Response
```

Application Insights can capture this entire operation.

---

**Request telemetry**

The incoming HTTP request becomes a **Request telemetry** item.

Example:

```text
GET /api/orders/1001

Status: 200
Duration: 245 ms
```

If something fails:

```text
GET /api/orders/1001

Status: 500
Duration: 1.2 sec
```

App Insights can show:

* URL
* HTTP method
* status code
* duration
* success/failure
* operation/trace ID

---

**Dependency tracing**

Suppose your API calls SQL:

```text
API
 ↓
OrderService
 ↓
SQL
```

App Insights can record the SQL dependency:

```text
Request
  └── SQL dependency
       Duration: 850 ms
```

Similarly:

```text
API
 ↓
Payment API
```

can appear as an external dependency.

Common dependencies:

* SQL Server
* PostgreSQL
* HTTP APIs
* Azure Storage
* Service Bus
* Redis
* Cosmos DB

---

**Distributed tracing**

This becomes very important in microservices.

Suppose:

```text
Client
  ↓
API Gateway
  ↓
Order Service
  ↓
Payment Service
  ↓
Inventory Service
  ↓
SQL
```

One user request can generate many operations.

We want to correlate them:

```text
TraceId = ABC123

API Gateway
   │
   ├── Order Service
   │      │
   │      ├── Payment Service
   │      │
   │      └── Inventory Service
   │
   └── ...
```

**Trace vs Span**

**Trace** = complete request journey.

**Span** = individual operation within that journey.

```text
Trace
│
├── API span
├── Order Service span
├── Payment Service span
└── SQL span
```

This lets you answer:

> "Why did this request take 5 seconds?"

Maybe:

```text
API                 5 sec
 └── Payment API    4 sec
      └── SQL       100 ms
```

Now you know the payment dependency is the bottleneck.

---

**Correlation / Trace ID**

A very important production concept.

Suppose user reports:

> "Order 1001 failed."

You look at logs:

```text
TraceId = abc123
```

You can search App Insights using that trace/correlation ID and follow:

```text
Request
 ↓
Controller
 ↓
Service
 ↓
Payment API
 ↓
Exception
```

This is much easier than searching random log messages.

---

**Logs**

Your .NET application can write structured logs:

```csharp
_logger.LogInformation(
    "Processing order {OrderId}",
    orderId);
```

Or:

```csharp
_logger.LogError(
    ex,
    "Payment failed for Order {OrderId}",
    orderId);
```

App Insights collects this telemetry.

You can query it using **KQL (Kusto Query Language)**.

Example:

```kusto
requests
| where success == false
| order by timestamp desc
```

Exceptions:

```kusto
exceptions
| order by timestamp desc
```

Slow requests:

```kusto
requests
| where duration > 1000
| order by duration desc
```

---

**Metrics**

App Insights/Azure Monitor also provides metrics such as:

```text
Request count
Failure rate
Response time
CPU
Memory
Dependency duration
Exception count
```

For example:

```text
Normal:

Failure rate = 0.5%

Problem:

Failure rate = 8%
```

This can become an alert condition.

---

**Alerts — Complete Flow**

This is the part you should remember for the interview.

Suppose we want:

> Alert when API failure rate becomes too high.

`Step 1 — Application generates telemetry`

```text
API
 ↓
500 errors
 ↓
Application Insights
```

`Step 2 — Azure Monitor evaluates condition`

Example:

```text
Failure rate > 5%
for 5 minutes
```

`Step 3 — Alert rule fires`

```text
Condition satisfied
       ↓
   Alert created
```

`Step 4 — Action Group executes`

An **Action Group** defines what should happen.

For example:

```text
Alert
 ↓
Action Group
 ├── Email DevOps
 ├── Teams notification
 ├── Webhook
 └── Automation/runbook
```

---

**Important Alert Types**

You don't need to memorize every Azure alert type. Understand the categories:

`Metric alert`

Example:

```text
CPU > 80%
```

or:

```text
Failed requests > threshold
```

`Log query alert`

Example KQL:

```kusto
requests
| where success == false
| summarize FailedRequests=count()
```

Then alert if the result crosses a threshold.

`Availability alert`

For example:

```text
API health endpoint unavailable
```

---

**What should you alert on?**

Don't create alerts for everything.

Useful production alerts:

```text
5xx error rate
High latency
Dependency failures
Exception spikes
Availability failure
CPU/memory saturation
Queue backlog
Service Bus DLQ messages
Database failures
```

Avoid noisy alerts such as:

```text
One isolated 404
One harmless validation error
```

Otherwise you get **alert fatigue**.

---

**Logging vs Metrics vs Tracing**

This is a common interview question.

| Type    | Answers                       |
| ------- | ----------------------------- |
| Logs    | **What happened?**            |
| Metrics | **How much/how often?**       |
| Traces  | **Where did the request go?** |

Example:

```text
Metric:
500 errors increased to 10%

Trace:
API → Payment Service → SQL

Log:
Payment API returned timeout
```

Together they give you the complete picture.

---
**Final mental model**

```text
        APPLICATION
             ↓
      Telemetry generated
             ↓
     Application Insights
             ↓
 ┌───────────┼────────────┐
 ↓           ↓            ↓
Logs       Metrics       Traces
 └───────────┼────────────┘
             ↓
       Azure Monitor
             ↓
        Alert Rule
             ↓
       Action Group
             ↓
    DevOps / Developer
             ↓
     Investigate → Fix
```

> I create an Application Insights resource in Azure, obtain its connection string, add the Microsoft.ApplicationInsights.AspNetCore package, and register Application Insights telemetry using AddApplicationInsightsTelemetry() in Program.cs. ASP.NET Core can then automatically collect requests, exceptions and dependencies, while I can use ILogger for structured application logs and TelemetryClient for specific custom events or metrics. The telemetry is available in Application Insights/Azure Monitor, where I can use KQL to investigate failures and latency and configure alert rules with Action Groups for production notifications.

> The Application Insights SDK uses the configured Application Insights connection string to identify the telemetry destination. Historically this was commonly done using an instrumentation key; for modern applications, Microsoft recommends using the connection string.
-------
-------

## Production Support & Live Incident

A production incident is an unexpected issue affecting the live system.

Examples:

```text
API returning 500
API becoming very slow
Database unavailable
Payment failures
Azure Function not processing messages
Service Bus queue backlog
High CPU/memory
Users unable to login
File upload failing
```

`Step 1 — Detect`

The incident can be detected through:

* Application Insights alert
* Azure Monitor
* User/customer complaint
* Support team
* Service Bus/DLQ monitoring
* Health checks
* Automated monitoring

Example:

```text
Application Insights
       ↓
500 errors > 5%
       ↓
Alert
       ↓
DevOps receives notification
```

---

`Step 2 — Assess Impact`

**Don't immediately start changing things.**

First understand:

**What is broken?**

```text
Login?
Orders?
Payments?
Entire API?
One endpoint?
```

**Who is affected?**

```text
All users?
Some tenants?
One customer?
Specific region?
```

**When did it start?**

```text
10:05 AM
```

**What changed recently?**

```text
10:00 AM → New deployment
10:03 AM → Error rate increased
```

This timeline can be extremely useful.

---

`Step 3 — Severity / Impact`

You should understand the business impact.

For example:

| Severity | Example                              |
| -------- | ------------------------------------ |
| Critical | Entire application unavailable       |
| High     | Payment/order processing unavailable |
| Medium   | Important feature degraded           |
| Low      | Minor UI/functionality issue         |

Don't spend 30 minutes investigating the perfect root cause while customers are unable to use a critical service.

---

`Step 4 — Mitigate First`

This is a **senior engineer mindset**.

> **Restore service first, investigate root cause second.**

Suppose:

```text
10:00 → v1.5 deployed
10:05 → 500 errors increase
```

If you determine the deployment is causing the issue, rollback:

```text
v1.5 ❌
  ↓
Rollback
  ↓
v1.4 ✅
```

Other mitigation options:

```text
Rollback
Scale resources
Disable feature
Stop problematic job
Restart unhealthy instance
Fail over dependency
Increase capacity
Move messages to appropriate processing path
```

The correct mitigation depends on the incident.

---

`Step 5 — Investigate`

Now investigate systematically.

For a .NET API:

```text
Application Insights
      ↓
Requests
      ↓
Exceptions
      ↓
Dependencies
      ↓
Logs
      ↓
Distributed Trace
```

Example:

```text
POST /api/orders → 500
        ↓
OrderService
        ↓
PaymentService → Timeout
        ↓
Payment DB → Slow
```

Now you have a direction.

---

**Check Application Insights**

`Failed requests`

```kusto
requests
| where success == false
| order by timestamp desc
```

`Exceptions`

```kusto
exceptions
| order by timestamp desc
```

`Slow requests`

```kusto
requests
| where duration > 1000
| order by duration desc
```

`Dependencies`

Look for:

```text
SQL
HTTP API
Redis
Service Bus
Storage
```

Ask:

> Is the application failing, or is a dependency failing?

---

`Trace the Request`

Suppose:

```text
TraceId = ABC123
```

You can follow:

```text
API Gateway
     ↓
Order API
     ↓
Payment API
     ↓
Database
```

Maybe you discover:

```text
Order API       200 ms
Payment API    4,800 ms
Database        100 ms
```

So instead of randomly modifying the Order API, you investigate the Payment API dependency.

---

`Check Recent Deployment`

Always ask:

> **"What changed?"**

Check:

```text
Code deployment
Configuration
Database migration
Azure infrastructure
Secrets
Third-party API
Certificate
Network
Feature flag
```

Example:

```text
10:00 → v2.3 deployed
10:04 → errors started
```

That is an important correlation to investigate.

It doesn't automatically prove the deployment caused the incident.

---
---

# Generative AI + AI Code Review + Prompt Engineering

For your GBIT interview, I would prepare this as **three connected topics**:

```text
Generative AI
     ↓
LLM / Coding Assistant
     ↓
Prompt Engineering
     ↓
Effective Prompt
     ↓
AI-generated code / review
     ↓
Developer validates + tests
```

---

## What is Generative AI?

**Generative AI is AI that can generate new content based on an input/prompt.**

It can generate:

* Text
* Code
* Documentation
* SQL
* Tests
* Images
* Summaries
* Designs

**How can AI help in Code Review?**

Suppose you have:

```csharp
public async Task<Order> GetOrder(int id)
{
    var orders = await _db.Orders.ToListAsync();

    return orders.FirstOrDefault(x => x.Id == id);
}
```

An AI code review might identify:

> The code loads all orders into memory before filtering. This can cause unnecessary database and memory usage.

Better:

```csharp
public async Task<Order?> GetOrder(int id)
{
    return await _db.Orders
        .FirstOrDefaultAsync(x => x.Id == id);
}
```

**AI can review for:**

```text
Security
Performance
Correctness
Maintainability
Code quality
Exception handling
Async/await usage
SQL/EF Core queries
Tests
Architecture
```

---

**AI Code Review — Senior Developer Approach**

Don't simply ask:

> "Review this code."

Give the AI **context + criteria + expected output**.

For example:

```text
You are reviewing an ASP.NET Core Web API.

Review the following method for:

1. Security vulnerabilities
2. EF Core performance
3. Async/await issues
4. Exception handling
5. SOLID/maintainability
6. Potential production issues

For each issue provide:
- Severity
- Problem
- Why it matters
- Recommended fix

Do not rewrite unrelated parts of the code.

Code:
...
```

This produces much more useful output.

---

> **"I use AI as an assistant, not as the final authority. I validate generated code through code review, compilation, automated tests, security analysis and application-specific business rules."**


**Prompt Engineering**

**Prompt engineering is the practice of designing clear and structured instructions so an AI model produces more useful and reliable results.**

Bad prompt:

```text
Fix this code.
```

Better:

```text
Review this ASP.NET Core API method.

Focus on:
- SQL performance
- async/await
- exception handling
- security

Return:
1. Issues
2. Severity
3. Explanation
4. Corrected code

Do not change the API contract.
```

The second prompt gives the model much more context.

---

**Effective Prompt Structure**

A simple interview-friendly structure:

```text
ROLE
  ↓
CONTEXT
  ↓
TASK
  ↓
CONSTRAINTS
  ↓
EXPECTED OUTPUT
```

**Example**

```text
ROLE:
Act as a senior .NET architect.

CONTEXT:
This is an ASP.NET Core 10 API using EF Core and PostgreSQL.

TASK:
Review this repository method for performance problems.

CONSTRAINTS:
Do not change the public API.
Use async EF Core methods.
Avoid loading unnecessary records.

OUTPUT:
List each issue with severity and provide corrected code.

CODE:
...
```

This is much more effective than:

```text
"Optimize this."
```

---

**Important Prompt Engineering Techniques**

`1. Be specific`

❌

```text
Optimize this code.
```

✅

```text
Optimize this EF Core query for database performance.
Do not change the returned data.
Explain each optimization.
```

---

`2. Provide context`

Instead of:

```text
Fix this API.
```

Give:

```text
.NET 10
ASP.NET Core
EF Core
PostgreSQL
10 million records
API response target < 500 ms
```

Now the AI has useful constraints.

---

`3. Define constraints`

For example:

```text
Do not introduce a new library.
Do not change the database schema.
Keep the existing API contract.
Use cancellation tokens.
```

This prevents unnecessary changes.

---

`4. Specify output format`

For example:

```text
Return a table:

Issue | Severity | Explanation | Recommendation
```

Or:

```text
Return:
1. Problems
2. Root cause
3. Recommended solution
4. Updated code
5. Test cases
```

---

> **"Effective prompting means providing enough context and clear instructions for the model to produce a useful result. I generally specify the role or context, the task, constraints and expected output format. For complex tasks I can provide examples of the expected output. I also validate the model's response rather than assuming it is correct."**

**Memorize this:**

```text
Effective Prompt
      ↓
Context
      +
Clear Task
      +
Constraints
      +
Expected Output
      +
Examples when useful
      ↓
Better AI Response
      ↓
Human Validation
```

And for **AI Code Review**:

```text
Code
 ↓
AI Review
 ↓
Security + Performance + Correctness
 ↓
Developer Validation
 ↓
Tests
 ↓
CI/CD
 ↓
Production
```

----
----
