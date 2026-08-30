- Request Pipeline overview & Middleware
- Routing (attribute routing, conventional routing)
- Filters: Action, Exception, Authorization, Resource, Result filters
- Dependency Injection lifetimes: Transient, Scoped, Singleton
- Controllers vs Minimal APIs
- API Versioning
- Content Negotiation
- Rate Limiting (built-in .NET 7+ feature)
- Caching
- Health Checks
- Exception Handling (global exception middleware, ProblemDetails)
- CORS

------------------------
------------------------

## What is API?
- API is stands for Application Programming Interface
- It has business logic or data communication logic which we will provide to client based on request
- It has some sets of rule before any request completed
- They are web based services

### Type of API
- Open API(Public API) - Available to developers and users with minimal restrictions
- Partner APIs: Shared with business partners, access requires specific rights
- Internal APIs(Private): Used within organisation
- Composite API: Combine multiple data or service APIs

------------------------
------------------------

## Request Pipeline overview & Middleware

> The journey an HTTP request takes through your application before a response comes back.

> The ASP.NET Core request pipeline is a sequence of middleware components through which every HTTP request and response passes.
Middleware can inspect or modify the request/response and can either handle the request or call the next middleware.
Typical middleware includes exception handling, HTTPS redirection, static files, routing, authentication, authorization, and endpoint execution.
The order is important because middleware wraps subsequent middleware, so the request flows forward and the response flows back through the pipeline.


**Big picture**

```text
Client
  │
  │ HTTP Request
  ▼
┌──────────────────────┐
│ ASP.NET Core Server  │
│      (Kestrel)       │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Middleware 1         │
└──────────┬───────────┘
           ▼
┌──────────────────────┐
│ Middleware 2         │
└──────────┬───────────┘
           ▼
┌──────────────────────┐
│ Middleware 3         │
└──────────┬───────────┘
           ▼
      Routing
           │
           ▼
    Authentication
           │
           ▼
     Authorization
           │
           ▼
     Controller / API
           │
           ▼
        Response
           │
           ▼
      Middleware
       runs back
           │
           ▼
         Client
```

### What is Middleware?

> In .NET Core middleware is software component that assembled into application which will handle request and response

- Each middleware will does something with the request pipeline
- Can execute some code logic and pass request to next middleware in pipeline
- Short circuit the pipeline(stop processing and send a response)
- Basically it is a security guard, inspector, helper and may be a special software component for request.

For example:

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("Before");
    await next();
    Console.WriteLine("After");
});
```

This is powerful because middleware wraps the next middleware.

**The "onion" model:**

Suppose you have:

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("A - Before");

    await next();

    Console.WriteLine("A - After");
});

app.Use(async (context, next) =>
{
    Console.WriteLine("B - Before");

    await next();

    Console.WriteLine("B - After");
});

app.Run(context =>
{
    Console.WriteLine("C");
    return Task.CompletedTask;
});
```

Request execution looks like:

```text
Request
   │
   ▼
A Before
   │
   ▼
B Before
   │
   ▼
C
   │
   ▼
B After
   │
   ▼
A After
   │
   ▼
Response
```

So middleware executes:

```text
A → B → C → B → A
```

### `Use`, `Run`, and `Map`

**`Use`**

Usually allows you to call the next middleware:

```csharp
app.Use(async (context, next) =>
{
    await next();
});
```

**`Run`**

Terminates the pipeline. Nothing after this middleware executes for that request.

```csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello");
});
```

**`Map`**

Creates a branch in the pipeline.

```csharp
app.Map("/admin", adminApp =>
{
    adminApp.Run(async context =>
    {
        await context.Response.WriteAsync("Admin");
    });
});
```

Conceptually:

```text
                 ┌── /admin → Admin pipeline
Request ─────────┤
                 └── other → Normal pipeline
```

**Why middleware order matters**

This is **very important**.

For example:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

should normally be in that order.
Why? Because authorization needs to know **who the authenticated user is**.

```text
Authentication
       ↓
User.Identity
       ↓
Authorization
       ↓
Is user allowed?
```

If you reverse them, you can get incorrect behavior.

***Exception handling should be early***

Usually:

```csharp
app.UseExceptionHandler(...);
```

is placed early in the pipeline.

Why?

Because it needs to catch exceptions thrown by middleware/endpoints later in the pipeline.

Conceptually:

```text
Exception Handler
       │
       ▼
Authentication
       │
       ▼
Authorization
       │
       ▼
Controller
       │
       X
    Exception
       │
       ▼
Exception Handler catches it
```

***Where does Dependency Injection fit?***

ASP.NET Core creates controllers/services using the DI container.

For example:

```csharp
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service)
    {
        _service = service;
    }
}
```

The pipeline eventually reaches the controller, and ASP.NET Core resolves:

```text
Controller
    ↓
IEmployeeService
    ↓
EmployeeService
    ↓
Repository
    ↓
Database
```

### Built in middlewares
- UseRouting()/UseEndpoints() - routing & endpoint binding
- UseStaticFiles() - to serve static files
- UseAuthentication() / UseAuthorization() - security pipeline
- UseExceptionHandler() / UseDeveloperExceptionPage() - global exception handling
- UseHttpsRedirection() / UseHsts() - HTTPs handling
- UseCors() - CORS Support
- UseWebSockets() - WebSocket Support

### What is a custom middleware?

ASP.NET Core already provides middleware, But sometimes **you need your own logic** that should run for many/all requests.

For example:

* Logging every request
* Adding a correlation/request ID
* Measuring API execution time
* Global exception handling
* Checking custom headers
* Auditing
* Request/response modification
* Blocking certain requests

For these, you can create **custom middleware**.

### Basic custom middleware

A middleware essentially looks like:

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("Request received");

        await _next(context);

        Console.WriteLine("Response completed");
    }
}
```

Then register it:

```csharp
app.UseMiddleware<LoggingMiddleware>();
```

### Understand `_next`

> "I have finished my work. Continue to the next middleware. It is called request delegate"

***What happens if we don't call `_next()`?***

This is very important.

Consider:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    Console.WriteLine("Request received");
    // No _next()
}
```

For example, you might reject a request:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    if (!context.Request.Headers.ContainsKey("X-API-KEY"))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("API key required");

        return;
    }

    await _next(context);
}
```

So:

```text
Valid request
     ↓
_next()
     ↓
Continue

Invalid request
     ↓
Return response
     ↓
STOP
```

### Three common ways to create middleware

***Approach 1 — Inline middleware***

For small logic:

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("Before");
    await next();
    Console.WriteLine("After");
});
```

Good for:
* Small logic
* Quick experiments
* Very simple middleware

***Approach 2 — Middleware class***

For reusable logic:

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine("Before");
        await _next(context);
        Console.WriteLine("After");
    }
}
```

Register:

```csharp
app.UseMiddleware<LoggingMiddleware>();
```

This is probably the **most common custom middleware approach** you'll see.

***Approach 3 — Extension method***

Usually, we don't want `Program.cs` to become:

```csharp
app.UseMiddleware<A>();
app.UseMiddleware<B>();
app.UseMiddleware<C>();
app.UseMiddleware<D>();
```

Instead, we create an extension:

```csharp
public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomLogging(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<LoggingMiddleware>();
    }
}
```

Then:

```csharp
app.UseCustomLogging();
```
---
### Real-world example

`1. Request logging:`

Suppose you want:

```text
Request:
GET /api/employees/10

Response:
200

Time:
45 ms
```

Custom middleware:

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        await _next(context);

        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation(
            "{Method} {Path} returned {StatusCode} in {Duration} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            duration.TotalMilliseconds);
    }
}
```

Register:

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

Now every request can be measured.

----
`2. Correlation ID:`

This is extremely common in enterprise applications.

Suppose a user calls:

```http
GET /api/orders/100
```

You generate:

```text
Correlation ID:
abc-123-xyz
```

Then logs throughout the application can contain:

```text
abc-123-xyz
```

So when something goes wrong, you can search logs using that ID.

Middleware:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var correlationId = Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    await _next(context);
}
```

Then another component can retrieve it:

```csharp
var correlationId = context.Items["CorrelationId"];
```

This is a very good **real-world middleware use case**.

----
`3. Global exception handling:`

You don't want every controller to do:

```csharp
try
{
    ...
}
catch
{
    ...
}
```

Instead, middleware can handle exceptions globally.

Conceptually:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        // Log exception

        context.Response.StatusCode = 500;

        await context.Response.WriteAsync(
            "Something went wrong");
    }
}
```

Flow:

```text
Request
   ↓
Exception Middleware
   ↓
Controller
   ↓
Service
   ↓
Exception 💥
   ↓
Exception Middleware
   ↓
500 Response
```

This is one of the most valuable uses of middleware.


**Middleware can access almost everything in `HttpContext`**

You have:

```csharp
context.Request
context.Response
context.User
context.Request.Headers
context.Request.Query
context.Request.RouteValues
context.Items
context.Connection
```

For example:

```csharp
var method = context.Request.Method;
var path = context.Request.Path;
var user = context.User;
var headers = context.Request.Headers;
var statusCode = context.Response.StatusCode;
```

So middleware can make decisions based on the HTTP request.

### Filter

Runs more specifically around MVC/controller execution.

```text
Request
 ↓
Middleware
 ↓
Controller
    ↓
Action Filter
    ↓
Action
```

So:

> **Middleware is broader; filters are more MVC/controller-specific.**

### Middleware ordering

This is probably the biggest practical issue.

Suppose:

```csharp
app.UseMiddleware<A>();
app.UseMiddleware<B>();
app.UseMiddleware<C>();
```

Execution:

```text
Request

A Before
   ↓
B Before
   ↓
C Before
   ↓
Endpoint
   ↓
C After
   ↓
B After
   ↓
A After

Response
```

Therefore:

> **The order in which middleware is registered matters.**

### A very practical ASP.NET Core example

You might have:

```csharp
var app = builder.Build();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
```

Think:

```text
                  REQUEST
                     │
                     ▼
            Exception Handler
                     │
                     ▼
          Correlation ID
                     │
                     ▼
                  Routing
                     │
                     ▼
             Authentication
                     │
                     ▼
              Authorization
                     │
                     ▼
             Request Logging
                     │
                     ▼
               Controller
                     │
                     ▼
                  Service
                     │
                     ▼
                Database
                     │
                     ▼
                RESPONSE
```

### Interview questions to remember

**"What happens if middleware doesn't call `next()`?"**

> "The pipeline is short-circuited. Subsequent middleware and the endpoint won't execute. This is useful when middleware itself wants to generate a response, for example when rejecting an unauthorized request."

**"Why is middleware order important?"**

> "Middleware executes in the order it is registered for the request and in reverse order as control returns for the response. Therefore, middleware that depends on something being established by another middleware must be registered after it."

**"What are common use cases for custom middleware?"**

> "Cross-cutting concerns such as global exception handling, request/response logging, correlation IDs, security headers, auditing, request timing, custom authentication or validation, and rate limiting."

-----

### DI lifetimes and thread-safety
- Middleware is usually registered as a singleton component of pipeline (class with RequestDelegate next is constructed once). 
BUT if your middleware has constructor-injected scoped services, it’s fine — DI resolves scoped dependencies per request when using UseMiddleware<T>(). 
- Avoid storing scoped services into middleware fields and re-using them across requests.
- If you implement IMiddleware, DI can create middleware per-request (depending on registration), avoiding lifetime pitfalls.
- Do not store HttpContext or other request scoped objects on static fields/singletons.

-----

### Performance & best practices
- Avoid blocking I/O — always prefer async/await.
- Don’t copy large responses into memory (response-capture) unless necessary.
- If reading a request body, use EnableBuffering() carefully and set size limits.
- Keep middleware lightweight; heavy logic may belong to background services.
- Use caching, compression, and CDN for static assets instead of middleware when appropriate.

-----

### Why should we not read http header or request data and use it in middleware for any reason? [JP Morgan - 7 Aug 2025]

**Request Body can only be read once by default**
In ASP.NET Core, the request body (HttpContext.Request.Body) is a forward-only stream.
Once you read it in middleware, it’s consumed — meaning downstream middleware or MVC controllers won’t be able to read it (e.g., FromBody model binding will fail).

```csharp
// Middleware reads body
using var reader = new StreamReader(context.Request.Body);
var body = await reader.ReadToEndAsync();
```

// Controller tries to read body → but stream is empty
Fix if you must read it:
Enable buffering first, then rewind the stream:

```csharp
context.Request.EnableBuffering();
using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
var body = await reader.ReadToEndAsync();
context.Request.Body.Position = 0; // Reset so downstream can read
```

**Headers are fine, but…**
Headers can be read as many times as you want — no problem there.
The risk is timing:
Some headers may not yet be set until a certain middleware runs (especially if reverse proxies modify them).
If you depend on a header (e.g., X-Forwarded-For) before UseForwardedHeaders(), you’ll get the wrong value.

**Performance considerations**
Reading large request bodies in middleware can slow down the pipeline significantly, especially for file uploads or JSON payloads.
If you store the body in memory for logging/inspection, it can consume a lot of RAM and cause OutOfMemoryExceptions under load.

**Security concerns**
Logging raw headers or request bodies in middleware can expose sensitive data (passwords, tokens, PII) to logs.
Middleware runs for every request — including authentication requests, payment data, etc.
→ Always filter or mask sensitive info before logging.

**Rule of thumb**
Headers: safe to read anytime (just watch middleware order).
Body: read only if necessary, and use EnableBuffering() so you don’t break downstream processing.
Always consider performance + security.

--------------------------
--------------------------

## Routing (attribute routing, conventional routing)

> **Routing decides which endpoint should handle an incoming HTTP request.**

Suppose the client sends:

```http
GET /api/employees/10
```

ASP.NET Core needs to determine:

```text
/api/employees/10
        ↓
Which code should execute?
        ↓
EmployeeController.GetById(10)
```

**Simple example**

Suppose you have:

```csharp
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetEmployee(int id)
    {
        return Ok();
    }
}
```

A request:

```http
GET /api/employee/10
```

matches:

```csharp
[HttpGet("{id}")]
```

and ASP.NET Core executes:

```csharp
GetEmployee(10)
```

**Routing has two major concepts**

1. Conventional Routing
2. Attribute Routing

For Web APIs, **attribute routing is very common**.

### Attribute Routing

You specify the route directly using attributes.

```csharp
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        ...
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        ...
    }

    [HttpPost]
    public IActionResult Create(Employee employee)
    {
        ...
    }
}
```

Now you have:

```text
GET     /api/employees
GET     /api/employees/10
POST    /api/employees
```

**`[Route]`**

`[Route]` defines the route template.

```csharp
[Route("api/employees")]
```

**`HTTP method attributes`**
You can specify both the HTTP method and route.

```csharp
[HttpGet]
```

```csharp
[HttpGet("{id}")]
```

```csharp
[HttpPost]
```

```csharp
[HttpPut("{id}")]
```

```csharp
[HttpDelete("{id}")]
```

For example:

```csharp
[HttpGet("{id}")]
public IActionResult Get(int id)
```

means:

```text
GET /api/employees/10
```

while:

```csharp
[HttpDelete("{id}")]
public IActionResult Delete(int id)
```

means:

```text
DELETE /api/employees/10
```

Same URL pattern, different HTTP method.


**Route Parameters**

This:

```csharp
[HttpGet("{id}")]
```

defines a route parameter.

Request:

```http
GET /api/employees/25
```

gives:

```csharp
id = 25
```

Example:

```csharp
[HttpGet("{id}")]
public IActionResult GetEmployee(int id)
{
    Console.WriteLine(id);
    
    return Ok();
}
```

The routing system extracts `25` and passes it to your action.

**Route Constraints**

This is an important routing feature.

Suppose:

```csharp
[HttpGet("{id:int}")]
```

Now `id` must be an integer.

This works:

```http
GET /api/employees/10
```

But:

```http
GET /api/employees/abc
```

doesn't match this route.

You can use constraints such as:

```csharp
{id:int}
{id:guid}
{id:bool}
{id:min(1)}
{id:max(100)}
{id:length(5)}
```

Example:

```csharp
[HttpGet("{id:int:min(1)}")]
```

means:

> `id` must be an integer and must be at least 1.


**Query Parameters vs Route Parameters**

***Query parameter***

```http
GET /api/employees?department=IT
```

Controller:

```csharp
[HttpGet]
public IActionResult Get(string department)
{
    ...
}
```

Here:

```text
department=IT → Query parameter
```

Think:

```text
/api/employees/10
                ↑
          Route parameter


/api/employees?department=IT
                ↑
          Query parameter
```

**`[controller]` & `[action]`**

You can also use:

```csharp
[Route("api/[controller]/[action]")]
```

For:

```csharp
public IActionResult GetAll()
```

the route becomes approximately:

```text
/api/employee/getall
```

But in modern REST APIs, it's generally preferable to design resource-oriented routes rather than exposing action names unnecessarily.

For example:

```text
GET    /api/employees
GET    /api/employees/10
POST   /api/employees
PUT    /api/employees/10
DELETE /api/employees/10
```

------------------------
------------------------

## Filters

> A filter allows you to run code before or after specific stages of controller/action execution.

> Filters provide a way to execute custom logic at specific stages of the MVC request pipeline, such as authorization, resource processing, action execution, exception handling, and result execution. They can be applied globally, at the controller level, or at the action level. Unlike middleware, which operates at the broader HTTP pipeline level, filters are primarily focused on MVC/controller execution.

### Why do we need filters?

Suppose you have 50 controller actions:

```text
EmployeeController
OrderController
ProductController
CustomerController
...
```

And you want to perform something before every action:

```text
Log request
Check something
Validate something
Check authorization
Handle exceptions
Measure execution time
```

You don't want to write this inside every action:

```csharp
public IActionResult GetEmployees()
{
    Log();
    CheckSomething();
    
    // actual logic
}
```

Instead, create a filter once and apply it where needed.

### Basic flow

Without filters:

```text
Request
   ↓
Middleware
   ↓
Controller
   ↓
Action
   ↓
Response
```

With filters:

```text
Request
   ↓
Middleware
   ↓
Controller
   ↓
Authorization Filter
   ↓
Resource Filter
   ↓
Action Filter
   ↓
Action
   ↓
Result Filter
   ↓
Response
```

### Types of filters

1. Authorization Filter
2. Resource Filter
3. Action Filter
4. Exception Filter
5. Result Filter

A useful order to remember:

```text
Authorization
      ↓
Resource
      ↓
Action
      ↓
Controller Action
      ↓
Result
```

**`1. Authorization Filter`**

Runs very early.

> **Determine whether the request is authorized to execute the action.**

For example:

```csharp
[Authorize]
public IActionResult GetEmployees()
{
    ...
}
```

Authorization filters are part of the authorization stage.

Conceptually:

```text
Request
   ↓
Authorization
   ↓
Allowed?
 ┌─┴─┐
No  Yes
↓    ↓
401  Action
```

If the user isn't authorized, the action doesn't execute.

---

**`2. Action Filter`**

This is probably the filter you'll use most often when learning custom filters.

It allows code to run:

```text
Before action
      ↓
Controller Action
      ↓
After action
```

For example:

```csharp
public class LoggingActionFilter : IActionFilter
{
    public void OnActionExecuting(
        ActionExecutingContext context)
    {
        Console.WriteLine("Before action");
    }

    public void OnActionExecuted(
        ActionExecutedContext context)
    {
        Console.WriteLine("After action");
    }
}
```

Then apply it:

```csharp
[ServiceFilter(typeof(LoggingActionFilter))]
public IActionResult GetEmployees()
{
    return Ok();
}
```

Flow:

```text
Request
   ↓
Before Action Filter
   ↓
GetEmployees()
   ↓
After Action Filter
   ↓
Response
```

**`3. Async Action Filter`**

In real applications, you'll often use:

```csharp
IAsyncActionFilter
```

Example:

```csharp
public class LoggingFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        Console.WriteLine("Before");

        var result = await next();

        Console.WriteLine("After");
    }
}
```

Notice something familiar?

```csharp
await next();
```

This is conceptually similar to middleware:

```csharp
await _next(context);
```

Both allow execution to continue and then perform logic afterward.


**`4. Exception Filter`**

Exception filters handle exceptions thrown during MVC/controller execution.

For example:

```csharp
public class MyExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        Console.WriteLine(context.Exception.Message);

        context.Result = new ObjectResult("Something went wrong")
                        {
                            StatusCode = 500
                        };
    }
}
```

Conceptually:

```text
Request
   ↓
Controller
   ↓
Service
   ↓
Exception 💥
   ↓
Exception Filter
   ↓
500 Response
```

However, in modern ASP.NET Core applications, **global exception-handling middleware** is often preferred for truly global exception handling.


**`5. Result Filter`**

Result filters execute around the result execution. You might use result filters for things such as:

* Modifying response behavior
* Adding common response headers
* Logging result execution

Example:

```csharp
public class MyResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        Console.WriteLine("Before result");
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        Console.WriteLine("After result");
    }
}
```

---

### Where can you apply a filter?

***1. Action level***

```csharp
[MyFilter]
public IActionResult Get()
{
}
```

Only this action.

***2. Controller level***

```csharp
[MyFilter]
public class EmployeeController : ControllerBase
{
}
```

All actions in this controller.

***3. Global level***

Register globally:

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<MyFilter>();
});
```
---

### Practical example: Execution-time filter

Suppose you want to measure how long every controller action takes.

```csharp
public class ExecutionTimeFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync( ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        await next();
        stopwatch.Stop();
        Console.WriteLine(
            $"{context.ActionDescriptor.DisplayName} " +
            $"took {stopwatch.ElapsedMilliseconds} ms");
    }
}
```

Now:

```text
Request
   ↓
ExecutionTimeFilter
   ↓
Controller Action
   ↓
Service
   ↓
Database
   ↓
ExecutionTimeFilter
   ↓
Response
```

This is a very practical use of filters.

---
### When should I use what?

A good practical rule:

| Requirement                     | Usually use                          |
| ------------------------------- | ------------------------------------ |
| Global exception handling       | Middleware                           |
| Log every HTTP request          | Middleware                           |
| Correlation ID                  | Middleware                           |
| Security headers                | Middleware                           |
| Authentication                  | Authentication middleware/system     |
| Authorization                   | Authorization system / `[Authorize]` |
| Logic before controller action  | Action Filter                        |
| Logic after controller action   | Action Filter                        |
| Controller-specific logging     | Action Filter                        |
| MVC-specific caching            | Resource Filter                      |
| Modify result execution         | Result Filter                        |
| MVC-specific exception handling | Exception Filter                     |


-------------------
-------------------

## Dependency Injection (DI) lifetimes

> Dependency Injection is a design pattern where an object's dependencies (the other objects/services it needs to work) are provided from outside, rather than the object creating them itself. It's a specific form of Inversion of Control (IoC).

The DI container manages:

* Creating objects
* Providing dependencies
* Managing their lifetime
* Disposing them when appropriate

### Service Lifetimes in .NET DI Container

**``1. Transient``**

> **Create a new instance every time the service is requested from DI.**

```csharp
builder.Services.AddTransient<IEmployeeService, EmployeeService>();
```

Example:

```text id="9uvg5f"
Request
   │
   ├── Service A → EmployeeService #1
   │
   ├── Service B → EmployeeService #2
   │
   └── Service C → EmployeeService #3
```

Every resolution gets a new instance.

***Use Transient when:***

The service is:

* Lightweight
* Stateless
* Doesn't need to be shared
* Cheap to create

Examples:

```text id="6xj3is"
Formatting service
Validation service
Small calculation service
Mapper/helper-like services
```

**`2. Scoped (Default)`**

> **One instance per DI scope. One HTTP request = one scope**

```csharp
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
```

```text id="7p7b4x"
HTTP Request #1
       │
       ├── EmployeeService → Instance A
       ├── OrderService    → Instance A
       └── Another consumer → Instance A

HTTP Request #2
       │
       ├── EmployeeService → Instance B
       ├── OrderService    → Instance B
       └── Another consumer → Instance B
```

So:

```text id="h9l2wz"
Request 1 → A
Request 2 → B
Request 3 → C
```

***Why is Scoped so important?***

Because many ASP.NET Core services are naturally **request-oriented**.

The most famous example is:

```csharp
DbContext
```

Entity Framework Core normally registers `DbContext` as:

```csharp
AddDbContext<MyDbContext>()
```

which uses a scoped lifetime by default.

Why?

You generally want:

```text id="8yn1yr"
HTTP Request
     │
     ├── Controller
     ├── EmployeeService
     ├── Repository
     └── DbContext
             │
             ▼
          Database
```

All those components can participate in the same request-level unit of work.

Then the request ends and the scoped `DbContext` is disposed.

***Common Scoped services***

```text id="iq0zqi"
DbContext
Repository
Business services
Unit of Work
Request-specific services
```

**`3. Singleton`**

> **Create one instance and reuse it for the lifetime of the application/service provider.**

```csharp
builder.Services.AddSingleton<IEmployeeService, EmployeeService>();
```

Conceptually:

```text id="w3m6d9"
Application starts
       │
       ▼
EmployeeService Instance A
       │
       ├──────── Request 1
       ├──────── Request 2
       ├──────── Request 3
       ├──────── Request 100
       └──────── Request 10000
```

Everyone gets the same instance.

***When should you use Singleton?***

Good candidates are usually:

* Stateless services
* Thread-safe services
* Expensive-to-create objects
* Shared application-wide data/configuration
* Caches

For example:

```csharp
builder.Services.AddSingleton<ICacheService, CacheService>();
```

But there is a BIG warning:

> **A singleton must be thread-safe.**

Because many requests can access it simultaneously.

```text id="w6v4tj"
Request 1 ──┐
Request 2 ──┤
Request 3 ──┼──> SAME Singleton
Request 4 ──┤
Request 5 ──┘
```

---

### Lifetime mismatch — VERY important

This is one of the most common interview questions.

Suppose:

```text id="xgqk4j"
Singleton
   ↓
Scoped Service
```

This is problematic.

For example:

```csharp
builder.Services.AddSingleton<MySingleton>();
builder.Services.AddScoped<MyScoped>();
```

And:

```csharp
public class MySingleton
{
    public MySingleton(MyScoped scoped)
    {
    }
}
```

ASP.NET Core will generally throw an error when resolving this dependency in the normal DI container because:

> A singleton cannot safely depend on a scoped service.

Why?

The singleton lives for the whole application.

But the scoped service belongs to one request.

Imagine:

```text id="0ydr6d"
Application
│
└── Singleton
      │
      └── Scoped Service from Request #1 ❌
```

Request #2 comes:

```text id="y9w8dw"
Request #2
    ↓
New Scope
    ↓
Should have a different Scoped Service
```

But the singleton is still holding the old one. That's a lifetime mismatch.

### Dependency direction rule

A singleton can safely depend on another singleton.

A scoped service can depend on:

```text id="42u4vz"
Scoped
   ↓
Transient
   ↓
Singleton
```

A transient service can depend on:

```text id="0ecm3b"
Transient
   ↓
Scoped
   ↓
Singleton
```

But be careful with how those dependencies are used and resolved.

---

***Why shouldn't DbContext be Singleton?***

Bad:

```csharp
builder.Services.AddSingleton<MyDbContext>();
```

`DbContext` is **not designed to be shared concurrently across requests**.

You could end up with:

```text id="d5k0gq"
Request 1 ──┐
Request 2 ──┤
Request 3 ──┼──> SAME DbContext ❌
Request 4 ──┘
```

Instead, use the normal scoped registration:

```csharp
builder.Services.AddDbContext<MyDbContext>();
```

Conceptually:

```text id="l1i9ib"
Request 1 → DbContext A

Request 2 → DbContext B

Request 3 → DbContext C
```

----------------
----------------

## Minimal APIs

> "Minimal APIs are a lightweight way of building HTTP APIs in ASP.NET Core without requiring controller classes and action methods. Endpoints are defined directly using methods such as `MapGet`, `MapPost`, `MapPut`, and `MapDelete`. They still support dependency injection, routing, authentication, authorization, middleware, model binding, and endpoint filters."

If asked:

### "Controllers vs Minimal APIs?"

> "Controllers provide a more structured MVC programming model with controller classes, actions, model binding, filters, and many MVC-specific features, making them suitable for larger and more complex APIs. Minimal APIs reduce ceremony and are particularly convenient for small APIs, microservices, and simple endpoints. Both use the same underlying ASP.NET Core infrastructure such as DI, middleware, routing, and endpoint routing."

### The mental model

```text
                ASP.NET CORE
                     │
        ┌────────────┴────────────┐
        │                         │
   Controllers               Minimal APIs
        │                         │
 Controller                MapGet / MapPost
 Action methods             Endpoint handlers
        │                         │
        └────────────┬────────────┘
                     ↓
              Endpoint Routing
                     ↓
                Middleware
                     ↓
               HTTP Response
```

**Simple rule:**

> **Controllers = more structure/features**

> **Minimal APIs = less ceremony/simplicity**

With Minimal API:

```csharp
app.MapGet("/api/employees", () =>
{
    return Results.Ok(new[] { "Swapnil", "Rahul" });
});

app.MapGet("/api/employees/{id}", (int id) =>
{
    return Results.Ok(id);
});
```

That's the basic idea.

> **Minimal APIs let you define endpoints directly, without requiring controllers.**

### HTTP methods

Minimal APIs have methods for the common HTTP verbs:

```csharp
app.MapGet(...);

app.MapPost(...);

app.MapPut(...);

app.MapPatch(...);

app.MapDelete(...);
```

Example:

```csharp
app.MapGet("/api/employees", GetEmployees);

app.MapPost("/api/employees", CreateEmployee);

app.MapPut("/api/employees/{id}", UpdateEmployee);

app.MapDelete("/api/employees/{id}", DeleteEmployee);
```

So you can build a complete REST API without controllers.


***Does Minimal API mean "less performance"?***

Not necessarily.

Minimal APIs were designed with **low ceremony and performance in mind**, and they avoid some MVC/controller infrastructure.

But don't choose them purely because:

> "Minimal API is faster."

In a real application, database calls, network latency, serialization, business logic, etc. can dominate the overall request time.

Architecture and maintainability are usually more important than tiny framework-level differences.

***When would I choose Minimal APIs?***

```text
Small API
Microservice
Internal service
Simple CRUD API
Health endpoints
Lightweight service
Prototype
```
---------------------
---------------------

## API Versioning
> It’s the practice of supporting multiple versions of your API endpoints so that:
Existing clients keep working without breaking changes.
You can evolve and improve your API over time.

### Why do we need API Versioning?

Imagine you have:

```http
GET /api/employees/10
```

Today it returns:

```json
{
  "id": 10,
  "name": "Swapnil",
  "department": "IT"
}
```

Six months later, you want to completely change the response:

```json
{
  "employeeId": 10,
  "fullName": "Swapnil",
  "departmentName": "IT",
  "location": "Mumbai"
}
```

But you have existing clients:

```text
Mobile App
Frontend
Another Microservice
Third-party client
```

They expect the **old response format**.

If you simply change the API, you might break them.

So instead:

```text
/api/v1/employees/10 → Old behavior
/api/v2/employees/10 → New behavior
```

Now old clients can continue using V1 while new clients move to V2.

### Common API versioning strategies

There are several ways to specify the version.

The most common are:

1. URL path
2. Query string
3. HTTP header
4. Media type / Accept header

**`1. Version in URL path`**


```http
GET /api/v1/employees
```

and:

```http
GET /api/v2/employees
```

Controller:

```csharp
[ApiController]
[Route("api/v1/employees")]
public class EmployeesV1Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        ...
    }
}
```

V2:

```csharp
[ApiController]
[Route("api/v2/employees")]
public class EmployeesV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        ...
    }
}
```

**`2. Query string versioning`**

Another approach:

```http
GET /api/employees?api-version=1.0
```

or:

```http
GET /api/employees?api-version=2.0
```

The URL resource remains:

```text
/api/employees
```


**`3. Header versioning`**

You can specify the version using a custom header:

```http
GET /api/employees
api-version: 2.0
```

The URL stays:

```text
/api/employees
```


**`4. Media type versioning`**

This is a more advanced approach.

For example:

```http
Accept: application/vnd.company.employee-v2+json
```

The version is represented by the media type.

Conceptually:

```text
Accept
  ↓
application/vnd.company.employee-v2+json
  ↓
V2 representation
```
---

### Which approach is most common?

In many real-world APIs you'll encounter:

```text
/api/v1/...
/api/v2/...
```

because it's:

* Easy to understand
* Easy to test
* Easy to document
* Easy for consumers to see
* Straightforward with routing

But there's no universal "correct" strategy.

The important thing is to **choose one strategy and apply it consistently**.

``` csharp
services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
```
---------------------
---------------------

## Content Negotiation

> "Content negotiation is the process by which ASP.NET Core determines the representation format of an HTTP response based on the client's `Accept` header and the formatters configured by the application. For example, a client can request `application/json` or `application/xml`, and ASP.NET Core selects a suitable output formatter to serialize the response. `Content-Type`, on the other hand, describes the format of the request body being sent by the client."

### Remember these 4 things:

```text
Accept
   ↓
What response format I want

Content-Type
   ↓
What format I am sending

Input Formatter
   ↓
Request → .NET Object

Output Formatter
   ↓
.NET Object → Response
```

**`Accept`**

Tells the server:

> **"What response format can I accept?"**

Example:

```http
Accept: application/json
```

**`Content-Type`**

Tells the server:

> **"What format is the data I'm sending?"**

Example:

```http
Content-Type: application/json
```

These are often confused.

> 406 Not Acceptable is error message if content not match

-----------------------
-----------------------

## Rate Limiting

> **Rate limiting controls how many requests a client is allowed to make within a certain period of time.**

For example:

```text
Maximum: 100 requests / minute
```

If a client sends:

```text
Request 1
Request 2
Request 3
...
Request 100   ✅
Request 101   ❌
```

The API can reject request #101, typically with:

```http
429 Too Many Requests
```

### ASP.NET Core has built-in rate limiting

Modern ASP.NET Core provides rate-limiting middleware through:

```csharp
Microsoft.AspNetCore.RateLimiting
```

You configure it in `Program.cs`.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                }));
});
```

Then add middleware:

```csharp
app.UseRateLimiter();
```

Conceptually:

```text
Request
   ↓
Rate Limiter
   ↓
Allowed?
 ┌─┴─┐
Yes  No
 ↓    ↓
API  429
```

### Concurrency Limiter

This is slightly different.

Instead of:

> "How many requests per minute?"

it asks:

> "How many requests can execute simultaneously?"

For example:

```text
Maximum concurrent requests = 10
```

Then:

```text
Request 1 ─┐
Request 2  │
Request 3  │
...        ├── 10 running
Request 10 ┘

Request 11
    ↓
Rejected / queued depending on configuration
```

This is useful for protecting expensive resources.

For example:

```text
Heavy report generation
Database-intensive operation
CPU-intensive processing
```

### Fixed Window example in ASP.NET Core

You can configure a named policy:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});
```

Then:

```csharp
app.UseRateLimiter();
```

Apply it to an endpoint:

```csharp
app.MapGet("/api/employees", () =>
{
    return Results.Ok();
})
.RequireRateLimiting("fixed");
```

Now:

```text
/api/employees
        ↓
fixed policy
        ↓
10 requests/minute
```

------------

### latency and why is it required?

> Latency is the delay in response, and while it’s unavoidable to some degree (because physics), our goal is to minimize it so that systems feel fast and responsive.

- It’s usually measured in milliseconds (ms)

Formula (simplified):
Latency = Response start time − Request send time

Example:
You click “Get Products” in a web app.
The browser sends a request to your API at 10:00:00.000.
The first byte of the response arrives at 10:00:00.120.
Latency = 120 ms.

**Why it matters**

**We care about latency because:**
- User experience → High latency makes apps feel slow and unresponsive.
- Performance → Low latency means faster feedback loops.
- Scalability → If each request takes long, fewer requests can be served at once.
- Competitive advantage → Faster APIs are often a selling point.

**Where latency comes from**
- Network delays → Distance between client and server (ping time).
- Server processing time → How long it takes your code to handle a request.
- Database calls → Slow queries increase latency.
- External API calls → Waiting on third-party services.
- Congestion → Too many requests causing queuing.

**Types of Latency:**
- Network latency – Time to send data over the internet.
- Application latency – Time your API takes to process the request.
- End-to-end latency – Total delay from user action to final response.

**Why we measure latency**
- We don’t “require” latency, but we require latency monitoring because:
- It’s a key performance indicator (KPI) for APIs.
- Helps detect bottlenecks early.
- Useful in Service Level Agreements (SLAs) — e.g., “99% of requests will have latency < 200 ms.”

Example in .NET
You can log latency in middleware:
```csharp
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    await next();
    stopwatch.Stop();
    Console.WriteLine($"Latency: {stopwatch.ElapsedMilliseconds} ms");
});
```

---

### What is Throughput?
> Throughput is the number of requests your system can handle in a given time.

- It’s usually measured in:
    Requests per second (RPS)
    Transactions per second (TPS)
    MB/s or GB/s for data transfer.

Example analogy:
Latency: How long it takes a single coffee to be made.
Throughput: How many coffees can be made in one minute.

Example
Your API takes 200 ms latency to respond.
Your server handles 50 requests per second.
If latency increases to 1 second, throughput will likely drop (because fewer requests can be handled in parallel).


**Why Throughput Matters**
- Capacity planning → Know how much traffic you can serve.
- Scalability testing → Find limits before production.
- SLA compliance → e.g., “We guarantee 1000 RPS sustained throughput.”
- Cost optimization → Avoid over/under provisioning.
 
**Improving Throughput**
- Optimize database queries.
- Use caching (in-memory, Redis).
- Reduce unnecessary I/O.
- Use async programming in .NET (async/await).
- Scale horizontally (add more servers).
- Use load balancers.

**In .NET — Measuring Throughput**
You can measure throughput with performance tools like:
- Apache JMeter
- k6
- wrk
- Azure Application Insights

Or with custom middleware:
```csharp
int requestCount = 0;
var timer = new Timer(_ =>
{
    Console.WriteLine($"Throughput: {requestCount} requests/sec");
    requestCount = 0;
}, null, 0, 1000);

app.Use(async (context, next) =>
{
    Interlocked.Increment(ref requestCount);
    await next();
});
```
----------------------------------
----------------------------------

## Caching
> Caching is a technique for storing frequently accessed or expensive-to-generate data or responses so that subsequent requests can be served faster without repeating the underlying work. ASP.NET Core supports mechanisms such as in-memory caching, distributed caching, response caching, and output caching.

```text
                    CACHING
                       │
       ┌───────────────┼────────────────┐
       │               │                │
   Data Cache      Output Cache    Response Cache
       │               │                │
 IMemoryCache      Server-side       HTTP rules
 IDistCache        endpoint output   Cache-Control
       │
       ▼
  Reduce DB calls
```
**This can reduce:**

- Database load
- Response time
- CPU usage
- Network traffic

Cache Hit = data found in cache.

Cache Miss = data not found, so you need to retrieve it.

### Types of caching in ASP.NET Core

1. In-memory caching
2. Distributed caching
3. Response caching
4. Output caching
5. Browser/client-side caching

**`1. In-Memory Cache`**

ASP.NET Core provides: IMemoryCache
```csharp
You register it:
builder.Services.AddMemoryCache();

Then inject it:

private readonly IMemoryCache _cache;

public EmployeeService(IMemoryCache cache)
{
    _cache = cache;
}
```


Example:
``` csharp
public async Task<List<Employee>> GetEmployees()
{
    if (_cache.TryGetValue("employees", out List<Employee>? employees))
    {
        return employees!;
    }

    employees = await _repository.GetEmployees();

    _cache.Set(
        "employees",
        employees,
        TimeSpan.FromMinutes(5));

    return employees;
}
```

Server B doesn't know about Server A's cache.So each server has its own cache.

This is where Distributed Cache becomes useful.

**`2. Distributed Cache`**
> A distributed cache is shared between application instances.

Conceptually:

              Load Balancer
               /         \
              ▼           ▼
          Server A     Server B
              \           /
               \         /
                ▼       ▼
              Distributed
                 Cache

***Common technologies include:***

- Redis
- SQL Server
- Other distributed cache providers

In .NET you'll encounter: IDistributedCache

> Single server → Memory Cache can be enough

> Multiple application instances → Distributed Cache is often preferable

**`3. Response Caching`**
Instead of caching the data inside your service:

```text
Controller
    ↓
Database result
    ↓
Cache object
```
> Response caching deals with caching HTTP responses according to HTTP caching rules.

For example:
``` csharp
GET /api/products
```
could have caching headers such as:

```text
Cache-Control: public,max-age=60
```

This tells compatible clients/proxies that the response can be reused for a period of time.

You may see:
```csharp
builder.Services.AddResponseCaching();
```
and:
```csharp
app.UseResponseCaching();
```
Then an action can specify caching behavior with attributes such as:
```csharp
[ResponseCache(Duration = 60)]
[HttpGet]
public IActionResult GetProducts()
{
    ...
}
```
The important idea is:

```csharp
HTTP Response
     ↓
Caching rules
     ↓
Can response be reused?
```

**`4. Output Caching`**

This is particularly important in modern ASP.NET Core.

> ASP.NET Core provides Output Caching, which lets you cache generated endpoint responses on the server.

Register:
```csharp
builder.Services.AddOutputCache();
```
```csharp
Middleware:

app.UseOutputCache();

Then:

app.MapGet("/api/products", () =>
{
    return products;
})
.CacheOutput();
```

> Response Caching Primarily follows HTTP caching semantics. Output Caching ASP.NET Core's server-side mechanism for caching generated responses.

---------------------------
---------------------------

## Health Checks

> Health Checks provide endpoints that allow us and infrastructure such as load balancers or Kubernetes to determine whether an application and its critical dependencies are functioning correctly. ASP.NET Core provides built-in health-check middleware and supports custom checks for dependencies such as databases, Redis, and external services

> Liveness determines whether the application is alive, while readiness determines whether it is ready to receive traffic
ASP.NET Core built-in Health Checks

Register health checks:
```csharp
builder.Services.AddHealthChecks();
```
Then expose the endpoint:
```csharp
app.MapHealthChecks("/health");
```
Now:
```csharp
GET /health
```
can be used by monitoring infrastructure.

The basic flow is:
```text
Monitoring System
       │
       │ GET /health
       ▼
ASP.NET Core
       │
       ▼
Health Check
       │
       ▼
Healthy / Unhealthy
```
------------------------
------------------------