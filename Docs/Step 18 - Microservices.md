## Monolith vs Modular Monolith vs Microservices

**`Monolith`**

> A monolith is an application where the major functionality is deployed as **one application/process**.

Example e-commerce application:

```text
                 E-Commerce Application
                         │
       ┌─────────────────┼─────────────────┐
       ↓                 ↓                 ↓
     Orders           Payments          Inventory
       │                 │                 │
       └─────────────────┼─────────────────┘
                         ↓
                    PostgreSQL
```

In a typical monolith:

```text
One codebase
One deployment
One process
Often one database
```

Example .NET solution:

```text
ECommerce.Api
 ├── Controllers
 ├── Services
 ├── Repositories
 ├── Orders
 ├── Payments
 ├── Inventory
 └── Users
```

Deploy:

```text
ECommerce.Api.dll
       ↓
     Server
```

**Advantages**

* Simple to develop initially
* Simple deployment
* Simple debugging
* Easy database transactions
* Easy local development
* Lower infrastructure complexity

**Disadvantages**

Suppose Inventory has a huge workload:

```text
Orders       → normal traffic
Payments     → normal traffic
Inventory    → very high traffic
```

You generally scale the **whole application** rather than independently scaling only Inventory.

Also, a change to one area can require rebuilding/redeploying the application.

Microsoft notes that a monolith can be the better choice when independent feature scaling/deployment isn't actually needed, because microservices add significant complexity.

---

**`Modular Monolith`**

This is **very important for interviews**.

A Modular Monolith is:

> **One deployable application, but internally divided into strongly isolated business modules.**

Example:

```text
             E-Commerce Application
                    │
      ┌─────────────┼──────────────┐
      ↓             ↓              ↓
   Orders        Payments       Inventory
    Module         Module         Module
      │             │              │
      └─────────────┼──────────────┘
                    ↓
               PostgreSQL
```

But unlike a poorly structured monolith:

```text
Orders
  ↓
Payment internals
  ↓
Inventory tables
```

modules should have **clear boundaries**.

For example:

```text
Orders Module
 ├── API
 ├── Application
 ├── Domain
 └── Infrastructure

Payments Module
 ├── API
 ├── Application
 ├── Domain
 └── Infrastructure
```

They are still inside:

```text
One .NET application
One deployment
Usually one process
```

**Why use it?**

You get some of the architectural benefits of microservices **without immediately taking on distributed-system complexity**.

---

**`Microservices`**

Now we physically separate the services.

```text
                       API Gateway
                            │
          ┌─────────────────┼─────────────────┐
          ↓                 ↓                 ↓
   Order Service      Payment Service    Inventory Service
          │                 │                 │
          ↓                 ↓                 ↓
      Order DB         Payment DB       Inventory DB
```

Each service:

* Owns its business capability
* Runs independently
* Can be deployed independently
* Can scale independently
* Owns its domain data
* Communicates through defined contracts

Microsoft describes microservices as independently deployable services that implement specific business capabilities and own their related domain data/logic. 

---

**The Most Important Difference**

Think:

```text
Monolith
    ↓
One deployment unit


Modular Monolith
    ↓
One deployment unit
+
Strong internal business boundaries


Microservices
    ↓
Multiple independently deployable units
+
Strong business boundaries
+
Independent data ownership
```

---

**Comparison**

|                          | Monolith                | Modular Monolith        | Microservices                     |
| ------------------------ | ----------------------- | ----------------------- | --------------------------------- |
| Deployment               | One                     | One                     | Multiple                          |
| Process                  | Usually one             | Usually one             | Multiple                          |
| Modules                  | Often loosely organized | Strong boundaries       | Independent services              |
| Database                 | Usually shared          | Usually shared          | Each service owns data            |
| Scaling                  | Whole application       | Whole application       | Per service                       |
| Communication            | In-process              | In-process              | HTTP/gRPC/messages                |
| Deployment independence  | ❌                       | ❌                       | ✅                                 |
| Distributed transactions | Easy                    | Relatively easy         | Difficult                         |
| Operational complexity   | Low                     | Medium                  | High                              |
| Debugging                | Easier                  | Easier                  | Harder                            |
| Infrastructure           | Simple                  | Moderate                | Significant                       |
| Best for                 | Smaller/simple systems  | Growing/complex systems | Large/complex distributed systems |

The independent data ownership and deployment characteristics are central to microservices; Microsoft also highlights eventual consistency, resilient communication and operational complexity as major challenges. 

---

**Real Example**

Imagine we have:

```text
E-Commerce
```

Business areas:

```text
Customer
Order
Payment
Inventory
Shipping
Notification
```

**Traditional Monolith**

```text
ECommerce.API
       │
       ├── Customer
       ├── Order
       ├── Payment
       ├── Inventory
       ├── Shipping
       └── Notification
                │
                ↓
           One Database
```

**Modular Monolith**

```text
ECommerce.API
       │
 ┌─────┼──────────────┐
 ↓     ↓              ↓
Order Payment      Inventory
Module Module       Module
 │       │             │
 └───────┼─────────────┘
         ↓
    Shared DB
```

**Microservices**

```text
                 API Gateway
                      │
       ┌──────────────┼───────────────┐
       ↓              ↓               ↓
 Order Service   Payment Service   Inventory Service
       │              │               │
       ↓              ↓               ↓
   Order DB       Payment DB      Inventory DB
       │
       └──────────────┐
                      ↓
                Message Broker
                      │
             ┌────────┴────────┐
             ↓                 ↓
        Notification       Shipping
          Service           Service
```

Now we have introduced problems that don't exist, or are simpler, in a monolith:

```text
Network failures
Distributed transactions
Eventual consistency
Message duplication
Service discovery
Distributed tracing
Retry
Circuit breaker
Idempotency
```

**Those problems are exactly why we're going to learn the patterns in your roadmap.**

--------------

**When Should You Choose Microservices?**

* Do we need independent deployment?

```text
Order team
   ↓
Deploy Order
without deploying Payment?
```

If yes → microservices may help.

* Do different areas need independent scaling?

```text
Inventory → 20 instances
Order     → 5 instances
Payment   → 3 instances
```

Microservices can provide this independently.

* Do different teams own different business capabilities?

```text
Team A → Orders
Team B → Payments
Team C → Inventory
```

Microservices can align well with that organizational structure.

* Is the domain large and complex?

Microservices are particularly useful when there are multiple evolving subsystems and the organization can support the additional operational complexity. 

---

**When NOT to Use Microservices**

Avoid them when:

* Application is small
* Small development team
* No independent scaling requirement
* No independent deployment requirement
* Domain boundaries aren't understood
* Infrastructure/DevOps maturity is low
* Distributed complexity isn't justified

Microsoft explicitly notes that in some cases a monolithic deployment is preferable because the costs of microservices outweigh the benefits.

---

**What About Modular Monolith?**

This is an excellent answer in a system-design interview:

> "If I don't yet need independent deployment or scaling, but the application has multiple business domains, I would consider a modular monolith. It gives me strong module boundaries while keeping deployment and operational complexity low. If the boundaries and scaling/deployment requirements become clear later, individual modules can potentially be extracted into microservices."

---

**Most Important Concept: Business Capability**

Don't create services based on technical layers.

❌ Bad:

```text
UserService
DatabaseService
LoggingService
ValidationService
```

That's not necessarily a good business decomposition.

Think in terms of **business capabilities**:

```text
Ordering
Payment
Inventory
Shipping
Customer Management
```

Microsoft recommends designing services around business capabilities rather than horizontal technical layers, with loose coupling and high functional cohesion. 

---

**Interview Questions**

Q1. What is a monolith?

> A monolithic application is deployed as a single application unit, where multiple business capabilities typically run together.

Q2. What is a modular monolith?

> A modular monolith is a single deployable application internally divided into strongly isolated business modules.

Q3. What is a microservice?

> A microservice is an independently deployable service that implements a cohesive business capability, owns its domain logic/data, and communicates with other services through defined contracts.

Q4. Why microservices?

Mention:

```text
Independent deployment
Independent scaling
Team autonomy
Fault isolation
Technology flexibility
```

Q5. What are the disadvantages?

Mention:

```text
Network failures
Distributed transactions
Eventual consistency
Operational complexity
Monitoring/tracing
Deployment complexity
Data synchronization
```

Q6. Microservice vs Modular Monolith?

**Best answer:**

> "The major distinction isn't simply the number of modules. A modular monolith has strong internal boundaries but remains one deployment unit, whereas microservices are independently deployable and typically independently own their data."

-------
-------

## Service Boundaries / Bounded Context

**What is a Service Boundary?**

A **service boundary** defines what functionality, business rules, data, and responsibilities belong inside a particular microservice.

Think:

> **“Where does one business responsibility end and another begin?”**

For example, in an e-commerce system:

```text
                    E-Commerce System
                           |
          +----------------+----------------+
          |                |                |
       Order            Payment          Inventory
       Service           Service           Service
          |                |                |
     Order data       Payment data     Stock data
```

Each service owns its own business responsibility and domain data.

---

**What is Bounded Context?**

**Bounded Context (BC)** comes from **Domain-Driven Design (DDD)**.

> A Bounded Context is a boundary within which a particular **domain model and business language have a specific meaning**.

Microsoft recommends using domain analysis and bounded contexts to help identify microservice boundaries.

**Simple example**

Suppose we have:

```text
Customer
```

The meaning of Customer can differ depending on the business area.

**CRM**

```text
Customer
 ├── Name
 ├── Address
 ├── Phone
 ├── Email
 ├── LeadScore
 └── CommunicationHistory
```

**Ordering**

```text
Customer
 ├── CustomerId
 ├── Name
 └── ShippingAddress
```

**Support**

```text
Customer
 ├── CustomerId
 ├── SupportTickets
 └── SupportHistory
```

They refer to the same real-world person, but **each context needs a different model**.

You don't necessarily create one giant `Customer` model shared by every service.

This is one of the core ideas behind Bounded Context.

---

**Bounded Context vs Microservice**

These are related but **not exactly the same thing**.

| Bounded Context                       | Microservice                          |
| ------------------------------------- | ------------------------------------- |
| DDD concept                           | Architectural/physical implementation |
| Logical boundary                      | Independently deployable service      |
| Defines domain model                  | Implements business capability        |
| Can exist inside a monolith           | Usually separate process              |
| Doesn't require network communication | Communicates through APIs/events      |
| Defines business language             | Owns implementation/data              |

A Bounded Context **can be implemented as a microservice**, but the concepts aren't identical. Microsoft notes that a BC can sometimes contain multiple physical services.

**Interview answer**

> “Bounded Context is a DDD concept that defines the boundary of a domain model, while a microservice is a deployable architectural unit that can implement that bounded context.”

---

**Business Capability — Most Important Concept**

Don't create services based on technical layers.

❌ Bad design

```text
User Interface Service
Database Service
Business Logic Service
Logging Service
```

These are technical responsibilities.

✅ Better

```text
Order Service
Payment Service
Inventory Service
Shipping Service
Notification Service
```

These represent **business capabilities**.

Microsoft specifically recommends designing microservices around business capabilities rather than horizontal technical layers.

---

**High Cohesion + Low Coupling**

This is a very common interview question.

**High Cohesion**

Things inside a service should strongly belong together.

```text
Order Service

CreateOrder()
CancelOrder()
AddItem()
RemoveItem()
CalculateTotal()
GetOrder()
```

All are related to **Order**.

That's high cohesion.

**Low Coupling**

Services should have minimum dependency on each other's internal implementation.

```text
Order Service
      |
      | API/Event
      ↓
Payment Service
```

Order should **not** directly access:

```text
Payment DB ❌
Payment EF Core entities ❌
Payment internal classes ❌
```

Microsoft describes good microservice boundaries as having **loose coupling and high functional cohesion**.

---

**How Do We Identify Service Boundaries?**

This is the question I'd expect in a senior .NET interview.

Look at:

1. Business capability

> What business responsibility does this functionality represent?

Example:

```text
Order
Payment
Inventory
Shipping
```

2. Business rules

If a group of functionality has its own rules, it may indicate a boundary.

```text
Payment
 ├── Payment authorization
 ├── Refund
 ├── Payment status
 └── Payment gateway integration
```

3. Data ownership

> Who owns this data?

```text
Order Service
    ↓
Order DB

Payment Service
    ↓
Payment DB

Inventory Service
    ↓
Inventory DB
```

Each service should own its domain data.

4. Different business language

If the same term has different meanings in different areas, that's a strong signal for separate contexts.

```text
User Context       → User
CRM Context        → Customer
Ordering Context   → Buyer
Payment Context    → Payer
```

5. Change independently

> Can this functionality evolve independently?

For example:

```text
Payment rules change frequently
Order rules change independently
```

That may indicate separate boundaries.

6. Transaction boundary

> Which operations need to change together atomically?

If two sets of data always need to be changed together, separating them may create unnecessary distributed transaction complexity.

---

**Real Example — Order / Payment / Inventory**

Imagine:

```text
                  Client
                    |
                    ↓
              Order Service
               /          \
              ↓            ↓
       Payment Service   Inventory Service
```
**Order Service owns**

```text
Order
OrderItem
OrderStatus
TotalAmount
```

**Payment Service owns**

```text
Payment
Transaction
PaymentStatus
Refund
```

**Inventory Service owns**

```text
ProductStock
Reservation
AvailableQuantity
```

Each service has its own model.

```text
Order DB       Payment DB       Inventory DB
   │                │                │
   ▼                ▼                ▼
Orders          Payments          Stock
```

The services communicate through **APIs/events**, not by directly accessing each other's database. 

---

**Important: Don't Share Domain Entities**

❌ Bad

```text
Shared Customer.cs

Used by:
Order
Payment
CRM
Support
```

Now changing `Customer` potentially affects multiple services.

✅ Better

```text
Order Service
    OrderCustomer

Payment Service
    Payer

CRM Service
    Customer
```

They can share the same identifier:

```text
CustomerId = 101
```

but have **different models** appropriate to their context. 

---

**Avoid the Distributed Monolith**

This is a very important interview concept.

Suppose:

```text
Order
 ↓
Payment
 ↓
Inventory
 ↓
Shipping
 ↓
Notification
```

And every operation requires all services synchronously.

Then:

```text
Payment unavailable
       ↓
Order unavailable
       ↓
Inventory unavailable
```

Even though you have multiple services, they are **strongly coupled operationally**.

That's effectively moving monolith coupling onto the network.

Good microservice design aims for **autonomous services with well-defined contracts**.

We'll deal with this further when we study **Sync vs Async communication, Saga, Outbox, retries and Circuit Breaker**.

---

**How would you identify microservice boundaries?**

> "I would start with the business domain rather than technical layers. I would identify business capabilities and bounded contexts, then look at business rules, data ownership, business language, transaction boundaries, and coupling between functionalities. Each service should have high cohesion and low coupling and should own its domain data. For example, in an e-commerce application I would typically separate Order, Payment, Inventory and Shipping based on their independent business responsibilities."

---

**Quick Interview Questions**

Q1. What is Bounded Context?

> A boundary within which a specific domain model and business language have a defined meaning.

Q2. Is Bounded Context the same as Microservice?

> No. Bounded Context is a DDD concept; a microservice is a deployable architectural unit that can implement a bounded context.

Q3. How do you decide service boundaries?

> Business capabilities, domain models, business rules, data ownership, transaction boundaries, team ownership and coupling.

Q4. Should services share the same database?

> Generally no. Each microservice should own its domain data and expose it through APIs or messaging.

Q5. Should services share domain entities?

> No. Each bounded context should have its own domain model appropriate to its business requirements.

Q6. What is high cohesion?

> Related business responsibilities are grouped together within the same service.

Q7. What is low coupling?

> A service can change internally without requiring changes to other services.

Q8. Should every microservice be very small?

> No. Size isn't the primary goal. Meaningful business boundaries, cohesion, autonomy and low coupling are more important. 

---------
---------

## Database per Service

> Each microservice owns its data and is the only service allowed to directly access that data.
---

**1. Why Database per Service?**

Consider an e-commerce system:

```text
                 E-Commerce
                     │
       ┌─────────────┼─────────────┐
       ↓             ↓             ↓
    Order         Payment       Inventory
    Service        Service        Service
       │             │             │
       ↓             ↓             ↓
   Order DB      Payment DB     Inventory DB
```

Each service:

* owns its data
* controls its schema
* controls its EF Core `DbContext`
* can change its database independently
* doesn't directly query another service's database

This gives the service **data autonomy**.

---

**2. What Does "Database per Service" Actually Mean?**

It **doesn't necessarily mean**:

> One physical database server for every microservice.

It means:

> **The data ownership boundary is per service.**

For example, you could have:

```text
PostgreSQL Server
│
├── OrderDb
├── PaymentDb
└── InventoryDb
```

All databases can technically be hosted on the same PostgreSQL server.

The important thing is that:

```text
Order Service → OrderDb
Payment Service → PaymentDb
Inventory Service → InventoryDb
```

and **Order Service cannot directly query PaymentDb**.

---

**3. Shared Database — What Is Wrong With It?**

❌ Shared database

```text
Order Service ─────┐
Payment Service ───┼──→ Shared DB
Inventory Service ─┘
```

Now imagine:

```text
Payment Service
      ↓
ALTER TABLE Orders
```

The Order service can break.

Or:

```text
Order Service
      ↓
SELECT * FROM Payments
```

Now Order is coupled to Payment's database schema.

If Payment changes:

```text
Payments
──────────────
Status
```

to:

```text
Payments
──────────────
PaymentStatus
```

Order might break.

This destroys independent deployment and evolution.

---

**4. Correct Approach**

```text
┌───────────────────┐
│   Order Service   │
│                   │
│ OrderDbContext    │
└─────────┬─────────┘
          ↓
       Order DB


┌───────────────────┐
│ Payment Service   │
│                   │
│ PaymentDbContext  │
└─────────┬─────────┘
          ↓
      Payment DB
```

If Order needs payment information:

```text
Order Service
      │
      │ API / Message
      ↓
Payment Service
      │
      ↓
 Payment DB
```

**Never:**

```text
Order Service
      │
      ↓
Payment DB ❌
```

Services should access another service's data through its API or asynchronous messaging. 

---

**5. .NET + EF Core Example**

Imagine two independent services.

Order Service

```csharp
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
```

Connection:

```json
{
  "ConnectionStrings": {
    "OrderDb": "Host=localhost;Database=OrderDb;..."
  }
}
```

---

Payment Service

```csharp
public class PaymentDbContext : DbContext
{
    public DbSet<Payment> Payments => Set<Payment>();
}
```

Connection:

```json
{
  "ConnectionStrings": {
    "PaymentDb": "Host=localhost;Database=PaymentDb;..."
  }
}
```

Each service manages its own migrations:

```text
Order Service
    └── OrderDbContext
         └── OrderDb migrations

Payment Service
    └── PaymentDbContext
         └── PaymentDb migrations
```

EF Core's `DbContext` represents the persistence boundary for that service. 

---

**6. Can They Use Different Database Technologies?**

Yes.

This is called **Polyglot Persistence**.

Example:

```text
Order Service
    ↓
PostgreSQL

Payment Service
    ↓
SQL Server

Catalog Service
    ↓
MongoDB

Search Service
    ↓
Elasticsearch
```

The database should fit the service's requirements. Microsoft explicitly describes microservices as potentially using different storage technologies, including SQL and NoSQL. 

But don't use different databases **just because you can**.

For example, if your organization is comfortable with PostgreSQL and it satisfies all services:

```text
Order       → PostgreSQL
Payment     → PostgreSQL
Inventory   → PostgreSQL
```

is perfectly reasonable.

---

**7. Biggest Problem: Cross-Service Queries**

Suppose the UI needs:

```text
Order
Customer Name
Payment Status
Product Name
Inventory Status
```

In a monolith:

```sql
SELECT ...
FROM Orders
JOIN Payments ...
JOIN Products ...
```

Easy.

But in microservices:

```text
Order DB       Payment DB       Inventory DB
    │               │               │
    └─────── ❌ SQL JOIN ❌ ────────┘
```

You cannot simply perform a SQL join across independently owned service databases.

Microsoft identifies cross-service queries as one of the major distributed-data challenges. 

Possible solutions include:

* API composition
* API Gateway aggregation
* CQRS/read models
* materialized views
* events
* reporting/data warehouse

We'll cover these later when we reach **CQRS and messaging**.

---

**8. Biggest Problem: Transactions**

In a monolith:

```text
Order + Payment + Inventory
          ↓
     One transaction
          ↓
      COMMIT
```

In microservices:

```text
Order DB
Payment DB
Inventory DB
```

You generally don't have one normal ACID transaction spanning all three.

Instead, distributed workflows typically use patterns such as:

```text
Eventual Consistency
        +
Saga
        +
Outbox
```

---

Order service needs PaymentStatus. What would you do?

> "I would not directly access the Payment database. The Order service can obtain the information through the Payment service API, or consume payment events and maintain the required data locally. The choice depends on whether the information needs to be real-time and on the consistency requirements."

That's much better than:

> "I'll create a SQL JOIN." ❌

---

**Database per Service ≠ No Data Duplication**

This is important.

Suppose Payment needs:

```text
CustomerId
CustomerName
```

You might maintain a local copy:

```text
Payment DB

CustomerId
CustomerName
```

even though Customer information originated elsewhere.

This is acceptable when designed intentionally.

```text
Customer Service
       │
       │ CustomerUpdated event
       ↓
Payment Service
       │
       ↓
Local Customer projection
```

Now Payment doesn't need to call Customer Service for every operation.

The trade-off is **eventual consistency**.

---

**Interview Questions**

Q1. What is Database per Service?

> Each microservice owns and controls its domain data, and other services cannot directly access its database.

Q2. Does it require separate physical database servers?

> No. The key requirement is independent data ownership. Multiple service databases can run on the same database server.

Q3. Can microservices share the same database schema?

> Generally no, because shared schemas create coupling and prevent independent evolution.

Q4. How do services communicate when they need another service's data?

> Through APIs or asynchronous messaging/events.

Q5. How do you perform joins across microservices?

> You don't directly perform database joins. Use API composition, CQRS/read models, materialized views, or event-driven projections depending on the requirement.

Q6. How do you handle transactions across databases?

> Avoid relying on a distributed ACID transaction; use patterns such as Saga and eventual consistency for cross-service business workflows.

Q7. What is Polyglot Persistence?

> Using different database technologies for different services based on their requirements.
-------
-------

## API Gateway

> An API Gateway is a single entry point between clients and backend microservices. It acts as a reverse proxy, routes requests to the appropriate services, and can handle cross-cutting concerns such as authentication, rate limiting, SSL termination, logging, and request aggregation.
---

**Without API Gateway**

Suppose we have:

```text
                    Angular App
                  /      |       \
                 ↓       ↓        ↓
             Order API Payment API Inventory API
```

The frontend needs to know:

```text
/api/orders
/api/payments
/api/inventory
```

Now the client is tightly coupled to your internal service architecture.

If you change:

```text
Payment Service → PaymentServiceV2
```

the frontend may need changes.

---

**With API Gateway**

```text
                    Angular App
                         |
                         ↓
                  ┌─────────────┐
                  │ API Gateway │
                  └──────┬──────┘
                         |
             ┌───────────┼───────────┐
             ↓           ↓           ↓
          Order       Payment     Inventory
          Service      Service      Service
```

Frontend only knows:

```text
https://api.myapp.com
```

The gateway knows where the internal services are.

This decouples clients from the internal microservice topology. 

---

**Main Responsibilities**

Think of API Gateway in **5 major categories**.

| Responsibility  | Example                            |
| --------------- | ---------------------------------- |
| Routing         | `/orders` → Order Service          |
| Authentication  | Validate JWT                       |
| Authorization   | Check policies/roles               |
| Rate limiting   | 100 requests/minute                |
| Aggregation     | Combine Order + Payment + Shipping |
| SSL termination | HTTPS handled at gateway           |
| Logging         | Request/response metadata          |
| Caching         | Cache selected responses           |
| IP filtering    | Allow/block specific clients       |

Not every gateway must perform all of these; capabilities depend on the chosen technology. 

---

**Gateway Routing**

This is the simplest use case.

Client:

```http
GET /api/orders/123
```

Gateway:

```text
/api/orders/*
       ↓
Order Service
```

Another request:

```http
GET /api/payments/456
```

Gateway:

```text
/api/payments/*
       ↓
Payment Service
```

So:

```text
Client
  |
  ↓
API Gateway
  |
  ├── /orders    → Order Service
  ├── /payments  → Payment Service
  └── /inventory → Inventory Service
```

This is called **Gateway Routing**. 

---

**Gateway Aggregation**

Suppose Angular's Order Details page needs:

```text
Order
Customer
Payment
Shipment
```

Without gateway aggregation:

```text
Angular
   |
   ├──→ Order Service
   ├──→ Customer Service
   ├──→ Payment Service
   └──→ Shipping Service
```

Four network calls.

With aggregation:

```text
Angular
   |
   ↓
API Gateway
   |
   ├──→ Order Service
   ├──→ Customer Service
   ├──→ Payment Service
   └──→ Shipping Service
   |
   ↓
Combined Response
   |
   ↓
Angular
```

The client makes **one request**.

This reduces client-side chattiness and can simplify client communication. 

---

**Gateway Offloading**

Some responsibilities don't belong in every individual service.

Instead of:

```text
Order Service       → JWT validation
Payment Service     → JWT validation
Inventory Service   → JWT validation
Shipping Service    → JWT validation
```

You can centralize some concerns:

```text
                    API Gateway
                         |
       ┌─────────────────┼─────────────────┐
       │                 │                 │
 Authentication     Rate Limiting      SSL
       │
       ↓
     Services
```

This is called **Gateway Offloading**. 

But don't put **business logic** into the gateway.

---

**Very Important: Gateway Should NOT Contain Business Logic**

❌ Bad

```text
API Gateway

if customer.IsPremium
   apply 20% discount

if inventory < 10
   change order status
```

Now your gateway understands your business domain.

That creates coupling.

✅ Good

```text
API Gateway

Authentication
Routing
Rate Limiting
Request transformation
Aggregation
Observability
```

Business rules remain inside the services.

Microsoft explicitly recommends keeping domain knowledge out of the gateway. 

---

**API Gateway vs Load Balancer**

Very common interview question.

| API Gateway              | Load Balancer                             |
| ------------------------ | ----------------------------------------- |
| Application/API level    | Primarily traffic distribution            |
| Layer 7 routing          | Can operate at L4/L7 depending on product |
| Authentication/policies  | Usually not business/API policy focused   |
| API aggregation          | Usually no                                |
| Rate limiting            | Often supported                           |
| Routes based on API/path | Distributes traffic among instances       |

Example:

```text
API Gateway
     |
     ├── /orders → Order Service
     └── /payments → Payment Service
                       |
                       ↓
                  Load Balancer
                   /        \
                  ↓          ↓
             Payment #1  Payment #2
```

You can use **both**.

---

**API Gateway vs Reverse Proxy**

A reverse proxy sits between clients and backend servers.

```text
Client
  ↓
Reverse Proxy
  ↓
Backend
```

An API Gateway is essentially a **reverse proxy specialized for API/microservice scenarios**, usually with additional capabilities such as authentication, rate limiting, aggregation and API policies. 

---

**What Should We Use in .NET/Azure?**

For your interview background, remember these:

**Azure API Management**

```text
Angular
   ↓
Azure API Management
   ↓
.NET Microservices
```

Microsoft recommends evaluating **Azure API Management** for production API gateways in large Azure microservice applications.

It provides capabilities such as:

* API policies
* authentication/authorization integration
* rate limiting
* API versioning
* developer portal
* monitoring
* transformations

**.NET/YARP**

YARP = **Yet Another Reverse Proxy**.

Useful when you want to build/customize a reverse-proxy/gateway solution in .NET.

**Ocelot**

A .NET-oriented API Gateway commonly used for simpler microservice gateway scenarios.

---

**API Gateway vs BFF**

**BFF = Backend for Frontend**

Suppose you have:

```text
                  API Gateway
                 /           \
                ↓             ↓
          Web Backend     Mobile Backend
              ↓                ↓
          Services           Services
```

Web and mobile may need different responses.

Instead of one generic API:

```text
Web → BFF → Services
Mobile → BFF → Services
```

For example:

```text
Web BFF
    ↓
Detailed Order Response

Mobile BFF
    ↓
Compact Order Response
```

BFF is often implemented as a specialized gateway for a particular client type.

---

**Security Flow**

A typical architecture:

```text
Angular
   |
   | JWT
   ↓
API Gateway
   |
   | Validate / apply policies
   ↓
Order Service
   |
   ↓
Order DB
```

The gateway can handle some authentication/security concerns, but **authorization/business access control may still need to be enforced by the services**, especially for domain-specific rules.

Example:

```text
Gateway:
"Is this token valid?"

Order Service:
"Can this user access Order #123?"
```

That's a good separation of responsibility.

---

> **API Gateway centralizes access, but it shouldn't become a single-instance bottleneck or failure point.**

---

**Biggest Disadvantage**

API Gateway is useful, but it introduces another component.

Potential problems:

```text
                API Gateway
                     ↓
             ┌───────┴───────┐
             ↓               ↓
          Services        Services
```

If poorly designed, the gateway can become:

* bottleneck
* single point of failure
* too much business logic
* difficult to maintain
* overly coupled to backend services

Also, aggregation can increase gateway complexity and latency because it waits on multiple backend calls. 

---

**Interview Questions**

Q1. What is API Gateway?

> A single entry point that routes client requests to microservices and can centralize cross-cutting concerns such as authentication, rate limiting and SSL termination.

Q2. Why do we need an API Gateway?

> To hide internal service topology, simplify clients, centralize API policies and reduce direct exposure of microservices.

Q3. What is Gateway Routing?

> Routing a client request to the appropriate backend service based on path, HTTP method or other routing rules.

Q4. What is Gateway Aggregation?

> The gateway calls multiple backend services and combines their responses into one response for the client.

Q5. What is Gateway Offloading?

> Moving cross-cutting concerns such as authentication, SSL termination and rate limiting from individual services to the gateway.

Q6. Should business logic be implemented in API Gateway?

> No. The gateway should handle infrastructure/API concerns; domain/business logic belongs to the microservices.

Q7. API Gateway vs Load Balancer?

> A load balancer primarily distributes traffic among instances, while an API gateway provides API-level routing and policies and can perform aggregation and other cross-cutting functions.

Q8. What is BFF?

> Backend for Frontend is a gateway/backend specifically tailored to the needs of a particular client such as web or mobile.

---
---

## Sync vs Async Communication

**`Synchronous Communication`**

> **Service A sends a request to Service B and waits for the response.**

Most commonly:

```text
Order Service
     |
     | HTTP Request
     ↓
Payment Service
     |
     | Response
     ↓
Order Service
```

Example:

```http
POST /api/payments
```

Payment service responds:

```json
{
  "paymentId": 123,
  "status": "Success"
}
```

Order Service waits for that response before continuing.


**.NET Example — Synchronous**

Using `HttpClient`:

```csharp
public async Task<PaymentResponse> ProcessPayment(
    PaymentRequest request)
{
    var response = await httpClient.PostAsJsonAsync(
        "/api/payments",
        request);

    response.EnsureSuccessStatusCode();

    return await response.Content
        .ReadFromJsonAsync<PaymentResponse>();
}
```

Flow:

```text
Order Service
     │
     │ HTTP
     ↓
Payment Service
     │
     │ Payment Response
     ↓
Order Service
     │
     ↓
Continue processing
```

**Important**

`async/await` in C# **does not make the business communication asynchronous**.

The communication is still **request/response over HTTP**.

This is a common interview trap.

---

**`Asynchronous Communication`**

> Service A sends a message/event and does not wait for Service B to finish processing it.

Example:

```text
Order Service
     |
     | OrderCreated Event
     ↓
 Message Broker
     |
     ├────────→ Inventory Service
     |
     ├────────→ Notification Service
     |
     └────────→ Analytics Service
```

Order Service can continue after publishing the event.

----
**Real Example**

Suppose an order is created.

**Synchronous approach**

```text
Create Order
     ↓
Call Payment
     ↓
Wait
     ↓
Call Inventory
     ↓
Wait
     ↓
Send Notification
     ↓
Return response
```

This creates a long dependency chain.

**Asynchronous approach**

```text
Create Order
     ↓
Publish OrderCreated
     ↓
Return / continue
     ↓
       Message Broker
        /     |      \
       ↓      ↓       ↓
   Payment Inventory Notification
```

Services process the event independently.

---

**Request vs Event**

This distinction is **very important**.

`Request`

> "Payment Service, please process this payment."

```text
Order → Payment
```

This is generally a **command/request**.

The sender expects processing to happen as part of the interaction.

`Event`

> "An order has been created."

```text
Order → OrderCreated Event
```

The sender is saying:

> "This already happened."

Other services decide whether they care.

---
**Synchronous vs Asynchronous**

|                         | Synchronous                  | Asynchronous               |
| ----------------------- | ---------------------------- | -------------------------- |
| Communication           | HTTP/gRPC commonly           | Message/Event              |
| Wait for response       | Yes                          | No                         |
| Coupling                | Higher                       | Lower                      |
| Response                | Immediate                    | Usually later              |
| Failure handling        | Immediate failure possible   | Can retry later            |
| Availability dependency | Higher                       | Lower                      |
| Consistency             | Easier immediate consistency | Often eventual consistency |
| Complexity              | Simpler initially            | More infrastructure        |
| Example                 | Get payment status           | OrderCreated event         |

---

**When Should We Use Synchronous?**

Use synchronous communication when the caller **needs an immediate answer**.

---

**When Should We Use Asynchronous?**

Use asynchronous communication when the operation:

* doesn't need an immediate response
* can happen in the background
* may take time
* should be decoupled
* can tolerate eventual consistency

Examples:

```text
Order Created
     ↓
Send Email

Order Created
     ↓
Generate Invoice

Order Created
     ↓
Update Analytics

Order Created
     ↓
Update Search Index
```

These don't necessarily need to block the user's request.

---
---

## Message Broker

> **A message broker is infrastructure that receives messages from producers and delivers them to consumers, allowing services to communicate asynchronously without directly depending on each other.**

Think of it as a **middleman**:

```text
Order Service
     |
     | Message
     ↓
┌─────────────────┐
│ Message Broker  │
└────────┬────────┘
         |
         ↓
Payment Service
```

The producer doesn't need to directly know where/how the consumer is running.

---

**Why Do We Need a Message Broker?**

Without a broker:

```text
Order ─────→ Payment
Order ─────→ Inventory
Order ─────→ Notification
```

Order Service directly knows about all these services.

With a broker:

```text
                    ┌→ Payment
                    │
Order → Broker ─────┼→ Inventory
                    │
                    └→ Notification
```

This gives us:

* loose coupling
* asynchronous processing
* buffering
* retry capability
* load leveling
* publish/subscribe
* independent consumers

Microsoft describes message brokers/service buses as a common infrastructure for asynchronous communication between microservices. 

---

**Producer and Consumer**

These two terms are essential.

**`Producer`**

The service that **sends/publishes** the message.

```text
Order Service
     ↓
  Producer
```

**`Consumer`**

The service that **receives/processes** the message.

```text
  Consumer
     ↓
Payment Service
```

So:

```text
Producer → Broker → Consumer
```

---

**Queue**

A **queue** is generally used for **point-to-point / single-consumer processing**.

```text
Order Service
     |
     ↓
┌──────────────┐
│ Order Queue  │
└──────┬───────┘
       ↓
Payment Service
```

Suppose 100 messages arrive:

```text
Queue:
[M1][M2][M3][M4][M5]...
```

Consumers process them.

You can have multiple instances:

```text
              Queue
                |
        ┌───────┼───────┐
        ↓       ↓       ↓
    Consumer1 Consumer2 Consumer3
```

The workers can **compete for messages**, which helps distribute work. Azure Service Bus supports this competing-consumer model. 

> **Queue = distribute work**

---

**Topic + Subscription**

A **topic** is generally used for **publish/subscribe**.

Example:

```text
                 Order Service
                      |
                      ↓
                OrderCreated
                      |
                      ↓
                 ┌─────────┐
                 │  Topic  │
                 └────┬────┘
                      |
          ┌───────────┼───────────┐
          ↓           ↓           ↓
     Subscription  Subscription  Subscription
          ↓           ↓           ↓
      Inventory     Email       Analytics
```

Each subscription gets its own copy of the published message. Azure Service Bus uses topics and subscriptions for this model. 

> **Topic = broadcast event**

---

**Queue vs Topic**

| Queue                                       | Topic                                        |
| ------------------------------------------- | -------------------------------------------- |
| Point-to-point                              | Publish/Subscribe                            |
| Usually one processing consumer per message | Multiple subscriptions can receive the event |
| Work distribution                           | Event broadcasting                           |
| `OrderCreated` → Payment worker             | `OrderCreated` → Payment + Email + Analytics |
| Competing consumers                         | Independent subscribers                      |

---

**Message vs Event vs Command**

This distinction is extremely useful.

`Command`

> "Please do this."

```text
ProcessPayment
```

```text
Order → Payment
```

Usually one intended consumer.

---

`Event`

> "This already happened."

```text
OrderCreated
```

Multiple services may be interested.

```text
Order
  ↓
OrderCreated
  ↓
 ├── Inventory
 ├── Notification
 └── Analytics
```
---
`Message`

Generic term for the data transmitted through the broker.

So:

> **Command and Event are types of messages.**

---

**Real Example**

Suppose customer places an order.

Step 1

Order Service saves:

```text
OrderId = 1001
Status = Created
```

Step 2

It publishes:

```json
{
  "eventType": "OrderCreated",
  "orderId": 1001
}
```

Step 3

Broker receives it.

```text
Order Service
     ↓
Message Broker
     ↓
OrderCreated
```

Step 4

Consumers process it:

```text
                  OrderCreated
                       |
             ┌─────────┼─────────┐
             ↓         ↓         ↓
         Payment   Inventory   Notification
```

Now Order Service doesn't need to directly call all three.

---

**Azure Service Bus**

Microsoft describes Azure Service Bus as a fully managed enterprise message broker supporting **queues and publish/subscribe topics**. 

Typical architecture:

```text
Angular
   ↓
API Gateway
   ↓
Order Service
   ↓
Azure Service Bus
   ├── Queue
   └── Topic
         ├── Payment Subscription
         ├── Inventory Subscription
         └── Notification Subscription
```

---

**NET Example — Sending a Message**

Modern Azure SDK:

```bash
dotnet add package Azure.Messaging.ServiceBus
```

Microsoft currently documents `Azure.Messaging.ServiceBus` as the .NET client library. 

Example:

```csharp
await using var client =
    new ServiceBusClient(connectionString);

ServiceBusSender sender =
    client.CreateSender("orders");

var message = new ServiceBusMessage(
    JsonSerializer.Serialize(orderCreated));

await sender.SendMessageAsync(message);
```

Conceptually:

```text
Order Service
     ↓
ServiceBusClient
     ↓
Queue/Topic
```

---

**Consuming a Message**

A consumer receives the message:

```csharp
await using var client =
    new ServiceBusClient(connectionString);

var processor =
    client.CreateProcessor("orders");

processor.ProcessMessageAsync += async args =>
{
    var message = args.Message;

    // Process message
    await ProcessOrder(message);
};

processor.ProcessErrorAsync += args =>
{
    // Handle processing error
    return Task.CompletedTask;
};

await processor.StartProcessingAsync();
```

In a real ASP.NET Core application, this kind of background processing is commonly hosted through a worker/`BackgroundService` pattern.

---

**What Happens If Consumer Fails?**

Suppose:

```text
Broker
   ↓
Payment Service
   ↓
❌ Processing failed
```

You don't necessarily want to lose the message.

A broker can support retry/redelivery mechanisms.

Conceptually:

```text
Message
   ↓
Consumer
   ↓
Failure
   ↓
Retry
   ↓
Consumer
```

After repeated failures, the message can be moved to a:

> **Dead Letter Queue (DLQ)**

```text
Queue
  ↓
Consumer
  ↓
❌ Failed repeatedly
  ↓
Dead Letter Queue
```

Azure Service Bus supports dead-letter queues and at-least-once delivery. 

---

**At-Least-Once Delivery**

This is a **very important interview concept**.

A message broker may deliver a message more than once.

Example:

```text
OrderCreated
     ↓
Payment
     ↓
Payment succeeds
     ↓
Network failure before acknowledgement
     ↓
Broker doesn't know processing succeeded
     ↓
Message delivered again
```

Now:

```text
Payment processed twice ❌
```

Therefore consumers should often be **idempotent**.

Example:

```text
MessageId = ABC123
```

Consumer checks:

```text
Have I already processed ABC123?
        |
    ┌───┴───┐
   Yes      No
    ↓        ↓
 Ignore    Process
```

**Idempotency** is one of your later roadmap topics, so we'll cover it properly there.

Azure Service Bus documents at-least-once delivery and duplicate-related reliability considerations. 

---

**Message Ordering**

Sometimes order matters.

Example:

```text
OrderCreated
PaymentCompleted
OrderShipped
```

You don't want:

```text
OrderShipped
     ↓
OrderCreated
```

Some messaging systems provide ordering features.

Azure Service Bus supports **sessions** for scenarios where message ordering matters. 

But don't assume every broker guarantees ordering automatically.


> "Message ordering depends on the broker and configuration; if business processing requires ordering, I explicitly design for it rather than assuming it."

---

**RabbitMQ vs Azure Service Bus vs Kafka**

|                                      | RabbitMQ                        | Azure Service Bus          | Kafka                                          |
| ------------------------------------ | ------------------------------- | -------------------------- | ---------------------------------------------- |
| Type                                 | Message broker                  | Managed enterprise broker  | Distributed event streaming platform           |
| Queue                                | ✅                               | ✅                          | Consumer groups                                |
| Pub/Sub                              | ✅                               | ✅                          | ✅                                              |
| Azure managed service                | ❌                               | ✅                          | Azure Event Hubs is a different Azure service  |
| Strong enterprise messaging features | ✅                               | ✅                          | Different focus                                |
| Event streaming/high throughput      | Possible, but not primary focus | Not primary focus          | ⭐ Strong                                       |
| Typical use                          | Application messaging           | Enterprise/Azure messaging | Event streaming, analytics, high-volume events |


> "Kafka is just a message queue." ❌

Kafka is fundamentally an **event streaming platform/log**, whereas RabbitMQ and Azure Service Bus are commonly used as messaging brokers.

---

**Message Broker vs Event Bus**

These terms are sometimes used loosely.

A useful interview distinction:

```text
Application concept
       ↓
   Event Bus
       ↓
Infrastructure
       ↓
RabbitMQ / Azure Service Bus
```

Your application might define:

```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T @event);
}
```

Then infrastructure implements it using:

```text
RabbitMQ
```

or:

```text
Azure Service Bus
```

---

**Critical Problem — Database + Message Broker**

Suppose:

```text
Order Service
    |
    ├── Save Order to DB
    |
    └── Publish OrderCreated
```

What if:

```text
DB Save       → SUCCESS
Message       → FAILED
```

Now:

```text
Order exists
BUT
OrderCreated wasn't published
```

This is a classic distributed-system problem.

And this leads directly to:

**Outbox Pattern**

```text
Order Service
     |
     ├── Order Table
     |
     └── Outbox Table
              |
              ↓
         Message Publisher
              |
              ↓
       Message Broker
```

We'll study **Outbox** later in your roadmap.

---

**Message Broker Architecture**

Put everything together:

```text
                         Angular
                            │
                            ↓
                       API Gateway
                            │
                            ↓
                      Order Service
                            │
                            │ Publish
                            ↓
                  ┌───────────────────┐
                  │   Message Broker  │
                  └─────────┬─────────┘
                            │
              ┌─────────────┼─────────────┐
              ↓             ↓             ↓
          Payment       Inventory      Notification
          Consumer       Consumer        Consumer
              │             │             │
              ↓             ↓             ↓
          Payment DB     Stock DB       Email/API
```

This is the architecture you should be able to draw on a whiteboard.

---
---

## Saga Pattern 

> **How do we maintain business consistency when one business transaction spans multiple microservices, each with its own database?**

In a monolith, you might use one database transaction. In microservices, each service has its own local transaction, so a single normal ACID transaction doesn't span all services. Saga coordinates a sequence of local transactions and uses **compensating transactions** when a later step fails.

---

**The Problem**

Suppose placing an order involves:

```text
Order
  ↓
Payment
  ↓
Inventory
  ↓
Shipping
```

And each service has its own DB:

```text
Order DB
Payment DB
Inventory DB
Shipping DB
```

You can't simply do:

```csharp
using var transaction = ...
```

across all four databases.

Instead, each service performs its own local transaction.

---

**Without Saga**

Imagine:

```text
1. Create Order       ✅
2. Payment            ✅
3. Reserve Inventory  ❌
```

Now what?

Payment has already succeeded.

You need to **reverse/refund the payment** and potentially cancel the order.

That's where Saga comes in.

---

**Saga — Basic Idea**

> **A sequence of local transactions where each successful step triggers the next step, and failures trigger compensating actions for previously completed steps.** 

Example:

```text
Create Order
     ↓
Process Payment
     ↓
Reserve Inventory
     ↓
Create Shipment
     ↓
Order Completed
```

If Inventory fails:

```text
Create Order       ✅
     ↓
Payment            ✅
     ↓
Inventory          ❌
     ↓
Compensate Payment
     ↓
Cancel Order
```

---

**Compensating Transaction**

This is the **key Saga concept**.

A compensation isn't necessarily a database `ROLLBACK`.

Instead, it's a **business operation that reverses or compensates for a previous operation**.

Example:

```text
Original operation:
Payment → Capture ₹1,000

Compensation:
Payment → Refund ₹1,000
```

Another:

```text
Original:
Inventory → Reserve 2 items

Compensation:
Inventory → Release 2 items
```

Another:

```text
Original:
Order → Create Order

Compensation:
Order → Cancel Order
```

---

**Real Example**

**Successful flow**

```text
                    Saga
                     │
                     ↓
              Create Order
                     │
                     ↓
             Process Payment
                     │
                     ↓
           Reserve Inventory
                     │
                     ↓
             Create Shipment
                     │
                     ↓
                COMPLETED
```

Each step updates its own database.

```text
Order Service      → Order DB
Payment Service    → Payment DB
Inventory Service  → Inventory DB
Shipping Service   → Shipping DB
```

---

**Failure Scenario**

Suppose:

```text
Create Order       ✅
Payment            ✅
Inventory          ❌
```

Saga executes compensation:

```text
Inventory
   ❌
   ↓
Compensate Payment
   ↓
Refund Payment
   ↓
Compensate Order
   ↓
Cancel Order
```

Final state:

```text
Order      → Cancelled
Payment    → Refunded
Inventory  → Not Reserved
```

The system reaches a consistent business state through compensation rather than one global rollback. 

---

**Two Types of Saga**

There are two major approaches:

```text
1. Choreography
2. Orchestration
```

---

**`Choreography`**

There is **no central coordinator**.

Each service listens for events and publishes the next event.

```text
Order Service
     │
     │ OrderCreated
     ↓
 Message Broker
     │
     ↓
Payment Service
     │
     │ PaymentCompleted
     ↓
 Message Broker
     │
     ↓
Inventory Service
     │
     │ InventoryReserved
     ↓
 Message Broker
     │
     ↓
Shipping Service
```

Each service decides what to do based on events.

Microsoft describes choreography as allowing individual services to participate in and decide how the workflow progresses rather than relying on a central orchestrator. 

---

**`Choreography failure`**

```text
OrderCreated
     ↓
Payment
     ↓
PaymentCompleted
     ↓
Inventory ❌
     ↓
InventoryReservationFailed
     ↓
Payment Service
     ↓
RefundPayment
     ↓
Order Service
     ↓
CancelOrder
```

No central Saga service is controlling everything.

---

**Advantages**

* No central orchestrator
* Services are relatively autonomous
* Good for simple workflows
* Naturally event-driven

**Disadvantages**

As the workflow grows:

```text
A → B → C → D → E → F
```

it becomes harder to understand:

* who triggers what
* where failures are handled
* where compensation happens
* overall workflow state

Microsoft notes that choreography can become difficult to manage as the number of services and dependencies grows. 

---

**`Orchestration`**

Here we introduce a **central Saga Orchestrator**.

```text
              Saga Orchestrator
              /       |        \
             ↓        ↓         ↓
          Order    Payment   Inventory
          Service   Service    Service
```

The orchestrator tells each service what to do.

Example:

```text
Orchestrator
     |
     | CreateOrder
     ↓
Order Service
     |
     | Success
     ↓
Orchestrator
     |
     | ProcessPayment
     ↓
Payment Service
     |
     | Success
     ↓
Orchestrator
     |
     | ReserveInventory
     ↓
Inventory Service
```

---

**Orchestration Failure**

Suppose:

```text
Order       ✅
Payment     ✅
Inventory   ❌
```

The orchestrator knows:

```text
Completed:
✓ Order
✓ Payment

Failed:
✗ Inventory
```

So it can execute:

```text
Refund Payment
     ↓
Cancel Order
```

This makes complex workflows easier to visualize and control.

---

**Choreography vs Orchestration**

|                     | Choreography                        | Orchestration               |
| ------------------- | ----------------------------------- | --------------------------- |
| Coordinator         | ❌ No central coordinator            | ✅ Orchestrator              |
| Communication       | Events                              | Commands + responses/events |
| Simplicity          | Good for simple flows               | Good for complex flows      |
| Central control     | Low                                 | High                        |
| Workflow visibility | Harder                              | Easier                      |
| Coupling            | Can grow through event dependencies | Orchestrator knows workflow |
| Failure handling    | Distributed                         | Centralized workflow logic  |
| Best suited         | Smaller/simple workflows            | Complex business workflows  |


> **"Choreography distributes workflow decisions across services using events, while orchestration uses a central coordinator to control the sequence and compensation of the Saga."**

---

**Very Important — Saga Is NOT a Rollback**

This is a common interview trap.

Database transaction

```text
BEGIN
   Operation A
   Operation B
   Operation C
ROLLBACK
```

Database rollback restores the transaction's database state.

Saga

```text
Order created
Payment captured
Inventory failed

→ Refund payment
→ Cancel order
```

These are **new business operations**.

So:

> **Saga compensation is not the same thing as database rollback.**

This distinction is extremely important.

---

**Saga + Message Broker**

Your previous topic now connects directly.

```text
Order Service
      │
      ↓
Message Broker
      │
      ↓
Payment Service
      │
      ↓
Message Broker
      │
      ↓
Inventory Service
```

The broker transports commands/events.

Saga defines the **business workflow and compensation**.

So:

```text
Message Broker
    = HOW messages move

Saga
    = HOW distributed business transaction is coordinated
```

---

**.NET Example — Conceptual Orchestrator**

You don't need to memorize a huge implementation for interviews.

Understand the flow:

```csharp
public async Task ProcessOrder(Order order)
{
    await orderService.Create(order);

    var payment = await paymentService.Process(order);

    if (!payment.Success)
    {
        await orderService.Cancel(order.Id);
        return;
    }

    var inventory = await inventoryService.Reserve(order);

    if (!inventory.Success)
    {
        await paymentService.Refund(payment.Id);
        await orderService.Cancel(order.Id);
        return;
    }

    await shippingService.Create(order);

    await orderService.Complete(order.Id);
}
```

This is **conceptual Saga orchestration**.

Real production implementation would normally involve durable workflow state, messaging, retries, idempotency, and reliable event publishing rather than simply holding one HTTP request open.

---

**Saga State**

For complex workflows, the system needs to know:

```text
SagaId
OrderId
CurrentStep
Status
CompletedSteps
FailureReason
```

For example:

```text
SagaId:       S-1001
OrderId:      O-1001
CurrentStep:  Inventory
Status:       Compensation
Completed:
  ✓ Order
  ✓ Payment
Failed:
  ✗ Inventory
```

This allows the workflow to recover/retry appropriately.

---

**Saga and Eventual Consistency**

Saga usually means the system is **temporarily inconsistent while the workflow is progressing**.

Example:

```text
Time T1:
Order = Created
Payment = Pending
Inventory = Pending

Time T2:
Order = Created
Payment = Paid
Inventory = Pending

Time T3:
Order = Confirmed
Payment = Paid
Inventory = Reserved
```

The system reaches the desired consistent business state after the workflow completes.

This is closely related to **eventual consistency**, which is next after Outbox in your roadmap. Microsoft describes Saga as a way to manage consistency across independently stored data without a distributed transaction.

---

**Saga + Retry**

Suppose Payment Service temporarily fails:

```text
Payment
   ↓
Timeout
```

Should Saga immediately compensate?

Not necessarily.

A transient failure may be retried first:

```text
Payment
   ↓
Failure
   ↓
Retry
   ↓
Success
```

If persistent failure occurs:

```text
Retry
 ↓
Retry
 ↓
Retry
 ↓
Still failed
 ↓
Compensation
```

This is why **Retry, Timeout, Circuit Breaker and Idempotency** are important companions to Saga.

---

**When Should You Use Saga?**

Good use case:

```text
Order
 → Payment
 → Inventory
 → Shipping
```

Multiple services have to participate in one **business workflow**.

Use Saga when:

* multiple independent databases are involved
* distributed transaction is required conceptually
* workflow can tolerate eventual consistency
* compensating actions can be defined

Microsoft identifies maintaining consistency across independently stored microservice data as the primary use case. 

---

**When Saga May Be a Bad Choice**

Don't introduce Saga unnecessarily.

If the operation is:

```text
Single Service
     ↓
Single Database
     ↓
Simple Transaction
```

just use a normal database transaction.

```csharp
using var transaction =
    await db.Database.BeginTransactionAsync();
```

No Saga required.

**Mental rule**

> **One service + one DB → normal transaction.**

> **Multiple services + independent DBs + business workflow → consider Saga.**

---
---

## Outbox Pattern

> How do we guarantee that a database change and the corresponding message/event are not accidentally separated?

---

**The Problem — Dual Write**

Suppose Order Service does:

```text
1. Save Order to DB
2. Publish OrderCreated to Message Broker
```

Code conceptually:

```csharp
await db.SaveChangesAsync();

await messageBroker.PublishAsync(orderCreated);
```

Looks fine, but there is a dangerous gap:

```text
Save DB
   ↓
   ✅
   │
   │ Application crashes here 💥
   │
   ↓
Publish Event
   ↓
   ❌ Never happens
```

Now:

```text
Order DB
   ↓
Order exists ✅

Message Broker
   ↓
OrderCreated missing ❌
```

Other services never know that the order was created.

Microsoft identifies this exact failure mode when the database update succeeds but the application crashes before publishing the integration event.

---

**The Outbox Solution**

Instead of directly publishing the event, store it in an **Outbox table in the same database transaction** as the business data.

```text
Order Service
     │
     │ Transaction
     ↓
┌──────────────────────┐
│ Order DB              │
│                      │
│ Orders               │
│ OutboxMessages       │
└──────────────────────┘
```

Inside one transaction:

```text
Order → INSERT
OutboxMessage → INSERT
       ↓
    COMMIT
```

Both succeed or both fail.

Microsoft describes the Outbox as a transactional table that stores integration events and is committed together with the business data.

---

**Complete Flow**

```text
                   Order Service
                        │
                        ↓
                 Begin Transaction
                        │
             ┌──────────┴──────────┐
             ↓                     ↓
         Orders Table        Outbox Table
             │                     │
             └──────────┬──────────┘
                        ↓
                     COMMIT
                        │
                        ↓
                 Background Worker
                        │
                        ↓
                 Message Broker
                        │
            ┌───────────┼───────────┐
            ↓           ↓           ↓
         Payment    Inventory   Notification
```

The important part:

> **Business data + Outbox message are saved atomically.**

---

**Example Database**

Orders

```text
Orders
------------------
Id
CustomerId
Total
Status
CreatedAt
```

OutboxMessages

```text
OutboxMessages
------------------
Id
EventType
Payload
CreatedAt
ProcessedAt
RetryCount
```

For example:

```text
Id:          101
EventType:   OrderCreated
Payload:     {...}
ProcessedAt: NULL
```

---

**.NET + EF Core Example**

Suppose:

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int RetryCount { get; set; }
}
```

Your `DbContext`:

```csharp
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
}
```

---

**Saving Order + Outbox**

Conceptually:

```csharp
await using var transaction =
    await db.Database.BeginTransactionAsync();

var order = new Order
{
    CustomerId = request.CustomerId,
    Total = request.Total,
    Status = "Created"
};

db.Orders.Add(order);

var message = new OutboxMessage
{
    Id = Guid.NewGuid(),
    Type = "OrderCreated",
    Payload = JsonSerializer.Serialize(
        new
        {
            OrderId = order.Id
        }),
    CreatedAt = DateTime.UtcNow
};

db.OutboxMessages.Add(message);

await db.SaveChangesAsync();

await transaction.CommitAsync();
```

Now:

```text
Order saved        ✅
Outbox message     ✅
```

or:

```text
Order saved        ❌
Outbox message     ❌
```

because they're part of the same local transaction.

---

**Then Who Publishes the Event?**

A **background worker**.

```text
Outbox Table
     │
     │ Poll
     ↓
BackgroundService
     │
     ↓
Message Broker
```

For example:

```csharp
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Read unpublished messages
            // Publish to broker
            // Mark as processed

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }
}
```

In production, you would implement batching, locking/claiming, retries, error handling and idempotency rather than simply polling everything.

---

**What If the Broker Is Down?**

This is where Outbox becomes powerful.

```text
Order DB
   ↓
Order + Outbox
   ↓
COMMIT ✅
   ↓
Worker
   ↓
Broker ❌
```

The message remains:

```text
ProcessedAt = NULL
```

The worker can retry later:

```text
Retry 1 ❌
Retry 2 ❌
Retry 3 ✅
```

So the event isn't lost simply because the broker was temporarily unavailable.

---

**Important: Outbox Doesn't Mean "Exactly Once"**

This is a **very important interview point**.

Suppose:

```text
Worker
   ↓
Publish event
   ↓
Broker accepts it ✅
   ↓
Worker crashes 💥
   ↓
ProcessedAt not updated
```

The worker may publish the same event again.

```text
OrderCreated
OrderCreated
```

Therefore, consumers should be **idempotent**.

```text
MessageId = ABC123

First time:
ABC123 → Process

Second time:
ABC123 → Ignore
```

Microsoft's messaging guidance explicitly highlights idempotence/deduplication as an important concern with event-based communication. ([Microsoft Learn][3])

We'll cover **Idempotency** later.

---

**Outbox + Saga**

Now connect the concepts we've learned:

```text
Order Service
     │
     │ Local Transaction
     ↓
Order DB + Outbox
     │
     ↓
Outbox Worker
     │
     ↓
Message Broker
     │
     ↓
Payment Service
     │
     ↓
Payment DB + Outbox
     │
     ↓
Message Broker
     │
     ↓
Inventory Service
```

So:

`Saga`

Defines the **distributed business workflow**.

`Outbox`

Makes **event publishing reliable relative to the local database transaction**.

`Message Broker`

Moves the messages between services.

These three concepts work together.

---

**Outbox vs Saga**

Very common interview question.

| Saga                                      | Outbox                                             |
| ----------------------------------------- | -------------------------------------------------- |
| Coordinates distributed business workflow | Reliable event publishing                          |
| Handles business compensation             | Solves DB → message dual-write problem             |
| Order → Payment → Inventory               | Order DB + OrderCreated event                      |
| Defines workflow                          | Defines reliable persistence/publication mechanism |
| May use events/commands                   | Usually uses DB + background publisher             |

**Simple example

```text
Saga:
"Payment failed → Refund → Cancel Order"

Outbox:
"Order saved + OrderCreated event saved atomically"
```

---

**Outbox vs Event Sourcing**

Don't confuse these.

`Outbox`

Normal database state:

```text
Orders
   +
OutboxMessages
```

You still store the **current state**.

`Event Sourcing`

Events are the primary source of truth:

```text
OrderCreated
OrderItemAdded
PaymentCompleted
OrderShipped
```

Current state is reconstructed from events.

Microsoft distinguishes the Outbox approach from full Event Sourcing; Outbox persists only integration events needed for communication rather than using events as the complete source of domain state.

---

**Outbox vs Direct Publishing**

❌ Without Outbox

```text
DB
 ↓
Save

Broker
 ↓
Publish
```

There is a failure window between the two.

✅ With Outbox

```text
DB Transaction
      │
      ├── Order
      │
      └── Outbox Event
             ↓
          COMMIT
             ↓
        Background Worker
             ↓
        Message Broker
```

---

1. Polling / Background Worker

Something needs to publish pending events.

Could be:

```text
BackgroundService
```

or another worker/job mechanism.

2. Retry

Failed publishing needs retry.

```text
RetryCount
```

3. Dead-letter / Failed state

Messages that repeatedly fail may need a failed state or operational dead-letter mechanism.

4. Idempotency

Consumers must safely handle duplicates.

5. Ordering

If:

```text
OrderCreated
OrderUpdated
```

both exist, you may need to ensure the consumer receives/processes them in the correct order. Microsoft specifically calls out preserving event ordering as an implementation concern. 

6. Cleanup

Processed outbox records shouldn't necessarily remain forever.

You may archive/delete them based on retention requirements.

---

"What happens if the database transaction succeeds but publishing to RabbitMQ/Azure Service Bus fails?"

> "I would use the Transactional Outbox pattern. The business entity and integration event are stored in an Outbox table within the same local database transaction. After the transaction commits, a background worker publishes pending outbox events to the message broker. If publishing fails, the event remains pending and can be retried. Because delivery can be at least once, consumers should be idempotent."


Why not use a distributed transaction between PostgreSQL and the message broker?"

> "Distributed transactions add significant complexity and reduce the autonomy and availability characteristics we're generally trying to achieve with microservices. The Outbox pattern uses a local ACID transaction for the service's database and then reliably publishes the event asynchronously."

### 🔗 How Our Patterns Fit Together

This is worth memorizing for your interview:

```text
                    CLIENT
                       │
                       ↓
                  API Gateway
                       │
                       ↓
                 Order Service
                       │
              ┌────────┴────────┐
              ↓                 ↓
          Order DB         Outbox Table
              │                 │
              └────────┬────────┘
                       │
                       ↓
                Outbox Worker
                       │
                       ↓
                Message Broker
                       │
              ┌────────┴────────┐
              ↓                 ↓
         Payment Service    Inventory Service
              │                 │
              ↓                 ↓
          Payment DB        Inventory DB
```

And **Saga** determines what happens when these distributed business steps succeed or fail.

---
---

## Eventual Consistency

**Database per Service → Async Messaging → Saga → Outbox → Eventual Consistency**

> Eventual consistency means that distributed services may temporarily have different states, but if no new updates occur and communication succeeds, their data will eventually converge to a consistent state.

In microservices, this is common because each service owns its own database and updates happen through APIs/events rather than one global database transaction.

---

**1. Why Do We Need Eventual Consistency?**

Consider:

```text
Order Service      Payment Service      Inventory Service
     │                    │                    │
  Order DB             Payment DB          Inventory DB
```

There is no single transaction across all three.

Suppose:

```text
Order = Created
Payment = Pending
Inventory = Pending
```

Then events are processed:

```text
OrderCreated
     ↓
Payment Service
     ↓
PaymentCompleted
     ↓
Inventory Service
     ↓
InventoryReserved
```

Eventually:

```text
Order = Confirmed
Payment = Paid
Inventory = Reserved
```

The system wasn't globally consistent at every instant, but it **converged to the desired state**.

---

**Simple Example**

Imagine you order a product.

Immediately after clicking **Place Order**:

```text
Order DB
Status = Created

Payment DB
Status = Pending

Inventory DB
Status = Pending
```

A moment later:

```text
Order DB
Status = Created

Payment DB
Status = Paid

Inventory DB
Status = Pending
```

Later:

```text
Order DB
Status = Confirmed

Payment DB
Status = Paid

Inventory DB
Status = Reserved
```

That temporary difference is **eventual consistency**.

---

**Why Not Strong Consistency Everywhere?**

In a monolith:

```text
Order + Payment + Inventory
          ↓
    Single DB Transaction
          ↓
       COMMIT
```

You can often get strong transactional consistency relatively easily.

In microservices:

```text
Order DB
Payment DB
Inventory DB
```

Using one distributed transaction across all of them creates significant coupling and operational complexity.

Instead:

```text
Local Transaction
       +
Messaging
       +
Saga
       +
Outbox
       ↓
Eventual Consistency
```

---

**Eventual Consistency Does NOT Mean "Inconsistent Forever**

This is an important interview distinction.

> "The services can be temporarily inconsistent while changes propagate, but they are designed to converge to a consistent business state."

Example:

```text
T0 → Order Created
T1 → Payment Completed
T2 → Inventory Reserved
T3 → Order Confirmed
```

The delay between T0 and T3 is the period during which the distributed state is converging.

---

**How Outbox Helps**

Remember our Outbox pattern:

```text
Order Service
     │
     ├── Orders
     │
     └── Outbox
            │
            ↓
       Worker
            │
            ↓
      Message Broker
            │
            ↓
     Payment Service
```

The Outbox makes sure the event isn't lost simply because the application failed after committing the business transaction.

So:

```text
Order Created
     ↓
Order + Outbox committed
     ↓
Event published
     ↓
Payment processes event
```

That propagation is part of how the system eventually reaches the desired state.

---

**Eventual Consistency + Saga**

Saga is a **business workflow**.

Example:

```text
Create Order
     ↓
Payment
     ↓
Inventory
     ↓
Shipping
```

Suppose:

```text
Order      ✅
Payment    ✅
Inventory  ❌
```

Saga may execute:

```text
Refund Payment
     ↓
Cancel Order
```

So the system eventually reaches:

```text
Order     = Cancelled
Payment   = Refunded
Inventory = Not Reserved
```

This is a key relationship:

> Saga manages the distributed workflow; eventual consistency describes how the separate service states converge as that workflow progresses.

---
**Strong vs Eventual Consistency**

| Strong Consistency                                                      | Eventual Consistency                         |
| ----------------------------------------------------------------------- | -------------------------------------------- |
| Reads see the latest committed state according to the consistency model | Reads may temporarily see an older state     |
| Updates coordinated more tightly                                        | Updates propagate asynchronously             |
| Easier transactional reasoning                                          | More distributed-system complexity           |
| Often associated with local ACID transactions                           | Common in distributed/event-driven workflows |
| Higher coordination                                                     | More autonomy                                |

Neither is universally "better"; the choice depends on business requirements.

---

**Common Interview Scenario**


"Payment service updates its database, but Order Service still shows Payment = Pending. Is the system broken?"

Not necessarily.

> "Not necessarily. If the architecture uses asynchronous communication and eventual consistency, Order Service may temporarily have the previous state while the PaymentCompleted event is being processed. I'd verify that the event was published, delivered and processed successfully, and that retries and idempotency are working."

----
----

## Retry / Timeout

When Service A calls Service B, the network can fail, Service B can be overloaded, or the response can simply take too long.

We use:

> Timeout → Retry transient failures → Backoff → eventually fail gracefully

---

**`Timeout`**

> A timeout defines the maximum amount of time we are willing to wait for an operation.

Example:

```text
Order Service
     |
     | HTTP request
     ↓
Payment Service
     |
     | No response...
     |
     | 5 seconds
     ↓
TIMEOUT ❌
```

Instead of keeping the connection waiting indefinitely:

```text
Payment service not responding
        ↓
Timeout
        ↓
Return failure / fallback
```

---

**Why Do We Need Timeouts?**

Imagine:

```text
100 requests
     ↓
Payment Service
     ↓
Every request waits 60 seconds
```

You can quickly consume:

* HTTP connections
* threads/resources
* memory
* connection pools

and cause a **cascading failure**.

A timeout limits how long each dependency call can occupy resources. Microsoft specifically recommends setting timeouts before implementing retries. 

---

**`Retry`**

> Retry means attempting an operation again after a failure that is likely to be temporary.

Example:

```text
Order
  ↓
Payment
  ↓
Timeout ❌
  ↓
Retry
  ↓
Payment
  ↓
Success ✅
```

Retries are intended mainly for **transient faults**, such as temporary network problems, service unavailability, throttling, or temporary overload.

---

**Transient vs Permanent Failure**

This is very important.

`Transient`

Temporary problem:

```text
Payment Service
    ↓
Temporary network issue
    ↓
Retry
    ↓
Success
```

`Permanent`

Invalid request:

```http
POST /payment
```

```http
400 Bad Request
```

Retrying the exact same invalid request won't fix it.

> "I retry failures that are likely to be transient, not permanent business or validation errors."

**Retry Count**

Always use a finite number of retries.

Example:

```text
Attempt 1 → Failed
Attempt 2 → Failed
Attempt 3 → Failed
Attempt 4 → Give up
```

For an interactive API, you might have a small retry count.

For background processing, you may allow longer retry behavior.

The correct values depend on the operation's latency and business requirements. 

---

**Exponential Backoff**

Instead:

```text
Attempt 1 → Fail
     ↓
Wait 1 sec

Attempt 2 → Fail
     ↓
Wait 2 sec

Attempt 3 → Fail
     ↓
Wait 4 sec

Attempt 4 → Fail
     ↓
Wait 8 sec
```

Conceptually:

```text
delay = base × 2^attempt
```

Real implementations usually also impose maximum delays and other limits.

Microsoft recommends exponential backoff for many cloud scenarios.

---

**Jitter**

Suppose 1,000 instances all fail at exactly:

```text
10:00:00
```

Without jitter:

```text
Retry → 10:00:01
Retry → 10:00:03
Retry → 10:00:07
```

All clients retry together.

With jitter:

```text
Client A → 1.2 sec
Client B → 1.7 sec
Client C → 2.1 sec
Client D → 2.8 sec
```

The retry traffic gets distributed over time.

> Jitter = random variation added to retry delays to avoid synchronized retries.

---

**Timeout + Retry Together**

Suppose:

```text
Timeout per attempt = 2 sec
Retries = 3
```

The overall operation can take significantly longer than 2 seconds because you also have retry delays.

```text
Attempt 1
   ↓
2 sec timeout
   ↓
Wait
   ↓
Attempt 2
   ↓
2 sec timeout
   ↓
Wait
   ↓
Attempt 3
```

Therefore:

> **Timeout and retry policies must be designed together.**

The total latency must still fit your API/SLA requirements.

---

**Example: Order → Payment**

Suppose:

```text
Order Service
      ↓
Payment Service
```

Policy:

```text
Attempt timeout = 2 sec
Max retries = 3
Backoff = exponential + jitter
```

Flow:

```text
Attempt 1
   ↓
Timeout
   ↓
Retry
   ↓
Attempt 2
   ↓
Success ✅
```

The user gets a successful response.

---

**What if All Retries Fail?**

Don't keep retrying.

```text
Attempt 1 ❌
Attempt 2 ❌
Attempt 3 ❌
       ↓
Stop
       ↓
Handle failure
```

Depending on the architecture:

```text
Synchronous API
    ↓
Return appropriate error

Async message
    ↓
Retry later / DLQ

Saga
    ↓
Compensation
```

This connects directly to the patterns we've already studied.

---

**.NET Example — Basic Cancellation Timeout**

You can explicitly control a request with `CancellationToken`.

```csharp
using var cts = new CancellationTokenSource(
    TimeSpan.FromSeconds(5));

var response = await httpClient.GetAsync(
    "/api/payment",
    cts.Token);
```

If the operation exceeds the timeout:

```text
5 seconds
   ↓
Cancellation
   ↓
Operation stops/fails
```

For production HTTP resilience, modern .NET provides `Microsoft.Extensions.Http.Resilience`, which supports standardized HTTP resilience pipelines including timeout, retry and circuit breaker strategies. ([Microsoft Learn][4])

---
**Remember This**

```text
             HTTP CALL
                 │
                 ↓
              TIMEOUT
                 │
                 ↓
           Transient failure?
             /        \
           Yes         No
            │           │
            ↓           ↓
          RETRY       FAIL
            │
            ↓
    Exponential Backoff
            +
          Jitter
            │
            ↓
       Still failing?
            │
            ↓
      Circuit Breaker
```
---
---

## Circuit Breaker

> Circuit Breaker prevents an application from repeatedly calling a dependency that is failing. After failures cross a configured threshold, the circuit opens and subsequent calls fail fast. After a recovery period, limited trial calls are allowed to determine whether the dependency has recovered

**Why Do We Need Circuit Breaker?**

Imagine:

```text
Order Service
     |
     ↓
Payment Service ❌
```

Without Circuit Breaker:

```text
Request
  ↓
Payment ❌
  ↓
Retry
  ↓
Payment ❌
  ↓
Retry
  ↓
Payment ❌
  ↓
...
```

Thousands of requests may continue hitting an unhealthy service.

This can cause:

```text
Payment failure
     ↓
More retries
     ↓
More load
     ↓
More timeouts
     ↓
Order Service resources exhausted
     ↓
Cascading failure
```

Circuit Breaker stops this behavior by **failing fast** after enough failures. 

---

**The Three States**

You absolutely should know these:

```text
CLOSED
OPEN
HALF-OPEN
```

---

**`CLOSED`**

Normal operation.

```text
Order
  ↓
Circuit Breaker
  ↓
Payment
  ↓
Success
```

The breaker monitors failures.

```text
Failure count:
0 → 1 → 2 → 3
```

Suppose threshold = 3.

```text
3 failures
     ↓
Circuit opens
```

---

**`OPEN`**

Now the dependency is considered unhealthy.

```text
Order
  ↓
Circuit Breaker
  ↓
OPEN
  ↓
❌ Don't call Payment
```

Requests **fail immediately** instead of waiting for Payment to timeout.

```text
Request
  ↓
Circuit OPEN
  ↓
Fail Fast
```

This protects both the caller and the failing dependency.

---

**`HALF-OPEN`**

After a configured break duration:

```text
OPEN
  ↓
Wait
  ↓
HALF-OPEN
```

The circuit allows a limited number of trial requests.

```text
         Payment
            ↑
            |
Order → Circuit
          HALF-OPEN
```

**If trial succeeds:**

```text
HALF-OPEN
     ↓
Success
     ↓
CLOSED
```

Normal traffic resumes.

**If trial fails:**

```text
HALF-OPEN
     ↓
Failure
     ↓
OPEN
```

The waiting period starts again.

---

**Complete Flow**

Memorize this:

```text
                 ┌─────────────┐
                 │   CLOSED    │
                 └──────┬──────┘
                        │
                 Too many failures
                        ↓
                 ┌─────────────┐
                 │    OPEN     │
                 └──────┬──────┘
                        │
                  Wait duration
                        ↓
                 ┌─────────────┐
                 │  HALF-OPEN  │
                 └──────┬──────┘
                    /          \
               Success          Failure
                 ↓                ↓
              CLOSED            OPEN
```

---

**Circuit Breaker vs Retry**

This is a **must-know distinction**.

**`Retry`**

```text
Failure
  ↓
Try again
```

**`Circuit Breaker`**

```text
Repeated failures
       ↓
STOP calling
       ↓
Wait
       ↓
Test recovery
```

So:

> Retry says "try again."

> Circuit Breaker says "stop trying for now."

---

**Retry + Circuit Breaker Together**

Real systems often use both.

```text
Order Service
      │
      ↓
Circuit Breaker
      │
      ↓
Retry Policy
      │
      ↓
Payment Service
```

Example:

```text
Attempt 1 → Timeout
Attempt 2 → 500
Attempt 3 → 500
       ↓
Circuit opens
       ↓
Future calls fail immediately
```

After recovery period:

```text
HALF-OPEN
    ↓
Trial request
    ↓
Success
    ↓
CLOSED
```

---

**.NET Implementation**

Modern .NET has built-in resilience infrastructure through:

```text
Microsoft.Extensions.Resilience
Microsoft.Extensions.Http.Resilience
```

These packages build on Polly. Microsoft recommends these modern packages; the older `Microsoft.Extensions.Http.Polly` package is deprecated. ([Microsoft Learn][3])

For an `HttpClient`:

```csharp
builder.Services
    .AddHttpClient<PaymentClient>()
    .AddStandardResilienceHandler();
```

The standard HTTP resilience pipeline includes:

```text
Total Request Timeout
        ↓
Retry
        ↓
Circuit Breaker
        ↓
Attempt Timeout
```

along with concurrency/rate limiting in the standard pipeline. ([Microsoft Learn][4])

---

**Custom Circuit Breaker Configuration**

You can customize the resilience pipeline rather than relying only on defaults.

Conceptually:

```csharp
builder.Services
    .AddHttpClient<PaymentClient>()
    .AddStandardResilienceHandler(options =>
    {
        // Configure retry
        // Configure circuit breaker
        // Configure timeouts
    });
```

The important interview point is not memorizing every option name.

Know:

```text
Failure threshold
Break duration
Sampling/window behavior
Recovery trial
```

---
**Real Architecture**

Now combine everything we've learned:

```text
                       Angular
                          │
                          ↓
                     API Gateway
                          │
                          ↓
                    Order Service
                          │
                    HttpClient
                          │
                          ↓
                 ┌─────────────────┐
                 │ Circuit Breaker │
                 └────────┬────────┘
                          │
                        Retry
                          │
                       Timeout
                          │
                          ↓
                  Payment Service
```

If Payment is healthy:

```text
Order
 ↓
Circuit CLOSED
 ↓
Payment
 ↓
Success
```

If Payment fails repeatedly:

```text
Order
 ↓
Circuit OPEN
 ↓
Fail Fast
```
---

**The 3 Words to Memorize**

```text
CLOSED
"Everything is normal."

OPEN
"Stop calling."

HALF-OPEN
"Let's test recovery."
```

---
---

## Idempotency 

Idempotency means performing the same operation multiple times produces the same effective business result as performing it once.

Suppose:

```text
POST /payments
Amount = ₹10,000
```

If the client times out and retries:

```text
Request 1 → Charge ₹10,000
Request 2 → Charge ₹10,000 again ❌
```

Customer gets charged **₹20,000**.

An idempotent payment operation ensures:

```text
Request 1 → Charge ₹10,000
Request 2 → Same request → Return existing result
```

Result: **₹10,000 charged only once.**

---

**Why is Idempotency important in Microservices?**

Because microservices commonly have:

```text
Retry
   ↓
Message redelivery
   ↓
Network failures
   ↓
At-least-once delivery
   ↓
Duplicate requests/messages
```

Microsoft specifically recommends idempotent message processing because messages can be delivered more than once. 

This is especially important for:

* Payments
* Order creation
* Inventory updates
* Message consumers
* External API calls

---

**HTTP Idempotency**

Common HTTP methods:

| Method | Idempotent? |
| ------ | ----------- |
| GET    | ✅           |
| PUT    | ✅           |
| DELETE | ✅           |
| POST   | ❌ Generally |
| PATCH  | ❌ Generally |

HTTP semantics define GET, PUT and DELETE as idempotent; POST is not inherently idempotent.

**Example**

```http
PUT /users/100
{
    "name": "Swapnil"
}
```

Calling it 1 time or 10 times results in:

```text
User 100 → Swapnil
```

So it is naturally idempotent.

But:

```http
POST /orders
```

can create a new order every time:

```text
Request 1 → Order #101
Request 2 → Order #102
```

So POST needs additional protection when retries are possible.

---

**Idempotency Key**

A common solution is an **Idempotency-Key**.

`Client`

```http
POST /payments
Idempotency-Key: PAY-123456
```

Server stores:

```text
IdempotencyKey | Status    | Response
-----------------------------------------
PAY-123456     | Completed | Payment #5001
```

If the same request arrives again:

```http
Idempotency-Key: PAY-123456
```

The API checks the database:

```text
Already processed?
       ↓
     YES
       ↓
Return previous result
```

No second payment is created.

---

**.NET Example**

A simplified model:

```csharp
public class IdempotencyRequest
{
    public string Key { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Response { get; set; } = null!;
}
```

Controller:

```csharp
[HttpPost("payments")]
public async Task<IActionResult> CreatePayment(
    [FromHeader(Name = "Idempotency-Key")] string key,
    CreatePaymentRequest request)
{
    var existing = await db.IdempotencyRequests
        .FirstOrDefaultAsync(x => x.Key == key);

    if (existing != null)
    {
        return Ok(existing.Response);
    }

    // Perform payment
    var payment = await paymentService.ProcessAsync(request);

    db.IdempotencyRequests.Add(new IdempotencyRequest
    {
        Key = key,
        Status = "Completed",
        Response = payment.Id.ToString()
    });

    await db.SaveChangesAsync();

    return Ok(payment);
}
```

---

**Idempotency vs Deduplication**

`Deduplication`

> "Have I already received/processed this message?"

Example:

```text
MessageId = ABC123

ABC123 exists?
→ Yes → Don't process
```

`Idempotency`

> "If I process this operation multiple times, will the business result remain safe?"

Example:

```text
Set balance = ₹10,000
```

Repeated execution:

```text
₹10,000 → ₹10,000 → ₹10,000
```

Safe.

But:

```text
balance += ₹1,000
```

Repeated execution:

```text
₹10,000
₹11,000
₹12,000 ❌
```

Not idempotent.

**Best practice:** combine duplicate detection with idempotent business operations where possible. 

---
---

## CQRS

**CQRS = Command Query Responsibility Segregation**

> **Separate operations that change data from operations that read data.**

> **Command = Do something**

> **Query = Give me something**


**Command vs Query**

| Operation   | Purpose              | Changes data? |
| ----------- | -------------------- | ------------- |
| **Command** | Create/Update/Delete | ✅ Yes         |
| **Query**   | Retrieve data        | ❌ No          |

Example:

```text
CreateOrderCommand
UpdateOrderCommand
CancelOrderCommand
        ↓
     WRITE
```

and

```text
GetOrderQuery
GetCustomerOrdersQuery
GetOrderSummaryQuery
        ↓
      READ
```

---

**Traditional CRUD vs CQRS**

`Traditional CRUD`

```text
                ┌───────────────┐
Request ───────►│ Order Service │
                │               │
                │   Order Model │
                │       ↓       │
                │   PostgreSQL  │
                └───────────────┘
                  ↑         ↑
                READ       WRITE
```

Same model handles both reads and writes.

This is perfectly fine for many applications.

---

`CQRS`

```text
                    Order Service
                         │
              ┌──────────┴──────────┐
              │                     │
           COMMAND                QUERY
              │                     │
              ↓                     ↓
       Write Model             Read Model
              │                     │
              ↓                     ↓
        Write Database        Read Database
```

Read and write responsibilities are separated.

---

**Important: CQRS does NOT require two databases**

This is a common interview trap.

You can have:

* Level 1 — Simple CQRS

```text
Command → Write Model ──┐
                        │
                     PostgreSQL
                        │
Query → Read Model ─────┘
```

**Same database, different models/handlers.**

---

* Level 2 — Separate databases

```text
Command
   ↓
Write DB
   │
   │ Event
   ↓
Read DB
   ↓
Query
```

Now reads and writes can be scaled independently.

This is useful when the read workload and write workload have very different requirements.

---

**.NET CQRS Example**

Imagine an Order Service.

`Command`

```csharp
public record CreateOrderCommand(
    int CustomerId,
    decimal Amount);
```

Handler:

```csharp
public class CreateOrderHandler
{
    public async Task<int> Handle(CreateOrderCommand command)
    {
        var order = new Order
        {
            CustomerId = command.CustomerId,
            Amount = command.Amount,
            Status = "Created"
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order.Id;
    }
}
```

The command is responsible for **changing state**.

---

`Query`

```csharp
public record GetOrderQuery(int OrderId);
```

Handler:

```csharp
public class GetOrderHandler
{
    public async Task<OrderDto?> Handle(GetOrderQuery query)
    {
        return await db.Orders
            .Where(x => x.Id == query.OrderId)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Status = x.Status
            })
            .FirstOrDefaultAsync();
    }
}
```

The query only **reads data**.

---

**Why use CQRS?**

**Problem**

Suppose your application has:

```text
10,000 writes/day
10,000,000 reads/day
```

The read workload is much larger.

With traditional architecture:

```text
READ + WRITE
     ↓
Same model
     ↓
Same DB
```

CQRS can allow:

```text
WRITE
  ↓
Write DB

READ
  ↓
Read DB
  ↓
Multiple read replicas
```

Benefits can include:

* Independent optimization
* Independent scaling
* Simpler read models
* Read-specific DTOs
* Reduced contention
* Better separation of business/write logic from query logic

These are among the reasons Microsoft identifies for CQRS.

---

**When NOT to use CQRS**

Don't use CQRS just because you're building microservices.

For a simple application:

```text
CRUD
 ↓
EF Core
 ↓
PostgreSQL
```

may be enough.

CQRS adds complexity:

* More code
* Separate handlers/models
* Potential synchronization issues
* Possible eventual consistency
* More infrastructure if read/write stores are separated

Microsoft also recommends considering the trade-offs before adopting CQRS.

---

**CQRS + MediatR**

You will often see this in .NET projects:

```text
Controller
    ↓
MediatR
    ↓
Command / Query
    ↓
Handler
    ↓
Database
```

Example:

```csharp
public record GetOrderQuery(int Id)
    : IRequest<OrderDto>;
```

Handler:

```csharp
public class GetOrderHandler
    : IRequestHandler<GetOrderQuery, OrderDto>
{
    public async Task<OrderDto> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        // query database
    }
}
```

MediatR is commonly used to implement command/query pipelines in .NET applications.

**But remember:** MediatR is an implementation tool; **CQRS is the architectural pattern**.

---
---

## Distributed Tracing  

> Distributed tracing tracks a **single request across multiple services** and shows where time was spent and where failures occurred.

Example:

```text
Angular
   ↓
API Gateway
   ↓
Order Service
   ↓
Payment Service
   ↓
Inventory Service
   ↓
Database
```

Without tracing, you may see:

```text
Order API → 500
```

But you don't know whether the problem was:

```text
Order Service       20ms
Payment Service    800ms  ← problem
Inventory Service   30ms
Database            10ms
```

Distributed tracing gives you this end-to-end picture.

---

**Trace vs Span**

These two terms are critical.

`Trace`

Represents the **complete journey of one request**.

```text
Trace
 ├── API Gateway
 ├── Order Service
 ├── Payment Service
 └── Inventory Service
```

`Span`

Represents **one unit of work** inside that trace.

```text
TraceId = ABC123

Span 1 → Gateway
Span 2 → Order Service
Span 3 → Payment Service
Span 4 → Inventory Service
```

A trace consists of one or more spans arranged in a parent/child relationship. 

**Easy mental model**

> **Trace = entire journey**

> **Span = one step in the journey**

---

**TraceId and SpanId**

Example:

```text
TraceId:  ABC123
```

All services participating in the same request share the same **TraceId**.

But each operation has its own:

```text
SpanId
```

Example:

```text
Trace ABC123

Gateway
 TraceId = ABC123
 SpanId  = 111

   ↓

Order Service
 TraceId = ABC123
 SpanId  = 222

   ↓

Payment Service
 TraceId = ABC123
 SpanId  = 333
```

This allows the entire request to be correlated across services. .NET uses W3C Trace Context by default for distributed tracing. 

---

**How does TraceId travel between services?**

This is called **Context Propagation**.

```text
Service A
   │
   │ Trace Context
   ↓
Service B
   │
   │ Trace Context
   ↓
Service C
```

For HTTP, the trace context is propagated using standard HTTP headers.

Conceptually:

```http
traceparent: 00-TRACE_ID-SPAN_ID-01
```

Service B receives the context and creates its own child span.

OpenTelemetry calls this **context propagation**, allowing traces to maintain their causal relationship across service/process boundaries. 

In .NET, ASP.NET Core and `HttpClient` have built-in support for propagating Activity/trace information over HTTP. 

---

**.NET: Activity**

This is particularly important for your interview.

In .NET:

```text
OpenTelemetry Span
        ↕
System.Diagnostics.Activity
```

A .NET `Activity` represents a unit of work and corresponds to an OpenTelemetry span. ([Microsoft Learn][4])

Example:

```csharp
using System.Diagnostics;

using var activity = ActivitySource.StartActivity("ProcessPayment");

activity?.SetTag("payment.amount", 10000);
```

You can record:

* Duration
* Status
* Tags/attributes
* Events
* Parent/child relationships

---

**OpenTelemetry**

**OpenTelemetry (OTel)** is a vendor-neutral standard/tooling ecosystem for generating, collecting and exporting telemetry.

In .NET, it works with framework APIs such as:

```text
ILogger
   → Logs

Meter
   → Metrics

ActivitySource / Activity
   → Traces
```

OpenTelemetry can then export telemetry to monitoring/APM systems. 

Common architecture:

```text
.NET Services
     │
     ├── Logs
     ├── Metrics
     └── Traces
           ↓
     OpenTelemetry
           ↓
    Collector / Exporter
           ↓
 Monitoring System
```

Examples include:

* Azure Monitor / Application Insights
* Jaeger
* Grafana ecosystem
* Other APM platforms

---

**Practical .NET Example**

Suppose:

```text
GET /orders/100
```

Order Service calls Payment Service.

You might see:

```text
Trace: ABC123

GET /orders/100                 500ms
│
├── Order Service               500ms
│   │
│   ├── PostgreSQL              50ms
│   │
│   └── Payment Service         430ms
│       │
│       └── PostgreSQL          400ms
```

Now you immediately know:

> Payment Service's database is taking most of the time.

That's the real value of distributed tracing.

---

**Logging vs Metrics vs Tracing**

Very common interview question.

| Telemetry   | Answers                   |
| ----------- | ------------------------- |
| **Logs**    | What happened?            |
| **Metrics** | How much/how often?       |
| **Traces**  | Where did the request go? |

**Example**

**Log**

```text
Payment failed for Order 100
```

**Metric**

```text
Payment failures = 523/min
```

**Trace**

```text
Gateway
 ↓
Order Service
 ↓
Payment Service ← 3 sec
 ↓
Payment DB     ← 2.8 sec
```

Together they provide **observability**.

---

**Trace vs Correlation ID**

Don't confuse these.

`Correlation ID`

Usually a custom application identifier used to correlate logs for a request.

```text
CorrelationId = ORDER-123
```

`Trace ID`

Part of the distributed tracing context and represents the distributed trace.

```text
TraceId = ABC123
```

Modern distributed tracing normally uses trace context rather than relying only on a custom correlation ID.

---

**Async Messaging and Tracing**

This is especially important because we've already covered **Message Broker + Saga + Outbox**.

Example:

```text
Order Service
     │
     │ OrderCreated
     ↓
Message Broker
     │
     ↓
Payment Service
     │
     ↓
Inventory Service
```

Distributed tracing can propagate context through messaging so the related work can be correlated.

Conceptually:

```text
Trace ABC123
│
├── Create Order
│
├── Publish OrderCreated
│
├── Process Payment
│
└── Reserve Inventory
```

This is extremely useful for debugging asynchronous workflows.

---
---

## Basic Microservices Questions

1. What are Microservices?

> Microservices is an architectural style where an application is divided into small, independently deployable services, with each service responsible for a specific business capability.

Each service generally:

* Has a clearly defined responsibility
* Owns its data
* Can be independently deployed
* Can be independently scaled
* Communicates with other services through APIs or messaging

**Example**

```text
E-Commerce
│
├── Order Service
├── Payment Service
├── Inventory Service
├── Customer Service
└── Notification Service
```

**Senior point**

> I would design services around **business capabilities**, not technical layers such as UI Service, Database Service, etc.

---

2. How do Microservices differ from Monolithic Architecture?

| Monolith                     | Microservices                              |
| ---------------------------- | ------------------------------------------ |
| One deployable application   | Multiple independently deployable services |
| Usually shared DB            | Each service owns its data                 |
| Scale entire application     | Scale individual services                  |
| Simple communication         | Network communication                      |
| Easier initially             | More operational complexity                |
| Failure can affect whole app | Failure can potentially be isolated        |
| Usually one release unit     | Independent releases                       |

**Example**

```text
Monolith

Angular
   ↓
.NET Application
   ↓
Single DB
```

vs.

```text
Microservices

Angular
   ↓
API Gateway
   ↓
 ┌───────┬─────────┬──────────┐
Order   Payment   Inventory
 │         │          │
DB        DB         DB
```

**Interview answer**

> "I wouldn't choose microservices just because they're modern. For a small application, a modular monolith may be simpler. Microservices become valuable when we need independent deployment, scaling, team ownership, or strong business boundaries."

---

3. What are the key features of Microservices?

Important features:

1. **Independent deployment**
2. **Business capability-based services**
3. **Loose coupling**
4. **High cohesion**
5. **Independent scaling**
6. **Service-owned data**
7. **API/message-based communication**
8. **Fault isolation**
9. **Technology flexibility**
10. **Automation and CI/CD**

**One-liner**

> "The key characteristics are independently deployable services, business-focused boundaries, loose coupling, data ownership, independent scaling, and resilient communication."

---

4. Name the main components of Microservices Architecture

Typical components:

```text
                    Client
                      ↓
                API Gateway
                      ↓
        ┌─────────────┼─────────────┐
        ↓             ↓             ↓
     Order         Payment       Inventory
     Service        Service        Service
        ↓             ↓             ↓
     Order DB      Payment DB    Inventory DB
        │
        └──────────┐
                   ↓
              Message Broker
                   ↓
            Other Services

Supporting infrastructure:
- Service Discovery
- Authentication/Authorization
- Configuration
- Logging
- Metrics
- Distributed Tracing
- Containerization
- Monitoring
```

---

5. Explain the role of an API Gateway

> API Gateway provides a **single client-facing entry point** to backend services.

```text
Angular
   ↓
API Gateway
   ↓
 ┌────────┬──────────┬──────────┐
Order   Payment   Inventory
```

It can handle:

* Routing
* Authentication
* Rate limiting
* SSL/TLS termination
* Request aggregation
* Logging
* Caching

**Important**

Don't put business logic into the gateway.

**.NET examples**

* YARP
* Ocelot
* Azure API Management

**Interview answer**

> "API Gateway hides internal service topology and provides a common entry point for cross-cutting API concerns such as routing, authentication, rate limiting and aggregation."

---

6. What is Service Discovery?

In microservices, service instances can change dynamically.

For example:

```text
Payment Service

Instance 1 → 10.0.0.10
Instance 2 → 10.0.0.11
Instance 3 → 10.0.0.12
```

Instead of hardcoding:

```text
http://10.0.0.10:5001
```

the caller discovers:

```text
payment-service
```

and the infrastructure finds a healthy instance.

**Service discovery types**

**Client-side:**

```text
Order → Service Registry → Payment instance
```

**Server-side:**

```text
Order → Load Balancer → Service Registry/Platform → Payment
```

**Kubernetes**

Kubernetes provides built-in service discovery using Services/DNS.

**Interview one-liner**

> "Service discovery allows services to dynamically locate available instances instead of relying on hardcoded network addresses."

---

7. What is a Container and why is it used in Microservices?

A container packages:

```text
Application
+
Dependencies
+
Runtime
```

into an isolated executable environment.

Example:

```text
Order Service
   ↓
Docker Image
   ↓
Container
```

Why useful:

* Consistent environment
* Easy deployment
* Isolation
* Fast startup
* Easy scaling
* Works well with Kubernetes

**Image vs Container**

> **Image = packaged template**

> **Container = running instance**

---

8. How do Microservices communicate with each other?

Two major approaches:

`Synchronous`

```text
Order → HTTP → Payment
             ← Response
```

Technologies:

* REST
* HTTP
* gRPC

Use when immediate response is required.

`Asynchronous`

```text
Order
  ↓
Message Broker
  ↓
Payment
```

Technologies:

* Azure Service Bus
* RabbitMQ
* Kafka

Use when services can process independently.

**Interview answer**

> "I use synchronous HTTP/gRPC when the caller needs an immediate response, and asynchronous messaging when I want loose coupling, resilience, and eventual consistency."

---

9. How would you handle a Microservices failure?

Use multiple resilience patterns:

```text
Failure
  ↓
Timeout
  ↓
Retry transient failures
  ↓
Circuit Breaker
  ↓
Fallback if appropriate
  ↓
Dead Letter Queue for failed messages
  ↓
Monitoring + Alerting
```

Also:

* Idempotency
* Health checks
* Bulkheads
* Graceful degradation
* Distributed tracing

**Example**

Payment Service is down:

```text
Order
 ↓
Payment call
 ↓
Timeout
 ↓
Limited Retry
 ↓
Circuit Breaker opens
 ↓
Order = PaymentPending
```

Don't keep calling Payment endlessly.

**Strong answer**

> "I would first apply a bounded timeout, retry only transient failures with exponential backoff, and use a circuit breaker to prevent cascading failures. For asynchronous processing I'd use retries and DLQ, while making consumers idempotent."

---

10. How can you ensure backward compatibility in Microservices?

This is about allowing old and new versions to coexist.

**Techniques**

`1. API versioning`

```text
/api/v1/orders
/api/v2/orders
```

`2. Backward-compatible contract changes`

Prefer:

```text
Add new field
```

instead of:

```text
Rename/remove existing field
```

`3. Consumer-driven contract testing`

Make sure consumers continue to work with provider changes.

`4. Event versioning`

```text
OrderCreatedV1
OrderCreatedV2
```

or evolve the event contract compatibly.

`5. Database migration strategy`

Don't deploy application changes that immediately depend on a schema change unavailable to older versions.

**Strong senior concept**

> **Expand → Migrate → Contract**

Example:

```text
Old DB
  ↓
Add new column
  ↓
Deploy new code
  ↓
Migrate data
  ↓
Remove old column later
```

---

11. What is OAuth and why is it used?

> OAuth 2.0 is an **authorization framework** that allows an application to access resources on behalf of a user or client without sharing the user's credentials with that application.

Typical architecture:

```text
Client
  ↓
Identity Provider
  ↓
Access Token
  ↓
API Gateway/API
```

The API validates the access token.

**Important distinction**

> **OAuth = Authorization**

For authentication/identity, OpenID Connect is commonly used on top of OAuth 2.0.

**Example**

```text
Angular
   ↓
Entra ID / Identity Provider
   ↓
Access Token
   ↓
.NET API
```

---

12. What is CQRS in Microservices? What problem does it solve?

`CQRS`:

> **Command Query Responsibility Segregation**

Separates:

```text
Commands → Change data
Queries  → Read data
```

Example:

```text
CreateOrderCommand
UpdateOrderCommand
CancelOrderCommand
```

vs.

```text
GetOrderQuery
GetCustomerOrdersQuery
```

**Problem it helps solve**

If reads and writes have different requirements:

```text
10K writes
10M reads
```

you can optimize and scale them differently.

**Important interview point**

CQRS **doesn't require separate databases**.

```text
Command → Write Model ─┐
                      ├→ Same DB
Query   → Read Model ─┘
```

Or:

```text
Command → Write DB
              ↓
            Events
              ↓
           Read DB
```

---

13. API Gateway vs Load Balancer

| API Gateway               | Load Balancer                 |
| ------------------------- | ----------------------------- |
| API-aware                 | Traffic distribution          |
| Routing based on API/path | Routes to healthy instances   |
| Authentication            | Usually not business/API auth |
| Rate limiting             | Load distribution             |
| Aggregation               | Generally no aggregation      |
| Transformation            | Generally limited/basic       |
| API policies              | Health checks                 |

**Example**

```text
Client
  ↓
API Gateway
  ↓
Payment Service
  ↓
Load Balancer
  ↓
Payment Instance 1
Payment Instance 2
Payment Instance 3
```

**Interview answer**

> "A load balancer primarily distributes traffic across healthy instances, whereas an API Gateway operates at the API boundary and can provide routing, authentication, rate limiting, aggregation and other API policies."

---

14. Common Microservices Design Principles

Remember:

`1. Single Responsibility`

Service should represent a cohesive business capability.

`2. High Cohesion`

Related functionality stays together.

`3. Loose Coupling`

Services shouldn't depend heavily on each other.

`4. Database per Service`

Each service owns its data.

`5. Independent Deployment`

Service can be deployed independently.

`6. Resilience`

Use:

```text
Timeout
Retry
Circuit Breaker
```

`7. Observability`

Use:

```text
Logs
Metrics
Traces
```

`8. Automation`

CI/CD + infrastructure automation.

`9. API Contract Stability`

Don't break consumers unnecessarily.

`10. Design Around Business Capability`

Not:

```text
UserController Service
Database Service
Validation Service
```

Instead:

```text
Order
Payment
Inventory
```

---

15. How do you ensure security in Microservices?

Think in layers:

```text
Client
 ↓
HTTPS
 ↓
API Gateway
 ↓
Authentication
 ↓
Authorization
 ↓
Microservice
 ↓
Database
```

**Key practices**

* HTTPS/TLS
* OAuth 2.0 / OpenID Connect
* JWT/access tokens
* Authentication
* Authorization
* Role/claim/policy-based access
* API Gateway policies
* Rate limiting
* Input validation
* Secrets management
* Encryption
* Network segmentation
* Least privilege
* Audit logging

**Important**

Don't rely only on the gateway.

A service should also enforce authorization for its own business resources.

---

16. Ways to achieve synchronous and asynchronous communication

`Synchronous`

```text
HTTP REST
gRPC
```

Example:

```csharp
await httpClient.GetAsync("/payments/100");
```

`Asynchronous`

```text
Azure Service Bus
RabbitMQ
Kafka
```

Example:

```text
OrderCreated
      ↓
Azure Service Bus
      ↓
Payment Service
```

**Interview answer**

> "REST or gRPC are common synchronous mechanisms, while brokers such as Azure Service Bus, RabbitMQ or Kafka support asynchronous event/command-based communication."

---

17. How do you implement Rate Limiting?

Rate limiting controls how many requests a client can make during a period.

Example:

```text
Client
 ↓
100 requests/minute
 ↓
Allowed

101st request
 ↓
429 Too Many Requests
```

**Algorithms**

Common approaches:

* Fixed window
* Sliding window
* Token bucket
* Concurrency limiting

**Where?**

Can be implemented at:

```text
API Gateway
```

or:

```text
Application/API
```

For example, ASP.NET Core has rate-limiting middleware.

**Interview answer**

> "I would generally enforce rate limiting at the API Gateway for global protection, and use application-level limits when business-specific rules are required."

---

18. What is Event-Driven Architecture?

> In Event-Driven Architecture, services communicate by publishing and consuming **events representing something that happened**.

Example:

```text
Order Service
     ↓
OrderCreated
     ↓
Message Broker
     ↓
 ┌───────────────┬───────────────┐
 ↓               ↓               ↓
Payment       Inventory      Notification
```

The producer doesn't need direct knowledge of every consumer.

`Event`

```text
OrderCreated
```

means:

> "The order was created."

`Command`

```text
ProcessPayment
```

means:

> "Please perform this operation."

**Benefits**

* Loose coupling
* Scalability
* Async processing
* Easy addition of consumers

**Trade-off**

Usually introduces:

* Eventual consistency
* More complex debugging
* Message ordering considerations
* Duplicate messages

---

19. What is Idempotency and why is it important?

> **Idempotency means repeated execution has the same effective business result as one execution.**

Payment example:

```text
Request
Idempotency-Key = ABC123
```

First:

```text
ABC123 → Charge ₹10,000
```

Retry:

```text
ABC123 → Return previous result
```

not:

```text
ABC123 → Charge ₹10,000 again ❌
```

**Why?**

Because microservices commonly have:

```text
Retries
+
At-least-once delivery
+
Network failures
```

which can create duplicate operations.

**Interview answer**

> "I use idempotency keys for APIs such as payments and unique message IDs for message consumers, backed by a database uniqueness constraint so duplicate requests don't create duplicate business effects."

---

20. What is Database Sharding and why is it used?

> **Sharding splits data across multiple database partitions/nodes based on a shard key.**

Example:

```text
Customers

Shard 1 → CustomerId 1–1M
Shard 2 → CustomerId 1M–2M
Shard 3 → CustomerId 2M–3M
```

Instead of:

```text
One huge database
```

you have:

```text
             Application
                  ↓
             Shard Router
          /       |       \
         ↓        ↓        ↓
      DB-1      DB-2      DB-3
```

**Why?**

* Very large datasets
* Higher throughput
* Horizontal scaling
* Distribute database load

**Important**

**Database per service ≠ database sharding.**

Database per service:

> Separates ownership between services.

Sharding:

> Splits one logical dataset across multiple database partitions.

---

21. How do you handle Data Consistency in Microservices?

Because services own separate databases:

```text
Order DB
Payment DB
Inventory DB
```

you generally can't rely on one normal ACID transaction across all services.

Instead use:

`Saga`

```text
Order
 ↓
Payment
 ↓
Inventory
```

with compensating actions when needed.

`Eventual consistency`

Data may temporarily differ while events propagate.

`Transactional Outbox`

```text
Business DB transaction
        ↓
Business Data + Outbox Event
        ↓
Publisher
        ↓
Message Broker
```

`Idempotency`

Prevents duplicate processing.

**Strong interview answer**

> "I avoid distributed database transactions where possible. I use local transactions within each service, Transactional Outbox for reliable event publication, Saga for multi-service workflows, and idempotent consumers with eventual consistency."

---

22. What is Canary Deployment?

> Canary deployment releases a new version to a **small percentage of traffic/users first**.

Example:

```text
Version 1 → 95%
Version 2 → 5%
```

Monitor:

```text
Errors
Latency
CPU
Business metrics
```

If healthy:

```text
5%
 ↓
25%
 ↓
50%
 ↓
100%
```

If problems occur:

```text
Version 2
   ↓
Stop/Rollback
```

**Why?**

Reduce blast radius.

**Compare**

```text
Canary
→ gradual traffic

Blue-Green
→ switch traffic between two environments
```

---

23. How would you monitor Microservices?

Use the three pillars:

```text
             Observability
             /     |      \
          Logs   Metrics   Traces
```

`Logs`

```text
Payment failed
OrderId = 100
```

`Metrics`

```text
Request rate
Error rate
Latency
CPU
Memory
Queue depth
```

`Traces`

```text
Gateway
 ↓
Order
 ↓
Payment ← 2.5 sec
 ↓
DB
```

`Also monitor`

* Health checks
* Dependency failures
* Database performance
* Message processing
* DLQ size
* Retry count
* Circuit breaker state

> "I'd use centralized structured logging, metrics and distributed tracing with correlation/trace context, then create dashboards and alerts around latency, error rate, throughput, dependency health and business-critical metrics."

---

24. What are the DDD Principles?

For microservices, focus on **Domain-Driven Design** concepts.

`Bounded Context`

Defines the boundary within which a domain model and terminology have a specific meaning.

Example:

```text
Customer

CRM Context
→ Lead information

Order Context
→ Shipping/customer information

Support Context
→ Support-related information
```

`Entities`

Objects identified by identity.

```text
OrderId = 100
```

`Value Objects`

Defined by their values rather than identity.

```text
Address
Money
EmailAddress
```

`Aggregate`

Cluster of related objects treated as a consistency boundary.

```text
Order
 ├── OrderItems
 └── ShippingAddress
```

`Aggregate Root`

Entry point to modify the aggregate.

```text
Order
 ↓
OrderItem
```

Application should generally modify `OrderItem` through `Order`.

`Domain Service`

Business logic that doesn't naturally belong to a single entity/value object.

`Repository`

Provides abstraction for persistence of domain aggregates.

---

25. How do you design Microservices for scalability?

Think horizontally.

Instead of:

```text
One huge server
```

use:

```text
             Load Balancer
             /    |    \
            ↓     ↓     ↓
         API-1 API-2 API-3
```

**Principles**

`1. Stateless services`

Don't store user/session state in local memory.

`2. Horizontal scaling`

Add instances.

`3. Caching`

Redis, distributed cache.

`4. Async processing`

Move long-running work to queues.

`5. Database optimization`

Indexes, query optimization, replicas/sharding when appropriate.

`6. Independent scaling`

If Payment gets heavy traffic:

```text
Order → 3 instances
Payment → 10 instances
Inventory → 4 instances
```

> "I design stateless services, scale horizontally, use caching and asynchronous processing where appropriate, optimize database access, and allow individual services to scale independently based on their workload."

---

26. What is Chaos Engineering?

> Chaos Engineering deliberately introduces controlled failures to test system resilience.

Example:

```text
Payment Service
     ↓
INTENTIONALLY STOP
     ↓
Observe:
- Does circuit breaker work?
- Does retry work?
- Does Order remain stable?
- Are alerts triggered?
```

Other experiments:

* Kill container
* Add network latency
* Drop requests
* Make dependency unavailable
* Exhaust resources

**Goal**

Not:

> "Break production randomly."

Instead:

> **"Validate resilience assumptions through controlled experiments."**

---

27. How do you perform logging in a distributed Microservices system?

Don't rely on logs inside individual servers.

Use:

```text
Service 1 ─┐
Service 2 ─┤
Service 3 ─┼→ Centralized Logging
Service 4 ─┘
```

Use **structured logging**:

```json
{
  "level": "Error",
  "message": "Payment failed",
  "orderId": "1001",
  "traceId": "abc123",
  "service": "PaymentService"
}
```

**Important**

Include:

* Timestamp
* Service name
* TraceId
* SpanId
* Request ID where appropriate
* Correlation/business ID
* Error details
* Environment

**Avoid**

```text
Console.WriteLine("Something went wrong");
```

Prefer structured `ILogger`.

---

28. What is the purpose of Retry Pattern?

> Retry attempts an operation again when it fails due to a **transient problem**.

Example:

```text
Payment Service
      ↓
Timeout
      ↓
Retry #1
      ↓
Retry #2
      ↓
Success
```

Use:

```text
Bounded retries
+
Exponential backoff
+
Jitter
```

Example:

```text
100ms
200ms
400ms
800ms
```

**Don't retry:**

```text
400 Bad Request
401 Unauthorized
403 Forbidden
Validation failure
Business rule failure
```

**Important**

Retrying a non-idempotent operation can cause duplicate effects.

So:

```text
Retry + Idempotency
```

often go together.

---

29. What are Sidecars in Microservices?

> A **sidecar** is a separate process/container deployed alongside an application service to provide supporting capabilities.

Conceptually:

```text
Pod
┌───────────────────────────┐
│                           │
│   Application Container   │
│           ↕               │
│   Sidecar Container       │
│                           │
└───────────────────────────┘
```

The sidecar can handle infrastructure concerns such as:

* Proxying
* Telemetry
* Service-to-service communication
* Security
* Configuration-related capabilities

`Service Mesh`

Service meshes can use sidecar proxies:

```text
Service A
   ↕
Proxy
   ↕
Proxy
   ↕
Service B
```

The application doesn't need to implement every network concern itself.

**Important**

Sidecar is **not another business microservice**.

It provides supporting infrastructure functionality.

---

30. Best Practices for Deploying Microservices in Production

This is a very good **senior interview question**.

I would structure the answer around these areas:

`1. Containerization`

```text
Docker
```

Use immutable versioned images.

```text
order-service:1.4.2
```

---

`2. Orchestration`

```text
Kubernetes
```

for:

* Scaling
* Service discovery
* Rolling deployment
* Self-healing

---

`3. CI/CD`

```text
Git
 ↓
Build
 ↓
Test
 ↓
Security Scan
 ↓
Docker Build
 ↓
Registry
 ↓
Deploy
```

---

`4. Configuration`

Don't hardcode environment-specific values.

```text
Development
Staging
Production
```

Use configuration management and secret stores.

---

`5. Security`

```text
HTTPS
OAuth/OIDC
JWT
Authorization
Secrets
Least privilege
Network security
```

---

`6. Observability`

```text
Logs
Metrics
Distributed Tracing
Alerts
Dashboards
```

---

`7. Resilience`

```text
Timeout
Retry
Circuit Breaker
Bulkhead
Idempotency
DLQ
```

---

`8. Health Checks`

Expose:

```text
/health
```

and preferably distinguish:

```text
Liveness
Readiness
```

A service can be alive but not ready to receive traffic.

---

`9. Deployment Strategy`

Use:

```text
Rolling
Canary
Blue-Green
```

depending on requirements.

---

`10. Database Migration`

Don't make destructive schema changes immediately.

Prefer:

```text
Expand
 ↓
Deploy
 ↓
Migrate
 ↓
Contract
```

---

**Your Complete Microservices Interview Cheat Sheet**

You have now covered almost all of the important concepts:

| Topic                | Remember this                                |
| -------------------- | -------------------------------------------- |
| Microservices        | Business capability + independent deployment |
| Bounded Context      | Domain boundary                              |
| Database per Service | Service owns its data                        |
| API Gateway          | Single client entry point                    |
| Service Discovery    | Find service instances dynamically           |
| Sync                 | HTTP/gRPC                                    |
| Async                | Message broker                               |
| Queue                | Work distribution                            |
| Topic                | Pub/Sub                                      |
| Saga                 | Distributed business workflow                |
| Outbox               | Reliable DB → message publishing             |
| Eventual Consistency | State converges over time                    |
| Retry                | Try transient failure again                  |
| Timeout              | Don't wait forever                           |
| Circuit Breaker      | Stop calling failing dependency              |
| Idempotency          | Duplicate operation has one effective result |
| CQRS                 | Separate read/write responsibilities         |
| Distributed Tracing  | Follow request across services               |
| Docker               | Package/run containers                       |
| Kubernetes           | Orchestrate containers                       |
| OAuth                | Authorization framework                      |
| Rate Limiting        | Control request rate                         |
| Event Driven         | Communicate through events                   |
| Sharding             | Split data across DB partitions              |
| Canary               | Gradually release new version                |
| DDD                  | Model business domain                        |
| Chaos Engineering    | Test resilience through controlled failures  |
| Sidecar              | Supporting infrastructure process            |
| Observability        | Logs + Metrics + Traces                      |

---
---