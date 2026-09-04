## Layered Architecture

> Divide the application into separate layers, where each layer has a specific responsibility.

> Three tier architecture.

Typical structure:

```text
┌─────────────────────────────┐
│     Presentation Layer      │
│       Controllers/API       │
└──────────────┬──────────────┘
               ↓
┌─────────────────────────────┐
│       Business Layer        │
│      Services / Logic       │
└──────────────┬──────────────┘
               ↓
┌─────────────────────────────┐
│        Data Layer           │
│ Repository / EF Core / DB   │
└──────────────┬──────────────┘
               ↓
            Database
```

### Presentation Layer

This is the layer that interacts with the outside world.

In ASP.NET Core:

```text
Controllers
API endpoints
Request/Response models
Authentication/Authorization
```

Example:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetEmployee(int id)
{
    var employee = await _employeeService.GetByIdAsync(id);

    return Ok(employee);
}
```

The controller should primarily handle:

```text
HTTP request
      ↓
Validation / HTTP concerns
      ↓
Call business layer
      ↓
HTTP response
```

**❌ Avoid putting business logic here**

Bad:

```csharp
[HttpPost]
public async Task<IActionResult> Create(Employee employee)
{
    if (employee.Salary > 100000)
    {
        // complicated business logic
    }

    // database logic
    // calculations
    // transactions
    // etc.
}
```

Controllers become huge and difficult to maintain.

### Business Layer

This contains the **application/business rules**.

Usually:

```text
Services
Business logic
Domain rules
Calculations
Orchestration
```

Example:

```csharp
public class EmployeeService
{
    private readonly IEmployeeRepository _repository;

    public async Task<Employee> CreateAsync(Employee employee)
    {
        if (employee.Salary <= 0)
            throw new ArgumentException("Invalid salary");

        // Business rules

        return await _repository.AddAsync(employee);
    }
}
```

Controller:

```csharp
var employee =
    await _employeeService.CreateAsync(request);
```

The controller doesn't need to know how the business rule works.

### Data Layer

This layer handles persistence.

Typical technologies:

```text
EF Core
DbContext
Repositories
SQL
Database
```

Example:

```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
```

The responsibility is:

> **How do I store/retrieve data?**

Not:

> **What should the business do?**


### Complete Request Flow

Suppose the client sends:

```http
GET /api/employees/10
```

Flow:

```text
Client
  ↓
Controller
  ↓
EmployeeService
  ↓
EmployeeRepository
  ↓
EF Core
  ↓
Database
```

Then response travels back:

```text
Database
   ↓
Repository
   ↓
Service
   ↓
Controller
   ↓
HTTP Response
   ↓
Client
```

### Why Do We Use Layers?

**`1. Separation of concerns`**

Each layer has a specific responsibility.

```text
Controller → HTTP
Service    → Business
Repository → Data
```

**`2. Maintainability`**

If database technology changes:

```text
SQL Server
   ↓
PostgreSQL
```

you ideally don't need to rewrite your controllers.

**`3. Testability`**

You can test the service independently.

```csharp
EmployeeService
      ↓
Mock IEmployeeRepository
```

You don't necessarily need a real database for every business-logic test.

**`4. Reusability`**

The same service can potentially be used by:

```text
REST API
Background Worker
Message Consumer
```

**`5. Team Development`**

Different developers/teams can work on different parts with clear boundaries.

**Disadvantages**

**`1. Too much boilerplate`**

A simple operation might become:

```text
Controller
 ↓
Interface
 ↓
Service
 ↓
Interface
 ↓
Repository
 ↓
DbContext
```

For a simple CRUD application, this can be unnecessary.

**`2. Changes can cross many layers`**

A small requirement might require changes in:

```text
Controller
Service
Repository
DTO
Interface
Tests
```

**`3. Can become "anemic"`**

Sometimes developers create classes like:

```text
EmployeeService
EmployeeRepository
EmployeeManager
EmployeeHelper
EmployeeProcessor
EmployeeHandler
```

without meaningful separation.

You end up with:

> **Layers for the sake of layers.**
----------------
----------------

## Clean Architecture / Onion Architecture

> **Clean Architecture keeps business logic independent from frameworks, databases, UI, and external services.**

The most important rule is:

> **Dependencies point inward, toward the business/domain.**

### Why Do We Need Clean Architecture?

Imagine a traditional application:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
EF Core
    ↓
PostgreSQL
```

Looks fine.

But now your business logic starts depending on:

```text
ASP.NET Core
EF Core
PostgreSQL
Redis
Azure
External APIs
```

Then your core business logic becomes tightly coupled to infrastructure.

For example:

```csharp
public class OrderService
{
    private readonly AppDbContext _db;
    private readonly RedisCache _redis;

    // Business logic mixed with infrastructure
}
```

Now changing:

```text
PostgreSQL → SQL Server
Redis → another cache
EF Core → another ORM
```

can affect business logic.

Clean Architecture tries to prevent this.

### The Onion Model

Think of the application as circles:

```text
┌──────────────────────────────────────────┐
│              Presentation               │
│                                          │
│   ┌──────────────────────────────────┐   │
│   │          Infrastructure          │   │
│   │                                  │   │
│   │   ┌──────────────────────────┐   │   │
│   │   │       Application         │   │   │
│   │   │                          │   │   │
│   │   │   ┌──────────────────┐   │   │   │
│   │   │   │      Domain      │   │   │   │
│   │   │   │                  │   │   │   │
│   │   │   │ Business Rules   │   │   │   │
│   │   │   └──────────────────┘   │   │   │
│   │   └──────────────────────────┘   │   │
│   └──────────────────────────────────┘   │
└──────────────────────────────────────────┘
```

The **Domain is at the center**.

Dependencies should point toward it.


### The Four Main Parts

A common Clean Architecture implementation has:

```text
1. Domain
2. Application
3. Infrastructure
4. Presentation
```

Think:

```text
Domain
  ↓
Application
  ↓
Infrastructure
  ↓
Presentation
```

But the **dependency direction** is the important part, not simply the physical order.


**`1. Domain Layer`**

The Domain is the **heart of the application**.

It contains business concepts and business rules.

Examples:

```text
Entities
Value Objects
Domain Services
Domain Events
Business rules
```

Example:

```csharp
public class Order
{
    public int Id { get; private set; }

    public decimal TotalAmount { get; private set; }

    public void ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 50)
            throw new InvalidOperationException(
                "Invalid discount.");

        TotalAmount -=
            TotalAmount * percentage / 100;
    }
}
```

Notice what isn't here:

```text
❌ EF Core
❌ PostgreSQL
❌ ASP.NET Core
❌ Redis
❌ HttpClient
```

That's intentional.

**`2. Application Layer`**

The Application layer contains:

> **Use cases / application workflows.**

For example:

```text
CreateOrder
CancelOrder
GetOrder
UpdateOrder
ProcessPayment
```

Example:

```csharp
public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;

    public async Task<int> HandleAsync(
        CreateOrderCommand command)
    {
        var order = new Order();

        // application workflow

        await _repository.AddAsync(order);

        return order.Id;
    }
}
```

Notice:

```csharp
IOrderRepository
```

is an abstraction.

The Application layer doesn't need to know whether the implementation uses:

```text
EF Core
Dapper
MongoDB
PostgreSQL
```

**`3. Infrastructure Layer`**

Infrastructure contains implementations for external concerns.

Examples:

```text
EF Core
Repositories
Redis
Email
Azure services
External APIs
File storage
Message brokers
```

For example:

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public async Task AddAsync(Order order)
    {
        _db.Orders.Add(order);

        await _db.SaveChangesAsync();
    }
}
```

Here:

```text
IOrderRepository
        ↑
        │
OrderRepository
```

The interface can live in the Application/Domain-facing layer while the implementation lives in Infrastructure.

**`4. Presentation Layer`**

This is your:

```text
ASP.NET Core API
Controllers
Minimal APIs
Request/Response models
Authentication
HTTP concerns
```

Example:

```csharp
[HttpPost]
public async Task<IActionResult> Create(
    CreateOrderRequest request)
{
    var result = await _handler.HandleAsync(
        new CreateOrderCommand(request));

    return Ok(result);
}
```

The controller shouldn't contain complicated business rules.

----
### Dependency Inversion

This is the concept that makes Clean Architecture work.

Suppose Application needs a repository.

Bad:

```csharp
public class OrderService
{
    private readonly OrderRepository _repository;
}
```

Now Application depends on a concrete infrastructure implementation.

Instead:

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
}
```

Define:

```csharp
public interface IOrderRepository
{
    Task AddAsync(Order order);
}
```

Infrastructure implements it:

```csharp
public class OrderRepository : IOrderRepository
{
    // EF Core implementation
}
```

Now:

```text
Application
    ↓
IOrderRepository
    ↑
    │
Infrastructure
    │
OrderRepository
```

The dependency points toward the abstraction.

### Dependency Injection Completes the Picture

In `Program.cs`:

```csharp
builder.Services.AddScoped<IOrderRepository,
                           OrderRepository>();
```

Now the Application layer asks for:

```csharp
IOrderRepository
```

and DI provides:

```text
OrderRepository
```

The Application layer doesn't need to know the concrete implementation.

**Clean Architecture vs Layered Architecture**

This is a very common interview question.

**`Layered`**

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

**`Clean Architecture`**

```text
           Domain
             ↑
        Application
             ↑
      Infrastructure
             ↑
       Presentation
```

The focus is:

> **Dependency direction and protecting the business core.**

---

### Simple Comparison

|                      | Layered                | Clean                |
| -------------------- | ---------------------- | -------------------- |
| Main focus           | Separation by layer    | Dependency direction |
| Domain isolation     | Not necessarily strong | Strong               |
| Database dependency  | Often lower-level      | Kept outside core    |
| Business logic       | Service layer          | Domain/Application   |
| Complexity           | Lower                  | Higher               |
| Small CRUD app       | Often enough           | May be overkill      |
| Large complex system | Can work               | Very useful          |


------------------
------------------

## Microservices Basics

> Microservices architecture divides a large application into small, independently deployable services, where each service owns a specific business capability.

> Microservices decompose an application around business capabilities into independently deployable services. Services communicate through synchronous protocols like REST/gRPC or asynchronous messaging, and each service ideally owns its data. The benefit is independent deployment and scaling, while the major trade-off is increased distributed-system complexity.

For example, instead of one large e-commerce application:

```text
                E-Commerce Application
                       │
        ┌──────────────┼──────────────┐
        ↓              ↓              ↓
     Orders         Payments        Users
        │              │              │
     Products       Shipping       Notifications
```

Each can become an independently deployable service.


### Monolith vs Microservices

**`Monolith`**

Everything is inside one application:

```text
┌───────────────────────────────────────┐
│             E-Commerce API            │
│                                       │
│ Users | Orders | Payments | Products │
│                                       │
└──────────────────┬────────────────────┘
                   ↓
               Database
```

You deploy the whole application together.

**`Microservices`**

```text
┌──────────┐    ┌──────────┐    ┌──────────┐
│ User     │    │ Order    │    │ Payment  │
│ Service  │    │ Service  │    │ Service  │
└────┬─────┘    └────┬─────┘    └────┬─────┘
     │               │               │
     ↓               ↓               ↓
   DB/User         DB/Order       DB/Payment
```

Each service can potentially be:

* developed independently
* deployed independently
* scaled independently
* owned by a separate team


**What Makes Something a Microservice?**

Don't define it simply as:

> "A small API."

That's incomplete.

A better definition:

> **A microservice is an independently deployable service focused on a specific business capability, with clear boundaries and usually independent ownership of its data.**

For example:

```text
Order Service
     ↓
Responsible for
orders, order lifecycle, order rules
```

Not:

```text
OrderService
     ↓
100 unrelated responsibilities ❌
```

**How Do Services Communicate?**

There are two major approaches.

```text
Service A
    │
    ├── Synchronous
    │
    └── Asynchronous
```

---

**Synchronous Communication**

Service A directly calls Service B.

For example:

```text
Order Service
      │
      │ HTTP/gRPC
      ↓
Payment Service
```

Example:

```http
POST /api/payments
```

Order Service waits for the response.

```text
Order Service
      │
      │ Request
      ↓
Payment Service
      │
      │ Response
      ↓
Order Service
```

Common technologies:

```text
HTTP REST
gRPC
```

**Advantages of Synchronous Communication**

* Simple to understand
* Immediate response
* Easy request/response flow
* Good for operations where caller needs an immediate result

Example:

```text
Get customer details
        ↓
User Service
        ↓
Response
```

**Disadvantages**

Service A becomes dependent on Service B being available.

```text
Order Service
      ↓
Payment Service ❌ DOWN
      ↓
Order request fails/waits
```

You can also get:

```text
Network latency
Timeouts
Retries
Cascading failures
```

**Asynchronous Communication**

Instead of directly calling another service, publish a message/event.

```text
Order Service
      ↓
Message Broker
      ↓
Payment Service
```

For example:

```text
OrderCreated
```

The Order Service publishes:

```text
OrderCreated Event
```

Payment Service consumes it.

```text
Order Service
     ↓
"OrderCreated"
     ↓
Message Broker
     ↓
Payment Service
```

The producer doesn't need to wait for the consumer to finish.

**Message Brokers**

Common examples:

```text
RabbitMQ
Azure Service Bus
Kafka
```

Conceptually:

```text
Producer
   ↓
Message Broker
   ↓
Consumer
```

This connects directly with the **Channels** topic we just covered, but don't confuse them.

```text
Channel<T>
→ In-process communication

RabbitMQ / Azure Service Bus
→ Distributed communication between services/processes
```

**Why Use Asynchronous Communication?**

Suppose:

```text
Order created
```

You need to:

```text
Send email
Update analytics
Notify shipping
Update inventory
```

Instead of:

```text
Order
 ↓
Email Service
 ↓
Analytics Service
 ↓
Shipping Service
 ↓
Inventory Service
```

which creates many synchronous dependencies:

```text
Order Service
     │
     ├──→ Email
     ├──→ Analytics
     ├──→ Shipping
     └──→ Inventory
```

you can publish:

```text
OrderCreated
      ↓
Message Broker
      ↓
 ┌────┼────┬──────┐
 ↓    ↓    ↓      ↓
Email Analytics Shipping Inventory
```

Much more decoupled.

### API Gateway

This is another major interview topic.

Suppose your frontend needs:

```text
Users
Orders
Payments
Products
```

Without an API Gateway:

```text
Frontend
   ├──→ User Service
   ├──→ Order Service
   ├──→ Payment Service
   └──→ Product Service
```

The client needs to know about many services.

With an API Gateway:

```text
                 Frontend
                    ↓
              API Gateway
             ┌──────┼──────┐
             ↓      ↓      ↓
           Users  Orders Payments
```

The client communicates with one entry point.

**What Does an API Gateway Do?**

Common responsibilities:

```text
Routing
Authentication
Authorization
Rate limiting
Load balancing
Request aggregation
Logging
Monitoring
Transformation
```

For example:

```text
GET /api/orders
        ↓
API Gateway
        ↓
Order Service
```

**API Gateway Is Not the Same as Load Balancer**

Important interview distinction.

**`Load Balancer`**

Primarily distributes traffic:

```text
Client
  ↓
Load Balancer
  ├── Server 1
  ├── Server 2
  └── Server 3
```

**`API Gateway`**

Provides API-level capabilities:

```text
Client
  ↓
API Gateway
  ├── Authentication
  ├── Routing
  ├── Rate limiting
  ├── Aggregation
  └── Service calls
```

A gateway may use/load-balance traffic, but the concepts are not identical.

### Client-Side vs Server-Side Discovery

**`Client-side`**

The calling service queries the registry.

```text
Order Service
      ↓
Service Registry
      ↓
Payment instance
```

**`Server-side`**

The caller sends the request to infrastructure that performs discovery/routing.

```text
Order Service
      ↓
Load Balancer / Gateway
      ↓
Payment Service instance
```

The exact implementation depends on the platform.

**`Database Per Service`**

This is a very important microservices principle.

Ideally:

```text
User Service
    ↓
User DB

Order Service
    ↓
Order DB

Payment Service
    ↓
Payment DB
```

rather than:

```text
User Service ─┐
Order Service ├──→ One shared DB
Payment ──────┘
```

Why?

Because each service should own its data and persistence decisions.

**Why Shared Database Can Be a Problem**

Suppose:

```text
Order Service
      ↓
Shared DB
      ↑
Payment Service
```

Now Payment Service directly depends on Order Service's tables.

You lose independence.

For example:

```text
Order Service changes schema
       ↓
Payment Service breaks
```

That creates tight coupling.

**How Does One Service Get Another Service's Data?**

Instead of directly querying another service's database:

```text
Order Service
     ↓
Payment DB ❌
```

use:

```text
Order Service
     ↓
Payment Service API
```

or asynchronous events:

```text
Payment Service
     ↓
PaymentCompleted
     ↓
Message Broker
     ↓
Order Service
```

### Distributed Transactions

This becomes difficult in microservices.

Suppose:

```text
Order
 ↓
Payment
 ↓
Inventory
```

In a monolith you might have:

```text
BEGIN TRANSACTION
   Order
   Payment
   Inventory
COMMIT
```

With separate databases:

```text
Order DB
Payment DB
Inventory DB
```

### Microservices and Failure

In a monolith:

```text
Application
```

may fail as one unit.

In microservices:

```text
Service A
Service B
Service C
Service D
```

can fail independently.

So you need:

```text
Timeout
Retry
Circuit Breaker
Fallback
Bulkhead
Idempotency
```

For example:

```text
Order Service
      ↓
Payment Service
      ↓
Timeout
      ↓
Retry?
      ↓
Circuit Breaker?
```

These are critical distributed-system concepts.

### Circuit Breaker — Basic Idea

Suppose Payment Service is down.

Without circuit breaker:

```text
Order → Payment ❌
Order → Payment ❌
Order → Payment ❌
Order → Payment ❌
...
```

You're continuously sending requests to a failing service.

Circuit breaker:

```text
Normal
  ↓
Failures increase
  ↓
Circuit OPEN
  ↓
Stop calling Payment temporarily
```

Later:

```text
Circuit HALF-OPEN
       ↓
Test request
       ↓
Success
       ↓
Circuit CLOSED
```

### Retries Need Care

Don't blindly retry everything.

For example:

```text
POST /payment
```

If the payment succeeded but your response was lost:

```text
Client doesn't know payment succeeded
       ↓
Retry POST
       ↓
Payment may happen twice ❌
```

That's why:

> **Idempotency**

is extremely important in distributed systems.

### What Is Idempotency?

An operation is idempotent if performing it multiple times has the same intended effect as performing it once.

For example:

```text
PUT /users/10
```

setting:

```text
Name = Swapnil
```

multiple times should result in the same state.

For payments, you might use:

```text
Idempotency-Key
```

to ensure the same payment request isn't processed twice.

---

### Microservices Advantages

**`Independent deployment`**

```text
Payment Service
```

can be deployed without deploying everything else.

**`Independent scaling`**

If payments receive heavy traffic:

```text
Payment Service
→ 10 instances
```

while:

```text
User Service
→ 2 instances
```

**`Team autonomy`**

Different teams can own different services.

**`Fault isolation`**

One service can fail without necessarily taking everything down.

**`Technology flexibility`**

Different services can potentially use different technologies where justified.

### Microservices Disadvantages

*`Complexity`*

Instead of:

```text
1 application
1 deployment
1 database
```

you may have:

```text
20 services
20 deployments
multiple databases
message brokers
API gateway
monitoring
service discovery
```

**`Network failures`**

Calls now cross the network:

```text
Service A
   ↓
Network
   ↓
Service B
```

Network calls can:

```text
Timeout
Fail
Become slow
Return partial results
```

**`Distributed transactions`**

Much harder than transactions inside one database.

 **`Debugging`**

A single request could travel through:

```text
Gateway
 ↓
Order
 ↓
Payment
 ↓
Inventory
 ↓
Message Broker
 ↓
Notification
```

Tracing becomes important.

**`Operational overhead`**

You need:

```text
Monitoring
Logging
Tracing
Deployment
Service discovery
Secrets
Configuration
Alerting
```


### Should Every Application Use Microservices?

Absolutely not.

This is one of the most important interview answers.

> **"Microservices aren't automatically better than a monolith."**

For a small application:

```text
10 endpoints
3 developers
Simple business logic
```

a modular monolith may be much better.

Microservices make more sense when you have requirements such as:

```text
Independent scaling
Independent deployment
Large teams
Clear business boundaries
Different availability/scaling requirements
Organizational ownership
```

> "Design an e-commerce system using microservices."

A good high-level answer:

```text
                         Clients
                            ↓
                       API Gateway
                            │
          ┌─────────────────┼─────────────────┐
          ↓                 ↓                 ↓
      User Service      Order Service    Product Service
          │                 │                 │
        User DB          Order DB         Product DB
                            │
                            ↓
                     Message Broker
                     ┌──────┼──────┐
                     ↓      ↓      ↓
                 Payment Inventory Notification
                   Service  Service    Service
```

Then discuss:

```text
Authentication
Service communication
Retries/timeouts
Circuit breaker
Idempotency
Observability
Database ownership
Caching
Scaling
Failure handling
```

### Final Mental Model

```text
                    CLIENT
                       ↓
                 API GATEWAY
                       ↓
       ┌───────────────┼───────────────┐
       ↓               ↓               ↓
   USER SERVICE   ORDER SERVICE   PAYMENT SERVICE
       │               │               │
     User DB         Order DB       Payment DB
                       │
                       ↓
                MESSAGE BROKER
                 ┌─────┼─────┐
                 ↓     ↓     ↓
             Inventory Email Analytics
```

### Remember these 7 points:

```text
1. Microservice = business capability

2. Independently deployable

3. Ideally owns its data

4. REST/gRPC → synchronous communication

5. Message broker → asynchronous communication

6. API Gateway → client entry point + cross-cutting concerns

7. Microservices solve organizational/scaling problems
   but introduce distributed-system complexity
```
--------------
--------------

## CQRS Pattern

CQRS is a very common **architecture/system-design interview topic**.

The easiest way to remember it:

> **CQRS = Command Query Responsibility Segregation**

It means:

> **Separate operations that change data from operations that only read data.**

> **CQRS separates commands that modify state from queries that retrieve state. It allows the read and write sides to evolve and scale independently, and the read side can use optimized models when needed. CQRS doesn't inherently require separate databases, microservices, MediatR, or event sourcing. Its main trade-off is increased complexity, so I wouldn't use it for a simple CRUD application.**

**`First Understand Normal CRUD`**

In a normal application, you might have:

```text
EmployeeService
     │
     ├── GetEmployee()
     ├── GetEmployees()
     ├── CreateEmployee()
     ├── UpdateEmployee()
     └── DeleteEmployee()
```

Everything goes through the same service/data model.

This is perfectly fine for many applications.

**`CQRS Splits Read and Write`**

Instead:

```text
                 Application
                     │
            ┌────────┴────────┐
            ↓                 ↓
         COMMAND             QUERY
            │                 │
         Changes             Reads
          data                data
```

**Command**

> Changes state.

Examples:

```text
CreateOrder
UpdateOrder
CancelOrder
ProcessPayment
```

**Query**

> Only retrieves data.

Examples:

```text
GetOrder
GetOrders
GetCustomer
SearchProducts
```

**The Most Important Rule**

Remember:

```text
Command
→ Changes state
→ Does NOT return data as its primary purpose

Query
→ Reads state
→ Does NOT change state
```

This is the heart of CQRS.

### Simple Example

**`Command`**

```csharp
public record CreateOrderCommand(
    int CustomerId,
    decimal Amount);
```

Handler:

```csharp
public class CreateOrderHandler
{
    public async Task HandleAsync(
        CreateOrderCommand command)
    {
        var order = new Order
        {
            CustomerId = command.CustomerId,
            Amount = command.Amount
        };

        await _repository.AddAsync(order);
    }
}
```

This changes the system.

Therefore:

```text
CreateOrderCommand
        ↓
      WRITE
```

**`Query`**

```csharp
public record GetOrderQuery(int OrderId);
```

Handler:

```csharp
public class GetOrderHandler
{
    public async Task<OrderDto?> HandleAsync(
        GetOrderQuery query)
    {
        return await _db.Orders
            .Where(x => x.Id == query.OrderId)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                Amount = x.Amount
            })
            .FirstOrDefaultAsync();
    }
}
```

This only reads.

```text
GetOrderQuery
      ↓
     READ
```

**Why Separate Them?**

Because **read requirements and write requirements are often very different**.

Imagine an e-commerce system.

### CRUD vs CQRS

**`CRUD`**

```text
             Service
                │
       ┌────────┴────────┐
       ↓                 ↓
     Read              Write
       │                 │
       └────────┬────────┘
                ↓
             Database
```

**`CQRS`**

```text
              API
               │
        ┌──────┴──────┐
        ↓             ↓
     Commands       Queries
        ↓             ↓
    Write Model    Read Model
        ↓             ↓
    Write DB       Read DB
```

The second architecture gives you more flexibility, but also more complexity.


### CQRS in ASP.NET Core

A simple structure:

```text
Orders
│
├── Commands
│   ├── CreateOrderCommand.cs
│   └── CreateOrderHandler.cs
│
├── Queries
│   ├── GetOrderQuery.cs
│   └── GetOrderHandler.cs
│
└── DTOs
```

Controller:

```csharp
[HttpPost]
public async Task<IActionResult> Create(
    CreateOrderCommand command)
{
    await _handler.HandleAsync(command);

    return Ok();
}
```

Query:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    var result = await _queryHandler
        .HandleAsync(new GetOrderQuery(id));

    return Ok(result);
}
```

---

### Advantages

**`Separation`**

Reads and writes have clear responsibilities.

**`Independent optimization`**

You can optimize query and command sides differently.

**`Independent scaling`**

Especially when using separate read infrastructure.

**`Better organization for complex use cases`**

Each command/query represents a specific operation.

**`Fits event-driven systems`**

CQRS works well with:

```text
Events
Message brokers
Event-driven architecture
```

---

### Disadvantages

**`Complexity`**

More classes:

```text
Command
Handler
Query
Handler
DTO
Mapper
```

**`Eventual consistency`**

If separate read models/databases are used.

**`More infrastructure`**

Potentially:

```text
Message broker
Read database
Event processing
```

**`Debugging becomes harder`**

Especially when events are involved.

**`Not needed for simple CRUD`**

This is a major point.

-----------------
-----------------

## Repository & Unit of Work Pattern

This is an important .NET interview topic, especially because **EF Core already implements some Repository and Unit of Work concepts**.

The first thing to understand:

> Repository = abstraction around data access

> Unit of Work = coordinate multiple changes and commit them as one unit


### Repository Pattern

Suppose your application directly uses EF Core everywhere:

```csharp
var employee = await _db.Employees
    .FirstOrDefaultAsync(x => x.Id == id);
```

The Repository pattern introduces an abstraction:

```text
Service
   ↓
IEmployeeRepository
   ↓
EmployeeRepository
   ↓
EF Core
   ↓
Database
```

Example:

```csharp
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task AddAsync(Employee employee);
    Task DeleteAsync(Employee employee);
}
```

Implementation:

```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _db.Employees
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Employee employee)
    {
        await _db.Employees.AddAsync(employee);
    }

    public Task DeleteAsync(Employee employee)
    {
        _db.Employees.Remove(employee);
        return Task.CompletedTask;
    }
}
```

**Why Use Repository?**

The main idea is:

> The business/application layer shouldn't need to know how data is persisted.

Instead of:

```csharp
_db.Employees
```

your service depends on:

```csharp
IEmployeeRepository
```

This gives you:

**`Abstraction`**

Business code doesn't depend directly on EF Core.

**`Testability`**

You can mock:

```csharp
IEmployeeRepository
```

**Centralized data-access logic**

For example:

```csharp
GetActiveEmployeesAsync()
GetEmployeesByDepartmentAsync()
```

can be kept in the repository.

-----------

### Generic Repository

You may see:

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task DeleteAsync(T entity);
}
```

Then:

```csharp
public class Repository<T> : IRepository<T>
{
    private readonly AppDbContext _db;

    public Repository(AppDbContext db)
    {
        _db = db;
    }
}
```

This is called a:

> **Generic Repository**

### Why Generic Repository Can Be Problematic

EF Core already provides powerful querying:

```csharp
_db.Employees
    .Where(...)
    .Include(...)
    .Select(...)
    .OrderBy(...)
    .AsNoTracking()
```

If your generic repository exposes only:

```csharp
GetById()
Add()
Delete()
```

you may lose useful EF Core functionality or end up rebuilding EF Core badly.

You might eventually create:

```csharp
GetById()
GetAll()
Find()
FindWithInclude()
FindWithFilter()
FindWithSort()
FindPaged()
FindWithProjection()
...
```

Congratulations:

> **You have started rebuilding EF Core.** 😄

---

### Unit of Work

Now suppose you need to update:

```text
Order
Inventory
Payment
```

and all changes should be committed together.

Conceptually:

```text
              Unit of Work
                   │
        ┌──────────┼──────────┐
        ↓          ↓          ↓
      Order     Inventory   Payment
        │          │          │
        └──────────┼──────────┘
                   ↓
                Commit
```

If everything succeeds:

```text
COMMIT
```

If something fails:

```text
ROLLBACK
```

**EF Core Already Gives You Unit of Work**

This is the key point:

```csharp
_db.Orders.Add(order);
_db.Inventory.Update(inventory);
_db.Payment.Update(payment);

await _db.SaveChangesAsync();
```

`SaveChangesAsync()` commits the tracked changes as one unit.

So:

```text
DbContext
    ↓
Tracks changes
    ↓
SaveChangesAsync()
    ↓
Commit
```

That's why:

> **You usually don't need to create a separate UnitOfWork wrapper around DbContext just for the sake of the pattern.**

**Example Without Custom Unit of Work***

```csharp
public async Task CreateOrderAsync(Order order)
{
    _db.Orders.Add(order);

    var inventory = await _db.Inventory
        .FirstAsync(x => x.ProductId == order.ProductId);

    inventory.Quantity -= order.Quantity;

    await _db.SaveChangesAsync();
}
```

Both changes are part of the same `DbContext` unit of work.

### Custom Unit of Work

You might see:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

Implementation:

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
```

But notice what happened:

```text
IUnitOfWork
     ↓
DbContext
     ↓
SaveChangesAsync()
```

You've created a wrapper around something EF Core already provides.

So ask:

> **What additional value does this abstraction provide?**

### Repository + Unit of Work Together

A traditional architecture might look like:

```text
Controller
    ↓
Service
    ↓
Repositories
    ↓
UnitOfWork
    ↓
Database
```

Example:

```csharp
await _employeeRepository.AddAsync(employee);
await _departmentRepository.UpdateAsync(department);

await _unitOfWork.SaveChangesAsync();
```

Conceptually:

```text
EmployeeRepository ─┐
                    │
DepartmentRepository├──→ UnitOfWork → Commit
                    │
OtherRepository ────┘
```

---

### Unit of Work Advantages

* Coordinates multiple repository changes
* Provides a clear commit boundary
* Helps coordinate transactions
* Can hide persistence implementation

### Unit of Work Disadvantages

If you're using EF Core:

```text
DbContext
```

already does much of this.

So a custom UoW can become:

```text
IUnitOfWork
    ↓
UnitOfWork
    ↓
DbContext
```

with no meaningful benefit.

---------------
---------------

## Domain-Driven Design (DDD) Basics

DDD is less about a specific framework and more about:

> **Designing software around the business domain and its rules.**

For interviews, focus on these concepts:

```text
Entity
Value Object
Aggregate
Aggregate Root
Domain Service
Domain Event
Bounded Context
```

We'll do them one by one.

**Entity**

An **Entity** is an object that has a **unique identity** that remains important even if its other properties change.

Example:

```csharp
public class Customer
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }
}
```

Suppose:

```text
Customer A
Id = 101
Name = Swapnil
```

Later:

```text
Id = 101
Name = Swapnil Patil
```

It's still the **same customer** because:

```text
Identity = 101
```

---

**Entity vs Value Object**

Think about a physical company employee.

Today:

```text
Employee ID = 1001
Name = Swapnil
Salary = 100,000
```

Tomorrow:

```text
Employee ID = 1001
Name = Swapnil
Salary = 120,000
```

It's still the same employee.

Therefore:

> **Identity matters more than attributes.**

---

### Entity Example

```csharp
public class Order
{
    public Guid Id { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal Total { get; private set; }
}
```

Even if:

```text
Status changes
Total changes
```

the order remains the same order because:

```text
Order.Id
```

identifies it.

---

### Value Object

A **Value Object** doesn't have its own meaningful identity.

Its identity is based on its **value**.

Classic examples:

```text
Money
Address
EmailAddress
DateRange
Coordinates
```

For example:

```csharp
public record Money(
    decimal Amount,
    string Currency);
```

Now:

```csharp
var money1 = new Money(100, "INR");
var money2 = new Money(100, "INR");
```

Conceptually:

```text
money1 == money2
```

because their values are the same.

We don't care about:

```text
Money.Id
```

because there isn't one.

---

### Entity vs Value Object

This is one of the most common interview questions.

| Entity                                      | Value Object            |
| ------------------------------------------- | ----------------------- |
| Has identity                                | No independent identity |
| Identity matters                            | Value matters           |
| Usually mutable through controlled behavior | Usually immutable       |
| Example: Customer                           | Example: Money          |
| Example: Order                              | Example: Address        |

Think:

```text
Entity
→ "WHO is this?"

Value Object
→ "WHAT value is this?"
```

---

### Domain Event

A Domain Event represents:

> **Something important that happened in the domain.**

Examples:

```text
OrderCreated
OrderConfirmed
PaymentCompleted
OrderCancelled
CustomerRegistered
```

Example:

```csharp
public record OrderConfirmed(Guid OrderId);
```

When Order is confirmed:

```text
Order.Confirm()
      ↓
OrderConfirmed event
```

Other parts of the system can react.

### Why Domain Events?

Suppose:

```text
Order confirmed
```

You need:

```text
Send email
Update analytics
Notify shipping
```

Instead of putting everything inside:

```csharp
Order.Confirm()
```

you can publish:

```text
OrderConfirmed
       ↓
 ┌─────┼─────┐
 ↓     ↓     ↓
Email Shipping Analytics
```

This reduces coupling.

---

### Domain Event vs Integration Event

Important distinction.

### Domain Event

Usually represents something important **inside the domain/application boundary**.

```text
OrderConfirmed
```

### Integration Event

Used to communicate between separate systems/services.

```text
OrderConfirmedIntegrationEvent
       ↓
Message Broker
       ↓
Shipping Service
```

So:

```text
Domain Event
→ Internal domain concern

Integration Event
→ Cross-service communication
```

---

### DDD Concepts Together

Let's put everything together.

```text
                BOUNDED CONTEXT
                       │
          ┌────────────┴────────────┐
          │                         │
      Order Aggregate          Customer Aggregate
          │                         │
      Order (Root)             Customer (Root)
          │
      ┌───┴────┐
      ↓        ↓
 OrderItem   OrderItem

Value Objects:
Money
Address
Email

Domain Services:
Business logic spanning entities

Domain Events:
OrderConfirmed
OrderCancelled
```

---


--------------
------------
## Event-Driven Architecture & Messaging

This topic connects directly with what we already covered:

```text
Microservices
     ↓
CQRS
     ↓
Domain Events
     ↓
Channels
     ↓
Message Brokers
```

The simplest definition:

> **Event-Driven Architecture (EDA) is an architecture where components communicate by producing and consuming events representing things that have happened.**

### What Is an Event?

An event represents:

> **Something that already happened.**

Examples:

```text
OrderCreated
PaymentCompleted
OrderShipped
UserRegistered
InvoiceGenerated
```

Notice the naming:

```text
OrderCreated
```

means:

> "The order has been created."

It is not asking someone to do something.

### Event vs Command

This is a very common interview question.

*`Command`*

A command says:

> **"Please do this."**

Examples:

```text
CreateOrder
ProcessPayment
CancelOrder
SendEmail
```

It represents an instruction.

*`Event`*

An event says:

> **"This already happened."**

Examples:

```text
OrderCreated
PaymentProcessed
OrderCancelled
EmailSent
```

Think:

```text
COMMAND
"Do this!"
   ↓
Service processes it
   ↓
EVENT
"This happened."
```

---

### Why Events?

Without events:

```text
Order Service
    ↓
Email Service
    ↓
Inventory Service
    ↓
Analytics Service
```

This creates direct dependencies.

With events:

```text
                Order Service
                     ↓
                OrderCreated
                     ↓
               Message Broker
              /      |       \
             ↓       ↓        ↓
          Email   Inventory  Analytics
```

The Order Service doesn't need to know all consumers.

That's:

> **Loose coupling**


### Publisher and Consumer

Two important terms.

#### Publisher / Producer

Creates/publishes the event.

```text
Order Service
     ↓
OrderCreated
```

#### Consumer / Subscriber

Consumes the event.

```text
OrderCreated
     ↓
Notification Service
```

So:

```text
Publisher
   ↓
Message Broker
   ↓
Consumer
```

------------------
------------------

## Message Broker

A message broker sits between producers and consumers.

Examples:

```text
RabbitMQ
Azure Service Bus
Kafka
```

Conceptually:

```text
Producer
   ↓
┌──────────────────┐
│ Message Broker   │
└──────────────────┘
   ↓
Consumer
```

The broker provides capabilities such as:

```text
Message delivery
Buffering
Retries
Acknowledgement
Routing
Persistence
Dead-letter handling
```

The exact capabilities vary by technology.


### Why Not Direct HTTP?

Suppose:

```text
Order Service
      ↓ HTTP
Email Service
```

If Email Service is down:

```text
Order Service
      ↓
Email Service ❌
```

The Order operation may be affected depending on how the call is designed.

With asynchronous messaging:

```text
Order Service
      ↓
Message Broker
      ↓
Email Service
```

If Email Service is temporarily unavailable:

```text
Message Broker
      ↓
Message waits
      ↓
Email Service comes back
      ↓
Process message
```

This provides **decoupling and buffering**.

### Queue vs Topic

This distinction is important.

**`Queue`**

Typically:

```text
Producer
   ↓
Queue
   ↓
Consumer
```

A message is generally processed by **one consumer/consumer instance** from that queue.

Example:

```text
OrderCreated
     ↓
Email Queue
     ↓
Email Worker
```

If you have multiple worker instances:

```text
              Queue
          ┌─────┼─────┐
          ↓     ↓     ↓
       Worker Worker Worker
```

they can share the workload.


**`Topic / Publish-Subscribe`**

With pub/sub:

```text
                 Event
                   ↓
                 Topic
            ┌──────┼──────┐
            ↓      ↓      ↓
         Email  Inventory Analytics
```

Each subscriber can receive the event independently.

This is useful when:

> **Multiple independent consumers need to react to the same event.**

---

### Event-Driven Example

Imagine:

```text
Customer places order
```

Order Service:

```text
Create order
     ↓
Save order
     ↓
Publish OrderCreated
```

Broker:

```text
             OrderCreated
                   ↓
                Topic
          ┌────────┼─────────┐
          ↓        ↓         ↓
      Inventory  Payment   Notification
```

Inventory:

```text
Reserve stock
```

Payment:

```text
Process payment
```

Notification:

```text
Send confirmation
```

Now the services are much less tightly coupled.

---

### Delivery Guarantees

Interviewers love this topic.

Common concepts:

```text
At-most-once
At-least-once
Exactly-once
```

*`At-Most-Once`*

> **Message is delivered zero or one time.**

Potentially:

```text
Message
  ↓
Consumer
  ↓
Processing fails
  ↓
Message lost
```

Advantage:

```text
No duplicates
```

Disadvantage:

```text
Messages can be lost
```

*`At-Least-Once`*

> **The system attempts to ensure the message is processed at least once, so duplicates are possible.**

Example:

```text
Broker
  ↓
Consumer
  ↓
Process message
  ↓
Crash before acknowledgement
  ↓
Broker retries
  ↓
Same message again
```

Now:

```text
Message processed twice
```

This is why consumers should often be:

> **Idempotent**

*`Exactly-Once`*

> "The message will be processed exactly once."

But distributed systems make this difficult.

You should be careful saying:

> "Kafka/RabbitMQ guarantees exactly once end-to-end."

That's generally too simplistic.

There can be multiple stages:

```text
Receive
 ↓
Process
 ↓
Database
 ↓
External API
 ↓
Acknowledge
```

A failure can occur between any of them.

A better interview answer:

> **"Exactly-once processing semantics are difficult to guarantee end-to-end in distributed systems. Systems may provide exactly-once guarantees within specific boundaries, but application-level idempotency is still important."**

-----------

### Retry

Suppose:

```text
Payment Service
```

temporarily fails.

You might retry:

```text
Attempt 1 → fail
Attempt 2 → fail
Attempt 3 → success
```

A common strategy is:

> **Exponential backoff**

Example:

```text
1 sec
2 sec
4 sec
8 sec
```

instead of:

```text
1 sec
1 sec
1 sec
1 sec
```

This reduces pressure on an already struggling dependency.

---

### Event-Driven vs Synchronous Architecture

*`Synchronous`*

```text
A
↓
HTTP
↓
B
↓
Response
```

A waits for B.

*`Asynchronous`*

```text
A
↓
Event
↓
Broker
↓
B
```

A doesn't need to wait for B to finish.

---

### When Should You Use Messaging?

*`Background processing`*

```text
API
 ↓
Queue
 ↓
Worker
```

*`Notifications`*

```text
OrderCreated
 ↓
Email
SMS
Push
```

*`Integration between services`*

```text
Service A
 ↓
Event
 ↓
Service B
```

*`High-volume workloads`*

```text
10000 requests
      ↓
Queue
      ↓
Workers process at controlled rate
```

*`Decoupling`*

When producers and consumers should evolve independently.

---

### RabbitMQ vs Kafka


#### RabbitMQ

Strong fit for:

```text
Message queuing
Work distribution
Routing
Commands/tasks
Traditional broker patterns
```

#### Kafka

Strong fit for:

```text
High-throughput event streaming
Event logs
Large-scale event pipelines
Analytics
Stream processing
```

Conceptually:

```text
RabbitMQ
→ "Deliver this work/message."

Kafka
→ "Store and stream this event history."
```

That's simplified, but useful for interviews.

---

### Azure Service Bus

Since you're preparing for .NET/Azure interviews, know this.

Azure Service Bus provides messaging capabilities such as:

```text
Queues
Topics
Subscriptions
Dead-lettering
Retries
Message scheduling
```

Conceptually:

```text
Producer
   ↓
Azure Service Bus
   ↓
Queue / Topic
   ↓
Consumer
```

It's commonly used for reliable asynchronous communication in Azure-based .NET systems.

### Channel vs Message Broker

We covered `Channel<T>` earlier.

Don't confuse them.

### Channel

```text
Application Process
      │
      ↓
Channel<T>
      ↓
Worker
```

It's **in-process**.

### Message Broker

```text
Service A
     ↓
Network
     ↓
RabbitMQ / Azure Service Bus / Kafka
     ↓
Service B
```

It's designed for **distributed communication**.

---

### Domain Event → Integration Event

This connects with DDD.

Inside Order domain:

```text
OrderConfirmed
```

could be a Domain Event.

Then application/infrastructure translates it into:

```text
OrderConfirmedIntegrationEvent
```

and publishes it:

```text
Domain
 ↓
Domain Event
 ↓
Application/Infrastructure
 ↓
Integration Event
 ↓
Message Broker
 ↓
Other Service
```

This is a very useful architecture pattern.

---

### Outbox Pattern

This is a **very important advanced interview topic**.

Consider:

```text
Save Order
   ↓
Publish Event
```

What if:

```text
Order saved successfully
      ↓
Application crashes
      ↓
Event NOT published
```

Now:

```text
Order DB = updated
Message Broker = no event
```

Your system becomes inconsistent.

---

### Event-Driven Architecture — Full Example

Let's build an e-commerce flow:

```text
                    Client
                       ↓
                  API Gateway
                       ↓
                 Order Service
                       ↓
                 Order Database
                       ↓
                 Outbox Table
                       ↓
                Message Broker
                       ↓
            OrderCreated Event
          ┌────────────┼────────────┐
          ↓            ↓            ↓
     Inventory       Payment     Notification
      Service        Service        Service
          ↓            ↓            ↓
       Inventory      Payment       Email
          DB            DB
```

If Payment succeeds:

```text
PaymentCompleted
       ↓
Broker
       ↓
Order Service
```

If Inventory fails:

```text
InventoryReservationFailed
       ↓
Broker
       ↓
Order Service
       ↓
Cancel / compensate order
```

Now we're entering:

> **Saga / distributed workflow**

which we'll cover separately.

------
-----

## API Gateway & BFF


> **API Gateway = a single entry point between clients and backend services.**

---

### Without API Gateway

Imagine you have:

```text
                    Frontend
                       │
        ┌──────────────┼──────────────┐
        ↓              ↓              ↓
   User Service   Order Service   Payment Service
        │              │              │
      User DB        Order DB       Payment DB
```

The frontend needs to know:

```text
User Service URL
Order Service URL
Payment Service URL
```

It also has to deal with:

```text
Authentication
Retries
Rate limiting
Service failures
Different APIs
```

This creates unnecessary complexity on the client side.

---

### With API Gateway

Instead:

```text
                    Frontend
                       │
                       ↓
                 API Gateway
                       │
        ┌──────────────┼──────────────┐
        ↓              ↓              ↓
   User Service   Order Service   Payment Service
```

The client knows only:

```text
API Gateway
```

The gateway knows where backend services are.

---

### What Does an API Gateway Do?

Think of the Gateway as:

> **The front door to your backend.**

Common responsibilities:

```text
1. Routing
2. Authentication
3. Authorization
4. Rate limiting
5. Request/response transformation
6. Load balancing
7. Caching
8. Request aggregation
9. Logging
10. Monitoring
11. TLS termination
12. API versioning
```

Not every gateway necessarily provides every capability, but these are common gateway concerns.

---

### Why BFF?

Imagine the web application needs:

```text
Large detailed response
```

while mobile needs:

```text
Small optimized response
```

Without BFF:

```text
Mobile
  ↓
Generic API
  ↓
Huge response
```

With BFF:

```text
Mobile
  ↓
Mobile BFF
  ↓
Optimized API response
```

This can reduce:

```text
Payload size
Network calls
Client-side orchestration
Client complexity
```

---

### API Gateway vs BFF

This is a common interview question.

#### API Gateway


> **Generic entry point for multiple clients/services.**

Example:

```text
             API Gateway
          /      |       \
       Web     Mobile    Partner
```

It focuses on:

```text
Routing
Security
Rate limiting
Cross-cutting concerns
```

#### BFF

Usually:

> **Client-specific backend layer.**

Example:

```text
Web
 ↓
Web BFF
 ↓
Services

Mobile
 ↓
Mobile BFF
 ↓
Services
```

It focuses on:

```text
Client-specific aggregation
Transformation
Response shaping
Client-specific workflows
```
-----
-----