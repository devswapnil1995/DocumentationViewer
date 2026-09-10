## `appsettings.json`

ASP.NET Core applications commonly store configuration in:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Issuer": "my-api",
    "Audience": "my-client"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

You can read configuration using `IConfiguration`:

```csharp
public class MyService
{
    private readonly IConfiguration _configuration;

    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Test()
    {
        var issuer = _configuration["Jwt:Issuer"];
    }
}
```

For nested values:

```csharp
_configuration["Jwt:Issuer"]
```

or:

```csharp
_configuration.GetSection("Jwt")["Issuer"];
```

**Environment-specific configuration**

You normally have:

```text
appsettings.json
appsettings.Development.json
appsettings.Staging.json
appsettings.Production.json
```

For example:

`appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Production/Default"
  }
}
```

### `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "LocalDatabase"
  }
}
```

If:

```text
ASPNETCORE_ENVIRONMENT=Development
```

ASP.NET Core loads:

```text
appsettings.json
        ↓
appsettings.Development.json
```

The environment-specific value overrides the base value.

**Which one wins if the same key exists in both files?**

> `appsettings.Development.json` wins because it is loaded later.

**Configuration provider order**

This is particularly important.

Typical configuration sources include:

```text
appsettings.json
        ↓
appsettings.{Environment}.json
        ↓
User Secrets (Development)
        ↓
Environment Variables
        ↓
Command-line arguments
```

Generally:

> **Later providers override earlier providers when the same key exists.**

For example:

```json
// appsettings.json
{
    "MySetting": "ABC"
}
```

Environment variable:

```text
MySetting=XYZ
```

Then:

```csharp
_configuration["MySetting"]
```

returns:

```text
XYZ
```

---------------
---------------

## Options Pattern

Instead of repeatedly doing:

```csharp
_configuration["Jwt:Issuer"]
_configuration["Jwt:Audience"]
_configuration["Jwt:ExpiryMinutes"]
```

you can strongly type the configuration.

**Configuration**

```json
{
  "Jwt": {
    "Issuer": "my-api",
    "Audience": "my-client",
    "ExpiryMinutes": 60
  }
}
```

**Class**

```csharp
public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryMinutes { get; set; }
}
```

Register:

```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
```

Now you can inject the options.

**`IOptions<T>`**

```csharp
public class AuthService
{
    private readonly JwtOptions _options;

    public AuthService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
}
```

**Characteristics**

```text
IOptions<T>
   ↓
Reads configuration
   ↓
Singleton-style options access
   ↓
Does NOT react to configuration changes
```

It's good for configuration that doesn't need to change while the application is running.

------------------

**`IOptionsSnapshot<T>`**

```csharp
public AuthService(IOptionsSnapshot<JwtOptions> options)
{
    var config = options.Value;
}
```

It can pick up configuration changes for a new scope/request.

Think:

```text
Request 1 → configuration snapshot A

Configuration changes

Request 2 → configuration snapshot B
```

Important:

> `IOptionsSnapshot<T>` is **scoped**.

Therefore, it's commonly useful in web applications where configuration may change between requests.

---

**`IOptionsMonitor<T>`**

```csharp
public AuthService(IOptionsMonitor<JwtOptions> options)
{
    var config = options.CurrentValue;
}
```

It can observe configuration changes and provides the current value.

You can also subscribe:

```csharp
_options.OnChange(options =>
{
    Console.WriteLine("Configuration changed!");
});
```

### Comparison

|                       | `IOptions`      | `IOptionsSnapshot`         | `IOptionsMonitor` |
| --------------------- | --------------- | -------------------------- | ----------------- |
| Configuration changes | ❌ Doesn't react | ✅ New scope gets new value | ✅ Yes             |
| Lifetime              | Singleton       | Scoped                     | Singleton         |
| Current value         | `.Value`        | `.Value`                   | `.CurrentValue`   |
| `OnChange()`          | ❌               | ❌                          | ✅                 |
| Common use            | Static config   | Per-request config         | Dynamic config    |

### Easy interview memory

> IOptions = once

> IOptionsSnapshot = per scope/request

> IOptionsMonitor = monitor changes

---------
---------

## Dependency Injection Container Internals

When you write:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
```

you're registering a service in the **DI container**.

Later:

```csharp
public OrderController(IOrderService service)
{
    _service = service;
}
```

ASP.NET Core asks the DI container:

```text
IOrderService ?
       ↓
OrderService
       ↓
Create/resolve OrderService
       ↓
Inject into Controller
```

**Lifetimes**

| Lifetime      | Meaning                                                            |
| ------------- | ------------------------------------------------------------------ |
| **Transient** | New instance every time requested                                  |
| **Scoped**    | One instance per scope; in web apps typically one per HTTP request |
| **Singleton** | One instance for the application's DI container lifetime           |

Example:

```csharp
services.AddTransient<IEmailService, EmailService>();

services.AddScoped<IOrderService, OrderService>();

services.AddSingleton<ICacheService, CacheService>();
```

**Important interview point**

A singleton can live for the entire application lifetime.

Therefore, don't blindly inject a scoped service into a singleton:

```text
Singleton
   ↓
Scoped ❌
```

because their lifetimes don't align.

------------
------------

## Hosted Services

> Start this work when my application starts, and stop/clean it up when my application shuts down

A hosted service is a service that runs as part of your application's host lifecycle.

Interface:

```csharp
IHostedService
```

It has:

```csharp
StartAsync()
StopAsync()
```

Example:

```csharp
public class MyHostedService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopped");
        return Task.CompletedTask;
    }
}
```

Register:

```csharp
builder.Services.AddHostedService<MyHostedService>();
```

--------------------
--------------------

## BackgroundService

`BackgroundService` is a convenient base class for implementing long-running background work.

> BackgroundService as a ready-made implementation of IHostedService for long-running background work.

Instead of manually implementing `IHostedService`:

```csharp
public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Processing...");

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);
        }
    }
}
```

Register:

```csharp
builder.Services.AddHostedService<Worker>();
```

**Mental model**

```text
ASP.NET Core Application
        |
        +---- Controllers
        |
        +---- Middleware
        |
        +---- BackgroundService
                    |
                    +---- Background work
```

Typical use cases:

* Queue processing
* Sending emails
* Scheduled cleanup
* Polling
* Processing messages
* Periodic jobs

> For a long-running background task such as queue processing, periodic cleanup, or message consumption, I would normally use BackgroundService. I'd implement IHostedService directly when I need more explicit control over the application's startup and shutdown lifecycle

-----------
-----------
## Logging

ASP.NET Core provides:

```csharp
ILogger<T>
```

Example:

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(int orderId)
    {
        _logger.LogInformation(
            "Processing order {OrderId}",
            orderId);
    }
}
```

Notice:

```csharp
"Processing order {OrderId}"
```

This is **structured logging**.

Instead of:

```csharp
_logger.LogInformation(
    $"Processing order {orderId}");
```

prefer:

```csharp
_logger.LogInformation(
    "Processing order {OrderId}",
    orderId);
```

because logging systems can treat `OrderId` as structured data.

### Log levels

Know these:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Think:

```text
Trace       → Very detailed
Debug       → Developer debugging
Information → Normal application flow
Warning     → Something unexpected
Error       → Operation failed
Critical    → Severe application/system failure
```

Example:

```csharp
_logger.LogInformation("Order created");

_logger.LogWarning("Payment retry required");

_logger.LogError(exception, "Payment failed");

_logger.LogCritical(exception, "Database unavailable");
```

---

### Serilog

`ILogger` is an **abstraction**.

Serilog is a popular **logging implementation/provider**.

Conceptually:

```text
Your application
       ↓
ILogger
       ↓
Logging provider
       ↓
Serilog
       ↓
Console / File / Seq / Elasticsearch / etc.
```

This is why your application generally depends on:

```csharp
ILogger<T>
```

rather than directly depending everywhere on Serilog APIs.

----------
--------

## Correlation ID

> A correlation ID uniquely identifies a request or business transaction and allows us to trace it across multiple services. We include the same ID in logs and downstream requests so that when a production issue occurs, we can search that ID and reconstruct the complete request flow.

> A **Correlation ID** is used to **track one request across multiple services/components**.

The easiest way to understand it:

> **Correlation ID = one unique ID that follows a request throughout the entire system.**

**Example**

Imagine a user places an order:

```text
Frontend
   |
   | CorrelationId: ABC-123
   ↓
Order API
   |
   | CorrelationId: ABC-123
   ↓
Payment Service
   |
   | CorrelationId: ABC-123
   ↓
Inventory Service
   |
   | CorrelationId: ABC-123
   ↓
Notification Service
```

Every service logs the same ID.

**Why is it useful?**

Suppose something goes wrong.

You see this in Payment Service:

```text
Payment failed
CorrelationId = ABC-123
```

You can search your logs for:

```text
ABC-123
```

and find:

```text
Order API       → Order created
Payment Service → Payment started
Payment Service → Payment failed
Inventory       → Inventory not updated
Notification    → Notification not sent
```

Without correlation ID, you might have thousands of requests mixed together:

```text
Request 1 → Order created
Request 2 → Payment failed
Request 3 → Inventory updated
Request 4 → Order created
Request 5 → Payment failed
```

It's difficult to know which logs belong to the same business request.

---

**How it looks in an HTTP request**

The client can send:

```http
GET /api/orders/123
X-Correlation-ID: ABC-123
```

Your API reads it:

```csharp
var correlationId = Request.Headers["X-Correlation-ID"].FirstOrDefault();
```

Then include it in logging:

```csharp
_logger.LogInformation(
    "Processing order {OrderId}, CorrelationId: {CorrelationId}",
    orderId,
    correlationId);
```

And when calling another service:

```http
POST /api/payment
X-Correlation-ID: ABC-123
```

So the ID travels with the request.

---

**In microservices, it becomes VERY important**

Consider:

```text
             Correlation ID
                  │
                  ▼
┌──────────┐  ┌──────────┐  ┌──────────┐
│ Order API│→ │ Payment  │→ │Inventory │
└──────────┘  └──────────┘  └──────────┘
      │             │             │
      ▼             ▼             ▼
    Logs          Logs           Logs
      │             │             │
      └─────────────┴─────────────┘
                    │
             Search ABC-123
                    ↓
          Complete request flow
```

This is especially useful for:

* Microservices
* Distributed systems
* API Gateway
* Async messaging
* Debugging production issues
* Distributed tracing

---

### Correlation ID vs Request ID

You may hear both terms.

**Request ID**

Usually identifies a **specific request/operation at one service**.

```text
Order API request → Request ID: R1
Payment request   → Request ID: R2
```

**Correlation ID**

Can identify the **overall business/request flow**:

```text
Order API
   R1
   |
   +---- Correlation ID: C1
              |
Payment       R2
              |
Inventory     R3
```

All can share:

```text
Correlation ID = C1
```

---

**Modern .NET: `Activity` / distributed tracing**

In modern .NET applications, you will also encounter:

```csharp
System.Diagnostics.Activity
```

and tracing concepts such as:

```text
TraceId
SpanId
```

These are more powerful than a simple manually generated correlation ID.

For example:

```text
TraceId: ABC123
   |
   +-- Span: Order API
   |
   +-- Span: Payment Service
   |
   +-- Span: Inventory Service
```

Tools such as Application Insights, OpenTelemetry, Jaeger, etc. can use this information to visualize the complete distributed request.

**Correlation ID = "Show me everything that happened for this request."**

-----------------
------------------