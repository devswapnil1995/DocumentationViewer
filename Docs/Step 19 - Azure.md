## Azure Functions 

**1. What is Azure Functions?**

> Azure Functions is Azure's serverless, event-driven compute service. It allows you to run a piece of code in response to an event without managing the underlying servers yourself.

Think:

```text
Traditional .NET API

Client
  ↓
ASP.NET Core API
  ↓
Server/App Service
  ↓
Your code
```

Azure Function:

```text
Event
  ↓
Azure Function
  ↓
Your code
```

You focus mainly on the code; Azure manages much of the hosting infrastructure.

---

**2. Why do we need Azure Functions?**

Imagine you need to perform a task:

> "Whenever an order is created, send a confirmation email."

You could create a complete ASP.NET Core service:

```text
Order API
    ↓
Background Worker
    ↓
Email
```

But perhaps the task is small and event-driven.

Azure Functions lets you do:

```text
OrderCreated Event
       ↓
Azure Function
       ↓
Send Email
```

No continuously running custom worker needs to be managed for that task.

**Common use cases**

* Background processing
* Scheduled jobs
* Queue processing
* Service Bus consumers
* File processing
* Webhooks
* Lightweight APIs
* Event-driven integrations
* Scheduled cleanup
* Notifications
* Data processing

---

**3. Azure Function vs ASP.NET Core API**

This is an important interview comparison.

| ASP.NET Core API                        | Azure Functions                              |
| --------------------------------------- | -------------------------------------------- |
| Usually continuously hosted application | Event-driven functions                       |
| You manage API application structure    | Azure provides Functions runtime             |
| Good for large APIs                     | Good for event-driven/specific workloads     |
| Controllers/endpoints                   | Functions/triggers                           |
| You manage more hosting concerns        | Azure handles much of hosting infrastructure |
| Long-running API application            | Short/event-triggered workloads are common   |

But don't say:

> "Azure Functions replaces ASP.NET Core."

It doesn't.

You choose based on the workload.

---

**4. The Most Important Concept: Trigger**

A **trigger determines what causes the function to execute**.

Microsoft states that every function has exactly **one trigger**. 

Example:

```text
HTTP Request
      ↓
HTTP Trigger
      ↓
Azure Function
```

Or:

```text
Service Bus Message
      ↓
Service Bus Trigger
      ↓
Azure Function
```

Or:

```text
Timer
      ↓
Timer Trigger
      ↓
Azure Function
```

---

# 5. Important Azure Function Triggers

You should know these for interviews:

| Trigger       | Runs when                        |
| ------------- | -------------------------------- |
| HTTP          | HTTP request received            |
| Timer         | Scheduled time                   |
| Service Bus   | Message arrives                  |
| Queue Storage | Queue message arrives            |
| Blob Storage  | Blob-related event/change        |
| Event Grid    | Event received                   |
| Event Hubs    | Event arrives                    |
| Cosmos DB     | Relevant database changes/events |

For your microservices preparation, pay special attention to:

`HTTP`

```text
Client
 ↓
HTTP Function
```

`Service Bus`

```text
Order Service
 ↓
Azure Service Bus
 ↓
Payment Function
```

`Timer`

```text
Every day 2 AM
       ↓
Azure Function
       ↓
Cleanup
```

Azure Functions supports these event-driven triggers and corresponding bindings. 

---

**6. Trigger vs Binding**

This is one of the most common interview questions.

`Trigger`

> **What starts the function?**

`Binding`

> **How does the function connect to another resource?**

Example:

```text
Service Bus Message
       ↓
    TRIGGER
       ↓
Azure Function
       ↓
    BINDING
       ↓
Storage / Database / Queue
```

Microsoft describes bindings as declarative connections between a function and other resources; input bindings provide data and output bindings write data. 

**Easy way to remember**

> **Trigger = Why did my function start?**

> **Binding = What resource is my function connected to?**

---

**7. Types of Bindings**

`Input Binding`

Reads data.

```text
Function
   ↓
Database / Storage
```

`Output Binding`

Writes data.

```text
Function
   ↓
Queue / Storage
```

You can also have multiple bindings.

Example:

```text
Service Bus Trigger
       ↓
Function
   ├── Database Input
   └── Queue Output
```

---

**8. How to Create Azure Function in .NET**

For modern .NET development, you should learn the **isolated worker model**.

This is particularly important now because Microsoft says support for the **in-process .NET model ends November 10, 2026** and recommends the isolated worker model. 

The isolated model gives you:

* Normal .NET dependency injection
* Middleware
* Independent .NET versioning
* Better separation from the Functions host process



---

**9. Create a .NET Azure Function**

You can create one through Visual Studio or Azure Functions Core Tools. Microsoft currently documents C# isolated-worker Functions as the standard approach for supported modern .NET versions. 

For example, conceptually:

```bash
func init MyFunctionApp --worker-runtime dotnet-isolated
```

Then create an HTTP-triggered function.

---

**10. Basic HTTP Function**

Modern isolated-worker C# example:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class HelloFunction
{
    [Function("Hello")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")]
        HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);

        response.WriteString("Hello from Azure Function!");

        return response;
    }
}
```

Flow:

```text
GET /api/Hello
       ↓
HttpTrigger
       ↓
Run()
       ↓
Response
```

The isolated worker model uses attributes such as `[Function]` and trigger attributes to identify function methods and their triggers. 

---

**11. HTTP Trigger Authorization**

You'll see:

```csharp
AuthorizationLevel.Function
```

Common levels include:

```text
Anonymous
Function
Admin
```

For example:

```csharp
[HttpTrigger(
    AuthorizationLevel.Function,
    "get")]
```

means the caller needs the function-level authorization mechanism.

For production APIs, however, don't treat function keys as your complete application security model. You may use API Management, Entra ID/OAuth, application authorization, etc., depending on the architecture.

---

**12. Dependency Injection**

This is particularly important for **you as a .NET developer**.

You can use normal .NET dependency injection with isolated worker Functions. 

`Program.cs`

Conceptually:

```csharp
var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Build().Run();
```

Then:

```csharp
public class OrderFunction
{
    private readonly IOrderService _orderService;

    public OrderFunction(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Function("CreateOrder")]
    public async Task<HttpResponseData> Run(...)
    {
        await _orderService.CreateAsync(...);

        ...
    }
}
```

So your familiar .NET architecture still applies:

```text
Function
   ↓
Application Service
   ↓
Repository
   ↓
Database
```

---

**13. Azure Function + Service Bus**

This is probably the **most important scenario for your microservices interviews**.

Suppose:

```text
Order Service
      ↓
OrderCreated
      ↓
Azure Service Bus
      ↓
Payment Function
```

The Function automatically gets invoked when the message arrives.

Example:

```csharp
[Function("ProcessPayment")]
public async Task Run(
    [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")]
    string message)
{
    // Process order
}
```

Conceptually:

```text
Message arrives
      ↓
Service Bus Trigger
      ↓
Azure Function
      ↓
Process message
```

Azure Functions provides a Service Bus trigger specifically for this event-driven pattern.

---

**14. Why use Function + Service Bus?**

Imagine 100,000 orders.

Instead of:

```text
Order API
 ↓
Process payment synchronously
 ↓
Send email
 ↓
Generate invoice
```

you can do:

```text
Order API
    ↓
Save Order
    ↓
Publish OrderCreated
    ↓
Service Bus
    │
    ├── Payment Function
    ├── Notification Function
    └── Invoice Function
```

Now processing can happen asynchronously.

This fits directly with the microservices concepts you've already learned:

```text
Message Broker
       +
Async Communication
       +
Event Driven Architecture
       +
Idempotency
       +
Retry
       +
DLQ
```

---

**15. Azure Function + Timer Trigger**

Suppose you need:

> Run every night at 2 AM and clean temporary records.

You don't need an HTTP endpoint.

Use Timer Trigger.

Conceptually:

```csharp
[Function("Cleanup")]
public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timer)
{
    await cleanupService.CleanupAsync();
}
```

Flow:

```text
2:00 AM
   ↓
Timer Trigger
   ↓
Cleanup Function
   ↓
Database
```

Good use cases:

* Scheduled reports
* Cleanup
* Data synchronization
* Periodic jobs
* Batch processing

---

**16. Azure Function + Blob Storage**

Another common scenario:

```text
File uploaded
      ↓
Blob Trigger
      ↓
Azure Function
      ↓
Process file
```

Example:

```text
Customer uploads CSV
       ↓
Azure Blob Storage
       ↓
Function triggered
       ↓
Read CSV
       ↓
Validate
       ↓
Store data
```

Very common in real enterprise systems.

---

**17. Azure Function vs BackgroundService**

This is useful for a .NET interview.

`ASP.NET Core BackgroundService`

Runs inside your application:

```text
ASP.NET Core
 ├── Controllers
 └── BackgroundService
```

You are responsible for hosting the application.

`Azure Function`

```text
Azure
 ↓
Function Runtime
 ↓
Your Function
```

Azure manages the hosting/execution environment according to the chosen hosting plan.

**When I would choose Function**

> Event-driven workload, scheduled task, queue processing, webhook, lightweight integration.

**When BackgroundService may be better**

> Work is tightly coupled to a continuously running ASP.NET Core application or needs long-running process behavior that doesn't fit Functions' execution model.

---

**18. Azure Functions Hosting Plans**

Azure Functions has several hosting options, including:

* Flex Consumption
* Consumption
* Premium
* Dedicated/App Service
* Container Apps

The exact capabilities and constraints vary by plan.

For modern serverless workloads, **Flex Consumption** is important to know. Microsoft currently notes that .NET 10 on Linux isn't supported on the Linux Consumption plan and should use Flex Consumption instead. 

**Interview-level understanding**

```text
Consumption
→ Serverless / scale based on demand

Premium
→ More control, warm instances, advanced requirements

Dedicated
→ App Service infrastructure

Flex Consumption
→ Modern flexible serverless hosting option
```

Don't memorize pricing unless the interviewer specifically asks.

---

**19. Cold Start**

This is an important serverless concept.

If your Function hasn't been running and Azure needs to start an instance:

```text
Request
  ↓
No active instance
  ↓
Start Function
  ↓
Execute
```

That startup delay is called a **cold start**.

This can matter for latency-sensitive applications.

Premium or other hosting choices can help address cold-start requirements depending on the workload.

---

**20. Function vs Microservice**

Don't confuse them.

`Microservice`

Architectural boundary:

```text
Order Service
Payment Service
Inventory Service
```

`Function`

Execution unit:

```text
ProcessPayment()
SendEmail()
GenerateInvoice()
```

You can have:

```text
Payment Microservice
       ↓
Azure Functions
```

But a Function isn't automatically a microservice.

**Interview answer**

> "A microservice is an architectural/business boundary, while an Azure Function is a serverless execution model. A function can implement part of a microservice or an event-driven workload."

---

**21. Durable Functions**

This is the next Azure Functions concept I would learn after basic Functions.

Normal Function:

```text
Event
 ↓
Function
 ↓
Done
```

Durable Function:

```text
Start Workflow
      ↓
Orchestrator
      ↓
Activity 1
      ↓
Activity 2
      ↓
Activity 3
      ↓
Complete
```

Durable Functions allow **stateful workflows** in a serverless environment. The runtime manages state, checkpoints, retries and recovery. 

**Example**

Order fulfillment:

```text
Create Order
     ↓
Process Payment
     ↓
Reserve Inventory
     ↓
Generate Invoice
     ↓
Send Notification
```

This could be represented as a durable workflow.

---

**22. Durable Functions vs Saga**

Very important given your microservices preparation.

They're related but not identical.

`Saga`

Architectural pattern for managing a distributed business transaction.

```text
Order
 ↓
Payment
 ↓
Inventory
```

with compensation:

```text
Inventory failed
 ↓
Refund Payment
 ↓
Cancel Order
```

`Durable Function`

Azure-specific serverless workflow technology that can **orchestrate** long-running/stateful workflows.

So:

> **Saga = pattern**

> **Durable Functions = technology that can implement/orchestrate certain workflows**

---

**23. Configuration**

Don't hardcode:

```csharp
var connectionString = "...";
```

Use configuration:

```text
local.settings.json
        ↓
Local development
```

and:

```text
Azure Function App Settings
        ↓
Production
```

For secrets, use proper secret management such as **Azure Key Vault** rather than committing secrets to source control.

---

**24. How does Azure Function fit into your .NET Architecture?**

Suppose you have:

```text
                 Angular
                    ↓
              API Gateway
                    ↓
              Order Service
                    ↓
             PostgreSQL
                    │
                    ↓
             Azure Service Bus
               /     |      \
              ↓      ↓       ↓
         Payment   Email   Invoice
         Function  Function Function
```

This is a very realistic Azure + .NET architecture.

Your normal ASP.NET Core services handle:

```text
API
Business Operations
CRUD
Transactions
```

Functions handle:

```text
Events
Background Jobs
Scheduled Work
Queue Processing
Integrations
```

---

**25. How do you deploy an Azure Function?**

Typical pipeline:

```text
Developer
   ↓
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
Azure Function App
```

Or:

```text
Docker Image
    ↓
Container Registry
    ↓
Azure hosting
```

Visual Studio also supports publishing C# Functions directly to Azure. 

---
---


**1. Existing Application**

Let's assume your current modular monolith looks approximately like this:

```text
MyCompany.Application
│
├── NameAddress
│   ├── Commands
│   ├── Queries
│   └── Services
│
├── Customers
├── Documents
└── ...
    
MyCompany.Domain
│
├── NameAddress
├── Customer
└── ...

MyCompany.Infrastructure
│
├── Persistence
│   └── AppDbContext
├── Repositories
└── ExternalServices

MyCompany.Web
│
├── Controllers
├── Middleware
└── Program.cs
```

Your current flow:

```text
User
 ↓
Angular/UI
 ↓
ASP.NET Core
 ↓
Name & Address Module
 ↓
AppDbContext
 ↓
Application DB
```

Now we're adding:

```text
                    EXISTING MONOLITH
                         │
                         │
                    Application DB
                         │
                         │
                  ┌──────┴───────┐
                  │              │
              Web App       Azure Function
                                 │
                              Timer
                                 │
                           Content Manager
```

---

**2. Where should the Function project live?**

Create a **separate project** in the same solution.

For example:

```text
MyCompany.sln
│
├── MyCompany.Web
│
├── MyCompany.Application
│
├── MyCompany.Domain
│
├── MyCompany.Infrastructure
│
└── MyCompany.Functions
```

This is a very good approach for your existing modular monolith.

**Why separate project?**

Because the Function has a different hosting model.

Your Web project:

```text
ASP.NET Core
     ↓
HTTP requests
```

Function:

```text
Azure Functions Runtime
     ↓
Timer Trigger
```

But they can share:

```text
Domain
Application
Infrastructure
```

where appropriate.

---

**3. What should NOT happen?**

Don't do this:

```text
MyCompany.Web
   │
   ├── Controllers
   ├── Modules
   ├── Azure Function
   └── Everything
```

And don't copy your entire business logic into the Function.

Instead:

```text
                 ┌─────────────────┐
                 │   Application   │
                 │     Layer       │
                 └────────┬────────┘
                          │
             ┌────────────┴────────────┐
             ↓                         ↓
        ASP.NET Core             Azure Function
             │                         │
          HTTP                      Timer
```

Both can call the same application services.

---

**4. Create the Azure Function Project**

In Visual Studio:

```text
File
 ↓
Add
 ↓
New Project
 ↓
Azure Functions
```

Choose:

```text
Language: C#
Worker: .NET isolated
Trigger: Timer Trigger
```

For new C# Functions, learn the **isolated worker model**. Microsoft currently recommends it for modern .NET, and the older in-process model reaches end of support on November 10, 2026. 

Your project becomes:

```text
MyCompany.Functions
│
├── Functions
│   └── NameAddressSyncFunction.cs
│
├── Program.cs
├── host.json
├── local.settings.json
└── MyCompany.Functions.csproj
```

---

**5. Add References to Existing Projects**

Your Function needs access to your existing business logic.

For example:

```text
MyCompany.Functions
       │
       ├────────→ MyCompany.Application
       │
       ├────────→ MyCompany.Domain
       │
       └────────→ MyCompany.Infrastructure
```

But don't blindly reference everything.

A cleaner architecture could be:

```text
Functions
   ↓
Application
   ↓
Domain

Functions
   ↓
Infrastructure
```

depending on how your existing solution is structured.

---

**6. Create the Function**

Example:

```csharp
public class NameAddressSyncFunction
{
    private readonly INameAddressSyncService _syncService;

    public NameAddressSyncFunction(
        INameAddressSyncService syncService)
    {
        _syncService = syncService;
    }

    [Function("NameAddressSync")]
    public async Task Run(
        [TimerTrigger("0 0 0 * * *")] TimerInfo timer)
    {
        await _syncService.SyncAsync();
    }
}
```

The Function itself should be **very thin**.

Its responsibility is:

```text
Timer Trigger
      ↓
Call Application Service
```

Not:

```text
Timer Trigger
      ↓
500 lines of business logic
```

---

**7. What does `0 0 0 * * *` mean?**

Azure Functions Timer Trigger uses a six-field NCRONTAB expression. Microsoft examples use expressions such as `0 */1 * * * *` for every minute. 

For our example:

```text
0 0 0 * * *
│ │ │ │ │ │
│ │ │ │ │ └─ Day of week
│ │ │ │ └─── Month
│ │ │ └───── Day
│ │ └─────── Hour
│ └───────── Minute
└─────────── Second
```

So:

```text
0 0 0 * * *
```

means:

> **Every day at 00:00:00**

---

**8. Now create the Application Service**

This is where your real business logic belongs.

For example:

```csharp
public interface INameAddressSyncService
{
    Task SyncAsync(CancellationToken cancellationToken);
}
```

Implementation:

```csharp
public class NameAddressSyncService : INameAddressSyncService
{
    private readonly INameAddressRepository _repository;
    private readonly IContentManagerService _contentManager;
    private readonly ILogger<NameAddressSyncService> _logger;

    public NameAddressSyncService(
        INameAddressRepository repository,
        IContentManagerService contentManager,
        ILogger<NameAddressSyncService> logger)
    {
        _repository = repository;
        _contentManager = contentManager;
        _logger = logger;
    }

    public async Task SyncAsync(
        CancellationToken cancellationToken)
    {
        // Business logic here
    }
}
```

---

**9. Fetch Only Eligible Records**

Don't transfer everything every night.

Suppose your table contains:

```text
NameAddress
--------------------------------
Id
Name
Address
Status
SyncStatus
LastSyncedAt
```

Query:

```csharp
var records = await _repository.GetPendingRecordsAsync(cancellationToken);
```

Repository could implement:

```csharp
return await _db.NameAddresses
    .Where(x =>
        x.Status == NameAddressStatus.Ready &&
        x.SyncStatus != SyncStatus.Completed)
    .ToListAsync(cancellationToken);
```

So:

```text
Database
   ↓
Only Ready + Not Synced
```

---

**10. Perform Validation**

Before sending to Content Manager:

```csharp
foreach (var record in records)
{
    if (string.IsNullOrWhiteSpace(record.Name))
    {
        // validation failure
        continue;
    }

    if (string.IsNullOrWhiteSpace(record.Address))
    {
        // validation failure
        continue;
    }

    // continue processing
}
```

Real implementation would ideally use your existing validation/application rules rather than duplicating them in the Function.

---

**11. Map Your Model to Content Manager Model**

Your application model might be:

```csharp
NameAddress
{
    Id
    FirstName
    LastName
    AddressLine1
    City
    State
    ZipCode
}
```

Content Manager may expect:

```csharp
ContentManagerAddress
{
    ExternalId
    FullName
    Address
    City
    State
    PostalCode
}
```

So:

```text
Your Model
    ↓
Mapping
    ↓
Content Manager DTO
```

Example:

```csharp
var dto = new ContentManagerAddress
{
    ExternalId = record.Id.ToString(),
    FullName = $"{record.FirstName} {record.LastName}",
    Address = record.AddressLine1,
    City = record.City,
    State = record.State,
    PostalCode = record.ZipCode
};
```

---

**12. Connect to Content Manager**

Assume Content Manager provides an API:

```text
POST /api/name-address
```

Create:

```csharp
public interface IContentManagerService
{
    Task SendAsync(
        ContentManagerAddress request,
        CancellationToken cancellationToken);
}
```

Implementation:

```csharp
public class ContentManagerService
    : IContentManagerService
{
    private readonly HttpClient _httpClient;

    public ContentManagerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendAsync(
        ContentManagerAddress request,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/name-address",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
```

---

**13. Register HttpClient**

In `Program.cs`:

```csharp
builder.Services.AddHttpClient<
    IContentManagerService,
    ContentManagerService>();
```

Now:

```text
Azure Function
      ↓
Application Service
      ↓
ContentManagerService
      ↓
HttpClient
      ↓
Content Manager API
```

---

**14. Register Your Existing Services**

Your `Program.cs` could look approximately like:

```csharp
var builder =
    FunctionsApplication.CreateBuilder(args);

builder.Services.AddScoped<
    INameAddressSyncService,
    NameAddressSyncService>();

builder.Services.AddScoped<
    INameAddressRepository,
    NameAddressRepository>();

builder.Services.AddHttpClient<
    IContentManagerService,
    ContentManagerService>();

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString("AppDb")));

builder.Build().Run();
```

The exact registration depends on your existing architecture/database.

The important concept is:

> **Azure Function uses the same .NET DI mechanism you're already familiar with.**

Microsoft's isolated worker model supports standard dependency injection and application configuration. 

---

**15. What about the Database Connection?**

Locally you might have:

```json
{
  "ConnectionStrings": {
    "AppDb": "..."
  }
}
```

But **don't put production credentials into source code**.

In Azure:

```text
Azure Function App
       ↓
Environment / Application Settings
       ↓
Database configuration
```

Azure Functions provides per-Function-App application settings for connection strings, environment variables, and other configuration. 

For Azure resources, prefer **Managed Identity** where supported rather than storing credentials/secrets. Microsoft documents managed identity as a way for Functions to access Azure resources without embedding credentials. 

---

**16. Local Development**

You should first test everything locally.

Run:

```text
Azure Function
      ↓
Local DB
      ↓
Test Content Manager / mock API
```

You don't need Azure yet.

You can run the Function locally from Visual Studio.

For example:

```text
Function started
Waiting for timer...
```

You can temporarily use a schedule that runs every minute:

```csharp
[TimerTrigger("0 */1 * * * *")]
```

Microsoft's quickstart uses this type of schedule for local/testing scenarios. 

After testing, change it to:

```text
0 0 0 * * *
```

---

**17. Add Logging**

Inject:

```csharp
ILogger<NameAddressSyncService>
```

Then:

```csharp
_logger.LogInformation(
    "Name Address sync started. ExecutionId: {ExecutionId}",
    executionId);
```

Record count:

```csharp
_logger.LogInformation(
    "Found {Count} records to synchronize.",
    records.Count);
```

Success:

```csharp
_logger.LogInformation(
    "Record {RecordId} synchronized successfully.",
    record.Id);
```

Failure:

```csharp
_logger.LogError(
    exception,
    "Failed to synchronize record {RecordId}.",
    record.Id);
```

---

**18. Use a Correlation/Execution ID**

At the beginning:

```csharp
var executionId = Guid.NewGuid();
```

Then all logs contain:

```text
ExecutionId = ABC123
```

Example:

```text
00:00:01 Sync started      ExecutionId=ABC123
00:00:02 Found 500 records ExecutionId=ABC123
00:00:03 Record 101 success ExecutionId=ABC123
00:00:03 Record 102 success ExecutionId=ABC123
00:00:04 Record 103 failed  ExecutionId=ABC123
...
00:04:32 Sync completed     ExecutionId=ABC123
```

This makes troubleshooting much easier.

---

**19. Add Retry**

For Content Manager API:

```text
Function
   ↓
Content Manager
   ↓
503
   ↓
Retry
```

In modern .NET, you can use the HTTP resilience pipeline.

Conceptually:

```csharp
builder.Services
    .AddHttpClient<IContentManagerService,
                   ContentManagerService>()
    .AddStandardResilienceHandler();
```

You would then tune the policy for your actual operation rather than blindly retrying everything.

And remember:

> **Retry + idempotency**

because if the first request actually succeeded but the response was lost, a retry could otherwise create a duplicate.

---

**20. What happens when one record fails?**

Don't necessarily fail all 1,000 records.

Prefer:

```text
500 records

499 → Success
1   → Failed
```

Log the failed record.

Depending on requirements:

```text
Failed record
     ↓
Retry next execution
```

or maintain a retry mechanism.

---

**21. Now Create the Azure Resources**

Once local development is working, move to Azure.

You'll need a **Function App**.

A Function App provides the execution context and deployment/scaling boundary for the functions it contains. Functions in the same Function App are deployed and scaled together. 

Create:

```text
Resource Group
      │
      ├── Function App
      ├── Storage Account
      └── Application Insights
```

Azure's Function App creation flow can provision the required storage and Application Insights resources. 

---

**22. Azure Function App Configuration**

When creating:

```text
Function App
```

select appropriate:

```text
Runtime: .NET
Worker: Isolated
OS: Linux/Windows based on your organization's standards
Hosting plan: based on workload
Region: same/appropriate region
```

For your simple nightly workload, a serverless plan may be suitable, but the actual choice should depend on organizational requirements, networking, execution duration, scaling, and cost.

---

**23. Configure Application Settings**

In:

```text
Azure Portal
 ↓
Function App
 ↓
Settings
 ↓
Environment variables
```

you can configure:

```text
ContentManager__BaseUrl
ConnectionStrings__AppDb
```

etc.

Azure stores Function App settings separately from your code. 

Your code:

```csharp
var url =
    configuration["ContentManager:BaseUrl"];
```

Azure setting:

```text
ContentManager__BaseUrl
```

The double underscore represents the configuration hierarchy.

---

**25. What about Secrets?**

Don't do:

```text
ContentManagerPassword=MyPassword123
```

in Git.

Instead, depending on your environment:

```text
Azure Function
      ↓
Managed Identity
      ↓
Azure Key Vault
      ↓
Secrets
```

or use identity-based authentication to the target service where supported.

---

**26. Database Network Access**

This is an important real-world issue people forget.

Your architecture might be:

```text
Azure Function
      ↓
Internet
      ↓
Database
```

But if your database is private:

```text
Azure Function
      ↓
VNet
      ↓
Private Endpoint
      ↓
Database
```

You need to ensure the Function can actually reach the database.

Same for Content Manager:

```text
Function
   ↓
Network
   ↓
Content Manager
```

If Content Manager is inside your company's network, you may need:

* VNet integration
* Private endpoint
* VPN/ExpressRoute
* Firewall allow-listing
* Appropriate DNS/network configuration

The exact architecture depends on where Content Manager is hosted.

---

**27. Deploy the Function**

You have several options:

`Development`

Visual Studio:

```text
Right click Function project
       ↓
Publish
       ↓
Azure
       ↓
Function App
```

Microsoft supports Visual Studio publishing for C# Azure Functions. ([Microsoft Learn][5])

`Production`

Prefer:

```text
Git
 ↓
Azure DevOps Pipeline
 ↓
Build
 ↓
Test
 ↓
Publish
 ↓
Azure Function App
```

Microsoft also supports CI/CD and deployment templates for Function Apps. 

---

**27. Production Pipeline**

For your Azure DevOps experience, explain it like:

```text
Developer
    ↓
Feature Branch
    ↓
Pull Request
    ↓
Build
    ↓
Unit Tests
    ↓
Security/Quality Checks
    ↓
Package Function
    ↓
Deploy to Dev
    ↓
Test
    ↓
Deploy to QA
    ↓
Deploy to Production
```

---

**28. Configure Midnight Trigger in Azure**

You don't need to manually create a cron job on a server.

Your code contains:

```csharp
[TimerTrigger("0 0 0 * * *")]
```

Azure Functions runtime interprets the schedule and invokes the function accordingly. Microsoft documents configuring the Timer Trigger schedule as part of the Function itself. 

You can also put the schedule into configuration, which is often better for environments:

```csharp
[TimerTrigger("%NameAddressSyncSchedule%")]
```

Then:

```text
Azure App Settings

Name:
NameAddressSyncSchedule

Value:
0 0 0 * * *
```

This means you can change the schedule without changing code.

---

**29. Final Production Architecture**

Now put everything together:

```text
                        EXISTING MODULAR MONOLITH
                        ─────────────────────────

 User
   │
   ↓
Angular
   │
   ↓
ASP.NET Core
   │
   ↓
Name & Address Module
   │
   ↓
Application DB
   │
   │
   │                 NEW COMPONENT
   │                ───────────────
   │
   └──────────────────────┐
                          │
                          ↓
                 Azure Function App
                          │
                 Timer Trigger
                          │
                    12:00 AM
                          │
                          ↓
              NameAddressSyncService
                          │
               ┌──────────┴─────────┐
               ↓                    ↓
          Validation           Idempotency
               │                    │
               └──────────┬─────────┘
                          ↓
                  Content Manager
                       HttpClient
                          │
                          ↓
                 Content Manager API
                          │
                          ↓
                 Content Manager DB


Monitoring:

Azure Function
      ↓
Application Insights
      ↓
Azure Monitor
      ↓
Logs / Exceptions / Metrics / Traces
```

---
---


```text
                    EXISTING MODULAR MONOLITH
                 ┌─────────────────────────────┐
                 │                             │
User             │  Angular                    │
 │               │     ↓                       │
 │ Upload         │  ASP.NET Core API           │
 └───────────────►│     ↓                       │
                 │ Document Central Module     │
                 │     │                       │
                 │     ├── Metadata → DB       │
                 │     │                       │
                 │     └── File → Azure Blob   │
                 └─────────────┬───────────────┘
                               │
                         Blob Created
                               │
                               ▼
                        Azure Event Grid
                               │
                               ▼
                     Azure Service Bus
                               │
                         2–3 min delay
                               │
                               ▼
                     Azure Function
                  DocumentProcessingFunction
                         │       │
                 ┌───────┘       └────────┐
                 ▼                        ▼
            SharePoint              Content Manager
```


---

**1. Decide what Azure services we need**

For this scenario, create these Azure resources:

```text
Azure Resource Group
│
├── Storage Account
│      └── Blob Container
│
├── Event Grid
│
├── Service Bus Namespace
│      └── Queue
│
├── Function App
│
└── Application Insights
```

Potentially:

```text
Key Vault
```

for secrets.

And if SharePoint/Content Manager are private:

```text
VNet
Private Endpoint
VPN / ExpressRoute
```

may also be required.

For the first implementation, focus on:

**Blob Storage + Event Grid + Service Bus + Azure Function.**

---

**2. Why do we need Blob Storage?**

Suppose user uploads:

```text
Contract.pdf
```

Don't send the actual file directly through the Service Bus message.

Instead:

```text
Document Central
      ↓
Azure Blob Storage
      ↓
blob:
documents/12345/Contract.pdf
```

The Blob contains the actual file.

The message contains only metadata/reference.

Example:

```json
{
  "documentId": 12345,
  "blobContainer": "documents",
  "blobName": "12345/Contract.pdf",
  "fileName": "Contract.pdf"
}
```

This is important because Service Bus should carry **messages**, not large document payloads.

---

**3. Create Storage Account**

In Azure Portal:

```text
Azure Portal
   ↓
Create Resource
   ↓
Storage Account
```

For example:

```text
Storage Account: mycompanydocumentstorage
```

Inside it create:

```text
Containers
   ↓
documents
```

So:

```text
Storage Account
     │
     └── documents
           │
           ├── 1001/contract.pdf
           ├── 1002/invoice.pdf
           └── 1003/report.pdf
```

You can use separate containers if your organization wants:

```text
documents
processed
failed
archive
```

but don't create unnecessary containers initially.

---

**4. Modify your existing .NET application**

This is where your existing application integrates with Azure.

Current:

```text
Angular
   ↓
Document API
   ↓
Document Central
   ↓
Database
```

Change it to:

```text
Angular
   ↓
Document API
   ↓
Document Central
   ├── Metadata → Database
   │
   └── File → Azure Blob Storage
```

Your existing API becomes responsible for:

1. Validate request
2. Create document record
3. Upload file to Blob
4. Save Blob reference
5. Return response to user

---

**5. Add Azure Blob SDK to .NET application**

Install:

```bash
dotnet add package Azure.Storage.Blobs
```

Then create an abstraction.

```csharp
public interface IDocumentStorageService
{
    Task<string> UploadAsync(
        Stream file,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}
```

Implementation:

```csharp
public class BlobDocumentStorageService 
    : IDocumentStorageService
{
    private readonly BlobContainerClient _container;

    public BlobDocumentStorageService(
        BlobContainerClient container)
    {
        _container = container;
    }

    public async Task<string> UploadAsync(
        Stream file,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var blobName =
            $"{Guid.NewGuid()}/{fileName}";

        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(
            file,
            new BlobHttpHeaders
            {
                ContentType = contentType
            },
            cancellationToken: cancellationToken);

        return blobName;
    }
}
```

---

**6. Register Blob Storage in your .NET application**

For local development you could use a connection string.

For production, preferably use **Managed Identity**.

Conceptually:

```csharp
builder.Services.AddSingleton(
    new BlobContainerClient(
        configuration["Storage:ConnectionString"],
        "documents"));
```

But production architecture should preferably become:

```text
.NET Application
      ↓
Managed Identity
      ↓
Azure Storage
```

rather than:

```text
.NET Application
      ↓
hardcoded storage key
      ↓
Azure Storage
```

---

**7. Document upload flow**

Your existing controller might look like:

```csharp
[HttpPost]
public async Task<IActionResult> Upload(
    IFormFile file)
{
    var documentId =
        await _documentService.CreateAsync(file);

    return Ok(documentId);
}
```

Application service:

```csharp
public async Task<Guid> CreateAsync(
    IFormFile file)
{
    // 1. Validate

    // 2. Create DB record

    // 3. Upload to Blob

    // 4. Save Blob reference

    // 5. Return document ID
}
```

Database could contain:

```text
Document
--------------------------------
Id
FileName
ContentType
BlobContainer
BlobName
Status
CreatedAt
ProcessingStatus
```

Example:

```text
Id             1001
FileName       Contract.pdf
Container      documents
BlobName       1001/Contract.pdf
Status         Uploaded
ProcessingStatus Pending
```

---

**7. Now comes the event-driven part**

Once Blob Storage receives:

```text
1001/Contract.pdf
```

we need to detect:

> "A new document has arrived."

This is where **Event Grid** comes in.

Architecture:

```text
Blob Storage
      │
      │ BlobCreated
      ▼
Azure Event Grid
```

Event Grid is basically telling us:

> A blob was created.

---

**9. Create Event Grid**

In Azure:

```text
Storage Account
     ↓
Events
     ↓
Create Event Subscription
```

Configure:

```text
Event Type:
Blob Created
```

Source:

```text
Storage Account
```

But don't directly make the Function handle everything if you require durable delayed processing.

We can use:

```text
Blob Storage
      ↓
Event Grid
      ↓
Service Bus
```

---

**10. Why Service Bus?**

This is extremely important for your interview.

You need reliable background processing.

Suppose Content Manager is down.

If you directly do:

```text
Blob
 ↓
Function
 ↓
Content Manager
```

the Function may fail.

With Service Bus:

```text
Blob
 ↓
Event Grid
 ↓
Service Bus Queue
 ↓
Function
 ↓
Content Manager
```

The message stays in the queue.

Therefore:

```text
Content Manager temporarily unavailable
             ↓
       Message remains
             ↓
          Retry later
```

You also get:

* Retry
* Dead-letter queue
* Durable messaging
* Load leveling
* Multiple consumers if needed

---

**11. Create Service Bus**

Azure Portal:

```text
Create Resource
   ↓
Service Bus
```

Create namespace:

```text
mycompany-document-servicebus
```

Then:

```text
Queues
   ↓
document-processing
```

Architecture:

```text
Service Bus Namespace
       │
       └── document-processing
```

---

**12. How do we achieve the 2–3 minute delay?**

You said:

> After 2–3 minutes push to SharePoint and Content Manager.

Don't use:

```text
Task.Delay(3 minutes)
```

inside the Function.

That's a bad design.

Instead use **Service Bus scheduled delivery**.

Conceptually:

```text
Event Grid
    ↓
Service Bus
    ↓
Scheduled message
    ↓
2–3 minutes
    ↓
Message becomes available
    ↓
Azure Function
```

For example:

```text
Document uploaded
10:00:00

Message scheduled
10:03:00

Function receives
10:03:00
```

This is much more reliable than keeping a process alive for three minutes.

---

**13. Create Azure Function**

Now create:

```text
MyCompany.Functions
```

Choose:

```text
Azure Functions
C#
.NET isolated worker
Service Bus Trigger
```

Project:

```text
MyCompany.Functions
│
├── Functions
│    └── DocumentProcessingFunction.cs
│
├── Services
│
├── Program.cs
│
├── host.json
│
└── local.settings.json
```

---

**14. Function responsibility**

The Function should be **thin**.

Don't put 300 lines of business logic inside:

```csharp
DocumentProcessingFunction
```

Instead:

```text
Service Bus Trigger
       ↓
DocumentProcessingService
       ↓
 ┌─────┴──────┐
 ↓            ↓
SharePoint   Content Manager
```

---

**15. Service Bus Function**

Conceptually:

```csharp
public class DocumentProcessingFunction
{
    private readonly IDocumentProcessingService _service;

    public DocumentProcessingFunction(IDocumentProcessingService service)
    {
        _service = service;
    }

    [Function("DocumentProcessing")]
    public async Task Run([ServiceBusTrigger("document-processing",
            Connection = "ServiceBus")]
        string message)
    {
        await _service.ProcessAsync(message);
    }
}
```

The exact trigger configuration depends on your chosen SDK/configuration model, but this is the architecture you should understand.

---

**16. Message model**

Don't send:

```json
{
   "file": ".....huge binary....."
}
```

Send:

```json
{
    "documentId": "1001",
    "blobContainer": "documents",
    "blobName": "1001/Contract.pdf",
    "fileName": "Contract.pdf"
}
```

Now Function can retrieve:

```text
Azure Function
      ↓
Blob Storage
      ↓
Contract.pdf
```

---

**17. Function processing service**

Create:

```csharp
public interface IDocumentProcessingService
{
    Task ProcessAsync(DocumentProcessingMessage message, CancellationToken cancellationToken);
}
```

Implementation:

```csharp
public class DocumentProcessingService
    : IDocumentProcessingService
{
    private readonly IBlobStorageService _blobStorage;
    private readonly ISharePointService _sharePoint;
    private readonly IContentManagerService _contentManager;

    public async Task ProcessAsync(
        DocumentProcessingMessage message,
        CancellationToken cancellationToken)
    {
        // 1. Check document status

        // 2. Check idempotency

        // 3. Download blob

        // 4. Push to SharePoint

        // 5. Push to Content Manager

        // 6. Update processing status
    }
}
```

---

**18. Function downloads document**

Your Function gets:

```text
documentId = 1001
blobName = 1001/Contract.pdf
```

Then:

```text
Azure Function
      ↓
BlobServiceClient
      ↓
documents/1001/Contract.pdf
```

For example:

```csharp
var blob =
    _blobContainer.GetBlobClient(message.BlobName);

var response =
    await blob.DownloadStreamingAsync(
        cancellationToken);
```

Now you have the document stream.

---

**19. Send to SharePoint**

Create another abstraction:

```csharp
public interface ISharePointService
{
    Task UploadAsync(
        Stream document,
        string fileName,
        CancellationToken cancellationToken);
}
```

Implementation could call Microsoft Graph / SharePoint APIs depending on your organization's integration.

Architecture:

```text
DocumentProcessingService
          ↓
ISharePointService
          ↓
Microsoft Graph / SharePoint API
          ↓
SharePoint
```

Don't put Graph-specific code into the Function itself.

---

**20. Send to Content Manager**

Same idea:

```csharp
public interface IContentManagerService
{
    Task UploadAsync(
        Stream document,
        string fileName,
        CancellationToken cancellationToken);
}
```

Implementation:

```csharp
public class ContentManagerService
    : IContentManagerService
{
    private readonly HttpClient _httpClient;

    public async Task UploadAsync(...)
    {
        // API call to Content Manager
    }
}
```

Architecture:

```text
Function
   ↓
Application Service
   ↓
ContentManagerService
   ↓
HttpClient
   ↓
Content Manager API
```

---
---

## Managed Identity

**What problem does Managed Identity solve?**

Suppose your Azure Function needs to read a document from Blob Storage.

**Without Managed Identity**

You might do:

```text
Azure Function
     ↓
Storage Account
```

and configure:

```json
{
  "StorageConnectionString":
    "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=SECRET..."
}
```

Now your application needs a **storage key/password**.

Problems:

* Secret has to be stored somewhere
* Secret can leak
* Secret needs rotation
* Developers may accidentally commit it
* You need to manage credentials

Managed Identity removes that requirement.

---

**Think of Managed Identity as a "service account"**

This is the easiest mental model.

For example:

```text
Human:
Swapnil
   ↓
Microsoft Entra ID
   ↓
Identity
```

For an Azure application:

```text
Azure Function
      ↓
Managed Identity
      ↓
Microsoft Entra ID
```

The Function gets an identity that Azure can recognize.

You can then tell Azure:

> "This Function is allowed to read from this Storage Account."

So:

```text
Function
   ↓
"I am Function XYZ"
   ↓
Azure verifies identity
   ↓
Does XYZ have permission?
   ↓
YES
   ↓
Allow access
```

---

**3. Managed Identity is NOT something you generate in .NET**

This is an important distinction.

You don't write:

```csharp
var identity = new ManagedIdentity();
```

Instead, **Azure creates/manages the identity**.

Your .NET application simply uses it.

There are two types:

| Type            | Meaning                                                            |
| --------------- | ------------------------------------------------------------------ |
| System-assigned | Identity belongs to one Azure resource                             |
| User-assigned   | Separate identity that can be attached to multiple Azure resources |

---

**System-assigned Managed Identity**

Let's use your Azure Function.

You create:

```text
Function App
    ↓
MyCompanyDocumentFunction
```

In Azure Portal:

```text
Function App
   ↓
Settings
   ↓
Identity
   ↓
System assigned
   ↓
On
   ↓
Save
```

Azure now creates an identity for your Function.

Conceptually:

```text
MyCompanyDocumentFunction
          │
          ▼
System-Assigned Managed Identity
          │
          ▼
Microsoft Entra ID
```

You don't need to create a username/password.

---

**Now give the identity permission**

This is the second half.

Creating an identity **doesn't automatically give it access to anything**.

Suppose:

```text
Function
    ↓
needs to READ
    ↓
Blob Storage
```

Go to:

```text
Storage Account
   ↓
Access Control (IAM)
   ↓
Add Role Assignment
```

Select something like:

```text
Storage Blob Data Reader
```

Then:

```text
Assign access to:
Managed Identity

Select:
MyCompanyDocumentFunction
```

Now:

```text
Function
   │
   │ Managed Identity
   ▼
Microsoft Entra ID
   │
   │ authorized
   ▼
Storage Account
   │
   ▼
Blob Container
```

Azure RBAC is what determines what the identity is allowed to do. ([Microsoft Learn][3])

---

**Now what happens inside .NET?**

This is where `Azure.Identity` comes in.

Install:

```bash
dotnet add package Azure.Identity
```

Then:

```csharp
using Azure.Identity;
```

For example:

```csharp
var credential = new DefaultAzureCredential();
```

And:

```csharp
var blobServiceClient =
    new BlobServiceClient(
        new Uri(storageUrl),
        credential);
```

So your application code doesn't contain:

```text
Storage Account Key
```

Instead:

```text
.NET
 ↓
DefaultAzureCredential
 ↓
Managed Identity
 ↓
Microsoft Entra token
 ↓
Azure Storage
```

The Azure SDK can obtain a Microsoft Entra token using the application's managed identity. 

---

**The really useful part: local development**

You may ask:

> "But my laptop doesn't have a Managed Identity. How can I run this locally?"

This is where `DefaultAzureCredential` is very useful.

```csharp
var credential = new DefaultAzureCredential();
```

When running locally, it can use your developer credentials, such as your Azure CLI/Visual Studio sign-in.

When running in Azure, it can use the Azure resource's Managed Identity.

So the same code can work:

```text
LOCAL
────────────────

.NET Application
      ↓
DefaultAzureCredential
      ↓
Developer's Azure login
      ↓
Azure


AZURE
────────────────

Azure Function
      ↓
DefaultAzureCredential
      ↓
Managed Identity
      ↓
Azure
```

That's one of the biggest benefits of using the Azure SDK credential model.

---

**Let's apply this to our Scenario 2**

Our architecture was:

```text
Document Central
      ↓
Azure Blob Storage
      ↓
Event Grid
      ↓
Service Bus
      ↓
Azure Function
      ↓
SharePoint
      ↓
Content Manager
```

The Function needs several permissions.

For example:

`Blob Storage`

Function needs:

```text
READ Blob
```

Give:

```text
Storage Blob Data Reader
```

`Service Bus`

Function needs:

```text
RECEIVE messages
```

Give an appropriate Service Bus data receiver role.

`Key Vault`

If using Key Vault:

```text
Function
   ↓
Managed Identity
   ↓
Key Vault
```

Give the appropriate Key Vault data-plane permission.

So you might have:

```text
                Managed Identity
                       │
          ┌────────────┼────────────┐
          ↓            ↓            ↓
      Blob Storage  Service Bus  Key Vault
        Reader       Receiver      Secrets
```

The identity itself doesn't give access.

**RBAC permissions attached to the identity give access.**

---

**Example .NET Blob code**

Suppose:

```text
Storage Account:
mycompanydocumentstorage

Container:
documents
```

You can configure:

```text
Storage__BlobServiceUri
```

as:

```text
https://mycompanydocumentstorage.blob.core.windows.net
```

Then:

```csharp
using Azure.Identity;
using Azure.Storage.Blobs;

var blobServiceClient =
    new BlobServiceClient(
        new Uri(configuration["Storage:BlobServiceUri"]!),
        new DefaultAzureCredential());
```

Then:

```csharp
var containerClient =
    blobServiceClient.GetBlobContainerClient("documents");
```

And:

```csharp
var blobClient =
    containerClient.GetBlobClient(
        "1001/Contract.pdf");
```

Then:

```csharp
var response =
    await blobClient.DownloadStreamingAsync();
```

Notice something important:

**There is no storage account key in the code.**

---

**What is actually happening behind the scenes?**

Your code:

```csharp
new DefaultAzureCredential()
```

eventually obtains an access token.

Conceptually:

```text
.NET Function
      │
      │ "I need access to Blob Storage"
      ▼
Managed Identity
      │
      ▼
Microsoft Entra ID
      │
      │ Access Token
      ▼
Azure Storage
      │
      ▼
RBAC checks permissions
      │
      ▼
Allow / Deny
```

Azure manages the credentials used to obtain the identity token; your application doesn't need to handle a password or secret for the managed identity. 

---

**What about User-Assigned Managed Identity?**

Suppose you have:

```text
Function App A
Function App B
Web App
Background Worker
```

and all four need the same Azure permissions.

Instead of creating separate identities:

```text
Function A → Identity A
Function B → Identity B
Web App    → Identity C
Worker     → Identity D
```

you could create:

```text
                 User Assigned Identity
                         │
             ┌───────────┼───────────┐
             ↓           ↓           ↓
         Function A  Function B   Web App
```

User-assigned identities are standalone Azure resources and can be associated with multiple Azure resources. 

Create one from:

```text
Azure Portal
   ↓
Managed Identities
   ↓
Create
```

For example:

```text
MyCompanyDocumentIdentity
```

Then attach it to your Function App.

---

# 12. System vs User Assigned

Remember this for interviews:

|                                  | System Assigned          | User Assigned       |
| -------------------------------- | ------------------------ | ------------------- |
| Created                          | With Azure resource      | Separately          |
| Lifecycle                        | Tied to resource         | Independent         |
| Can attach to multiple resources | No                       | Yes                 |
| Simple setup                     | ✅                        | Slightly more setup |
| Reusable                         | ❌                        | ✅                   |
| Good for                         | One application/resource | Shared identity     |

Microsoft's current documentation describes system-assigned identities as tied to one resource and user-assigned identities as reusable across resources. 

---

**Do we need Managed Identity in the existing .NET application too?**

Potentially **yes**.

This is important for our architecture.

Suppose your existing ASP.NET Core application uploads the document to Blob Storage:

```text
Angular
   ↓
ASP.NET Core
   ↓
Blob Storage
```

If that ASP.NET Core application itself runs in:

```text
Azure App Service
```

you can enable Managed Identity on the **App Service** too.

Then:

```text
ASP.NET Core App
       ↓
Managed Identity
       ↓
Blob Storage
```

And separately:

```text
Azure Function
       ↓
Managed Identity
       ↓
Blob Storage
```

You can give each application only the permissions it actually needs.

For example:

```text
ASP.NET Core
   → Blob Data Contributor

Function
   → Blob Data Reader
```

This follows the **least-privilege** principle.

---

**What if our existing application is running on-premises?**

This is where the design changes.

If:

```text
ASP.NET Core
    ↓
On-premise server
```

then that server doesn't automatically have an Azure Managed Identity like an Azure-hosted resource does.

You would typically use another authentication approach, or potentially move the workload to an Azure-hosted environment depending on your architecture.

But our newly created:

```text
Azure Function
```

is Azure-hosted, so Managed Identity is directly applicable.

---

**Managed Identity vs App Registration**

This is another common interview question.

`Traditional application authentication`

You might create:

```text
Entra App Registration
     ↓
Client ID
Client Secret
```

Then:

```text
.NET
 ↓
Client ID + Secret
 ↓
Entra ID
 ↓
Token
```

Now you have a secret to protect and rotate.

`Managed Identity`

```text
Azure Function
     ↓
Managed Identity
     ↓
Entra ID
     ↓
Token
```

No application-managed client secret is required.

---
---
