## Azure Functions

**Azure Functions** = serverless, event-driven compute. Azure manages the hosting/execution environment and the function runs in response to a trigger.

```text
Trigger/Event → Function → Azure-managed execution
```

### Azure Functions vs ASP.NET Core API

| ASP.NET Core API | Azure Functions |
|---|---|
| Continuously hosted application | Event-driven execution |
| Controllers/endpoints | Functions/triggers |
| Good for APIs and long-running application processes | Good for specific event-driven workloads |
| Application manages its structure | Azure provides the Function runtime |

**Do not say:** Azure Functions replaces ASP.NET Core. They solve different problems.

### Triggers

Every Function has **one trigger** — the event that starts execution.

| Trigger | Typical use |
|---|---|
| HTTP | Lightweight API/webhook |
| Timer | Scheduled jobs |
| Service Bus | Async message processing |
| Queue Storage | Queue processing |
| Blob Storage | Blob/file events |
| Event Grid | Event-driven integration |
| Event Hubs | Event/stream processing |
| Cosmos DB | Database change events |

**Interview examples:**
- HTTP Trigger → lightweight API
- Service Bus Trigger → asynchronous processing
- Timer Trigger → cleanup/report/batch job

### Trigger vs Binding

- **Trigger** = what starts the Function.
- **Binding** = how the Function connects to input/output resources.

```text
Trigger → Function → Input/Output bindings
```

- Input binding → read data
- Output binding → write data

### .NET Azure Functions

For modern .NET Functions, use the **isolated worker model**.

Benefits:
- Normal .NET dependency injection
- Middleware support
- Independent .NET versioning

```bash
func init MyFunctionApp --worker-runtime dotnet-isolated
```

Basic HTTP Function:

```csharp
[Function("Hello")]
public HttpResponseData Run(
    [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
{
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.WriteString("Hello from Azure Function!");
    return response;
}
```

**Dependency Injection**

```csharp
// Program.cs
builder.Services.AddScoped<IOrderService, OrderService>();

public class OrderFunction
{
    private readonly IOrderService _orderService;

    public OrderFunction(IOrderService orderService)
    {
        _orderService = orderService;
    }
}
```

---

**Important Function Scenarios**

—> `Function + Service Bus`

```text
Order Service
    ↓
OrderCreated
    ↓
Azure Service Bus
    ↓
Payment Function
```

```csharp
[Function("ProcessPayment")]
public async Task Run(
    [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")]
    string message)
{
    // Process message
}
```

Why use it?

```text
Order API
   ↓
Save Order
   ↓
Publish OrderCreated
   ↓
Service Bus
   ├── Payment Function
   ├── Notification Function
   └── Invoice Function
```

Benefits:
- Asynchronous communication
- Message broker
- Load leveling
- Retry
- Dead-letter queue (DLQ)
- Event-driven architecture
- Supports idempotent consumers

—> `Function + Timer`

Use Timer Trigger for scheduled work.

```csharp
[Function("Cleanup")]
public async Task Run(
    [TimerTrigger("0 0 2 * * *")] TimerInfo timer)
{
    await cleanupService.CleanupAsync();
}
```

Typical uses:
- Scheduled reports
- Cleanup
- Data synchronization
- Batch jobs
- Periodic processing

—> `Function + Blob`

```text
File uploaded
    ↓
Blob Storage
    ↓
Blob Trigger
    ↓
Azure Function
    ↓
Process file
```

**Function vs BackgroundService**

**BackgroundService:** runs inside your ASP.NET Core application; you manage the hosting/application lifecycle.

**Azure Function:** Azure manages the execution environment and is better suited to event-driven, scheduled, queue, webhook, and integration workloads.

Choose BackgroundService when work is tightly coupled to a continuously running ASP.NET Core application or needs long-running process behavior that does not fit the Functions execution model.

**Function vs Microservice**

- **Microservice** = architectural/business boundary.
- **Function** = serverless execution unit.

```text
Payment Microservice
       ↓
Azure Functions
```

A Function is **not automatically a microservice**.

**Hosting Plans**

Know the major options:
- Flex Consumption
- Consumption
- Premium
- Dedicated/App Service
- Container Apps

Interview-level understanding:

```text
Consumption/Flex Consumption → serverless / scale with demand
Premium → more control / warm instances
Dedicated → App Service infrastructure
```

Do not memorize pricing unless asked.

**Cold Start**

When an inactive Function needs a new instance:

```text
Request
  ↓
No active instance
  ↓
Start instance
  ↓
Execute
```

The startup delay is called **cold start**. Hosting choices such as Premium can help when cold-start latency matters.

---

**Durable Functions**

Durable Functions support **stateful, long-running workflows** in a serverless environment. They can manage state, checkpoints, retries, and recovery.

```text
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

Example:

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

**Durable Functions vs Saga**

**Saga** = distributed transaction/business workflow pattern.

**Durable Functions** = Azure technology for orchestrating stateful serverless workflows.

```text
Saga = pattern
Durable Functions = technology
```

A Durable Function can orchestrate a workflow that follows Saga-like business logic, but the two concepts are not identical.

---

**Function in Clean Architecture / Modular Monolith**

A Function should be a **thin entry point**. Business logic belongs in Application services.

Recommended structure:

```text
MyCompany.sln
├── MyCompany.Web
├── MyCompany.Application
├── MyCompany.Domain
├── MyCompany.Infrastructure
└── MyCompany.Functions
```

```text
                 Application Layer
                  /             \
                 ↓               ↓
        ASP.NET Core         Azure Function
             ↓                    ↓
           HTTP                  Timer/Event
```

The Function can share appropriate Domain/Application/Infrastructure components with the existing application.

**Thin Function**

```csharp
public class NameAddressSyncFunction
{
    private readonly INameAddressSyncService _syncService;

    public NameAddressSyncFunction(INameAddressSyncService syncService)
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

The Function should mainly do:

```text
Trigger → Application Service
```

Do **not** put hundreds of lines of business logic inside the Function.

**Timer CRON**

Azure Functions Timer Trigger uses a six-field NCRONTAB expression:

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

`0 0 0 * * *` = every day at midnight.

For environment-specific schedules:

```csharp
[TimerTrigger("%NameAddressSyncSchedule%")]
```

Then configure the value in Function App settings.

---

**Application Service / Processing Pattern**

Example service:

```csharp
public interface INameAddressSyncService
{
    Task SyncAsync(CancellationToken cancellationToken);
}
```

Typical flow:

```text
Timer Trigger
    ↓
Application Service
    ↓
Fetch eligible records
    ↓
Validate
    ↓
Map DTO
    ↓
Call external API
    ↓
Update status
```

**Important Practices**

**Fetch only eligible records:**

```csharp
var records = await _repository
    .GetPendingRecordsAsync(cancellationToken);
```

Avoid loading unnecessary records.

**CancellationToken:** pass it through database and HTTP calls.

**Logging:** log execution start, record count, success/failure, and completion.

**Execution/Correlation ID:** use one ID per Function execution so logs can be correlated.

```csharp
var executionId = Guid.NewGuid();
```

**One-record failure:** do not necessarily fail the entire batch. Log the failed record and use a retry/status strategy appropriate to the business requirement.

---

**Retry + Idempotency**

For external API failures:

```text
Function
   ↓
External API
   ↓
503
   ↓
Retry
```

Modern .NET HTTP clients can use the standard resilience handler:

```csharp
builder.Services
    .AddHttpClient<IContentManagerService, ContentManagerService>()
    .AddStandardResilienceHandler();
```

**Critical:** Retry should be combined with **idempotency**. If the first request succeeded but the response was lost, an unsafe retry can create a duplicate.

Key interview phrase:

> Retry + idempotency must be considered together.

---

**Configuration & Secrets**

Do not hardcode connection strings, passwords, or keys.

Local:

```text
local.settings.json
```

Azure:

```text
Function App Settings / Environment Variables
```

Example:

```text
ContentManager__BaseUrl
ConnectionStrings__AppDb
```

`__` represents configuration hierarchy.

For secrets, prefer:

```text
Application
    ↓
Managed Identity
    ↓
Azure Key Vault
    ↓
Secrets
```

---

**Production Architecture — Scheduled Sync**

```text
Existing Modular Monolith

Angular
   ↓
ASP.NET Core
   ↓
Application DB

                 ↓
          Azure Function App
                 ↓
           Timer Trigger
                 ↓
       NameAddressSyncService
              /       \
             ↓         ↓
       Validation   Idempotency
              \       /
                 ↓
          Content Manager API
```

Monitoring:

```text
Azure Function
    ↓
Application Insights
    ↓
Azure Monitor
    ↓
Logs / Metrics / Exceptions / Traces
```

---

**Event-Driven Document Processing**

A useful enterprise architecture from the document:

```text
User
  ↓
Angular
  ↓
ASP.NET Core API
  ↓
Document Central
  ├── Metadata → Database
  └── File → Azure Blob Storage
                    ↓
                 Event Grid
                    ↓
                Service Bus
                    ↓
              Azure Function
                /         \
               ↓           ↓
         SharePoint    Content Manager
```

**Azure Resources**

```text
Resource Group
├── Storage Account / Blob Container
├── Event Grid
├── Service Bus Namespace / Queue
├── Function App
├── Application Insights
└── Key Vault (if required)
```

Potential networking components for private systems:

```text
VNet
Private Endpoint
VPN / ExpressRoute
Firewall / DNS configuration
```

***Why Blob Storage?***

Store the actual document in Blob Storage rather than putting large binary content into Service Bus.

Message should contain a reference/metadata:

```json
{
  "documentId": "1001",
  "blobContainer": "documents",
  "blobName": "1001/Contract.pdf",
  "fileName": "Contract.pdf"
}
```

**Document Upload Flow**

```text
API
 ↓
Validate request
 ↓
Create DB record
 ↓
Upload file to Blob
 ↓
Save Blob reference
 ↓
Return document ID
```

**Typical DB fields:**

```text
Document
----------------
Id
FileName
ContentType
BlobContainer
BlobName
Status
CreatedAt
ProcessingStatus
```

**Blob SDK**

```bash
dotnet add package Azure.Storage.Blobs
```

Use an abstraction such as:

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

Prefer Managed Identity in production instead of storage account keys.

---

**Event Grid + Service Bus**

Blob creation can produce a `BlobCreated` event:

```text
Blob Storage
    ↓ BlobCreated
Event Grid
    ↓
Service Bus Queue
    ↓
Azure Function
```

**Why Service Bus?**

If the downstream system is unavailable:

```text
Blob
 ↓
Event Grid
 ↓
Service Bus Queue
 ↓
Function
 ↓
Content Manager unavailable
```

The message can remain available for later processing according to the queue/retry configuration.

Benefits:
- Durable messaging
- Retry
- Dead-letter queue
- Load leveling
- Multiple consumers

**Delayed Processing**

For a required 2–3 minute delay, do **not** use:

```csharp
Task.Delay(TimeSpan.FromMinutes(3));
```

Use **Service Bus scheduled delivery**:

```text
Event Grid
   ↓
Scheduled Service Bus message
   ↓
2–3 minutes
   ↓
Message becomes available
   ↓
Function processes it
```

---

**Document Processing Function**

Keep the Function thin:

```csharp
public class DocumentProcessingFunction
{
    private readonly IDocumentProcessingService _service;

    public DocumentProcessingFunction(IDocumentProcessingService service)
    {
        _service = service;
    }

    [Function("DocumentProcessing")]
    public async Task Run(
        [ServiceBusTrigger("document-processing", Connection = "ServiceBus")]
        string message)
    {
        await _service.ProcessAsync(message);
    }
}
```

Processing service:

```text
Service Bus Trigger
      ↓
DocumentProcessingService
      ↓
Check document status
      ↓
Check idempotency
      ↓
Download Blob
      ↓
Upload to SharePoint
      ↓
Upload to Content Manager
      ↓
Update processing status
```

Message should contain metadata/reference, not the binary file.

```csharp
public interface IDocumentProcessingService
{
    Task ProcessAsync(
        DocumentProcessingMessage message,
        CancellationToken cancellationToken);
}
```

External integrations should be behind abstractions:

```text
DocumentProcessingService
   ├── IBlobStorageService
   ├── ISharePointService
   └── IContentManagerService
```

---

### Deployment & Production

***Local → Azure***

```text
Developer
   ↓
Git / Pull Request
   ↓
Build
   ↓
Unit Tests
   ↓
Security / Quality Checks
   ↓
Package
   ↓
Deploy Dev
   ↓
Test
   ↓
Deploy QA
   ↓
Deploy Production
```

**Function App**

A Function App is the deployment/scaling boundary for the Functions it contains; Functions in the same Function App are deployed and scaled together.

Typical resources:

```text
Resource Group
├── Function App
├── Storage Account
└── Application Insights
```

Choose runtime/hosting/networking based on workload, execution duration, scaling, cost, and organizational requirements.

**Network Access**

If DB or external systems are private:

```text
Function
   ↓
VNet / Private Connectivity
   ↓
Private Endpoint / VPN / ExpressRoute
   ↓
Private resource
```

Also consider firewall rules and DNS.

---
---

## Managed Identity

***What problem does it solve?***

Without Managed Identity:

```text
Application
   ↓
Storage Key / Password / Connection Secret
   ↓
Azure Resource
```

Problems:
- Secrets must be stored
- Secrets can leak
- Rotation is required
- Risk of accidental source-control commits

With Managed Identity:

```text
Azure Application
      ↓
Managed Identity
      ↓
Microsoft Entra ID
      ↓
Access Token
      ↓
Azure Resource
      ↓
RBAC permission check
```

**Mental model:** Managed Identity is an Azure-managed identity/service identity for an application.

**System-assigned vs User-assigned**

| System-assigned | User-assigned |
|---|---|
| Tied to one Azure resource | Separate identity resource |
| Created/managed with resource | Can be attached to multiple resources |
| Deleted with resource | Lifecycle is independent |

Important: **Creating an identity does not automatically grant access.**

You still need Azure RBAC role assignments on the target resource.

---

**Azure RBAC**

Example:

```text
Function App Managed Identity
          ↓
Storage Account
          ↓
Access Control (IAM)
          ↓
Role Assignment
          ↓
Storage Blob Data Reader
```

Typical roles from the scenario:

```text
Blob Storage → Storage Blob Data Reader / Contributor
Service Bus  → appropriate Data Receiver role
Key Vault    → Key Vault Secrets User
```

RBAC role assignments can be scoped at appropriate levels:

```text
Management Group
      ↓
Subscription
      ↓
Resource Group
      ↓
Resource
```

**Identity authenticates; RBAC authorizes.**

---

**Managed Identity with .NET**

Install:

```bash
dotnet add package Azure.Identity
```

Use:

```csharp
using Azure.Identity;

var credential = new DefaultAzureCredential();
```

Example Blob client:

```csharp
var blobServiceClient = new BlobServiceClient(
    new Uri(configuration["Storage:BlobServiceUri"]!),
    new DefaultAzureCredential());

var containerClient =
    blobServiceClient.GetBlobContainerClient("documents");

var blobClient =
    containerClient.GetBlobClient("1001/Contract.pdf");

var response =
    await blobClient.DownloadStreamingAsync();
```

No storage account key is required in the application code.

**DefaultAzureCredential**

The same application code can work in both environments:

```text
LOCAL
.NET Application
      ↓
DefaultAzureCredential
      ↓
Developer credential
(Azure CLI / Visual Studio)

AZURE
Azure Function / App Service
      ↓
DefaultAzureCredential
      ↓
Managed Identity
```

Microsoft Entra ID issues the token; the target Azure resource validates the token and RBAC permissions.

---

**Authentication vs Managed Identity**

Do not confuse these two scenarios.

**User → API authentication**

Example:

```text
Angular
   ↓
Microsoft Entra ID / JWT
   ↓
ASP.NET Core API
```

This answers:

> Who is the user and are they authenticated?

**API → Azure resource authentication**

Example:

```text
ASP.NET Core / Function
   ↓
Managed Identity
   ↓
Microsoft Entra ID
   ↓
Azure Storage / Key Vault / Service Bus
```

This answers:

> Can this application securely access the Azure resource?

You can use both in the same application.

---

**Final Architecture to Remember**

```text
                         USER
                           ↓
                        Angular
                           ↓
                    ASP.NET Core API
                           ↓
                    Document Central
                     /             \
                    ↓               ↓
               Metadata DB      Blob Storage
                                    ↓
                                Event Grid
                                    ↓
                              Service Bus
                                    ↓
                           Azure Function
                            /           \
                           ↓             ↓
                     SharePoint    Content Manager

Authentication / Azure access:

Function / API
      ↓
Managed Identity
      ↓
Microsoft Entra ID
      ↓
Access Token
      ↓
Azure Resource
      ↓
RBAC

Monitoring:

Function
   ↓
Application Insights
   ↓
Azure Monitor
```

**Must Remember**

1. Function = event-driven execution.
2. Trigger starts the Function; binding connects resources.
3. Keep Functions thin; business logic belongs in Application services.
4. Service Bus = durable asynchronous messaging.
5. Blob Storage = large file/object storage.
6. Event Grid = event notification/routing.
7. Durable Functions = stateful serverless workflows.
8. Saga = distributed transaction pattern.
9. Retry must be considered with idempotency.
10. Managed Identity removes the need to store Azure credentials.
11. Entra ID authenticates; RBAC authorizes.
12. `DefaultAzureCredential` supports local developer credentials and Azure Managed Identity.
13. Use Key Vault/identity-based access instead of hardcoded secrets.
14. Use Application Insights/Azure Monitor for production observability.
------
---------