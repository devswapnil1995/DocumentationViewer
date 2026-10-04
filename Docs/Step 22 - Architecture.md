## .NET Core application stucture with moduler monolith

> One deployable application, but internally divided into strongly isolated business modules.

For example, instead of having one huge `Services` project:

```text
ECommerce
 ├── Orders
 ├── Payments
 ├── Customers
 ├── Products
 └── Inventory
```

Each module owns its **Domain + Application + Infrastructure + API endpoints**, while common infrastructure is shared carefully.

---

**1. Recommended Enterprise Architecture**

I would structure it like this:

```text
                    ┌──────────────────────────┐
                    │       API / Host         │
                    │                          │
                    │ Controllers / Endpoints  │
                    │ Middleware               │
                    │ Authentication           │
                    └────────────┬─────────────┘
                                 │
             ┌───────────────────┼───────────────────┐
             │                   │                   │
             ▼                   ▼                   ▼
       ┌───────────┐       ┌───────────┐       ┌───────────┐
       │  Orders   │       │ Payments  │       │ Customers │
       │  Module   │       │  Module   │       │  Module   │
       └─────┬─────┘       └─────┬─────┘       └─────┬─────┘
             │                   │                   │
       Domain/Application   Domain/Application  Domain/Application
             │                   │                   │
             ▼                   ▼                   ▼
       Infrastructure       Infrastructure       Infrastructure
             │                   │                   │
             └───────────────────┼───────────────────┘
                                 ▼
                         ┌───────────────┐
                         │   Database    │
                         │ SQL Server    │
                         └───────────────┘
```

But there is an important architectural rule:

> Modules communicate through contracts, not by directly accessing each other's internals.

---

**2. Solution Structure**

For a serious enterprise application, I would start with:

```text
EnterpriseApp.sln

src/
│
├── EnterpriseApp.Api/
│
├── BuildingBlocks/
│   ├── BuildingBlocks.Domain/
│   ├── BuildingBlocks.Application/
│   ├── BuildingBlocks.Infrastructure/
│   └── BuildingBlocks.Contracts/
│
├── Modules/
│   │
│   ├── Customer/
│   │   ├── Customer.Domain/
│   │   ├── Customer.Application/
│   │   ├── Customer.Infrastructure/
│   │   └── Customer.Contracts/
│   │
│   ├── Order/
│   │   ├── Order.Domain/
│   │   ├── Order.Application/
│   │   ├── Order.Infrastructure/
│   │   └── Order.Contracts/
│   │
│   ├── Payment/
│   │   ├── Payment.Domain/
│   │   ├── Payment.Application/
│   │   ├── Payment.Infrastructure/
│   │   └── Payment.Contracts/
│   │
│   └── Inventory/
│       ├── Inventory.Domain/
│       ├── Inventory.Application/
│       ├── Inventory.Infrastructure/
│       └── Inventory.Contracts/
│
└── Tests/
    ├── Customer.UnitTests/
    ├── Customer.IntegrationTests/
    ├── Order.UnitTests/
    ├── Order.IntegrationTests/
    └── ...
```

This is the structure I would recommend learning.

---

**3. Why Modular Monolith?**

Suppose you build an e-commerce system.

Traditional approach:

```text
ECommerce.API
ECommerce.Services
ECommerce.Repositories
ECommerce.Models
ECommerce.Data
```

Initially it looks clean.

After 3 years:

```text
Services/
   CustomerService
   OrderService
   PaymentService
   InventoryService
   ProductService
   NotificationService
   ...
```

Then:

```text
OrderService
    ↓
CustomerService
    ↓
PaymentService
    ↓
InventoryService
    ↓
ProductService
```

Everything depends on everything.

That's a **distributed monolith inside a monolith**.

---

**4. Modular Monolith**

Instead:

```text
Modules/

Customer/
Order/
Payment/
Inventory/
Product/
Notification/
```

Each module owns its business logic.

For example:

```text
Order
│
├── Domain
│   ├── Entities
│   ├── ValueObjects
│   ├── Events
│   └── Rules
│
├── Application
│   ├── Commands
│   ├── Queries
│   ├── Handlers
│   └── Interfaces
│
├── Infrastructure
│   ├── Persistence
│   ├── Repositories
│   └── ExternalServices
│
└── Contracts
    └── DTOs
```

The Order module doesn't know how Payment internally works.

---

**5. Domain Layer**

The Domain layer contains **business rules**.

Example:

```text
Order.Domain

Entities/
    Order.cs
    OrderItem.cs

ValueObjects/
    Money.cs
    OrderId.cs

Enums/
    OrderStatus.cs

Events/
    OrderCreatedDomainEvent.cs
    OrderConfirmedDomainEvent.cs

Exceptions/
    InvalidOrderException.cs
```

Example:

```csharp
public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    public void AddItem(
        Guid productId,
        int quantity,
        decimal price)
    {
        if (quantity <= 0)
            throw new DomainException(
                "Quantity must be greater than zero.");

        _items.Add(
            new OrderItem(
                productId,
                quantity,
                price));
    }

    public void Confirm()
    {
        if (!_items.Any())
            throw new DomainException(
                "Order cannot be confirmed without items.");

        Status = OrderStatus.Confirmed;
    }
}
```

Notice:

```text
No EF Core
No DbContext
No HTTP
No IConfiguration
No Azure
No SQL
```

That's intentional.

---

**6. Application Layer**

Application layer coordinates **use cases**.

Example:

```text
Order.Application

Orders/
│
├── Commands/
│   ├── CreateOrder/
│   │   ├── CreateOrderCommand.cs
│   │   ├── CreateOrderCommandHandler.cs
│   │   └── CreateOrderValidator.cs
│   │
│   └── ConfirmOrder/
│       ├── ConfirmOrderCommand.cs
│       └── ConfirmOrderCommandHandler.cs
│
└── Queries/
    ├── GetOrder/
    │   ├── GetOrderQuery.cs
    │   └── GetOrderQueryHandler.cs
    │
    └── GetOrders/
        ├── GetOrdersQuery.cs
        └── GetOrdersQueryHandler.cs
```

For example:

```csharp
public record CreateOrderCommand(
    Guid CustomerId,
    List<CreateOrderItem> Items
) : IRequest<Guid>;
```

Handler:

```csharp
public class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(
        IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = new Order(request.CustomerId);

        foreach (var item in request.Items)
        {
            order.AddItem(
                item.ProductId,
                item.Quantity,
                item.Price);
        }

        await _repository.AddAsync(
            order,
            cancellationToken);

        return order.Id;
    }
}
```

---

**7. Infrastructure Layer**

Infrastructure contains technical implementations.

```text
Order.Infrastructure

Persistence/
│
├── OrderDbContext.cs
├── Configurations/
│   ├── OrderConfiguration.cs
│   └── OrderItemConfiguration.cs
│
└── Migrations/

Repositories/
    OrderRepository.cs

Services/
    PaymentService.cs
    OrderNumberGenerator.cs
```

For example:

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        await _context.Orders.AddAsync(
            order,
            cancellationToken);
    }
}
```

The interface belongs to Application:

```csharp
public interface IOrderRepository
{
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken);
}
```

Implementation belongs to Infrastructure.

---

**8. Contracts Layer**

This is extremely useful in modular applications.

Example:

```text
Order.Contracts

Events/
    OrderCreatedIntegrationEvent.cs
    OrderCancelledIntegrationEvent.cs

DTOs/
    OrderSummaryDto.cs
```

Suppose Payment needs to know when an order is created.

Don't do:

```text
Payment
    ↓
OrderDbContext
```

Instead:

```text
Order
  │
  │ publishes
  ▼
OrderCreatedIntegrationEvent
  │
  ▼
Payment
```

Example:

```csharp
public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount);
```

---

**9. Module Communication**

This is one of the **most important interview concepts**.

Bad:

```text
Order
  ↓
PaymentService
  ↓
PaymentDbContext
```

Better:

```text
Order
  ↓
OrderCreatedEvent
  ↓
Payment
```

Inside a modular monolith you can initially use:

```text
In-process events
```

Later, when you extract Payment into a microservice:

```text
RabbitMQ / Azure Service Bus
```

The business boundary remains almost the same.

That's one of the biggest advantages of Modular Monolith.

---

**10. API Layer**

The API project should be relatively thin.

```text
EnterpriseApp.Api

Controllers/
    CustomersController.cs
    OrdersController.cs
    PaymentsController.cs

Middleware/
    ExceptionHandlingMiddleware.cs

Extensions/
    DependencyInjectionExtensions.cs
    AuthenticationExtensions.cs

Filters/
    ValidationFilter.cs

Program.cs
```

Controller:

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderCommand command)
    {
        var orderId =
            await _sender.Send(command);

        return Ok(orderId);
    }
}
```

Don't put business logic here.

---

**11. Global Exception Handling**

Enterprise application should have centralized exception handling.

```text
Request
   ↓
Middleware
   ↓
Controller
   ↓
Application
   ↓
Domain
```

Middleware:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    Error = ex.Message
                });
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    Error = "An unexpected error occurred."
                });
        }
    }
}
```

In production, I would use a structured `ProblemDetails` response rather than anonymous objects.

---

**12. Building Blocks**

The BuildingBlocks project contains **technical/common abstractions**, not business logic.

Example:

```text
BuildingBlocks.Domain

Entity.cs
AggregateRoot.cs
ValueObject.cs
DomainEvent.cs
DomainException.cs
```

Application:

```text
BuildingBlocks.Application

ICommand.cs
IQuery.cs
IUnitOfWork.cs
Result.cs
```

Infrastructure:

```text
BuildingBlocks.Infrastructure

DateTimeProvider
CurrentUserService
Outbox
Caching
Logging
```

But be careful.

Don't turn BuildingBlocks into:

```text
EverythingShared.cs
```

That's a common architecture mistake.

---

**13. Database Strategy**

For Modular Monolith, I recommend:

***Option A — Shared database, separate schemas***

For SQL Server:

```text
Database
│
├── customer
│   ├── Customers
│   └── Addresses
│
├── order
│   ├── Orders
│   └── OrderItems
│
├── payment
│   ├── Payments
│   └── Transactions
│
└── inventory
    ├── Products
    └── Stock
```

This is a very good enterprise starting point.

The important rule:

> A module owns its tables.

Order shouldn't directly query:

```sql
SELECT *
FROM customer.Customers
```

Instead:

```text
Order → Customer contract/application API
```

or maintain the necessary local data through events.

---

**14. DbContext**

I would prefer **one DbContext per module**.

For example:

```text
CustomerDbContext
OrderDbContext
PaymentDbContext
InventoryDbContext
```

Instead of:

```text
ApplicationDbContext
    300 DbSets
```

So:

```csharp
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public OrderDbContext(
        DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderDbContext).Assembly);
    }
}
```

---

**15. Dependency Direction**

This is critical.

For each module:

```text
             Domain
               ▲
               │
        Application
               ▲
               │
       Infrastructure
```

More accurately:

```text
Domain
  ↑
Application
  ↑
Infrastructure
```

Infrastructure implements interfaces defined by Application/Domain.

Example:

```text
Application
    │
    ├── IOrderRepository
    └── IPaymentGateway
             ▲
             │
Infrastructure
    │
    ├── OrderRepository
    └── StripePaymentGateway
```

---

**16. Module Dependency Rule**

Suppose:

```text
Customer
Order
Payment
Inventory
```

Avoid:

```text
Order → Customer.Infrastructure
Order → Payment.Infrastructure
Payment → Order.Infrastructure
```

Instead:

```text
Order
 │
 └── Order.Contracts
          │
          ▼
       Payment
```

or:

```text
Order
 │
 ▼
Integration Event
 │
 ▼
Payment
```

---

**17. CQRS**

You don't necessarily need full CQRS infrastructure.

I recommend **CQRS-lite**:

```text
Commands
    ↓
Change state

Queries
    ↓
Read state
```

Example:

```text
CreateOrderCommand
UpdateOrderCommand
CancelOrderCommand
```

versus:

```text
GetOrderQuery
GetCustomerOrdersQuery
GetOrderSummaryQuery
```

Commands can use EF Core.

Queries can use:

```text
EF Core
Dapper
Stored Procedure
Read model
```

depending on performance requirements.

---

**18. Validation**

Use FluentValidation.

```text
CreateOrderCommand
       ↓
CreateOrderValidator
       ↓
Handler
```

Example:

```csharp
public class CreateOrderValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0);
            });
    }
}
```

---

**19. Resilience**

For enterprise applications, add resilience around external calls.

Example:

```text
Order
  ↓
Payment API
```

Use:

```text
Timeout
Retry
Circuit Breaker
Fallback
```

Modern .NET applications can use **Microsoft.Extensions.Resilience / Polly-based resilience pipelines**.

Example conceptual flow:

```text
Payment API
     │
     ▼
 Timeout
     │
     ▼
 Retry
     │
     ▼
 Circuit Breaker
```

Don't blindly retry everything.

For example:

```text
GET → generally retryable

POST payment → potentially dangerous
```

You need idempotency.

---

**20. Outbox Pattern**

This is another enterprise-level concept you should include.

Problem:

```text
Save Order
   ↓
Publish Event
```

What happens if:

```text
Order saved successfully
Event publishing fails
```

Now database says:

```text
Order = Created
```

but Payment never receives the event.

Solution:

```text
Transaction
│
├── Orders
│
└── OutboxMessages
```

Then:

```text
Background Worker
       ↓
OutboxMessages
       ↓
Publish Event
       ↓
Mark Processed
```

Architecture:

```text
Order
 │
 ├── Order table
 │
 └── Outbox table
          │
          ▼
   Background Worker
          │
          ▼
      Event Bus
```

This is excellent enterprise interview material.

---

**21. Background Processing**

Use:

```csharp
BackgroundService
```

for things like:

```text
Outbox processing
Email processing
Report generation
Data synchronization
Cleanup jobs
```

Architecture:

```text
API
 │
 ▼
Database
 │
 ▼
Outbox
 │
 ▼
BackgroundService
 │
 ▼
Message/Event
```

---

**22. Caching**

Don't put caching everywhere.

Use it where needed:

```text
API
 ↓
Application
 ↓
Cache
 ↓
Database
```

Potential technologies:

```text
IMemoryCache
Redis
Distributed Cache
```

For enterprise:

```text
Redis
```

is generally more appropriate when you have multiple application instances.

---

**23. Authentication & Authorization**

Typical architecture:

```text
Client
   ↓
JWT / OAuth2
   ↓
ASP.NET Core Authentication
   ↓
Authorization
   ↓
Application
```

You can have:

```text
Roles

Admin
Manager
Customer
Operator
```

and policies:

```csharp
[Authorize(Policy = "CanManageOrders")]
```

Prefer policies over scattering role checks throughout business logic.

---

**24. Observability**

Enterprise application should have:

```text
Logging
Metrics
Tracing
Health Checks
```

For example:

```text
Application
    │
    ├── Serilog / ILogger
    │
    ├── OpenTelemetry
    │
    ├── Application Insights
    │
    └── Health Checks
```

Trace:

```text
Request
  ↓
Controller
  ↓
Command Handler
  ↓
EF Core
  ↓
SQL Server
```

with a correlation/trace ID.

---

**25. API Versioning**

Don't assume your API will never change.

Example:

```text
/api/v1/orders
/api/v2/orders
```

This becomes important when multiple clients consume your API.

---

**26. Configuration**

Avoid:

```csharp
Configuration["PaymentUrl"]
```

everywhere.

Use strongly typed options:

```csharp
public class PaymentOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; }
}
```

Then:

```csharp
builder.Services.Configure<PaymentOptions>(
    builder.Configuration.GetSection("Payment"));
```

---

**27. Enterprise Request Flow**

Now the whole request becomes:

```text
                    HTTP Request
                         │
                         ▼
                  ┌─────────────┐
                  │ Middleware  │
                  └──────┬──────┘
                         │
                Authentication
                         │
                Authorization
                         │
                         ▼
                  ┌─────────────┐
                  │ Controller  │
                  └──────┬──────┘
                         │
                         ▼
                 Command / Query
                         │
                         ▼
                  ┌─────────────┐
                  │ Validation  │
                  └──────┬──────┘
                         │
                         ▼
                    Handler
                         │
                         ▼
                     Domain
                         │
                         ▼
                  Repository/API
                         │
                         ▼
                   Infrastructure
                         │
                         ▼
                     Database
```

---

**28. Complete Enterprise Skeleton**

I'd ultimately want your project to look approximately like this:

```text
EnterpriseApp
│
├── src
│
│   ├── EnterpriseApp.Api
│   │
│   │   ├── Controllers
│   │   ├── Middleware
│   │   ├── Filters
│   │   ├── Extensions
│   │   ├── HealthChecks
│   │   └── Program.cs
│   │
│   ├── BuildingBlocks
│   │   │
│   │   ├── BuildingBlocks.Domain
│   │   │   ├── Entity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── DomainEvent.cs
│   │   │   └── DomainException.cs
│   │   │
│   │   ├── BuildingBlocks.Application
│   │   │   ├── Commands
│   │   │   ├── Queries
│   │   │   ├── Behaviors
│   │   │   └── Results
│   │   │
│   │   ├── BuildingBlocks.Infrastructure
│   │   │   ├── Persistence
│   │   │   ├── Outbox
│   │   │   ├── Caching
│   │   │   ├── Messaging
│   │   │   └── Resilience
│   │   │
│   │   └── BuildingBlocks.Contracts
│   │
│   └── Modules
│
│       ├── Customer
│       │
│       │   ├── Customer.Domain
│       │   │   ├── Entities
│       │   │   ├── ValueObjects
│       │   │   ├── Events
│       │   │   ├── Exceptions
│       │   │   └── Repositories
│       │   │
│       │   ├── Customer.Application
│       │   │   ├── Commands
│       │   │   ├── Queries
│       │   │   ├── Validators
│       │   │   └── Services
│       │   │
│       │   ├── Customer.Infrastructure
│       │   │   ├── Persistence
│       │   │   ├── Repositories
│       │   │   └── Services
│       │   │
│       │   └── Customer.Contracts
│       │
│       ├── Order
│       │
│       │   ├── Order.Domain
│       │   ├── Order.Application
│       │   ├── Order.Infrastructure
│       │   └── Order.Contracts
│       │
│       ├── Payment
│       │
│       │   ├── Payment.Domain
│       │   ├── Payment.Application
│       │   ├── Payment.Infrastructure
│       │   └── Payment.Contracts
│       │
│       └── Inventory
│
│           ├── Inventory.Domain
│           ├── Inventory.Application
│           ├── Inventory.Infrastructure
│           └── Inventory.Contracts
│
└── tests
    │
    ├── Customer.UnitTests
    ├── Customer.IntegrationTests
    ├── Order.UnitTests
    ├── Order.IntegrationTests
    ├── Payment.UnitTests
    └── ArchitectureTests
```

---

**29. Architecture Tests**

This is something I strongly recommend for your enterprise project.

You can automatically verify rules such as:

```text
Domain
  ❌ cannot reference Infrastructure

Application
  ❌ cannot reference Infrastructure implementation

Order
  ❌ cannot reference Payment.Infrastructure

Customer
  ❌ cannot reference Order.Domain
```

For example, use an architecture-testing approach with **NetArchTest** or equivalent tooling.

Then your architecture isn't merely documentation.

The build can enforce it.

---

**30. The Most Important Rule**

Don't make this:

```text
Modules
   ↓
Shared Services
   ↓
Shared Repository
   ↓
Shared DbContext
```

That destroys modularity.

Instead:

```text
                 API
                  │
      ┌───────────┼────────────┐
      ▼           ▼            ▼
   Customer      Order       Payment
      │           │            │
      ▼           ▼            ▼
   CustomerDB   OrderDB     PaymentDB
```

Communication:

```text
Customer ──────► Contract/Event ◄────── Order
```

not:

```text
Customer ──────► OrderDbContext
```

---

**31. When Should You Convert It to Microservices?**

This is where Modular Monolith becomes very powerful.

You start with:

```text
                Modular Monolith

Customer | Order | Payment | Inventory
                │
                ▼
          One deployment
```

Later, if Payment needs independent scaling:

```text
              API Gateway
                   │
       ┌───────────┼────────────┐
       ▼           ▼            ▼
    Monolith    Payment      Inventory
       │         Service       Service
       │
Customer/Order
```

The Payment module can be extracted because its boundary already exists.

That's the fundamental idea:

> Modular Monolith first, Microservices when justified.

Not:

> Let's create 15 microservices because microservices are enterprise.

---
---

## MediatR, RabbitMQ &  Azure Service Bus

For a **Modular Monolith**, think of them like this:

```text
                     Modular Monolith
                           │
          ┌────────────────┼────────────────┐
          │                │                │
       Customer           Order           Payment
          │                │                │
          └────────────────┼────────────────┘
                           │
                 How modules communicate?
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
           MediatR      RabbitMQ     Azure Service Bus
```

But they are **not interchangeable**.

---

**1. First: MediatR**

MediatR is primarily useful for **in-process communication**.

Meaning:

> Module A calls something inside the same application process without directly depending on its implementation.

For example:

```text
Order Module
     │
     │ MediatR
     ▼
Payment Module
```

Everything is still running inside:

```text
One application
One process
One deployment
```

***Example***

Order creates an order:

```csharp
public record CreateOrderCommand(
    Guid CustomerId,
    decimal Amount) : IRequest<Guid>;
```

Then:

```csharp
await _sender.Send(command);
```

MediatR finds:

```csharp
CreateOrderCommandHandler
```

and executes it.

So:

```text
Controller
    ↓
MediatR
    ↓
Command Handler
    ↓
Domain
    ↓
Database
```

This is extremely useful for:

- Commands
- Queries
- Pipeline behaviors
- Validation
- Logging
- Transactions
- Decoupling controllers from application logic

---

**2. MediatR is NOT a message broker**

This is important.

MediatR:

```text
Order Module
     ↓
MediatR
     ↓
Payment Module
```

is **in-process**.

If your application crashes:

```text
Application
    💥
```

the message doesn't sit somewhere waiting for the application to come back.

That's where RabbitMQ / Azure Service Bus become useful.

---

**3. RabbitMQ / Azure Service Bus**

These are **message brokers**.

Think:

```text
Order
  │
  │ publish message
  ▼
┌─────────────────────┐
│   Message Broker    │
│                     │
│ RabbitMQ / ASB      │
└──────────┬──────────┘
           │
           ▼
       Payment
```

The broker sits outside your application.

It can store messages until consumers process them.

---

**4. Why would you use a broker in a Modular Monolith?**

This is where things get interesting.

You **can** use RabbitMQ/ASB in a modular monolith, but you don't necessarily need it.

For example:

```text
Order Module
     │
     │ OrderCreated
     ▼
Payment Module
```

You could use:

***Option A***

```text
MediatR
```

***Option B***

```text
Domain Event
+
In-process event handler
```

***Option C***

```text
RabbitMQ / Azure Service Bus
```

The correct choice depends on what you need.

---

**5. Simple Modular Monolith**

If you're just learning:

```text
Customer
Order
Payment
```

I would start with:

```text
Controller
    ↓
MediatR
    ↓
Application Handler
    ↓
Domain
```

For example:

```text
POST /orders
      ↓
OrdersController
      ↓
CreateOrderCommand
      ↓
MediatR
      ↓
CreateOrderHandler
      ↓
Order
      ↓
OrderDbContext
```

This is enough.

---

**6. Where Domain Events Come In**

Suppose:

```text
Order Created
```

and several things need to happen:

```text
Order Created
     │
     ├── Send confirmation email
     ├── Update inventory
     ├── Start payment
     └── Create notification
```

Instead of Order knowing about all these modules:

```text
Order
 ├── EmailService
 ├── PaymentService
 ├── InventoryService
 └── NotificationService
```

you can publish:

```text
OrderCreatedEvent
```

Then:

```text
                    Order
                      │
                      │ OrderCreated
                      ▼
             ┌─────────────────┐
             │ Event Dispatcher│
             └────────┬────────┘
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
     Payment       Inventory    Notification
```

This is **event-driven architecture** inside your monolith.

---

**7. MediatR Notification**

MediatR has notifications that can be used for this kind of in-process event handling.

Conceptually:

```csharp
public record OrderCreatedEvent(
    Guid OrderId,
    decimal Amount) : INotification;
```

Then handlers:

```csharp
public class PaymentHandler
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(
        OrderCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Payment logic
    }
}
```

Another:

```csharp
public class InventoryHandler
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(
        OrderCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Inventory logic
    }
}
```

Now:

```text
OrderCreatedEvent
       │
       ├──── PaymentHandler
       │
       └──── InventoryHandler
```

No direct:

```text
Order → PaymentService
```

dependency.

---

**8. But there is a problem**

Suppose:

```text
Order Created
      ↓
MediatR
      ├── PaymentHandler ✅
      └── InventoryHandler ❌
```

Inventory handler fails.

You need to think about:

- retry
- transaction
- consistency
- failure handling

And that's where **Outbox + Message Broker** becomes more interesting.

---

**9. Outbox Pattern**

Instead of:

```text
Order
  ↓
Publish message
```

do:

```text
Transaction
│
├── Save Order
│
└── Save Outbox Message
```

Example:

```text
Order DB
│
├── Orders
│
└── OutboxMessages
        │
        ▼
 Background Worker
        │
        ▼
 RabbitMQ / Azure Service Bus
        │
        ├───────────────┐
        ▼               ▼
    Payment         Inventory
```

This is much more reliable.

---

**10. RabbitMQ**

RabbitMQ is a general-purpose message broker.

Conceptually:

```text
Order Module
      │
      ▼
 RabbitMQ
      │
 ┌────┴─────┐
 ▼          ▼
Payment   Inventory
```

RabbitMQ has concepts such as:

```text
Producer
Exchange
Queue
Consumer
```

Example:

```text
Order
  │
  │ publish
  ▼
Exchange
  │
  ├── payment.queue
  │       ↓
  │    Payment
  │
  └── inventory.queue
          ↓
       Inventory
```

This is useful when you need asynchronous communication.

---

**11. Azure Service Bus**

Azure Service Bus is Microsoft's managed enterprise messaging service.

Conceptually:

```text
Order Module
      │
      ▼
Azure Service Bus
      │
      ├── Payment Queue
      │
      └── Inventory Queue
```

It provides enterprise messaging features such as:

- queues
- topics/subscriptions
- retries
- dead-letter queues
- message locking
- duplicate detection
- scheduled messages
- Azure integration

So if your application is heavily Azure-based, Service Bus is often a natural choice.

---

**12. RabbitMQ vs Azure Service Bus**

At a high level:

| | MediatR | RabbitMQ | Azure Service Bus |
|---|---|---|---|
| Runs inside application | ✅ | ❌ | ❌ |
| External broker | ❌ | ✅ | ✅ |
| Async messaging | Limited/in-process | ✅ | ✅ |
| Message persistence | ❌ | ✅ | ✅ |
| Retry capabilities | Application-level | ✅ | ✅ |
| Dead-letter queue | ❌ | ✅ | ✅ |
| Good for microservices | ❌ | ✅ | ✅ |
| Good for modular monolith | ✅ | Optional | Optional |
| Azure managed service | ❌ | ❌ | ✅ |

---

**13. So what should YOU use?**

For your current goal of **understanding enterprise .NET architecture for interviews**, I'd learn them in this order:

***Level 1 — MediatR***

Understand:

```text
Controller
    ↓
MediatR
    ↓
Command / Query
    ↓
Handler
```

This teaches you **CQRS and application decoupling**.

---

***Level 2 — Domain Events***

Understand:

```text
Order
  ↓
OrderCreated
  ↓
Multiple handlers
```

This teaches you **event-driven design**.

---

***Level 3 — Outbox***

Understand:

```text
Business Transaction
       ↓
Database + Outbox
       ↓
Background Worker
```

This teaches you **reliable event publishing**.

---

***Level 4 — RabbitMQ / Azure Service Bus***

Then understand:

```text
Outbox
   ↓
Message Broker
   ↓
Consumer
```

This teaches you **asynchronous distributed communication**.

---

**14. And then Microservices**

This is where everything clicks.

Today:

```text
             MODULAR MONOLITH

       ┌─────────────────────────────┐
       │                             │
       │ Customer  Order  Payment    │
       │    │       │       │        │
       │    └───────┼───────┘        │
       │            │                │
       └────────────┼────────────────┘
                    │
                 Database
```

You use:

```text
MediatR
Domain Events
Outbox
```

Later you extract Payment:

```text
                 API
                  │
       ┌──────────┴──────────┐
       ▼                     ▼
 ┌──────────────┐      ┌──────────────┐
 │   Monolith   │      │   Payment    │
 │              │      │   Service    │
 │ Customer     │      │              │
 │ Order        │      │              │
 │ Inventory    │      │              │
 └──────┬───────┘      └──────┬───────┘
        │                       │
        └──── Message Broker ───┘
             RabbitMQ / ASB
```

Now the broker becomes **much more important**.

---

### The mental model to remember

```text
MediatR
   ↓
"In the same application, call/dispatch something."

Domain Event
   ↓
"Something happened in my business."

Outbox
   ↓
"Make sure I don't lose that event."

RabbitMQ / Azure Service Bus
   ↓
"Move messages reliably between independent components."

Microservice
   ↓
"Now those components can run/deploy independently."
```

------
------

## Angular architecture

For an enterprise Angular application, especially Angular 21+, I would recommend:

> **Feature/Module-based architecture + standalone components + lazy loading + smart/dumb component separation + facades/services + API layer + shared UI library.**

The most important idea is:

```text
Backend Modular Monolith                 Angular Modular Frontend

Customer Module                          Customer Feature
Order Module                             Order Feature
Payment Module                           Payment Feature
Inventory Module                         Inventory Feature
       │                                        │
       └──────── Business boundaries ───────────┘
```

---

**1. Start With This Mental Model**

Don't create:

```text
components/
services/
models/
```

with 200 files inside each.

That becomes difficult to maintain.

Instead:

```text
src/app/

├── core/
├── shared/
├── layout/
│
└── features/
    ├── customer/
    ├── order/
    ├── payment/
    └── inventory/
```

Think:

> **Every business capability gets its own feature boundary.**

---

**2. Recommended Angular Enterprise Structure**

For Angular 21:

```text
src/
└── app/
    │
    ├── core/
    │   ├── auth/
    │   ├── http/
    │   ├── interceptors/
    │   ├── guards/
    │   ├── config/
    │   ├── services/
    │   └── core.providers.ts
    │
    ├── shared/
    │   ├── components/
    │   ├── directives/
    │   ├── pipes/
    │   ├── models/
    │   └── utils/
    │
    ├── layout/
    │   ├── shell/
    │   ├── header/
    │   ├── sidebar/
    │   └── footer/
    │
    ├── features/
    │   │
    │   ├── customer/
    │   │   ├── pages/
    │   │   │   ├── customer-list/
    │   │   │   ├── customer-details/
    │   │   │   └── customer-create/
    │   │   │
    │   │   ├── components/
    │   │   │   ├── customer-form/
    │   │   │   └── customer-card/
    │   │   │
    │   │   ├── services/
    │   │   │   ├── customer-api.service.ts
    │   │   │   └── customer.facade.ts
    │   │   │
    │   │   ├── models/
    │   │   │   └── customer.model.ts
    │   │   │
    │   │   └── customer.routes.ts
    │   │
    │   ├── order/
    │   │   ├── pages/
    │   │   ├── components/
    │   │   ├── services/
    │   │   ├── models/
    │   │   └── order.routes.ts
    │   │
    │   ├── payment/
    │   │   ├── pages/
    │   │   ├── components/
    │   │   ├── services/
    │   │   ├── models/
    │   │   └── payment.routes.ts
    │   │
    │   └── inventory/
    │       ├── pages/
    │       ├── components/
    │       ├── services/
    │       ├── models/
    │       └── inventory.routes.ts
    │
    ├── app.routes.ts
    ├── app.config.ts
    └── app.component.ts
```

---

**3. What Is `core`?**

`core` contains things that are **application-wide**, not business-specific.

For example:

```text
core/
├── auth/
├── http/
├── interceptors/
├── guards/
├── config/
└── services/
```

Things like:

```text
Authentication
Authorization
HTTP interceptors
Global error handling
Application configuration
Token management
Logging
```

Example:

```text
core/auth/auth.service.ts
core/http/api-client.service.ts
core/interceptors/auth.interceptor.ts
core/interceptors/error.interceptor.ts
```

You should **not** put:

```text
OrderService
CustomerService
PaymentService
```

inside `core`.

Those belong to their feature.

---

**4. What Is `shared`?**

`shared` contains reusable UI/utility pieces.

For example:

```text
shared/
├── components/
│   ├── data-table/
│   ├── confirmation-dialog/
│   ├── loading-spinner/
│   └── empty-state/
│
├── directives/
│   └── permission.directive.ts
│
├── pipes/
│   └── currency.pipe.ts
│
└── utils/
```

For example:

```html
<app-confirmation-dialog />
```

can be used by:

```text
Customer
Order
Payment
Inventory
```

But don't make `shared` a dumping ground.

---

**5. Feature = Business Module**

This is the most important part.

Your Order feature:

```text
features/order/
```

owns everything related to Order UI.

```text
order/
│
├── pages/
│   ├── order-list/
│   ├── order-details/
│   └── order-create/
│
├── components/
│   ├── order-form/
│   ├── order-item-list/
│   └── order-summary/
│
├── services/
│   ├── order-api.service.ts
│   └── order.facade.ts
│
├── models/
│   └── order.model.ts
│
└── order.routes.ts
```

This is very similar to our .NET module.

---

**6. Angular Module vs Business Module**

Important interview distinction.

When I say:

```text
Customer
Order
Payment
```

I mean **business modules/features**.

I'm not saying you need:

```text
CustomerModule
OrderModule
PaymentModule
```

using the old Angular `NgModule` approach.

Modern Angular uses **standalone components**.

So:

```text
Customer Feature
```

is a business boundary, not necessarily an Angular `NgModule`.

---

**7. Routing**

Each feature should own its routes.

For example:

```typescript
// order.routes.ts

export const ORDER_ROUTES: Routes = [
  {
    path: '',
    component: OrderListComponent
  },
  {
    path: 'create',
    component: OrderCreateComponent
  },
  {
    path: ':id',
    component: OrderDetailsComponent
  }
];
```

Then application routes:

```typescript
export const APP_ROUTES: Routes = [
  {
    path: 'customers',
    loadChildren: () =>
      import('./features/customer/customer.routes')
        .then(m => m.CUSTOMER_ROUTES)
  },

  {
    path: 'orders',
    loadChildren: () =>
      import('./features/order/order.routes')
        .then(m => m.ORDER_ROUTES)
  },

  {
    path: 'payments',
    loadChildren: () =>
      import('./features/payment/payment.routes')
        .then(m => m.PAYMENT_ROUTES)
  }
];
```

Now you have:

```text
/orders
/customers
/payments
```

and each feature is **lazy loaded**.

---

**8. Why Lazy Loading?**

Imagine your application has:

```text
Customer
Order
Payment
Inventory
Reports
Admin
Analytics
```

You don't want the browser downloading everything immediately.

Instead:

```text
Initial Application
       │
       ├── Core
       ├── Layout
       └── Login
```

User clicks:

```text
Orders
```

Then:

```text
Browser
   ↓
Load Order Feature
   ↓
Order pages/components/services
```

This improves initial application loading and keeps feature boundaries clear.

---

**9. Components: Smart vs Presentational**

Another important enterprise concept.

Suppose:

```text
OrderListPage
```

Don't put everything inside it.

Instead:

```text
OrderListPage
      │
      ├── OrderFilter
      │
      ├── OrderTable
      │
      └── Pagination
```

The page/container handles orchestration:

```text
API
State
Navigation
Business workflow
```

while components focus on UI.

---

**10. Example**

***Page***

```typescript
@Component({
  selector: 'app-order-list',
  standalone: true,
  templateUrl: './order-list.component.html'
})
export class OrderListComponent {

  private readonly facade = inject(OrderFacade);

  readonly orders = this.facade.orders;

  ngOnInit() {
    this.facade.loadOrders();
  }
}
```

Template:

```html
<app-order-filter />

<app-order-table
  [orders]="orders()" />
```

The page isn't responsible for directly implementing every API detail.

---

**11. API Service**

Create a feature-specific API service.

```typescript
@Injectable({
  providedIn: 'root'
})
export class OrderApiService {

  private readonly http = inject(HttpClient);

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>('/api/orders');
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`/api/orders/${id}`);
  }

  createOrder(
    request: CreateOrderRequest
  ): Observable<Order> {

    return this.http.post<Order>(
      '/api/orders',
      request
    );
  }
}
```

This service knows:

```text
HTTP
API endpoints
DTOs
```

It shouldn't contain UI state.

---

**12. Then Facade**

The Facade sits between UI and API/state.

```text
OrderComponent
       ↓
OrderFacade
       ↓
OrderApiService
       ↓
HTTP
       ↓
.NET API
```

Example:

```typescript
@Injectable()
export class OrderFacade {

  private readonly api = inject(OrderApiService);

  readonly orders = signal<Order[]>([]);
  readonly loading = signal(false);

  loadOrders() {

    this.loading.set(true);

    this.api.getOrders()
      .subscribe({
        next: orders => {
          this.orders.set(orders);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      });
  }
}
```

This gives you:

```text
Component
   ↓
Facade
   ↓
API Service
```

instead of:

```text
Component
   ↓
HttpClient
```

everywhere.

---

**13. Where Does NgRx Fit?**

This is similar to your MediatR question.

You don't automatically need NgRx just because the application is enterprise-level.

Think:

***Simple feature***

```text
Component
   ↓
Facade
   ↓
API Service
```

Enough.

***Complex feature***

```text
Component
   ↓
Facade
   ↓
NgRx Store
   ↓
Effects
   ↓
API
```

NgRx becomes useful when you have complex shared state, many consumers, sophisticated workflows, caching, optimistic updates, etc.

---

***14. MediatR vs Angular Facade/NgRx***

Don't think they are exact equivalents, but the mental comparison is useful:

```text
.NET                         Angular

Controller                  Component/Page
     ↓                           ↓
MediatR                     Facade
     ↓                           ↓
Command Handler             State/API logic
     ↓                           ↓
Domain/Application          Feature
```

But:

> MediatR is not the Angular equivalent of NgRx.

MediatR handles in-process request/notification dispatch on the backend.

NgRx is primarily a client-side state management architecture.

---

**15. HTTP Interceptor**

Your Angular application should have centralized HTTP behavior.

```text
Component
    ↓
Facade
    ↓
API Service
    ↓
HttpClient
    ↓
Auth Interceptor
    ↓
Error Interceptor
    ↓
Backend
```

Auth interceptor:

```typescript
export const authInterceptor: HttpInterceptorFn =
  (req, next) => {

    const token = inject(AuthService).getToken();

    const request = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    return next(request);
  };
```

Now individual API services don't need:

```typescript
Authorization: Bearer ...
```

everywhere.

---

**16. Global Error Handling**

You can have:

```text
HTTP Error
     ↓
Error Interceptor
     ↓
Global Notification
```

For example:

```text
401 → logout / refresh token
403 → access denied
404 → not found
409 → business conflict
500 → generic error
```

Feature-specific business errors can still be handled inside the feature.

---

**17. Angular + .NET Architecture Together**

Now your full system starts looking really good:

```text
                    Angular
                       │
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
    Customer         Order         Payment
     Feature         Feature        Feature
        │              │              │
        ▼              ▼              ▼
    Customer API     Order API     Payment API
        │              │              │
        └──────────────┼──────────────┘
                       │
                  ASP.NET Core
                       │
       ┌───────────────┼────────────────┐
       ▼               ▼                ▼
   Customer          Order            Payment
    Module           Module            Module
       │               │                │
       ▼               ▼                ▼
   Customer DB       Order DB        Payment DB
```

That's a very clean enterprise architecture.

---

**18. Angular Feature Boundary**

Here's the key rule:

```text
Customer Feature
       ❌
       │
       └── Don't directly access
           Order internals
```

Instead:

```text
Customer Feature
       ↓
Public API / Contract
       ↓
Order Feature
```

Similarly, don't do:

```typescript
import { OrderService } from
'../../order/services/order.service';
```

from Customer.

That creates tight coupling.

---

**19. Public API of a Feature**

For larger applications, you can explicitly define what a feature exposes.

For example:

```text
features/order/
│
├── components/
├── services/
├── models/
│
└── public-api.ts
```

Then:

```typescript
export * from './models/order.model';
export * from './services/order.facade';
```

Other features only consume what's publicly exposed.

This is similar to:

```text
Order.Contracts
```

on the .NET side.

---

**20. Shared UI vs Business Logic**

This distinction is very important.

***Shared***

```text
Button
Modal
Table
DatePicker
LoadingSpinner
Pagination
```

Good:

```text
shared/components/
```

***Business-specific***

```text
OrderSummary
PaymentForm
CustomerAddress
InventoryStock
```

Don't put these into Shared.

They belong to:

```text
features/order
features/payment
features/customer
features/inventory
```

---

**21. State Management**

Think in levels:

***Level 1***

Local component state:

```typescript
signal()
```

Use for:

```text
isLoading
selectedRow
searchText
form state
```

***Level 2***

Feature state:

```text
Facade + Signals
```

Use when multiple components need the same feature state.

***Level 3***

Global state:

```text
NgRx
```

Use when application-wide state becomes complex.

For example:

```text
Current User
Permissions
Shopping Cart
Notifications
Global Settings
```

Don't put every API response into global NgRx state.

---

**22. Enterprise Angular Architecture I'd Recommend**

For your interview learning:

```text
Angular 21
│
├── Core
│   ├── Auth
│   ├── HTTP
│   ├── Interceptors
│   ├── Guards
│   └── Configuration
│
├── Shared
│   ├── UI Components
│   ├── Pipes
│   ├── Directives
│   └── Utilities
│
├── Layout
│
└── Features
    │
    ├── Customer
    │   ├── Pages
    │   ├── Components
    │   ├── Services
    │   ├── Models
    │   └── Routes
    │
    ├── Order
    │   ├── Pages
    │   ├── Components
    │   ├── Facade
    │   ├── API Service
    │   ├── Models
    │   └── Routes
    │
    ├── Payment
    │
    └── Inventory
```

---

**23. And If You Want Micro Frontend Later**

This architecture also gives you a path toward micro frontends.

Today:

```text
                Angular Application
                       │
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
     Customer         Order         Payment
      Feature         Feature        Feature
```

Later:

```text
                    Shell
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
   Customer MFE    Order MFE    Payment MFE
```

Using technologies such as:

```text
Module Federation
single-spa
```

The business boundaries you've already established make this transition much easier.

---

**24. The Full Mental Model**

Now connect everything you've learned:

```text
                 FRONTEND
                  Angular
                     │
        ┌────────────┼────────────┐
        ▼            ▼            ▼
    Customer       Order        Payment
     Feature       Feature       Feature
        │            │            │
      Facade       Facade       Facade
        │            │            │
     API Service  API Service  API Service
        │            │            │
        └────────────┼────────────┘
                     │
                  HTTP
                     │
                     ▼
                 BACKEND
                ASP.NET Core
                     │
        ┌────────────┼────────────┐
        ▼            ▼            ▼
    Customer        Order       Payment
     Module         Module        Module
        │            │            │
    MediatR       MediatR       MediatR
        │            │            │
        ▼            ▼            ▼
     Domain       Domain        Domain
        │            │            │
        ▼            ▼            ▼
      DB            DB           DB
```

And when asynchronous communication is required:

```text
Order
  │
  ▼
Domain Event
  │
  ▼
Outbox
  │
  ▼
RabbitMQ / Azure Service Bus
  │
  ├──────────► Payment
  │
  └──────────► Notification
```
-----------
-----------

## Session Management

For a modern **Angular + .NET enterprise application**, I would generally recommend:

> Short-lived access token + refresh token + server-side/session control where required.

And if you're using Microsoft identity, Entra ID is another strong option.

---

**1. First understand the problem**

Suppose the user logs in:

```text
Angular
   │
   │ Login
   ▼
.NET API
   │
   ▼
Authentication
   │
   ▼
Token / Session
```

After that, every API call needs to know:

```text
Who is this user?
Is the session still valid?
What permissions does the user have?
```

---

**2. There are 3 common approaches**

***Approach 1 — Cookie-based session***

Traditional:

```text
Angular
   │
   │ Cookie
   ▼
.NET
   │
   ▼
Server Session
```

***Approach 2 — JWT access token***

Common for SPAs/APIs:

```text
Angular
   │
   │ Authorization: Bearer <token>
   ▼
.NET API
```

***Approach 3 — OAuth2/OIDC / Entra ID***

Enterprise recommendation:

```text
Angular
   │
   ▼
Microsoft Entra ID
   │
   ▼
Access Token
   │
   ▼
.NET API
```

For the type of applications you've been discussing, **#2 or #3 is usually the direction I'd recommend.**

---

**3. JWT-based approach**

Imagine login:

```text
POST /api/auth/login
```

Angular sends:

```json
{
  "username": "swapnil",
  "password": "******"
}
```

.NET validates the credentials.

Then returns something like:

```json
{
  "accessToken": "...",
  "expiresIn": 900,
  "refreshToken": "..."
}
```

Now Angular can call:

```text
GET /api/orders
```

with:

```http
Authorization: Bearer eyJ...
```

---

**4. Access Token vs Refresh Token**

This is one of the most important things to understand.

***Access token***

Short-lived.

For example:

```text
15 minutes
```

Used for:

```text
Angular → .NET API
```

***Refresh token***

Longer-lived.

For example:

```text
days/weeks
```

Used to obtain a new access token.

```text
             Access Token
             15 minutes
                  │
                  ▼
Angular ─────────► .NET API
```

When it expires:

```text
Angular
   │
   │ Refresh Token
   ▼
.NET / Identity Provider
   │
   ▼
New Access Token
```

The user doesn't have to log in again.

---

**5. Where should Angular store the token?**

This is where security becomes important.

You may hear:

```text
localStorage
sessionStorage
cookie
memory
```

For sensitive authentication tokens, don't blindly choose `localStorage`.

Why?

If your application has an XSS vulnerability:

```text
Malicious JavaScript
       │
       ▼
localStorage
       │
       ▼
Access Token stolen
```

A more secure browser architecture is often:

```text
Access token
     ↓
Memory

Refresh token
     ↓
Secure + HttpOnly + SameSite cookie
```

The browser prevents JavaScript from directly reading the HttpOnly refresh token.

---

**6. Angular Architecture**

I would have:

```text
core/
│
├── auth/
│   ├── auth.service.ts
│   ├── auth.store.ts
│   ├── auth.models.ts
│   └── auth.guard.ts
│
└── interceptors/
    ├── auth.interceptor.ts
    └── error.interceptor.ts
```

---

**7. Auth Service**

Conceptually:

```typescript
@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly accessToken =
    signal<string | null>(null);

  login(credentials: LoginRequest) {
    // call login API
  }

  logout() {
    // clear token
    // call backend logout if required
  }

  getAccessToken() {
    return this.accessToken();
  }

  isAuthenticated() {
    return this.accessToken() !== null;
  }
}
```

You can use Angular Signals for the local authentication state.

---

**8. HTTP Interceptor**

You don't want to manually do this:

```typescript
http.get(url, {
  headers: {
    Authorization: `Bearer ${token}`
  }
});
```

for every request.

Instead:

```text
Angular
   │
   ▼
HttpClient
   │
   ▼
AuthInterceptor
   │
   ├── Add Authorization header
   │
   ▼
.NET API
```

Conceptually:

```typescript
export const authInterceptor: HttpInterceptorFn =
  (req, next) => {

    const auth = inject(AuthService);

    const token = auth.getAccessToken();

    if (!token) {
      return next(req);
    }

    return next(
      req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    );
  };
```

---

**9. What Happens When Token Expires?**

This is where enterprise applications need more thought.

Suppose:

```text
Access Token
expires
```

Angular calls:

```text
GET /orders
```

.NET returns:

```http
401 Unauthorized
```

Angular interceptor sees:

```text
401
 ↓
Try refresh
 ↓
Get new access token
 ↓
Retry original request
```

Conceptually:

```text
Angular
   │
   │ GET /orders
   ▼
.NET
   │
   │ 401
   ▼
Interceptor
   │
   │ POST /auth/refresh
   ▼
.NET
   │
   │ New Access Token
   ▼
Interceptor
   │
   │ Retry GET /orders
   ▼
.NET
```

This is the basic refresh-token flow.

---

**10. Important: Prevent Multiple Refresh Calls**

Imagine 10 API calls happen simultaneously:

```text
Request 1 → 401
Request 2 → 401
Request 3 → 401
...
Request 10 → 401
```

You don't want:

```text
10 refresh requests
```

Instead:

```text
Request 1 ──┐
Request 2 ──┤
Request 3 ──┤
             ▼
        One Refresh
             │
             ▼
        New Token
             │
      ┌──────┼──────┐
      ▼      ▼      ▼
     R1     R2     R3...
```

This is a good senior-level interview topic.

---

**11. .NET Authentication**

On the .NET side:

```text
Program.cs
    │
    ▼
AddAuthentication()
    │
    ▼
JWT Bearer
    │
    ▼
AddAuthorization()
```

Conceptually:

```csharp
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "...";
        options.Audience = "...";
    });

builder.Services.AddAuthorization();
```

Then:

```csharp
[Authorize]
[HttpGet]
public IActionResult GetOrders()
{
    ...
}
```

.NET validates the token.

---

**12. Authentication vs Authorization**

Don't mix these up.

***Authentication***

> Who are you?

```text
JWT
 ↓
User = Swapnil
```

***Authorization***

> What are you allowed to do?

For example:

```text
Swapnil
  │
  ├── CanReadOrders
  ├── CanCreateOrders
  └── CanCancelOrders
```

Use policies:

```csharp
[Authorize(Policy = "CanCancelOrder")]
```

rather than scattering:

```csharp
if (user.Role == "Admin")
```

everywhere.

---

**13. Claims**

The access token can contain claims such as:

```text
sub = user-id
name = Swapnil
email = ...
role = Admin
tenantId = ...
```

.NET can access them:

```csharp
var userId =
    User.FindFirst("sub")?.Value;
```

But don't trust arbitrary client-provided claims.

The token must be issued by a trusted identity provider and validated correctly.

---

**14. Where Does Session Actually Exist?**

This is an important conceptual point.

With JWT:

```text
.NET API
```

doesn't necessarily maintain a traditional server session.

Instead:

```text
Request
   ↓
JWT
   ↓
Validate signature
   ↓
Validate expiration
   ↓
Create HttpContext.User
```

This is largely **stateless authentication**.

That's one reason JWT is popular for APIs.

---

**15. But What If I Need to Force Logout?**

Here's the problem.

Suppose:

```text
Access token = valid for 15 minutes
```

Admin disables the user immediately.

The token may technically still be valid until it expires.

You have several approaches.

***Short access-token lifetime***

For example:

```text
10–15 minutes
```

***Token revocation / session store***

Maintain:

```text
UserSessions
```

or:

```text
RefreshTokens
```

in a database/Redis.

Then:

```text
User disabled
     ↓
Invalidate refresh token/session
     ↓
Access token expires
     ↓
Cannot obtain another token
```

For more immediate revocation, additional server-side checks may be used, but that trades away some of JWT's statelessness.

---

**16. Session Management with Redis**

For a large enterprise system, you may have:

```text
Angular
   │
   ▼
Load Balancer
   │
   ├──────────────┐
   ▼              ▼
API Server 1    API Server 2
   │              │
   └──────┬───────┘
          ▼
        Redis
```

Redis can store things such as:

```text
Refresh sessions
Revoked tokens
Rate-limit counters
Temporary session state
Distributed cache
```

Now you don't depend on one particular API server.

---

**17. Traditional ASP.NET Session**

You might also hear:

```csharp
HttpContext.Session
```

This is different.

It is **server-side application session state**, for example:

```text
Session["CartId"]
Session["SomeTemporaryValue"]
```

For a modern Angular + Web API enterprise application, I would generally **not use ASP.NET Session as the primary authentication mechanism**.

I'd use:

```text
OAuth2/OIDC
+
Access Token
+
Refresh Token
```

and use Redis/database only when server-side session control is actually required.

---

**18. Enterprise Option: Microsoft Entra ID**

Since you're already working with Azure concepts, this is particularly important for interviews.

Instead of your .NET application implementing:

```text
Username
Password
Hash
Login
Token generation
Refresh token
```

your identity provider can handle authentication.

Architecture:

```text
             ┌──────────────────┐
             │ Microsoft Entra  │
             │       ID         │
             └────────┬─────────┘
                      │
                Access Token
                      │
                      ▼
Angular ───────────► .NET API
                      │
                      ▼
                 Authorization
```

Angular authenticates through the identity provider.

.NET validates the access token.

This is often the preferred enterprise architecture.

---

**19. Entra + Angular**

Conceptually:

```text
Angular
   │
   │ Login
   ▼
Entra ID
   │
   │ Access Token
   ▼
Angular
   │
   │ Bearer Token
   ▼
.NET API
```

Angular commonly uses Microsoft's MSAL libraries for this scenario.

You don't normally implement your own password authentication system when an enterprise identity provider is already available.

---

**20. Multi-Tab Problem**

Suppose user has:

```text
Tab 1
Tab 2
Tab 3
```

and logs out from Tab 1.

Should Tabs 2 and 3 remain authenticated?

This is another session-management consideration.

You can coordinate browser tabs using mechanisms such as:

```text
BroadcastChannel
storage events
```

so that:

```text
Tab 1
  ↓
Logout
  ↓
Broadcast
  ↓
Tab 2 → Logout
Tab 3 → Logout
```

---

**21. Idle Timeout**

Enterprise applications may require:

> Log the user out after 30 minutes of inactivity.

You can implement:

```text
Last activity
      ↓
Timer
      ↓
30 minutes
      ↓
Warning
      ↓
Logout
```

But be careful not to simply rely on Angular for security.

The **server/token expiration remains authoritative**.

---

**22. A Good Enterprise Architecture**

Putting everything together:

```text
                         ┌──────────────────┐
                         │  Entra ID / IdP  │
                         └────────┬─────────┘
                                  │
                           Access Token
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────┐
│                      ANGULAR                            │
│                                                         │
│  AuthService                                            │
│      │                                                  │
│      ▼                                                  │
│  Auth State                                             │
│      │                                                  │
│      ▼                                                  │
│  HTTP Interceptor                                       │
│      │                                                  │
│      ▼                                                  │
│  Feature APIs                                           │
└────────────────────────┬────────────────────────────────┘
                         │
                    Bearer Token
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│                     ASP.NET CORE                        │
│                                                         │
│ Authentication                                          │
│       ↓                                                 │
│ Authorization                                           │
│       ↓                                                 │
│ Middleware                                              │
│       ↓                                                 │
│ Modular Monolith                                        │
│                                                         │
│ Customer │ Order │ Payment │ Inventory                  │
└────────────────────────┬────────────────────────────────┘
                         │
                    Optional
                         │
                         ▼
                  ┌─────────────┐
                  │ Redis / DB  │
                  │             │
                  │ Sessions    │
                  │ Revocation  │
                  │ Cache       │
                  └─────────────┘
```

---
---

## OAuth 2.0

> OAuth 2.0 is primarily an authorization protocol. OpenID Connect (OIDC) adds authentication on top of OAuth 2.0. Microsoft Entra ID supports both.

For an Angular SPA, the modern recommended flow is **Authorization Code + PKCE**, not the old implicit flow. Microsoft specifically recommends authorization code + PKCE for SPAs.

---

**1. The Architecture We Are Building**

Let's assume:

```text
Angular 21
     │
     │
     ▼
Microsoft Entra ID
     │
     │ Access Token
     ▼
.NET 10 Web API
     │
     ▼
Modular Monolith
     │
 ┌───┼───────────┐
 ▼   ▼           ▼
Customer Order Payment
     │
     ▼
 SQL Server
```

There are **three major parties** you need to understand:

```text
Angular
   │
   │ OAuth/OIDC
   ▼
Entra ID
   │
   │ Access Token
   ▼
.NET API
```

For our application:

| Role | Our component |
|---|---|
| User / Resource Owner | Person using Angular |
| Client | Angular SPA |
| Authorization Server | Microsoft Entra ID |
| Resource Server | .NET API |

---

**2. First Important Question: Why Do We Need Entra?**

Without Entra, you could build:

```text
Angular
   ↓
POST /login
   ↓
.NET
   ↓
Check username/password
   ↓
Generate JWT
```

Your .NET application would need to manage:

```text
Users
Passwords
Password hashing
MFA
Login
Token generation
Password reset
Account lockout
SSO
Identity federation
etc.
```

That's a lot.

With Entra:

```text
Angular
   ↓
Entra ID
   ↓
User authentication
   ↓
Tokens
   ↓
.NET API
```

Entra becomes your **Identity Provider / Authorization Server**.

Your application doesn't need to manage the user's password.

---

**3. OAuth 2.0 vs OpenID Connect**

This causes a lot of confusion in interviews.

***OAuth 2.0***

Primarily answers:

> Is this application allowed to access this API/resource?

It gives you an:

```text
Access Token
```

***OpenID Connect***

Answers:

> Who is the user?

It introduces:

```text
ID Token
```

So:

```text
OAuth 2.0
    ↓
Authorization
    ↓
Access Token


OIDC
    ↓
Authentication
    ↓
ID Token
```

---

**4. The Three Tokens You Should Know**

There are three important tokens in the Entra world.

```text
                 Entra ID
                    │
        ┌───────────┼───────────┐
        ▼           ▼           ▼
   Access Token  ID Token  Refresh Token
```

`Access Token`

Used to call APIs.

```text
Angular
   │
   │ Authorization: Bearer <access_token>
   ▼
.NET API
```

`ID Token`

Tells the client about the authenticated user.

For example:

```text
name
email
oid
tid
```

You **do not use the ID token to call your .NET API**. 

`Refresh Token`

Used to obtain new tokens when appropriate.

```text
Access Token
      ↓
expires
      ↓
MSAL obtains fresh token
```

---

**5. Now Let's Walk Through the Actual Login**

Imagine the user opens:

```text
https://myapp.com
```

Angular loads.

User clicks:

```text
Login
```

Angular doesn't send the username/password to your .NET API.

Instead:

```text
Angular
   │
   │ Redirect
   ▼
Microsoft Entra ID
```

---

**6. Angular Sends Authorization Request**

The browser gets redirected to an Entra `/authorize` endpoint.

Conceptually:

```text
https://login.microsoftonline.com/
    {tenant}/oauth2/v2.0/authorize
```

with parameters such as:

```text
client_id
response_type=code
redirect_uri
scope
state
code_challenge
code_challenge_method=S256
```

The important ones are:

***client_id***

Identifies your Angular application.

```text
Angular App
Client ID:
xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx
```

***redirect_uri***

Where Entra should return the browser after authentication.

Example:

```text
https://myapp.com/auth/callback
```

You register this URI in the Entra App Registration.

---

**7. What Is PKCE?**

This is **very important for SPA interviews**.

PKCE =

> Proof Key for Code Exchange

Angular generates:

```text
code_verifier
```

Then calculates:

```text
code_challenge
```

from it.

Conceptually:

```text
Angular

code_verifier
     │
     ▼
SHA256
     │
     ▼
code_challenge
```

Angular sends only:

```text
code_challenge
```

to Entra.

The original:

```text
code_verifier
```

is kept by the client.

---

**8. User Goes to Microsoft Login**

Now the browser is on:

```text
Microsoft Entra ID
```

User enters:

```text
Email
Password
MFA
```

Potentially:

```text
Microsoft Authenticator
Conditional Access
SSO
etc.
```

Your Angular application never sees the user's Microsoft password.

That's a major security benefit.

---

**9. Consent**

Suppose your Angular application requests:

```text
api://my-api/orders.read
api://my-api/orders.write
```

Entra may ask the user/admin for consent.

Conceptually:

```text
My Application wants:

✓ Read orders
✓ Create orders
✓ View profile
```

The user/admin approves.

---

**10. Entra Redirects Back to Angular**

After successful authentication:

```text
Entra
   │
   │ authorization code
   ▼
Angular
```

Something conceptually like:

```text
https://myapp.com/auth/callback
    ?code=ABC123
    &state=XYZ
```

Notice:

> The browser gets an authorization code, not the access token directly.

That's the authorization-code flow.

---

**11. Why `state`?**

The `state` parameter helps protect the authorization flow against CSRF-related attacks.

Angular generates:

```text
state = random-value
```

sends it to Entra.

Entra sends it back.

Angular verifies:

```text
Returned state
      ==
Original state
```

If not:

```text
❌ Reject authentication
```

---

**12. Now the Important Part: Code → Token**

Angular/MSAL now has:

```text
authorization_code
```

and:

```text
code_verifier
```

It sends them to the token endpoint.

Conceptually:

```text
Angular
   │
   │ code
   │ code_verifier
   ▼
Entra /token
```

Entra checks:

```text
Is this code valid?
Is it being redeemed by the right client?
Does code_verifier match code_challenge?
Is redirect_uri correct?
```

If everything is valid:

```text
Entra
   │
   ├── Access Token
   ├── ID Token
   └── Refresh Token / token renewal capability
   ▼
Angular/MSAL
```

---

**13. Now Angular Has an Access Token**

Suppose Angular wants:

```text
GET /api/orders
```

It sends:

```http
Authorization: Bearer eyJ...
```

So:

```text
Angular
   │
   │ Access Token
   ▼
.NET API
```

---

**14. .NET Does NOT Ask Entra on Every Request**

This is a common misconception.

You don't normally have:

```text
Angular
  ↓
.NET
  ↓
Entra
  ↓
.NET
  ↓
Response
```

for every API request.

Instead:

```text
Angular
   │
   │ Access Token
   ▼
.NET API
   │
   ▼
Validate Token
   │
   ▼
Authorize
   │
   ▼
Controller
```

The .NET authentication middleware validates the token using Entra's metadata/signing keys.

Microsoft recommends using the Microsoft-supported libraries/middleware such as `Microsoft.Identity.Web` for ASP.NET Core APIs.

---

**15. What Does .NET Validate?**

Conceptually, .NET checks things such as:

```text
Signature
Issuer
Audience
Expiration
Tenant
Scopes / roles
```

For example:

```text
Token

iss = Entra
aud = My API
exp = future
tid = my tenant
scp = orders.read
```

If valid:

```text
HttpContext.User
```

is populated.

---

**16. Audience Is VERY Important**

Imagine you have:

```text
Angular
     ↓
Order API
```

The access token should be intended for:

```text
Order API
```

not:

```text
Microsoft Graph
```

This is why you should not take an arbitrary token and accept it.

The resource server should validate that the token was issued for the resource it protects. Microsoft specifically warns about accepting tokens intended for another resource.

---

**17. Scope**

Suppose your API exposes:

```text
Orders.Read
Orders.Write
Orders.Delete
```

Angular asks Entra for:

```text
Orders.Read
```

The resulting access token may contain:

```text
scp = Orders.Read
```

Your API can enforce:

```csharp
[Authorize(
    Policy = "Orders.Read"
)]
```

Conceptually:

```text
Access Token
      │
      ▼
scp = Orders.Read
      │
      ▼
Can call GET /orders
```

But:

```text
scp = Orders.Read
```

doesn't mean:

```text
Can DELETE /orders
```

---

**18. Roles**

You can also have application roles.

For example:

```text
Admin
Manager
Employee
```

Token can contain role information.

Then:

```csharp
[Authorize(Roles = "Admin")]
```

or preferably policy-based authorization for more complex rules.

So:

```text
Authentication
       ↓
Who are you?
       ↓
Authorization
       ↓
What can you do?
```

---

**19. Complete Flow**

Now let's put everything together.

```text
                  ┌───────────────────┐
                  │   Microsoft       │
                  │    Entra ID       │
                  └─────────┬─────────┘
                            │
                       1. Login
                            │
                            ▼
┌──────────────┐      Authorization
│              │ ───────── Request
│   Angular    │
│    SPA       │
│              │ ◄────── 2. Auth Code
└──────┬───────┘
       │
       │ 3. Code + PKCE verifier
       │
       └─────────────────────────────► Entra
                                         │
                                         │
                              4. Tokens
                                         │
             ┌───────────────────────────┘
             │
             ▼
       Angular / MSAL
             │
             │ 5. Access Token
             ▼
      ┌───────────────┐
      │   .NET API    │
      └───────┬───────┘
              │
       6. Validate Token
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
              ▼
       Modular Monolith
              │
       ┌──────┼─────────┐
       ▼      ▼         ▼
    Customer Order    Payment
```

---

**20. Where Does MSAL Fit?**

You should **not manually implement all these OAuth requests** in Angular.

Microsoft recommends using Microsoft Authentication Libraries rather than manually crafting the protocol requests. 

For Angular:

```text
Angular
   │
   ▼
MSAL Angular
   │
   ▼
MSAL Browser
   │
   ▼
Microsoft Entra ID
```

MSAL handles things like:

```text
Login
Authorization code
PKCE
Token acquisition
Token caching
Silent token acquisition
Redirect/popup handling
Logout
```

Your application uses the library rather than implementing OAuth from scratch.

---

**21. Angular Architecture**

I'd structure it like:

```text
core/
│
├── auth/
│   ├── auth.service.ts
│   ├── auth.guard.ts
│   └── auth.config.ts
│
├── interceptors/
│   ├── auth.interceptor.ts
│   └── error.interceptor.ts
│
└── authorization/
    └── permission.service.ts
```

Conceptually:

```text
Angular Component
       │
       ▼
AuthService
       │
       ▼
MSAL
       │
       ▼
Entra ID
```

And API:

```text
Component
   ↓
OrderFacade
   ↓
OrderApiService
   ↓
HttpClient
   ↓
MSAL Interceptor
   ↓
Access Token
   ↓
.NET API
```

---

**22. .NET Architecture**

For .NET:

```text
Program.cs
    │
    ├── Microsoft.Identity.Web
    │
    ├── Authentication
    │
    └── Authorization
             │
             ▼
       Modular Monolith
```

Typical configuration conceptually looks like:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        builder.Configuration.GetSection(
            "AzureAd"));

builder.Services.AddAuthorization();
```

Then:

```csharp
[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
}
```

Your controller doesn't need to manually decode JWTs.

---

**23. App Registrations**

This is another important interview topic.

You generally have two application registrations in this architecture:

```text
Microsoft Entra

1. Angular SPA
2. .NET Web API
```

Think:

```text
              Entra ID
                 │
       ┌─────────┴─────────┐
       ▼                   ▼
 Angular App            .NET API
 Client ID              Application ID
       │                   │
       │                   │
       └────── scopes ─────┘
```

Angular is the **client application**.

.NET is the **resource/API**.

---

**24. .NET API Exposes Scopes**

Suppose your .NET API has:

```text
Application ID URI:

api://1234-5678
```

It can expose:

```text
Orders.Read
Orders.Write
```

So the fully qualified scope could look conceptually like:

```text
api://1234-5678/Orders.Read
```

Angular requests:

```text
scope =
api://1234-5678/Orders.Read
```

Entra knows:

```text
Angular wants access to
       ↓
Order API
       ↓
Orders.Read
```

---

**25. Consent**

The first time Angular requests access:

```text
Angular
    ↓
Entra
    ↓
"Allow this app to access Orders?"
```

Depending on tenant/admin settings and permissions, the user or administrator grants consent.

After that:

```text
Angular
    ↓
Entra
    ↓
Access Token
```

---

**26. Refresh / Silent Authentication**

Now imagine:

```text
Access Token expires
```

Angular should not necessarily show the login screen again.

MSAL attempts to acquire a new token silently.

Conceptually:

```text
Angular
   │
   │ acquireTokenSilent()
   ▼
MSAL
   │
   ▼
Entra / token infrastructure
   │
   ▼
New Access Token
```

The exact browser/session behavior depends on the MSAL configuration and Entra session state; the key concept is that your application should use the library's token acquisition mechanism rather than manually handling refresh-token protocol details in browser code.

---

**27. Logout**

User clicks:

```text
Logout
```

Angular/MSAL signs the user out from the application/identity-provider session as configured.

Conceptually:

```text
Angular
   │
   ▼
MSAL logout
   │
   ▼
Entra sign-out
   │
   ▼
Angular login state cleared
```

Important:

> Logging out of your Angular application and terminating every possible Entra/browser session are related but not necessarily identical operations.

---

**28. What Happens if Someone Steals the Access Token?**

This is why tokens are sensitive.

An access token is effectively:

```text
Bearer credential
```

Meaning:

> Whoever possesses it may be able to use it until it expires, subject to the API's validation and authorization rules.

Microsoft specifically recommends treating access tokens as sensitive credentials. 

Therefore:

```text
HTTPS
+
Short-lived access tokens
+
Secure token handling
+
XSS protection
+
Proper CSP/security headers
+
Correct OAuth flow
```

are important.

---

**29. What About Client Secret?**

This is a **very important interview question**.

For Angular SPA:

```text
❌ Don't put client_secret in Angular
```

Why?

Because Angular runs in the user's browser.

Anything shipped to the browser isn't a real secret.

That's why SPA uses:

```text
Authorization Code
+
PKCE
```

rather than a client secret.

---

**30. What About .NET → Another API?**

Now we get into another excellent enterprise concept.

Suppose:

```text
Angular
   ↓
.NET API
   ↓
Microsoft Graph
```

or:

```text
.NET Order API
   ↓
Payment API
```

There are different OAuth flows depending on whether the downstream API needs the **user's identity** or the **application's identity**.

***Application identity***

Use:

```text
Client Credentials
```

Example:

```text
Order API
   ↓
Payment API
```

No user interaction.


***User identity passed downstream***

You may use:

```text
On-Behalf-Of (OBO)
```

Conceptually:

```text
User
 │
 ▼
Angular
 │
 ▼
Order API
 │
 │ OBO
 ▼
Downstream API
```

This is a more advanced Entra topic, but very relevant for enterprise interviews.

---
---

## OAuth 2.0

**1. What is OAuth 2.0?**

OAuth 2.0 answers this problem:

> How can Application A access Resource/API B on behalf of a user without knowing the user's password?

For example:

```text
             User
              │
              │ wants to use
              ▼
        ┌─────────────┐
        │ Application │
        │      A      │
        └──────┬──────┘
               │
               │ wants access
               ▼
        ┌─────────────┐
        │   API /     │
        │  Resource   │
        │      B      │
        └─────────────┘
```

OAuth introduces a trusted **Authorization Server**:

```text
                    Authorization Server
                           │
                  grants permission
                           │
                           ▼
Application A ───────► Access Token ───────► API B
```

The important thing is:

> The application gets an access token instead of the user's password.

---

**2. Simple Real-World Example**

Imagine you build an application:

```text
MyPhotoApp
```

You want users to access their Google Photos.

Without OAuth:

```text
MyPhotoApp asks:

"Give me your Google username and password."
```

That's terrible.

Now with OAuth:

```text
MyPhotoApp
     │
     │ "I need access to photos"
     ▼
Authorization Server
     │
     │ User logs in
     │ User grants permission
     ▼
Access Token
     │
     ▼
Google Photos API
```

MyPhotoApp never needs the Google password.

---

**3. OAuth Is NOT Entra ID**

This distinction is important.

Think:

```text
OAuth 2.0
   │
   │ is a protocol/specification
   │
   ├───────────────┐
   ▼               ▼
Entra ID          Google
   │               │
OAuth/OIDC       OAuth/OIDC
```

Other identity providers can implement OAuth 2.0 too.

Examples:

- Microsoft Entra ID
- Google Identity
- Auth0
- Okta
- Keycloak
- Amazon Cognito

So:

> OAuth 2.0 is the protocol. Entra ID is a service that implements OAuth 2.0 (and OpenID Connect).

---

**4. OAuth 2.0 Actors**

There are four terms you should know for interviews.

`1. Resource Owner`

Usually:

```text
User
```

The person who owns the data.

`2. Client`

The application requesting access.

For your application:

```text
Angular SPA
```

`3. Authorization Server`

The server that authenticates/authorizes and issues tokens.

Examples:

```text
Entra ID
Google
Auth0
Okta
```

`4. Resource Server`

The API containing protected resources.

For your application:

```text
.NET API
```

So:

```text
Resource Owner
      │
      ▼
   Client
      │
      │ asks authorization
      ▼
Authorization Server
      │
      │ Access Token
      ▼
Resource Server
```

---

**5. Now Apply OAuth to Your Angular + .NET Application**

Forget Entra for a moment.

Imagine you have:

```text
Angular
.NET API
Some Authorization Server
```

User opens Angular:

```text
Angular
   │
   │ "I need access to Orders"
   ▼
Authorization Server
```

The authorization server authenticates the user.

Then:

```text
Authorization Server
        │
        │ Access Token
        ▼
     Angular
```

Angular calls:

```http
GET /api/orders
Authorization: Bearer eyJ...
```

.NET API validates the token.

```text
Angular
   │
   │ Access Token
   ▼
.NET API
   │
   ├── Valid?
   ├── Not expired?
   ├── Correct audience?
   └── Has required scope?
          │
          ▼
        Allow
```

**That's OAuth 2.0 in action.**

---

**6. Where Does the Password Go?**

This is the beauty of OAuth.

Angular doesn't do:

```text
Angular → .NET
username
password
```

Instead:

```text
Angular
   │
   │ "I need authorization"
   ▼
Authorization Server
   │
   │ User logs in here
   │
   │ password
   │ MFA
   │ biometric
   │ etc.
   │
   ▼
Authorization Code
   │
   ▼
Angular
   │
   ▼
Access Token
   │
   ▼
.NET API
```

The client doesn't need to know the user's password.

---

**7. Why Do We Need Authorization Codes?**

Suppose Angular directly receives an access token from the browser login.

That's historically what the **Implicit Flow** was about.

Modern applications instead generally use:

```text
Authorization Code Flow
```

For browser applications:

```text
Authorization Code
        +
       PKCE
```

So:

```text
Angular
   │
   │ Authorization Request
   ▼
Authorization Server
   │
   │ Authorization Code
   ▼
Angular
   │
   │ Code + PKCE verifier
   ▼
Authorization Server
   │
   │ Access Token
   ▼
Angular
```

That's the flow you saw earlier with Entra.

But **the flow itself is OAuth 2.0**.

Entra is simply the authorization server implementing it.

---

**8. Why PKCE?**

Imagine an attacker somehow steals the authorization code.

They try:

```text
Attacker
   │
   │ stolen code
   ▼
Authorization Server
```

But the legitimate Angular application has a secret value called:

```text
code_verifier
```

The authorization server requires the correct verifier.

So:

```text
Authorization Code
       +
Code Verifier
       ↓
Access Token
```

The attacker doesn't have the verifier.

Therefore:

```text
❌ Stolen code alone isn't enough
```

That's the purpose of PKCE at a conceptual level.

---

**9. OAuth Scopes**

This is another important concept.

Suppose your .NET API supports:

```text
Orders.Read
Orders.Write
Orders.Delete
```

Angular requests:

```text
Orders.Read
```

The authorization server issues an access token containing permission information.

Conceptually:

```text
Access Token

User: Swapnil
API: Order API

Scopes:
    Orders.Read
```

Then:

```text
GET /orders
```

is allowed.

But:

```text
DELETE /orders/123
```

might require:

```text
Orders.Delete
```

and therefore be rejected.

So:

> Scopes represent delegated permissions requested by a client to access a protected resource.

---

**10. OAuth Flow in One Picture**

This is the picture I'd memorize:

```text
                       USER
                         │
                         │ wants to use
                         ▼
                 ┌───────────────┐
                 │    CLIENT     │
                 │   Angular     │
                 └───────┬───────┘
                         │
                         │ Authorization Request
                         ▼
                 ┌───────────────┐
                 │ AUTHORIZATION │
                 │    SERVER     │
                 └───────┬───────┘
                         │
                    User Login
                         │
                         │ Authorization Code
                         ▼
                 ┌───────────────┐
                 │    CLIENT     │
                 └───────┬───────┘
                         │
                  Code + PKCE
                         │
                         ▼
                 ┌───────────────┐
                 │ AUTHORIZATION │
                 │    SERVER     │
                 └───────┬───────┘
                         │
                    Access Token
                         │
                         ▼
                 ┌───────────────┐
                 │ RESOURCE      │
                 │ SERVER        │
                 │ .NET API      │
                 └───────────────┘
```

That is the **OAuth 2.0 authorization-code flow**.

---
---

## Security

```text
                    SECURITY
                       │
       ┌───────────────┼────────────────┐
       │               │                │
   Identity        Application       Infrastructure
       │               │                │
   Authentication   Authorization     HTTPS
   OAuth/OIDC       Validation        Secrets
   MFA              CSRF              Network
   Tokens           XSS               Headers
                    Injection
                    Logging
```

For your Angular + .NET modular-monolith architecture, I would design it like this.

---

**1. First: Authentication**

---

**2. Authorization**

---

**3. Angular Security Architecture**

I would have:

```text
src/app/core/

├── auth/
│   ├── auth.service.ts
│   ├── auth.guard.ts
│   └── auth-state.ts
│
├── interceptors/
│   ├── auth.interceptor.ts
│   └── error.interceptor.ts
│
├── security/
│   └── permission.service.ts
│
└── config/
```

Then:

```text
Feature
   ↓
Facade
   ↓
API Service
   ↓
HttpClient
   ↓
Auth Interceptor
   ↓
.NET API
```

---

**4. Protect Angular Routes**

Suppose:

```text
/admin
/orders
/customers
```

You don't want unauthenticated users accessing them.

Use route guards.

Conceptually:

```typescript
export const authGuard: CanActivateFn = () => {

  const auth = inject(AuthService);

  if (auth.isAuthenticated()) {
    return true;
  }

  return auth.login();
};
```

Routes:

```typescript
export const routes: Routes = [

  {
    path: 'orders',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/order/order.routes')
        .then(x => x.ORDER_ROUTES)
  },

  {
    path: 'admin',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/admin/admin.routes')
        .then(x => x.ADMIN_ROUTES)
  }
];
```

But remember:

> Route guards are not a security boundary.

They prevent navigation in the UI. The API must still enforce authorization.

---

**5. Role/Permission-Based UI**

Suppose:

```text
Admin → Delete Order
Manager → Edit Order
Employee → View Order
```

Angular can hide buttons:

```html
<button *ngIf="permissions.canDeleteOrder">
    Delete
</button>
```

Or with a custom directive:

```html
<button *appHasPermission="'Orders.Delete'">
    Delete
</button>
```

This improves UX.

But the API still needs:

```csharp
[Authorize(Policy = "Orders.Delete")]
```

---

**6. Token Security**

This is a very important area.

Avoid casually doing:

```typescript
localStorage.setItem(
   'accessToken',
   token
);
```

Why?

If an XSS vulnerability exists:

```text
Malicious JavaScript
       ↓
localStorage
       ↓
Access Token
       ↓
Attacker
```

For an enterprise SPA, use the identity library's recommended token-cache/security approach and minimize token exposure to application JavaScript.

For refresh/session credentials, **HttpOnly + Secure + appropriate SameSite cookies** can be used where the architecture supports them.

The key principle is:

> Don't expose long-lived credentials to JavaScript unnecessarily.

---

**7. XSS — Cross-Site Scripting**

Angular provides strong protection against many common XSS cases because its template system escapes values by default.

For example:

```html
<div>
    {{ userName }}
</div>
```

Angular doesn't simply inject arbitrary HTML.

But developers can bypass those protections.

Be careful with:

```typescript
innerHTML
```

and especially:

```typescript
bypassSecurityTrustHtml()
```

Don't use them unless absolutely necessary and the content has been properly controlled/sanitized.

---

**8. Don't Trust User Input**

Suppose Angular sends:

```json
{
    "name": "<script>...</script>"
}
```

Your .NET backend must still treat it as untrusted.

Never assume:

> Angular already validated it.

Client-side validation is for UX.

Server-side validation is for security and correctness.

---

**9. .NET Input Validation**

Use request DTOs:

```csharp
public class CreateOrderRequest
{
    public int CustomerId { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; }
}
```

Validate:

```text
Required
Length
Range
Format
Business rules
```

For complex applications, FluentValidation is commonly used.

Architecture:

```text
HTTP Request
     ↓
DTO Validation
     ↓
Authorization
     ↓
Application
     ↓
Domain
```

---

**10. SQL Injection**

Never construct SQL like:

```csharp
var sql =
    $"SELECT * FROM Orders WHERE Id = {id}";
```

Especially:

```csharp
var sql =
    $"SELECT * FROM Users WHERE Name = '{name}'";
```

Instead use:

**EF Core**

```csharp
var orders = await db.Orders
    .Where(x => x.CustomerId == customerId)
    .ToListAsync();
```

Or parameterized SQL:

```csharp
await db.Database
    .SqlQuery<Order>($"SELECT ... WHERE Id = {id}");
```

The principle:

> Never concatenate untrusted input into SQL.

---

**11. API Security**

Your API should have:

```text
HTTPS
Authentication
Authorization
Validation
Rate limiting
CORS
Security headers
Exception handling
Logging
```

Pipeline concept:

```text
Request
  ↓
HTTPS
  ↓
CORS
  ↓
Authentication
  ↓
Authorization
  ↓
Validation
  ↓
Controller
  ↓
Application
  ↓
Database
```

---

**12. HTTPS**

Always use HTTPS.

```text
https://api.mycompany.com
```

Not:

```text
http://api.mycompany.com
```

Why?

Because otherwise:

```text
Angular
   │
   │ Access Token
   ▼
Network
   │
   └── Attacker can potentially intercept
```

HTTPS encrypts the connection.

---

**13. CORS**

Suppose:

```text
Angular:
https://app.mycompany.com
```

and:

```text
API:
https://api.mycompany.com
```

They are different origins.

Configure CORS explicitly:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("https://app.mycompany.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

Avoid:

```csharp
.AllowAnyOrigin()
```

for a production application unless you have a specific reason.

Especially don't casually combine unrestricted origins with credentials.

---

**14. CSRF**

This becomes particularly important with **cookie-based authentication**.

Imagine:

```text
Browser
   │
   │ automatically sends cookie
   ▼
.NET API
```

A malicious website could potentially cause requests from the user's browser.

That's where CSRF protection becomes important.

If you're using bearer tokens in the Authorization header rather than authentication cookies, classic CSRF risk is different because the browser doesn't automatically attach the Authorization header to another site's request.

But if you use:

```text
HttpOnly authentication cookie
```

then implement appropriate:

```text
SameSite
CSRF tokens
Origin/Referer validation
```

depending on your architecture.

---

**15. Cookie Security**

If cookies are used for authentication/session state:

```http
Set-Cookie:
    HttpOnly
    Secure
    SameSite=Lax/Strict
```

depending on your requirements.

`HttpOnly`

JavaScript cannot read it.

Helps reduce token theft through JavaScript.

`Secure`

Only send over HTTPS.

`SameSite`

Controls cross-site cookie behavior and helps mitigate CSRF.

---

**16. Secrets**

Never put:

```text
Database password
Client secret
API key
Stripe secret
OpenAI secret
```

inside Angular.

This is extremely important.

Anything shipped to the browser is potentially visible to the user.

Bad:

```typescript
const stripeSecret =
    'sk_live_xxxxx';
```

or:

```typescript
environment.ts

API_SECRET = 'xxxxx';
```

Angular environment configuration is **not a secret store**.

---

**17. Environment Configuration**

Angular can have:

```text
environment.development.ts
environment.production.ts
```

but treat them as **configuration**, not secret storage.

Safe-ish:

```typescript
apiUrl:
'https://api.mycompany.com'
```

Not safe:

```typescript
clientSecret:
'THIS_IS_A_SECRET'
```

---

**18. Exception Handling**

Never return:

```json
{
  "error": "SQL connection failed: Server=db01;User=sa;Password=..."
}
```

to Angular.

Instead:

```json
{
  "title": "Something went wrong",
  "status": 500,
  "traceId": "abc-123"
}
```

Use centralized exception middleware:

```text
Controller
    ↓
Exception
    ↓
Global Exception Middleware
    ↓
Log internally
    ↓
Safe response to client
```

---

**19. Logging**

Security logging is important.

Log events like:

```text
Login success
Login failure
Authorization failure
Admin action
Password/MFA changes handled by IdP
Sensitive operation
Suspicious request
```

But **don't log secrets**.

Never:

```text
Access Token
Refresh Token
Password
Client Secret
API Key
```

into logs.

---

**20. Correlation ID**

For enterprise systems, add:

```text
X-Correlation-ID
```

or use distributed tracing.

Example:

```text
Angular
  │
  │ CorrelationId = ABC123
  ▼
.NET API
  │
  ▼
Order Module
  │
  ▼
Database
```

Now if something fails:

```text
ABC123
```

lets you find the request across logs.

This is especially useful with:

```text
Application Insights
OpenTelemetry
Azure Monitor
```

---

**21. Rate Limiting**

Imagine someone sends:

```text
10,000 requests/sec
```

to:

```text
POST /api/login
```

or:

```text
POST /api/orders
```

You need protection.

ASP.NET Core has rate-limiting capabilities.

Conceptually:

```text
Client
  ↓
Rate Limiter
  ↓
Allowed?
  │
  ├── Yes → API
  │
  └── No → 429 Too Many Requests
```

This is useful against abuse and certain denial-of-service patterns.

---

**22. Password Security**

If you're using Entra ID:

> Don't manage user passwords yourself.

Let Entra handle:

```text
Password
MFA
Account lockout
Password reset
Conditional Access
Identity policies
```

If you ever build your own identity system, password storage must use a modern password hashing approach such as ASP.NET Core Identity's password hashing mechanisms—not plain hashing or encryption.

But for your enterprise application:

```text
Angular
   ↓
Entra ID
```

is preferable.

---

**23. API Security Headers**

Your frontend should also have appropriate security headers.

Important examples include:

```text
Content-Security-Policy
Strict-Transport-Security
X-Content-Type-Options
Referrer-Policy
Permissions-Policy
```

Modern browsers use these to reduce various attack surfaces.

For example:

```text
Content-Security-Policy
```

can significantly reduce the impact of certain XSS attacks by restricting where scripts/resources may load from.

---

**24. File Upload Security**

This is often forgotten.

Suppose:

```text
POST /api/documents/upload
```

Don't trust:

```text
file extension
Content-Type
filename
file size
```

alone.

Validate:

```text
Maximum size
Allowed extensions
Actual file type
Content type
Filename
Storage location
Malware scanning where required
```

And don't store arbitrary uploaded files directly under a publicly executable web root.

---

**25. Multi-Tenant Security**

This becomes especially important for enterprise SaaS.

Suppose:

```text
Tenant A
 ├── User A
 └── Orders A

Tenant B
 ├── User B
 └── Orders B
```

User from Tenant A must never see:

```text
Tenant B data
```

Your queries need tenant isolation.

For example:

```csharp
var orders = await db.Orders
    .Where(x =>
        x.TenantId == currentTenantId)
    .ToListAsync();
```

Better still, establish tenant isolation consistently in the architecture rather than relying on individual developers remembering to add a filter.

---

### Identity

```text
OAuth 2.0
OpenID Connect
Entra ID
Authorization Code + PKCE
MFA
```

### Angular

```text
MSAL
Route Guards
HTTP Interceptors
XSS protection
Secure token handling
CSP
No secrets in frontend
```

### API

```text
JWT validation
Authentication
Authorization
Policies
Scopes
Roles
Object-level authorization
Tenant isolation
```

### Input/Data

```text
DTO validation
EF Core / parameterized SQL
File upload validation
Output encoding
```

### Infrastructure

```text
HTTPS
Key Vault
Managed Identity
CORS
Rate limiting
Security headers
Least privilege
```

### Operations

```text
Centralized logging
Application Insights
Correlation IDs
Audit logs
Monitoring
Alerts
Dependency scanning
```
--------
---------

## PERFORMANCE

`Angular`

```text
Lazy loading
Code splitting
OnPush/signals
track
Virtual scrolling
RxJS debounce
Client caching
HTTP caching
Compression
CDN
```

`.NET`

```text
async/await
CancellationToken
DTO projection
AsNoTracking
Pagination
Caching
Redis
Connection pooling
Compression
Background processing
Rate limiting
Efficient serialization
```

`SQL Server`

```text
Proper indexes
Execution plans
Query optimization
Pagination
Avoid SELECT *
Avoid N+1
Avoid unnecessary functions on indexed columns
Short transactions
Lock analysis
Statistics/index maintenance
```

***Tools:***

Frontend
────────
Chrome DevTools
Angular DevTools
Lighthouse


.NET
────
Application Insights
OpenTelemetry
dotnet-counters
BenchmarkDotNet


SQL Server
──────────
SSMS
Actual Execution Plans
Query Store
Extended Events


Load Testing
────────────
k6
Azure Load Testing


Infrastructure
──────────────
Azure Monitor


Caching
───────
Redis monitoring

---
---

## Logging

The interview mental model is:

```text
Angular
   │
   │ HTTP / Trace
   ▼
.NET API
   │
   ├── Application logs
   ├── Exceptions
   ├── Dependencies
   ├── Metrics
   ├── Distributed traces
   │
   ▼
Application Insights
   │
   ├── Logs / Queries
   ├── Failures
   ├── Performance
   ├── Application Map
   ├── Live Metrics
   ├── Availability
   ├── Alerts
   └── Workbooks
          │
          ▼
      Azure Monitor
```

One important distinction first:

> Application Insights is an observability platform. Logging is only one part of it.

---

**1. What exactly does Application Insights give you?**

For your .NET application, think about these major capabilities:

```text
Application Insights
│
├── Logs
├── Exceptions
├── Requests
├── Dependencies
├── Distributed Tracing
├── Performance
├── Metrics
├── Application Map
├── Availability Tests
├── Live Metrics
├── Alerts
├── Workbooks
└── KQL Queries
```

So instead of:

```text
Console.WriteLine()
```

you get:

```text
What happened?
When?
For which request?
Which user/operation?
How long?
Which database query/dependency?
Did it fail?
What happened downstream?
```

---

**2. .NET Logging Architecture**

I would structure your .NET application like this:

```text
                    .NET Application
                          │
             ┌────────────┼─────────────┐
             │            │             │
          Logs        Exceptions      Metrics
             │            │             │
             └────────────┼─────────────┘
                          │
                   OpenTelemetry/
                  Azure Monitor setup
                          │
                          ▼
                Application Insights
```

Your application code should generally use:

```csharp
ILogger<T>
```

rather than directly coupling business code to Application Insights.

For example:

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task<Order> GetOrderAsync(int id)
    {
        _logger.LogInformation(
            "Fetching order {OrderId}",
            id);

        ...
    }
}
```

This is important architecturally.

Your application knows about:

```text
ILogger
```

not:

```text
ApplicationInsightsTelemetryClient
```

everywhere.

---

**3. Structured Logging**

Don't do this:

```csharp
_logger.LogInformation(
    "Order " + orderId + " created by " + userId);
```

Prefer:

```csharp
_logger.LogInformation(
    "Order {OrderId} created by {UserId}",
    orderId,
    userId);
```

Why?

Because Application Insights can understand the structured properties.

You can later query:

```text
OrderId = 123
UserId = 456
```

rather than searching through an unstructured sentence.

---

**4. Log Levels**

Understand these:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Typical production configuration:

```text
Information → useful business/application events
Warning     → something unexpected but recoverable
Error       → operation failed
Critical    → serious application/system failure
```

Don't log everything as:

```csharp
_logger.LogError(...)
```

That's a common mistake.

---

**5. Don't Log Sensitive Information**

Never log:

```text
Password
Access token
Refresh token
Client secret
API key
Credit card information
Personal sensitive information
```

For example, don't do:

```csharp
_logger.LogInformation(
    "User token: {Token}",
    token);
```

This is a serious security problem.

---

**6. Application Insights Automatically Tracks Requests**

This is where Application Insights becomes much more useful than simple logging.

Suppose:

```text
GET /api/orders/100
```

Application Insights can capture information around the request such as:

```text
Request
 ├── Duration
 ├── Success/failure
 ├── Response
 └── Dependencies
```

You can see:

```text
GET /api/orders/100
Duration: 1.8 sec
Status: 200
```

---

**7. Dependency Tracking**

This is one of the most useful features.

Suppose your API does:

```text
GET /api/orders
       │
       ├── SQL Server
       │
       ├── Redis
       │
       └── Payment API
```

Application Insights can track these dependencies.

You might discover:

```text
API                2.5 sec
│
├── SQL             1.9 sec
├── Redis            20 ms
└── Payment API     500 ms
```

Now you know the API itself isn't necessarily the problem.

The SQL dependency is.

---

**8. Distributed Tracing**

This is **very important for senior-level interviews**.

Imagine:

```text
Angular
   │
   ▼
.NET API
   │
   ▼
Order Module
   │
   ├── SQL
   │
   └── Payment Service
```

A request should ideally have a trace/correlation context that lets you follow the operation across components.

Conceptually:

```text
Trace ID: ABC123
```

Then:

```text
Angular request
       ↓
.NET request
       ↓
Order handler
       ↓
SQL dependency
       ↓
Payment API
```

You can investigate the whole chain.

---

**9. Application Map**

Application Insights can give you an **Application Map**.

Think:

```text
                ┌─────────────┐
                │ Angular/API │
                └──────┬──────┘
                       │
             ┌─────────┼─────────┐
             ▼         ▼         ▼
          SQL DB     Redis    Payment API
```

It helps you understand:

```text
Which components communicate?
Where are failures?
Which dependency is slow?
Where are errors occurring?
```

This is particularly useful for enterprise systems.

---

**10. Exceptions**

Application Insights can capture unhandled exceptions.

For example:

```csharp
throw new InvalidOperationException(
    "Unable to process order");
```

You can investigate:

```text
Exception type
Message
Stack trace
Timestamp
Request
User/context
Related dependencies
```

Instead of someone telling you:

> "Production is broken."

You can investigate the actual failure.

---

**11. Failed Requests**

Suppose:

```text
POST /api/orders
```

returns:

```text
500
```

Application Insights can help you identify:

```text
Which endpoint?
When?
How many times?
Which exception?
What dependency failed?
```

You can then correlate it with logs and traces.

---

**12. Performance Monitoring**

You can monitor endpoint latency:

```text
GET /orders        120ms
GET /customers     180ms
GET /reports      4500ms
```

Immediately:

```text
Reports endpoint
        ↓
Performance problem
```

Then drill into it.

---

**13. Percentiles**

Don't only look at average response time.

For example:

```text
Average: 250ms
p50:     150ms
p95:     800ms
p99:     4sec
```

This tells a much more interesting story.

Maybe most users are fine, but the slowest 1% are experiencing 4-second responses.

---

**14. Custom Application Metrics**

You can also send custom metrics for things important to your business.

For example:

```text
OrdersCreated
PaymentsSucceeded
PaymentsFailed
InvoicesGenerated
```

Conceptually:

```csharp
metric.TrackValue(ordersCreated);
```

Modern Azure/.NET applications can also use OpenTelemetry/Meter-based instrumentation and export telemetry to Application Insights.

This allows you to monitor technical **and** business signals.

---

**15. Business Events vs Logs**

Don't confuse them.

A log:

```text
"Order 123 created"
```

is useful for troubleshooting.

A metric:

```text
OrdersCreated = 1,245
```

is useful for monitoring trends.

So:

```text
Logs
 ↓
Detailed events

Metrics
 ↓
Numerical trends

Traces
 ↓
Request journey

Exceptions
 ↓
Failures
```

---

**16. Angular Logging**

Angular requires a slightly different approach.

The browser itself can log:

```typescript
console.log()
```

but you don't want production diagnostics to depend on users opening DevTools.

Instead, create an application logging service.

Conceptually:

```text
Angular
 │
 ├── LoggerService
 │
 ├── ErrorHandler
 │
 └── HTTP Interceptor
          │
          ▼
    Telemetry endpoint
```

You can use Application Insights' browser/JavaScript telemetry support or OpenTelemetry-based browser instrumentation depending on your telemetry architecture.

---

**17. Angular Global Error Handling**

Angular provides a global error handling mechanism.

Conceptually:

```typescript
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {

    handleError(error: unknown) {

        // send telemetry

        console.error(error);
    }
}
```

Then:

```text
Angular error
      ↓
Global ErrorHandler
      ↓
Telemetry
      ↓
Application Insights
```

This helps catch frontend errors such as:

```text
Component errors
Unhandled exceptions
Runtime errors
```

---

**18. HTTP Interceptor**

This is particularly useful.

```text
Angular
   │
   ▼
HTTP Interceptor
   │
   ├── Request timing
   ├── Status code
   ├── Failed request
   └── Correlation context
   │
   ▼
.NET API
```

You can capture:

```text
GET /api/orders
200
450ms
```

or:

```text
POST /api/orders
500
1200ms
```

This gives you frontend-side visibility into API calls.

---

**19. The Ideal Angular → .NET Trace**

This is what I'd want conceptually:

```text
User clicks "Create Order"
             │
             ▼
Angular
Trace ID: ABC123
             │
             │ POST /api/orders
             ▼
.NET API
Trace ID: ABC123
             │
             ▼
Order Handler
             │
             ▼
SQL Server
             │
             ▼
Payment API
             │
             ▼
Response
```

Now when production reports:

> "Creating an order takes 5 seconds."

You can follow:

```text
ABC123
   ↓
Angular: 5.1 sec
   ↓
.NET: 4.9 sec
   ↓
SQL: 3.8 sec
   ↓
Payment: 900ms
```

You have found the problem much faster.

---

**20. KQL — Extremely Important**

If you're using Application Insights, learn basic **KQL (Kusto Query Language)**.

This is one of the best things you can learn for interviews.

For example, finding requests:

```kusto
requests
| where timestamp > ago(1h)
| order by duration desc
```

Find failed requests:

```kusto
requests
| where success == false
| order by timestamp desc
```

Find slow requests:

```kusto
requests
| where duration > 1000
| project timestamp, name, duration, resultCode
| order by duration desc
```

---

**21. Find Exceptions**

Conceptually:

```kusto
exceptions
| where timestamp > ago(24h)
| order by timestamp desc
```

You can investigate:

```text
Exception type
Message
Timestamp
Operation
```

---

**22. Find Dependencies**

For example:

```kusto
dependencies
| where duration > 1000
| order by duration desc
```

You might find:

```text
SQL call       2.5 sec
Payment API    1.8 sec
Redis            20ms
```

That's incredibly useful.

---

**23. Find Slow APIs**

```kusto
requests
| summarize
    AvgDuration = avg(duration),
    P95 = percentile(duration, 95),
    Count = count()
    by name
| order by P95 desc
```

Now you can build a performance dashboard.

---

**24. Find Error Rate**

For example, conceptually:

```kusto
requests
| summarize
    Total = count(),
    Failed = countif(success == false)
    by bin(timestamp, 5m)
```

Then calculate/visualize the failure percentage.

This lets you see:

```text
10:00 → 0.2%
10:05 → 0.3%
10:10 → 18%
10:15 → 25%
```

You might correlate the spike with a deployment.

---

**25. Alerts**

This is another major feature.

Suppose:

```text
API p95 > 2 seconds
```

for several minutes.

You can configure:

```text
Metric/Log condition
       ↓
Alert
       ↓
Action Group
       ↓
Email / Teams / other notification
```

Or:

```text
Exception rate > threshold
```

or:

```text
Availability test fails
```

This turns Application Insights from a dashboard into an operational monitoring system.

---

**26. Availability Monitoring**

You can monitor whether an endpoint is reachable.

For example:

```text
https://api.company.com/health
```

Periodically:

```text
Azure
  ↓
Health endpoint
  ↓
200 OK
```

If it starts returning:

```text
500
503
timeout
```

you can get an alert.

For production applications, availability monitoring is extremely useful.

---

**27. Live Metrics**

Application Insights has a live monitoring capability.

This is useful during incidents:

```text
Production
    │
    ▼
Live Metrics
    │
    ├── Requests
    ├── Failures
    ├── CPU
    └── Performance
```

For example:

```text
Deploy
  ↓
Suddenly errors increase
  ↓
Live Metrics
  ↓
Investigate immediately
```

---

**28. Workbooks**

Workbooks are useful for creating dashboards.

For example, create an:

***API Health Dashboard***

```text
┌────────────────────────────────────────┐
│ Requests/sec          1,250            │
│ Error rate            0.4%             │
│ P95                   420ms            │
│ P99                   1.2s             │
├────────────────────────────────────────┤
│ Top Slow APIs                           │
│ /reports              2.3s             │
│ /orders               420ms            │
│ /customers             300ms           │
├────────────────────────────────────────┤
│ Top Errors                              │
│ SQL timeout             15             │
│ Payment API              8             │
└────────────────────────────────────────┘
```

That's much more useful for an operations team than looking at raw logs.

---

**29. Application Insights + SQL**

A very useful investigation flow is:

```text
API slow
  ↓
Application Insights
  ↓
Dependency slow
  ↓
SQL dependency
  ↓
Query information
  ↓
SQL Server
  ↓
Query Store
  ↓
Execution Plan
  ↓
Fix index/query
```

So Application Insights doesn't replace SQL monitoring.

It **helps you discover that SQL is the problem**.

Then SQL-specific tools investigate why.

---

**30. Application Insights + Redis**

Same idea:

```text
API
 ↓
Application Insights
 ↓
Redis dependency slow
 ↓
Redis monitoring
 ↓
Investigate cache
```

You might discover:

```text
Cache hit rate = 40%
```

when you expected:

```text
90%+
```

Then you investigate your caching strategy.

---

**31. Application Insights + Background Jobs**

Suppose:

```text
Order Created
     ↓
Event
     ↓
Background Worker
     ↓
Generate Invoice
```

You should also instrument background processing.

Monitor:

```text
Job duration
Success
Failure
Queue length
Retries
```

This becomes particularly important if you use:

```text
Azure Service Bus
RabbitMQ
BackgroundService
```

---

**32. Application Insights + Your Modular Monolith**

This fits your architecture very nicely.

Suppose:

```text
Modular Monolith

├── Order Module
├── Customer Module
├── Payment Module
├── Notification Module
└── Reporting Module
```

You can add structured logging:

```text
Module = Orders
Operation = CreateOrder
```

Then:

```text
Application Insights
        │
        ├── Orders
        ├── Customers
        ├── Payments
        ├── Notifications
        └── Reports
```

Now you can query:

```kusto
traces
| where customDimensions.Module == "Orders"
```

Conceptually.

This gives you observability **without destroying the modular architecture**.

---

**33. What NOT to Do**

❌ Don't log everything

Bad:

```csharp
_logger.LogInformation(
    JsonSerializer.Serialize(hugeObject));
```

for every request.

You'll create:

```text
Huge telemetry volume
Higher cost
Noisy logs
Potential sensitive-data exposure
```

---

❌ Don't use Application Insights as your database

Don't expect:

```text
Application Insights
```

to replace:

```text
SQL Server
```

It is for telemetry/observability.

---

❌ Don't put business logic inside logging

Bad:

```csharp
_logger.LogInformation(...);

if (...)
{
   // business decision
}
```

Logging should observe the application, not control it.

---

34. Production Architecture I'd Recommend

For your Azure stack:

```text
                   USERS
                     │
                     ▼
                  Angular
                     │
              Browser telemetry
                     │
                     ▼
                .NET API
                     │
       ┌─────────────┼─────────────┐
       │             │             │
     Logs         Traces        Metrics
       │             │             │
       └─────────────┼─────────────┘
                     │
               OpenTelemetry /
               Azure Monitor
                     │
                     ▼
           Application Insights
                     │
        ┌────────────┼─────────────┐
        │            │             │
      KQL        Dashboards      Alerts
        │            │             │
        ▼            ▼             ▼
     Analysis      Workbooks    Action Groups
```

And dependencies:

```text
                 .NET API
                    │
       ┌────────────┼────────────┐
       ▼            ▼            ▼
    SQL Server     Redis      External API
       │
   Query Store
   Execution Plan
```

---

### The 6 Application Insights concepts I'd memorize first

```text
1. Requests
2. Dependencies
3. Exceptions
4. Traces / Logs
5. Metrics
6. Distributed Tracing
```

Then learn:

```text
7. KQL
8. Alerts
9. Application Map
10. Availability
11. Live Metrics
12. Workbooks
```

Once you understand those 12, Application Insights becomes much easier to discuss in interviews.

-----
-----

## Azure Paas services

Sure. Think of **Azure PaaS services** as ready-made building blocks where Azure manages most of the infrastructure for you.

For an enterprise **Angular + .NET application**, these are the common ones:

| Azure Service | Simple meaning | Typical usage |
|---|---|---|
| **Azure App Service** | Host your web/API application | Host .NET API, Angular |
| **Azure Functions** | Run small pieces of code without managing a server | Background jobs, scheduled tasks, event processing |
| **Azure SQL Database** | Managed SQL Server database | Store application data |
| **Application Gateway** | Smart gateway/load balancer for web traffic | Route traffic, SSL termination, WAF |
| **Storage Account** | Cloud storage | Files, images, documents, blobs, queues |

**1. Azure App Service**

Think:

> **"I have a .NET application; I need somewhere to host it."**

```text
Angular
   ↓
Azure App Service
   ↓
.NET API
```

Azure manages things like:

- Server/VM infrastructure
- OS patching
- Scaling options
- SSL certificates/integration
- Deployment slots
- Monitoring integration

You deploy your:

```text
.NET 10 API
```

to App Service.

For Angular, you can also host the built static files there, although services such as Azure Static Web Apps or a CDN-backed storage setup may be preferable depending on the architecture.

---

**2. Azure Functions**

Think:

> I have a small piece of work that should run when something happens.

Example:

```text
Order Created
     ↓
Azure Function
     ↓
Send Email
```

Or:

```text
Every night at 2 AM
        ↓
Azure Function
        ↓
Generate Report
```

Or:

```text
File uploaded
      ↓
Azure Function
      ↓
Process file
```

Good for:

- Scheduled jobs
- Background processing
- Event processing
- Queue processing
- Lightweight APIs
- File processing

You don't need to manage a traditional server for the function.

---

**3. Azure SQL Database**

Think:

> I need SQL Server in the cloud.

```text
.NET API
    ↓
Azure SQL Database
    ↓
Tables
```

Azure manages much of:

- Database infrastructure
- Backups
- Patching
- High availability options
- Monitoring
- Scaling options

You still manage:

```text
Tables
Indexes
Queries
Stored procedures
Permissions
Database design
```

For your .NET applications, **EF Core → Azure SQL** is a very common combination.

---

**4. Application Gateway**

Think:

> I need something in front of my applications to control and route incoming web traffic.

Example:

```text
                  Internet
                     ↓
             Application Gateway
                /          \
               /            \
              ▼              ▼
       .NET App Service   Another Service
```

It can provide capabilities such as:

- Load balancing
- URL-based routing
- SSL/TLS termination
- Health probes
- Web Application Firewall (WAF)

For example:

```text
/api/*       → .NET API
/admin/*     → Admin application
```

The **WAF** capability is particularly useful for protecting web applications from common web attacks.

---

**5. Azure Storage Account**

Think:

> I need cloud storage for files/data that aren't relational database records.

It provides different storage capabilities, commonly including:

```text
Blob Storage
Queue Storage
File Storage
Table Storage
```

For example, your application might have:

```text
.NET API
   │
   ├── Customer data → Azure SQL
   │
   └── Customer documents → Blob Storage
```

So if a user uploads:

```text
invoice.pdf
profile.jpg
contract.pdf
```

you could store the actual files in **Blob Storage** and store only metadata/path information in Azure SQL.

---
***How they can work together***

A simple enterprise architecture could look like:

```text
                    Internet
                       │
                       ▼
              Application Gateway
                   + WAF
                       │
                       ▼
                Angular / Web
                       │
                       ▼
                .NET App Service
                       │
          ┌────────────┼────────────┐
          ▼            ▼            ▼
      Azure SQL     Storage       Azure
      Database      Account      Functions
                       │            │
                       │            │
                    Documents    Background
                    / Images      Jobs
```

### Easy way to remember

```text
App Service      → Host my application
Functions        → Run my code when needed
Azure SQL        → Store relational data
Storage Account  → Store files/blobs
App Gateway      → Control/route incoming web traffic
```
---------
---------

## OWASP Top 10

The OWASP Top 10 is essentially a list of the **most important/common categories of web application security risks**.

For your Angular + .NET application, I'd focus heavily on these:

```text
1. Broken Access Control
2. Cryptographic Failures
3. Injection
4. Insecure Design
5. Security Misconfiguration
6. Vulnerable Components
7. Authentication Failures
8. Software/Data Integrity Failures
9. Logging & Monitoring Failures
10. Mishandling of Exceptions / SSRF-related risks
```

The exact wording/order can vary by OWASP edition, so in an interview focus more on the concepts than memorizing the numbering.

---

**1. XSS — Cross-Site Scripting**

Attacker tries to inject JavaScript into your application.

For example, imagine a comment field:

```text
Hello <script>alert('Hacked')</script>
```

If your application renders it as executable HTML/JavaScript:

```text
User input
    ↓
HTML page
    ↓
Browser executes attacker code
```

The attacker could potentially steal sensitive information or perform actions as the victim.

---

**Angular protection**

Angular automatically escapes normal template interpolation:

```html
<div>
    {{ comment }}
</div>
```

So don't unnecessarily bypass Angular's sanitization.

Be particularly careful with:

```typescript
innerHTML
```

and:

```typescript
bypassSecurityTrustHtml()
```

For example, this is dangerous if the source isn't trusted/sanitized:

```html
<div [innerHTML]="userContent"></div>
```

**Interview answer**

> Angular provides built-in XSS protection through template escaping and sanitization, but I avoid bypassing sanitization and don't trust user-provided HTML.

---

**.NET protection**

Validate input and encode output appropriately.

Don't assume:

> Angular already validated it.

The backend should still treat incoming data as untrusted.

Also use security headers such as an appropriate **Content Security Policy (CSP)** where feasible.

---

**2. CSRF — Cross-Site Request Forgery**

This is slightly different from XSS.

Imagine you're logged into:

```text
bank.com
```

and your browser has an authentication cookie.

You visit:

```text
evil.com
```

The malicious website tries to make your browser send:

```text
POST bank.com/transfer
```

The browser may automatically attach the authentication cookie depending on the cookie/authentication setup.

That's the basic CSRF problem:

```text
Victim
  ↓
Logged into application
  ↓
Visits malicious site
  ↓
Malicious request
  ↓
Application trusts request
```

---

**How do we protect against CSRF?**

It depends on your authentication architecture.

`Cookie-based authentication`

Use appropriate:

```text
HttpOnly
Secure
SameSite
CSRF tokens
Origin/Referer validation
```

ASP.NET Core also has built-in antiforgery mechanisms for applicable cookie-based scenarios.

`Bearer token API`

If Angular obtains an access token and explicitly sends:

```http
Authorization: Bearer <token>
```

the browser does not automatically attach that Authorization header to arbitrary cross-site requests in the same way it automatically handles cookies.

So classic CSRF exposure is different.

However, you still need proper:

```text
CORS
Token protection
XSS protection
Authorization
```

---

**3. Injection**

This is a huge category.

The classic example is **SQL Injection**.

Bad:

```csharp
var sql =
    $"SELECT * FROM Users WHERE Name = '{name}'";
```

If the attacker supplies malicious SQL syntax, the generated query can be manipulated.

---

**Use EF Core**

Prefer:

```csharp
var user = await db.Users
    .FirstOrDefaultAsync(x => x.Name == name);
```

EF Core generates parameterized SQL.

Or use properly parameterized SQL when raw SQL is genuinely required.

The principle:

> Never concatenate untrusted input into SQL.

---

**4. Command Injection**

Suppose your application executes an operating-system command using user input.

Bad:

```text
User input
    ↓
OS command
```

An attacker could potentially inject additional commands.

Avoid executing shell commands with user-controlled strings.

If unavoidable:

```text
Validate
Allowlist
Parameterize where possible
Run with least privilege
```

---

**5. Broken Access Control**

This is one of the most important things for your **Angular + .NET** interviews.

Suppose:

```text
GET /api/orders/100
```

belongs to:

```text
User A
```

User B changes the URL:

```text
GET /api/orders/100
```

and gets the order.

Authentication is working:

```text
User B is authenticated ✓
```

But authorization is broken:

```text
User B should NOT access Order 100 ✗
```

---

**.NET protection**

Use:

```csharp
[Authorize]
```

and policies/roles/claims where appropriate.

But that's **not enough**.

You also need resource-level authorization:

```csharp
var order = await db.Orders
    .FirstOrDefaultAsync(x =>
        x.Id == orderId &&
        x.CustomerId == currentUserId);
```

For multi-tenant applications:

```csharp
.Where(x => x.TenantId == currentTenantId)
```

This is extremely important.

---

**6. Authentication Failures**

Examples:

```text
Weak authentication
Poor session management
Bad token handling
No MFA where required
Long-lived credentials
Credential stuffing
```

For your architecture:

```text
Angular
   ↓
OAuth 2.0 + OIDC
   ↓
Microsoft Entra ID
   ↓
.NET API
```

Use:

```text
Authorization Code + PKCE
Access tokens
Scopes
Roles/claims
MFA / Conditional Access where appropriate
```

Don't create your own authentication system unnecessarily when Entra ID is available.

---

**7. Cryptographic Failures**

Don't store sensitive information:

```text
Plain text password
Plain text secret
Plain text API key
```

Don't use weak/custom cryptography.

For your Azure architecture:

```text
.NET
  ↓
Managed Identity
  ↓
Azure Key Vault
  ↓
Secrets
```

For passwords, if you manage identities yourself, use established password hashing mechanisms such as ASP.NET Core Identity rather than inventing your own.

---

**8. Security Misconfiguration**

Examples:

```text
Debug mode in production
Detailed exception pages
Open CORS
Default passwords
Unused endpoints
Exposed admin interfaces
Missing security headers
```

Bad:

```csharp
AllowAnyOrigin()
AllowAnyHeader()
AllowAnyMethod()
```

without understanding the security requirements.

Production should have controlled configuration.

---

**9. Vulnerable Dependencies**

Your application might use:

```text
Angular packages
npm packages
NuGet packages
Docker images
OS packages
```

One dependency could contain a vulnerability.

Use:

```text
Dependabot
GitHub security scanning
npm audit
NuGet vulnerability checks
Container scanning
```

Keep dependencies patched.

---

**10. Security Logging & Monitoring**

This connects directly to what we discussed about Application Insights.

Suppose someone repeatedly gets:

```text
401
403
403
403
403
```

or an admin operation fails unexpectedly.

You want visibility.

```text
Angular
   ↓
.NET
   ↓
Application Insights
   ↓
Logs
Exceptions
Traces
Alerts
```

Monitor things such as:

```text
Authentication failures
Authorization failures
Unexpected errors
Suspicious activity
Admin actions
Security-sensitive events
```

But never log:

```text
Passwords
Access tokens
Refresh tokens
API secrets
```

---

**11. SSRF — Server-Side Request Forgery**

This is another useful senior-level concept.

Suppose your API accepts:

```json
{
    "url": "https://some-site.com/image.jpg"
}
```

and your server fetches that URL.

An attacker might try to make your server access internal resources.

```text
Attacker
   ↓
.NET API
   ↓
Internal resource
```

The server has network access that the attacker doesn't directly have.

Protection includes:

```text
URL allowlisting
Restrict protocols
Validate destinations
Block private/internal IP ranges where appropriate
Network egress controls
```

---

**12. File Upload Security**

Very common enterprise issue.

Suppose:

```text
POST /api/documents/upload
```

Don't trust only:

```text
.pdf extension
Content-Type
filename
```

Validate:

```text
File size
File type/content
Extension
Filename
Storage location
Malware scanning where required
```

Store uploaded files outside an executable web directory.

For example:

```text
Angular
   ↓
.NET
   ↓
Validate
   ↓
Azure Blob Storage
```

---

**13. Error Handling**

Don't expose:

```json
{
  "error": "SqlException: Server=db01; User=sa; ..."
}
```

to the browser.

Instead:

```json
{
  "title": "An unexpected error occurred",
  "status": 500,
  "traceId": "ABC123"
}
```

Internally:

```text
Exception
   ↓
Application Insights
   ↓
Full stack trace
```

Externally:

```text
Safe error
```

This is a very good enterprise practice.

---

**14. Security Headers**

Important headers to understand:

```text
Content-Security-Policy
Strict-Transport-Security
X-Content-Type-Options
Referrer-Policy
Permissions-Policy
```

For example:

```text
Strict-Transport-Security
```

helps ensure browsers use HTTPS for the site.

And:

```text
Content-Security-Policy
```

can restrict where scripts/resources can be loaded from, helping mitigate certain XSS attacks.

---

**15. How Angular + .NET Security Fits Together**

Think of security as multiple layers:

```text
                    INTERNET
                       │
                       ▼
                 HTTPS / WAF
                       │
                       ▼
                    Angular
                       │
              ┌────────┴────────┐
              │                 │
         XSS protection    Route Guards
              │                 │
              └────────┬────────┘
                       │
                  OAuth/OIDC
                       │
                       ▼
                 Microsoft
                 Entra ID
                       │
                  Access Token
                       │
                       ▼
                  .NET API
                       │
             ┌─────────┼─────────┐
             ▼         ▼         ▼
       Authentication Authorization Validation
             │         │         │
             └─────────┼─────────┘
                       ▼
                  Application
                       │
                       ▼
                    EF Core
                       │
                  Parameterized
                      SQL
                       │
                       ▼
                   Database
```

And around the entire system:

```text
Key Vault
Managed Identity
Application Insights
Logging
Monitoring
Dependency scanning
Rate limiting
Security headers
```

---
---

## SQL vs Azure SQL

> SQL Server is the database technology. Azure SQL is Microsoft's cloud-managed offering based on SQL Server.

But there are a few different Azure SQL products, so this distinction matters in interviews.

**1. SQL Server**

Traditional SQL Server usually means **you manage the SQL Server environment yourself**.

```text
Your Data Center / VM
        │
        ▼
   SQL Server
        │
   ┌────┼────┐
   │    │    │
Database Index Backup
```

You are responsible for things like:

- Installing SQL Server
- OS and SQL Server patching
- Server configuration
- Backups
- High availability
- Disaster recovery
- Scaling infrastructure
- Monitoring the server

You can run it:

- On-premises
- On an Azure VM
- In another cloud

---

**2. Azure SQL Database**

Azure SQL Database is a **fully managed PaaS database service**.

```text
.NET API
   │
   ▼
Azure SQL Database
   │
   ▼
Azure-managed infrastructure
```

Microsoft manages much of:

- Physical infrastructure
- OS
- Database engine patching
- Backups
- High availability
- Monitoring
- Scaling capabilities

You mainly focus on:

```text
Tables
Indexes
Queries
Stored procedures
Security
Database design
Performance
```

So if you're building a new cloud-native .NET application, **Azure SQL Database is often the natural choice**.

---

**3. Important: Azure SQL ≠ One Product**

When someone says "Azure SQL", they could mean several things.

| Option | What it is | Typical use |
|---|---|---|
| **SQL Server** | Database software | On-prem / VM / self-managed |
| **Azure SQL Database** | Managed database PaaS | Cloud-native applications |
| **Azure SQL Managed Instance** | Managed SQL Server instance | Migrating existing SQL Server applications |
| **SQL Server on Azure VM** | SQL Server running on your VM | Maximum control / compatibility |

This is an important interview distinction.

---

**4. When would I use SQL Server?**

Suppose a company already has:

```text
On-Premise Data Center

Application
    ↓
SQL Server
    ↓
Multiple databases
```

They may want complete control over:

```text
OS
SQL Server configuration
Networking
Installed components
Instance-level features
```

Then traditional SQL Server makes sense.

Another example:

> "Our application depends on SQL Server features that aren't available or aren't practical in Azure SQL Database."

You might use:

```text
Azure VM
   ↓
SQL Server
```

Now you get cloud infrastructure but still manage the SQL Server environment.

---

**5. When would I use Azure SQL Database?**

For a new application:

```text
Angular
   ↓
.NET 10 API
   ↓
Azure SQL Database
```

This is usually the simplest architecture.

For example, your enterprise modular monolith:

```text
.NET Modular Monolith
        │
        ▼
 Azure SQL Database
        │
 ├── Orders
 ├── Customers
 ├── Payments
 └── Notifications
```

You don't need to worry about provisioning a Windows Server and installing SQL Server.

---

**6. Azure SQL Managed Instance**

This is the middle ground.

Think:

```text
Traditional SQL Server
        │
        │ migrate to cloud
        ▼
SQL Managed Instance
```

It provides much broader SQL Server compatibility while still being a managed Azure service.

It's particularly useful when you have a large existing SQL Server application and want to move it to Azure without redesigning everything.

For example:

```text
Old environment

Application
    ↓
SQL Server
    ↓
Stored procedures
SQL Agent
Multiple databases
Instance-level features
```

Moving directly to Azure SQL Database may require significant changes.

Managed Instance can reduce that migration effort.

---

**7. Simple decision tree**

Use this in interviews:

```text
Need SQL database?
       │
       ▼
Are you building a new cloud application?
       │
      YES
       │
       ▼
Azure SQL Database
```

If:

```text
Existing SQL Server application
       │
       ▼
Needs high SQL Server compatibility?
       │
      YES
       │
       ▼
Azure SQL Managed Instance
```

If:

```text
Need complete control over OS
and SQL Server installation/configuration?
       │
      YES
       │
       ▼
SQL Server on Azure VM
```

If:

```text
On-premise / self-managed environment
       │
       ▼
SQL Server
```

---

**8. One misconception to avoid**

Don't say:

> "Azure SQL is a different database from SQL Server."

Better:

> **"Azure SQL Database uses the SQL Server database engine technology but provides it as a managed Azure PaaS service, with some feature and architectural differences compared with a full SQL Server instance."**

That's a much better interview answer.

---

**9. For your .NET applications**

For the kind of applications you're preparing for, I'd generally think:

**New Azure application**

```text
Angular
   ↓
App Service
   ↓
.NET 10
   ↓
Azure SQL Database
```

**Existing enterprise SQL Server application**

```text
Existing .NET
      ↓
SQL Server
      ↓
Migration
      ↓
Azure SQL Managed Instance
```

**Need maximum control**

```text
.NET
 ↓
Azure VM
 ↓
SQL Server
```

----
----

## Garbage Collector

> GC automatically manages memory by finding objects that your application no longer needs and reclaiming their memory.

---

**1. Why do we need GC?**

When your .NET application creates objects:

```csharp
var customer = new Customer();
var order = new Order();
var list = new List<Order>();
```

These objects are stored in **managed memory (heap)**.

Conceptually:

```text
.NET Application
      │
      ▼
   Managed Heap
      │
 ┌────┼────────┐
 ▼    ▼        ▼
Customer Order List
```

Eventually, some objects are no longer needed.

Without automatic memory management, you'd have to manually free them.

The GC does this for you.

```text
Object created
     ↓
Object used
     ↓
Object no longer reachable
     ↓
GC identifies it
     ↓
Memory reclaimed
```

---

**2. Simple Example**

```csharp
public void Process()
{
    var customer = new Customer();

    // use customer
}
```

After `Process()` finishes, assuming nothing else references `customer`:

```text
customer
   ↓
No references
   ↓
Eligible for GC
```

The GC can eventually reclaim that memory.

You don't normally write:

```csharp
delete customer;
```

because .NET's GC handles managed memory.

---

**3. How does GC know what to delete?**

This is the important part.

GC doesn't simply look at:

> Was this variable used recently?

It looks at **object reachability**.

Imagine:

```text
Root
 │
 ▼
Customer
 │
 ▼
Address
```

These objects are reachable:

```text
Root → Customer → Address
```

So they remain alive.

But:

```text
Order
Address2
TemporaryObject
```

have no path from a GC root.

They can become eligible for collection.

```text
GC Root
   │
   ▼
Customer ──→ Address

Order       ← unreachable
TempObject  ← unreachable
```

---

**4. What are GC Roots?**

GC starts from objects/references that are considered roots, such as references associated with:

```text
Active stack references
Static references
Runtime handles
Other GC roots
```

Then it follows references.

Conceptually:

```text
GC Roots
   │
   ├──→ Object A
   │      │
   │      └──→ Object B
   │
   └──→ Object C

Object D ← unreachable
```

Object D becomes eligible for collection.

---

**5. Generational GC**

This is **very important for interviews**.

.NET GC uses generations:

```text
Generation 0
Generation 1
Generation 2
```

Think:

```text
Gen 0 → New objects
Gen 1 → Objects that survived Gen 0
Gen 2 → Long-lived objects
```

---

**6. Generation 0**

Most newly created objects start in:

```text
Gen 0
```

Example:

```csharp
var customer = new Customer();
```

Conceptually:

```text
Managed Heap

Gen 0
┌─────────────────────┐
│ Customer            │
│ Temporary object    │
│ Request object      │
│ DTO                 │
└─────────────────────┘
```

Many objects are short-lived.

For example:

```csharp
public Customer GetCustomer()
{
    var dto = new CustomerDto();

    return Map(dto);
}
```

Temporary objects may become unreachable quickly.

So GC can clean Gen 0 frequently.

---

**7. Generation 1**

If an object survives a Gen 0 collection, it can be promoted.

```text
Gen 0
  │
  │ survives GC
  ▼
Gen 1
```

Gen 1 acts somewhat like a buffer between short-lived and long-lived objects.

---

**8. Generation 2**

Long-lived objects eventually reach:

```text
Gen 2
```

Examples might include:

```text
Long-lived caches
Application-wide objects
Long-lived collections
```

Conceptually:

```text
Gen 0
 ↓ survives
Gen 1
 ↓ survives
Gen 2
```

Gen 2 collections are generally more expensive than Gen 0 collections because there's more long-lived memory to examine.

---

**9. Why generations improve performance**

Imagine you have:

```text
1,000,000 objects
```

You don't want GC to scan everything every time a tiny temporary object becomes unreachable.

Instead:

```text
Most temporary objects
        ↓
Generation 0
        ↓
Collect frequently
```

Long-lived objects:

```text
Generation 2
        ↓
Collect less frequently
```

This makes garbage collection more efficient.

---

**10. What happens during GC?**

Simplified process:

```text
Application running
       ↓
Memory allocation
       ↓
GC triggered
       ↓
Find reachable objects
       ↓
Unreachable objects identified
       ↓
Memory reclaimed
       ↓
Heap may be compacted
       ↓
Application continues
```

The exact implementation is more sophisticated, but this is the right interview-level mental model.

---

**11. Heap Compaction**

Suppose memory looks like:

```text
┌──────┬──────┬──────┬──────┬──────┐
│ Used │Dead  │ Used │Dead  │ Used │
└──────┴──────┴──────┴──────┴──────┘
```

After collection, GC can compact memory:

```text
┌──────┬──────┬──────┬──────────────┐
│ Used │ Used │ Used │ Free         │
└──────┴──────┴──────┴──────────────┘
```

This reduces fragmentation.

---

**12. GC and Performance**

This is where GC becomes important for your **Senior .NET interview**.

Suppose your API processes:

```text
10,000 requests/sec
```

and each request creates thousands of unnecessary objects.

You could have:

```text
Many allocations
      ↓
Gen 0 fills quickly
      ↓
Frequent GC
      ↓
CPU overhead
      ↓
Latency increases
```

So GC is automatic, but **your code still needs to be allocation-efficient**.

---

**13. Example of Excessive Allocation**

Imagine:

```csharp
for (int i = 0; i < 1_000_000; i++)
{
    var data = new SomeObject();
}
```

You're creating huge numbers of objects.

Many will become unreachable quickly.

That creates GC pressure.

---

**14. What is GC Pressure?**

GC pressure basically means:

> Your application is creating enough allocations that the GC has to work increasingly hard to reclaim memory.

Example:

```text
High object allocation
        ↓
Gen 0 collections increase
        ↓
CPU spent on GC increases
        ↓
Application performance suffers
```

---

**15. Large Object Heap — LOH**

Another important interview topic.

Very large objects are handled differently and can go to the:

> Large Object Heap (LOH)

For example:

```csharp
var hugeArray = new byte[largeSize];
```

Large allocations can put pressure on the LOH.

LOH fragmentation can become a performance/memory concern.

You don't need to memorize the exact threshold for most interviews; just understand that **large allocations are handled specially**.

---

**16. IDisposable vs GC**

This is another common interview question.

People sometimes think:

> "If GC manages memory, why do we need IDisposable?"

Because GC manages **managed memory**, but some resources need deterministic cleanup.

Examples:

```text
File handles
Database connections
Network sockets
Streams
OS handles
```

For example:

```csharp
using var connection = new SqlConnection(connectionString);
```

Here:

```text
IDisposable
   ↓
Deterministic resource cleanup
```

while:

```text
GC
   ↓
Managed memory cleanup
```

They solve different problems.

---

**17. `using` and GC**

Example:

```csharp
using var stream = File.OpenRead("file.txt");
```

The `using` statement ensures `Dispose()` is called when the scope ends.

It does **not** mean:

> "Destroy the object immediately."

It means the object's disposable resources are released deterministically.

The object itself is still managed by GC.

---

**18. GC Doesn't Mean Memory Immediately Returns to the OS**

This is another subtle point.

You might see:

```text
Application memory = 500 MB
```

After GC:

```text
Used objects = 200 MB
```

but the process might still hold a larger managed heap.

So:

```text
Memory reserved/committed
≠
Live objects
```

This is why looking only at process memory isn't always enough.

---

**19. Server GC vs Workstation GC**

For enterprise web APIs, this is useful to know.

.NET has different GC modes, broadly including:

```text
Workstation GC
Server GC
```

**Workstation GC**

Generally optimized for desktop/client scenarios.

**Server GC**

Designed for server workloads where throughput is important.

For ASP.NET Core applications running on servers, **Server GC is commonly relevant**.

You don't normally need to manually force GC.

---

**20. Should we call `GC.Collect()` manually?**

Usually:

> **No.**

Avoid doing this routinely:

```csharp
GC.Collect();
```

The runtime's GC is designed to determine when collection is appropriate.

Manually forcing GC can hurt performance.

There are specialized scenarios where explicit collection may be justified, but that's not normal application code.

---

**21. How do you diagnose GC problems?**

This connects directly to the performance tools we discussed.

You can use:

```text
dotnet-counters
dotnet-trace
dotnet-dump
Visual Studio Profiler
Application Insights
```

You can monitor things such as:

```text
GC collections
Allocation rate
Heap size
GC pause/activity
CPU
Memory
```

For example:

```text
API becomes slow
      ↓
CPU increases
      ↓
GC activity increases
      ↓
Allocation rate is high
      ↓
Find code creating excessive objects
```

---

**22. GC and Your .NET API**

Imagine your enterprise API:

```text
Angular
   ↓
.NET API
   ↓
Service
   ↓
EF Core
   ↓
SQL
```

A request might create:

```text
Request DTO
Order DTO
LINQ objects
Strings
Collections
EF objects
JSON serialization objects
```

Many are temporary.

GC cleans them up when they become unreachable.

But if you're creating unnecessary allocations:

```text
Request
 ↓
1000 unnecessary objects
 ↓
More allocations
 ↓
More GC
 ↓
Higher latency
```

Performance optimization often involves reducing unnecessary allocations.

---

**The mental model to remember**

```text
             .NET Application
                    │
              Creates objects
                    │
                    ▼
              Managed Heap
                    │
            ┌───────┼────────┐
            ▼       ▼        ▼
          Gen 0   Gen 1    Gen 2
            │       │        │
            │       │        │
        Short-lived      Long-lived
            │
            ▼
        GC Collection
            │
            ▼
    Unreachable objects
         reclaimed
            │
            ▼
      Memory available
```

And remember this distinction:

```text
GC
 ↓
Managed memory

IDisposable
 ↓
External/unmanaged resources
 ↓
Deterministic cleanup
```
-----
-----