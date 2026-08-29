- HTTP status codes
- HTTP Verbs
- IConfiguration & Configuration Providers
- Key Vault
- Authentication in .NET Core
- Startup.cs & Program.cs
- MetaPackage
- RESTful API 

---

## HTTP status codes

## HTTP Verbs

**`1. GET`**

- **Purpose:** Retrieve data from the server.
- **Request Body:** Not allowed (ignored if sent).
- **Response:** Data only, no side effects.
- **Idempotent:** ✅ Yes (calling it multiple times doesn’t change the state).
- **Safe:** ✅ Yes (should not modify data).
- **Example:**

```csharp
GET /api/products

[HttpGet]
public IActionResult GetProducts() => Ok(_service.GetAll());
```

**`2. POST`**

- **Purpose:** Create a new resource or perform an action that changes state.
- **Request Body:** Allowed (usually JSON or form data).
- **Idempotent:** ❌ No (each call can create new data).
- **Safe:** ❌ No (modifies server state).
- **Example:**

```csharp
POST /api/products
Content-Type: application/json

{
  "name": "Laptop",
  "price": 1000
}

[HttpPost]
public IActionResult CreateProduct(ProductDto dto) => CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);

```

**`3. PUT`**

- **Purpose:** Update an entire existing resource.
- **Request Body:** Required (full representation of the resource).
- **Idempotent:** ✅ Yes (same request → same result).
- **Safe:** ❌ No (changes server state).
- **Example:**
```csharp
PUT /api/products/1
Content-Type: application/json

{
  "id": 1,
  "name": "Laptop Pro",
  "price": 1200
}

[HttpPut("{id}")]
public IActionResult UpdateProduct(int id, ProductDto dto)
{
    if (id != dto.Id) return BadRequest();
    _service.Update(dto);
    return NoContent();
}
```

**`4. PATCH`**

- **Purpose:** Partially update a resource.
- **Request Body:** Required (only fields to change).
- **Idempotent:** ✅ Usually yes, but can be ❌ if used improperly.
- **Safe:** ❌ No.
- **Example:**
```csharp
PATCH /api/products/1
Content-Type: application/json

{
  "price": 900
}

[HttpPatch("{id}")]
public IActionResult UpdatePartial(int id, JsonPatchDocument<ProductDto> patchDoc)
{
    var product = _service.GetById(id);
    if (product == null) return NotFound();
    patchDoc.ApplyTo(product);
    return NoContent();
}
```


**`5. DELETE`**

- **Purpose:** Remove a resource.
- **Idempotent:** ✅ Yes (deleting multiple times → same result: resource gone).
- **Safe:** ❌ No.
- **Example:**

```csharp
DELETE /api/products/1

[HttpDelete("{id}")]
public IActionResult DeleteProduct(int id)
{
    _service.Delete(id);
    return NoContent();
}
```

**`6. HEAD`**

- **Purpose:** Same as GET but without the response body.
- **Use Case:** Check if resource exists, or check headers/metadata.
- **Idempotent:** ✅ Yes.
- **Example:**

```csharp
HEAD /api/products

[HttpHead]
public IActionResult HeadCheck() => Ok();
```

**`7. OPTIONS`**

- **Purpose:** Get supported HTTP methods for a resource (often for CORS preflight).
- **Idempotent:** ✅ Yes.
- **Example:**    
```csharp
OPTIONS /api/products

Server Response:
Allow: GET, POST, PUT, PATCH, DELETE, OPTIONS

[HttpOptions]
public IActionResult Options() => Ok();
```

***Key .NET Notes***

> In ASP.NET Core, we use attributes like [HttpGet], [HttpPost], [HttpPut], [HttpPatch], [HttpDelete], [HttpHead], [HttpOptions].

- For CORS preflight requests, make sure to call:
```csharp
builder.Services.AddCors();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
```

**POST vs PUT:**
- POST → server generates ID, creates new resource
- PUT → client specifies ID, replaces resource entirely

**PUT vs PATCH:**
- PUT replaces entire resource
- PATCH updates only certain fields

---------------------------
---------------------------

## IConfiguration & Configuration Providers

> `IConfiguration` is the .NET abstraction used to read configuration values from different sources.

> `IConfiguration` provides a unified way to read hierarchical application configuration from multiple sources, 
while the Options Pattern provides a strongly typed way to consume related configuration settings.

For example, your application may have:

```text
appsettings.json
Environment Variables
User Secrets
Command-line arguments
Azure/AWS configuration
Custom configuration files
```

`IConfiguration` gives you **one common way to access all of them**.

Think:

```text
                  IConfiguration
                        │
        ┌───────────────┼────────────────┐
        ▼               ▼                ▼
appsettings.json   Environment       User Secrets
                        │
                        ▼
                 Configuration
```


***`appsettings.json`***

A very common configuration source is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EmployeeDb"
  },

  "Jwt": {
    "Issuer": "MyApi",
    "Audience": "MyClients"
  },

  "Application": {
    "Name": "Employee API",
    "Version": "1.0"
  }
}
```

ASP.NET Core loads this configuration automatically in a typical application. Reading configuration using `IConfiguration`.
You can inject it into a class:

```csharp
public class EmployeeService
{
    private readonly IConfiguration _configuration;

    public EmployeeService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
}
```

Then:

```csharp
var value = _configuration["Application:Name"];
```

Result:

```text
Employee API
```

**Nested configuration**

Suppose:

```json
{
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Name": "EmployeeDb"
  }
}
```

You can access:

```csharp
var host = _configuration["Database:Host"];

var port = _configuration["Database:Port"];

var name = _configuration["Database:Name"];
```

Conceptually:

```text
Database
   │
   ├── Host
   ├── Port
   └── Name
```

***`Connection strings`***

A special/common section is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EmployeeDb"
  }
}
```

You can retrieve it using:

```csharp
var connectionString = _configuration.GetConnectionString("DefaultConnection");
```

This is generally preferred over:

```csharp
_configuration["ConnectionStrings:DefaultConnection"]
```

because `GetConnectionString()` clearly communicates what you're retrieving.

### IOptions<T>

> "IConfiguration is useful for reading configuration values, while the Options Pattern lets us bind a related configuration section to a strongly typed class 
and inject it using IOptions<T>, making configuration cleaner, safer, and easier to validate and maintain."

Yes. This is where you should understand **`IConfiguration` → Options Pattern → `IOptions<T>`**.

The easiest way to remember it is:

> `IConfiguration` gives you configuration as strings. `IOptions<T>` converts a configuration section into a strongly typed C# object.

**The 3 lines you should remember**

If you're preparing for interviews, remember this pattern:

**1. JSON**

```json
"Jwt": {
  "Issuer": "MyApi",
  "Audience": "MyClient"
}
```

**2. Class**

```csharp
public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
}
```

**3. Register + inject**

```csharp
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));
```

```csharp
public MyService(IOptions<JwtOptions> options)
{
    var issuer = options.Value.Issuer;
}
```

That's the **core Options Pattern**.

### Final mental model

Think of `IConfiguration` as a **dictionary**:

```text
IConfiguration

"Jwt:Issuer" → "MyApi"
"Jwt:Audience" → "MyClient"
"Jwt:ExpirationMinutes" → "60"
```

Options Pattern transforms that into a **C# object**:

```text
JwtOptions

Issuer             → "MyApi"
Audience           → "MyClient"
ExpirationMinutes  → 60
```

So:

```text
                appsettings.json
                       │
                       ▼
                IConfiguration
                       │
                GetSection("Jwt")
                       │
                       ▼
                 JwtOptions
                       │
                       ▼
                IOptions<JwtOptions>
                       │
                       ▼
                  Your Service
```

### Important things to remember

```text
IConfiguration
      │
      ├── appsettings.json
      ├── appsettings.{Environment}.json
      ├── Environment Variables
      ├── User Secrets
      ├── Command-line arguments
      └── Other providers
```
-----------------------------------
-----------------------------------

## Key Vault

> **"I use Azure Key Vault to keep sensitive configuration outside my code and source control, integrate it with ASP.NET Core configuration, and use Managed Identity so the application can securely access those secrets without storing Azure credentials itself."**

Suppose your `appsettings.json` contains:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=myserver;Database=EmployeeDb;Username=admin;Password=12345"
  }
}
```

This is a **bad practice**.
Why?
Because `appsettings.json` may be committed to Git:

```text
Developer
   ↓
Git
   ↓
Repository
   ↓
Everyone with access can potentially see the password
```

Instead:

```text
appsettings.json
      ↓
Non-sensitive configuration

Azure Key Vault
      ↓
Passwords
API keys
Connection strings
Certificates
Secrets
```

***What does Key Vault solve?***

Think of your application having:

```text
Configuration
│
├── ApplicationName = Employee API       ← okay in appsettings
├── LogLevel = Information               ← okay
│
├── DB Password                          ← SECRET
├── JWT Secret                           ← SECRET
├── Payment API Key                      ← SECRET
└── Third-party credentials              ← SECRET
```

You don't want the secrets inside your source code.

Instead:

```text
                 Azure
                   │
             ┌─────────────┐
             │ Key Vault    │
             │              │
             │ DBPassword   │
             │ JwtSecret    │
             │ ApiKey       │
             └──────┬──────┘
                    │
                    ▼
              ASP.NET Core
```

***Key Vault is NOT your normal database***

This is important. Don't think:

> "I'll store all my application data in Key Vault."

No.
Key Vault is designed primarily for **secrets, keys, and certificates**.

For example:

```text
GOOD:
Database password
API key
JWT signing secret
Certificate
Encryption key
```

Not:

```text
Employee records
Orders
Products
Customer data
```

***How does ASP.NET Core get the secret?***

This is where your previous topic, **`IConfiguration`**, becomes important.
Imagine Key Vault contains:

```text
DatabasePassword = SuperSecret123
```

ASP.NET Core can load Key Vault values as configuration.

Conceptually:

```text
Azure Key Vault
       │
       ▼
Configuration Provider
       │
       ▼
IConfiguration
       │
       ▼
Your Application
```

So your application doesn't need to manually call Key Vault every time it wants a secret.

Then configure it in `Program.cs`.

For example:

```csharp
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
    new Uri("https://myapp-vault.vault.azure.net/"),
    new DefaultAzureCredential());

var app = builder.Build();
```

Now your configuration can include values from Key Vault.

---

***What is `DefaultAzureCredential`?***

This is a **very important interview topic**.

You don't want this:

```csharp
var password = "myAzurePassword";
```

or:

```csharp
var clientSecret = "some-secret";
```

inside your application.

Instead:

```csharp
new DefaultAzureCredential()
```

allows Azure Identity to find an appropriate authentication mechanism.

Conceptually:

```text
ASP.NET Core
     │
     ▼
DefaultAzureCredential
     │
     ├── Local development
     │      ↓
     │   Developer/Azure CLI credentials
     │
     └── Azure deployment
            ↓
        Managed Identity
```

The exact credential selected depends on the environment and available credentials.

---

***Managed Identity***

This is one of the most important things to mention in an interview. Suppose your API is running in:

```text
Azure App Service
```

You don't want to put an Azure username/password or client secret inside your application just to access Key Vault.
Instead, give the application an **Azure Managed Identity**.
Conceptually:

```text
             Azure
               │
       ┌───────┴────────┐
       ▼                ▼
   App Service       Key Vault
       │                │
       │ Managed        │
       │ Identity       │
       └───────►────────┘
              Access
```

Your application gets an identity from Azure.
Then you grant that identity permission to read secrets from Key Vault.

**Why Managed Identity is better**

Without Managed Identity:

```text
Application
    ↓
Client ID
Client Secret
    ↓
Key Vault
```

Now you have another secret to protect.

😐

With Managed Identity:

```text
Application
    ↓
Managed Identity
    ↓
Key Vault
```

No application-stored Azure credential is needed.

So the interview-friendly statement is:

> **"When an application runs in Azure, Managed Identity is generally preferred for authenticating to Key Vault because it eliminates the need to store Azure credentials in the application."**

**Good candidates for key vault**

```text
✓ Database passwords
✓ API keys
✓ JWT signing secrets
✓ OAuth client secrets
✓ Certificates
✓ Encryption keys
✓ Third-party credentials
```

**Usually don't put these there unnecessarily**

```text
✗ Application name
✗ Log level
✗ Feature descriptions
✗ Non-sensitive constants
✗ Normal business data
```

Not every configuration value is a secret.

### The 5 things to remember for interviews

```text
1. Key Vault
   → Secure storage for secrets, keys and certificates.

2. IConfiguration
   → Can consume Key Vault as a configuration source.

3. DefaultAzureCredential
   → Provides Azure authentication without hardcoding credentials.

4. Managed Identity
   → Preferred way for Azure-hosted apps to authenticate to Key Vault.

5. Options Pattern
   → Bind configuration into strongly typed classes.
```
--------------------------
--------------------------


## Startup.cs & Program.cs

- Before .NET 6 (e.g., .NET Core 3.1, .NET 5)
- We had two main files in ASP.NET Core apps:

***`1. Program.cs`***
- The entry point of the application.
- Contains the Main method.
- Creates and runs the Host (which manages app lifecycle, DI, logging, config, etc.).

👉 Example:
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

***`2. Startup.cs`***
- Defines how the application should behave.
- Has two important methods:
    - ConfigureServices(IServiceCollection services) → Register services for DI (DbContext, custom services, Identity, etc.).
    - Configure(IApplicationBuilder app, IWebHostEnvironment env) → Configure HTTP request pipeline (middlewares: routing, authentication, authorization, etc.).

👉 Example:
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddDbContext<AppDbContext>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

#### .NET 6 (Minimal Hosting Model)
- Microsoft simplified the startup process → merged Program.cs and Startup.cs into a single file.
- No separate Startup.cs by default.
- Program.cs uses top-level statements and WebApplication builder.
👉 Example (.NET 6/7/8 style):
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Why did Microsoft move to a single file?**
- Simplicity → Less boilerplate for small apps (microservices, APIs).
- Minimal APIs → With .NET 6+, we can build lightweight REST APIs with very little code.
- Flexibility → You can still separate configuration into Startup.cs if you want (Microsoft didn’t remove it, just made it optional).
- When to use one file vs two files?

**One file (Program.cs only, Minimal Hosting Model)**
- Good for small projects, microservices, APIs, and quick setups.
- Keeps things short and clean.


**Two files (Program.cs + Startup.cs)**
- Good for large enterprise apps.
- Provides separation of concerns:
- Program.cs → app startup/host config.
- Startup.cs → service registrations + middleware pipeline.
👉 Even in .NET 6+, you can still use Startup.cs if you like:

```csharp
var builder = WebApplication.CreateBuilder(args)
builder.Services.AddControllers();

builder.Host.ConfigureAppConfiguration((context, config) =>
{
    // Additional config
});

var app = builder.Build();

app.UseMiddleware<CustomMiddleware>();
app.MapControllers();

app.Run();
```

### Summary
- Startup.cs → Old style, for configuring services + middleware.
- Program.cs → Entry point.
- .NET 6+ → Both merged into one file (minimal hosting model).
- You can still use the old two-file approach if your project needs separation.
-----------------------------
-----------------------------

## MetaPackage 
> A MetaPackage is basically a NuGet package that bundles multiple other NuGet packages together.

- Instead of installing multiple individual packages (like Microsoft.AspNetCore.Mvc, Microsoft.EntityFrameworkCore, etc.), you can just install one MetaPackage (like Microsoft.AspNetCore.App), which references all of them.
- Think of it like a "package of packages."
- History of MetaPackages in .NET Core

**.NET Core 1.x**

Developers had to manually install individual packages.
 Example
 ```
 <PackageReference Include="Microsoft.AspNetCore.Mvc" Version="1.1.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="1.1.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="1.1.0" />
```
This was painful and caused version conflicts.

**.NET Core 2.x – MetaPackages introduced**

Microsoft introduced MetaPackages to simplify dependencies.
Two important ones:
```
Microsoft.AspNetCore.App
Includes ASP.NET Core + Entity Framework Core.
```

Example:
``` <PackageReference Include="Microsoft.AspNetCore.App" />```

No version number needed → it matches the .NET Core runtime version.

Microsoft.NETCore.App

Includes the base .NET Core runtime libraries (like System.*).
Used in every .NET Core project automatically.


**.NET Core 3.x and later (including .NET 5, 6, 7, 8…)**

Microsoft removed MetaPackages from explicit use.
Instead, they introduced the SDK-style project file with implicit references.
 Example:
 ```
 <Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
</Project>
```

When you specify Microsoft.NET.Sdk.Web, you automatically get all the required ASP.NET Core libraries.
So you don’t need to explicitly reference Microsoft.AspNetCore.App anymore.

### Why MetaPackages were useful
- Simplified dependency management.
- Avoided version mismatches between ASP.NET Core libraries.
- Made project files cleaner.

### Why they were removed
To make things even simpler:
> SDK project types (Microsoft.NET.Sdk, Microsoft.NET.Sdk.Web, Microsoft.NET.Sdk.Worker) already know what base packages you need.
So now, you only add extra NuGet packages if you use third-party libraries or optional features.

---------------
---------------

## RESTful API 

> REST (Representational State Transfer) is an architectural style for designing web APIs around resources, HTTP methods, and standard HTTP behavior.

> RESTful API = Resources + HTTP methods + stateless communication + standard HTTP semantics.

### What is a REST API?

Suppose you have an Employee application.

Your main resource is:

```text
Employee
```

A REST API exposes that resource through a URL:

```http
/api/employees
```

Then HTTP methods tell the API what you want to do.

| HTTP Method | Meaning        | Example                    |
| ----------- | -------------- | -------------------------- |
| GET         | Read           | `GET /api/employees`       |
| POST        | Create         | `POST /api/employees`      |
| PUT         | Replace/update | `PUT /api/employees/10`    |
| PATCH       | Partial update | `PATCH /api/employees/10`  |
| DELETE      | Delete         | `DELETE /api/employees/10` |

So REST is heavily based on:

```text
Resource + URL + HTTP Method
```

> **Each request should contain the information necessary for the server to process it; the server should not depend on remembering client session state between requests.**

Example:

```text
Request 1
GET /api/employees/10
Authorization: Bearer <token>

Request 2
GET /api/employees/20
Authorization: Bearer <token>
```

Each request carries the necessary authentication information.

Conceptually:

```text
Request 1 ──► API
              │
              └── Process request

Request 2 ──► API
              │
              └── Process request
```

The API shouldn't need:

```text
"Remember what this client did in request 1."
```

### REST principles

The classic REST constraints are:

**`1. Client-server`**

Client and server have separate responsibilities.

```text
Client
  ↕
API Server
```

**`2. Stateless`**

Each request contains the information necessary to process it.

**`3. Cacheable`**

Responses can indicate whether they may be cached.

**`4. Uniform Interface`**

Resources and interactions should follow consistent conventions.

For example:

```text
GET    /employees
POST   /employees
GET    /employees/10
DELETE /employees/10
```

**`5. Layered System`**

The client doesn't necessarily know whether it is communicating directly with the application server or through:

```text
Client
 ↓
Load Balancer
 ↓
API Gateway
 ↓
API
 ↓
Database
```

**`6. Code on Demand — optional`**

The server can optionally send executable code to the client.

This is the least commonly discussed constraint in typical Web API interviews.


### REST vs SOAP

Another common interview question.

| REST                       | SOAP                                     |
| -------------------------- | ---------------------------------------- |
| Architectural style        | Protocol                                 |
| Usually HTTP               | Can use multiple transports              |
| Commonly JSON              | Commonly XML                             |
| Lightweight                | More specification-heavy                 |
| Resource-oriented          | Operation/service-oriented               |
| Common for modern Web APIs | Common in some enterprise/legacy systems |


1. **REST is an architectural style, not a protocol.**
2. **URL represents a resource.**
3. **HTTP method represents the operation.**
4. **REST APIs are generally stateless.**
5. **Use proper HTTP status codes.**
6. **PUT = replacement/update; PATCH = partial modification.**
7. **Controllers/Minimal APIs are implementation mechanisms; REST is the design style.**

-------------------------------------

## API Management?

> API Management is a platform used to expose, secure, control, monitor, and manage APIs.

Suppose you have your ASP.NET Core API:

```text
Client
   │
   ▼
ASP.NET Core API
```

Without API Management, clients directly call your API.

With API Management:

```text
Client
   │
   ▼
┌──────────────────┐
│   API Management │
│      (APIM)      │
└────────┬─────────┘
         │
         ▼
  ASP.NET Core API
```

APIM becomes a **gateway in front of your APIs**.

**What does APIM actually do?**

Think of APIM as a **security + traffic + management layer**.

It can handle:

```text
                  Azure APIM
                      │
       ┌──────────────┼──────────────┐
       ▼              ▼              ▼
   Security        Traffic        Monitoring
       │              │              │
   API Keys        Rate Limit      Metrics
   JWT             Quotas          Logs
   IP filtering    Throttling
       │
       ▼
   Policies
       │
       ▼
    Backend API
```

**Why do we need it?**

Imagine your company has 20 APIs:

```text
Customer API
Employee API
Payment API
Order API
Product API
Notification API
...
```

Without APIM:

```text
Mobile App ────────► Customer API
Web App ───────────► Employee API
Partner ───────────► Payment API
Third Party ───────► Order API
```

Each API might have to independently handle:

```text
Authentication
Rate limiting
Logging
API keys
Versioning
Security
```

That's repetitive.

With APIM:

```text
                         Azure APIM
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
     Authentication      Rate Limit         Logging
          │                  │                  │
          └──────────────────┼──────────────────┘
                             │
                ┌────────────┼────────────┐
                ▼            ▼            ▼
             Employee     Payment      Order
                API          API         API
```

You get a **central API gateway/management layer**.

------------

### Azure API Management

**Azure API Management (Azure APIM)** is Microsoft's managed API Management service in Azure.

It helps you:

* Publish APIs
* Secure APIs
* Apply policies
* Control traffic
* Monitor API usage
* Version APIs
* Manage subscriptions
* Provide developer documentation/portal capabilities

So:

```text
API Management
       ↓
General concept/category

Azure API Management
       ↓
Microsoft Azure's implementation/service
```

***The most important APIM architecture***

Suppose you have:

```text
ASP.NET Core Web API
https://mycompany.com/api
```

Instead of exposing that directly:

```text
Client
   │
   ▼
APIM
   │
   ▼
ASP.NET Core API
```

The client gets an APIM URL such as:

```text
https://mycompany-apim.azure-api.net/employees
```

APIM then forwards the request to your backend API.

```text
Client
  │
  │ GET /employees
  ▼
Azure APIM
  │
  │ policies
  │ authentication
  │ rate limit
  │ logging
  ▼
ASP.NET Core API
  │
  ▼
Database
```

***APIM Policies***
APIM uses **policies** to modify or control requests and responses.

For example:

```text
Request
   ↓
APIM Policy
   ↓
Backend
   ↓
APIM Policy
   ↓
Response
```

Policies can do things like:

```text
✓ Rate limiting
✓ Quotas
✓ Authentication/authorization checks
✓ IP filtering
✓ Request/response transformation
✓ Header manipulation
✓ Caching
✓ Routing
✓ Logging/telemetry integration
```

***"Why use Azure APIM?"***

> "Azure API Management provides a managed gateway and management layer in front of backend APIs. It allows us to securely expose APIs, apply policies such as rate limiting and authentication, manage subscriptions and API products, support versioning and transformations, and monitor API usage without putting all of these cross-cutting concerns directly into every backend API."


***"Is APIM a replacement for ASP.NET Core?"***

> "No. APIM doesn't replace the backend API. It sits in front of the backend and manages API access and traffic. ASP.NET Core still handles the application's business logic, validation, data access, and domain operations."


***"What is an APIM policy?"***

> "An APIM policy is a set of rules executed during API request or response processing. Policies can implement concerns such as rate limiting, authentication checks, caching, header manipulation, request/response transformation, and traffic control without changing the backend application."

### Azure APIM — Advantages
- Central API Gateway — single entry point for multiple APIs
- Security — authentication, authorization, IP filtering, etc.
- Rate Limiting — prevents API abuse/overload
- Caching — reduces backend calls and improves performance
- API Versioning — manage multiple API versions
- Policies — apply common rules without changing backend code
- Monitoring & Analytics — track API usage, errors, performance
- Subscriptions/API Keys — manage API consumers
- Developer Portal — API documentation and API discovery
- Scalability — managed Azure service

### Azure APIM — Disadvantages
- Cost — can become expensive, especially at scale
- Additional Complexity — another component to configure/manage
- Learning Curve — policies and APIM concepts take time to learn
- Potential Latency — requests pass through an additional gateway layer
- Azure Dependency — strong dependency on Azure ecosystem
- Overkill for Small APIs — may not be necessary for simple internal applications
- Policy Debugging — troubleshooting complex policies can be difficult

------------------------
------------------------


