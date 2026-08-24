# .NET Interview Preparation

# .NET Interview Preparation — Topic Roadmap

Use this as your master index. Create one note per topic (or sub-topic) in your notes tool, and link them back here.

---

## 1. OOP Fundamentals

- Four Pillars: Abstraction, Encapsulation, Inheritance, Polymorphism
- Method Overloading vs Overriding
- Abstract Class vs Interface
- Sealed Classes and Methods
- Static vs Instance members
- Constructors (default, parameterized, static, private)
- Composition vs Inheritance ("has-a" vs "is-a")
- Boxing and Unboxing

## 2. SOLID Principles

- **S** – Single Responsibility Principle
- **O** – Open/Closed Principle
- **L** – Liskov Substitution Principle
- **I** – Interface Segregation Principle
- **D** – Dependency Inversion Principle
- Real-world code examples of violations vs fixes

## 3. Design Patterns

- Creational: Singleton, Factory, Abstract Factory, Builder
- Structural: Adapter, Decorator, Facade, Proxy
- Behavioral: Strategy, Observer, Repository, Unit of Work, Mediator (MediatR)
- Dependency Injection as a pattern (and built-in .NET DI container)

## 4. C# Language Deep Dive

- Value Types vs Reference Types
- `ref`, `out`, `in` parameters
- Nullable Reference Types
- Records (`record` vs `class` vs `struct`)
- Pattern Matching (switch expressions, `is`, `when`)
- Generics and Constraints
- Delegates, Events, Func/Action/Predicate
- Extension Methods
- Exception Handling best practices (custom exceptions, `try/finally`, exception filters)
- `Span<T>`, `Memory<T>` (performance-focused topics)
- Init-only properties, required members (C# 11+)

## 5. Async Programming & Multithreading

- `async`/`await` internals (Task, ValueTask)
- Task vs Thread vs Thread Pool
- `ConfigureAwait(false)` — when and why
- Deadlocks in async code
- `CancellationToken`
- `Parallel.For`, `Parallel.ForEach`
- `lock`, `Monitor`, `Mutex`, `Semaphore`, `SemaphoreSlim`
- `ConcurrentDictionary` and other thread-safe collections
- Channels (`System.Threading.Channels`)

## 6. LINQ

- Deferred vs Immediate Execution
- `IEnumerable` vs `IQueryable`
- Common operators: `Where`, `Select`, `GroupBy`, `Join`, `Aggregate`, `SelectMany`
- Method syntax vs Query syntax
- LINQ performance pitfalls (multiple enumeration, N+1 issues)
- Custom LINQ extension methods

## 7. Entity Framework Core

- Code First vs Database First
- DbContext lifecycle & scope (per-request in Web API)
- Migrations (Add-Migration, Update-Database)
- Change Tracking (Tracking vs No-Tracking queries)
- Relationships: One-to-Many, Many-to-Many, One-to-One
- Fluent API vs Data Annotations
- Lazy Loading vs Eager Loading (`Include`) vs Explicit Loading
- Transactions in EF Core
- Raw SQL queries (`FromSqlRaw`, `ExecuteSqlRaw`)
- Concurrency handling (optimistic concurrency)
- Performance: `AsNoTracking`, compiled queries, avoiding N+1

## 8. [ASP.NET](http://asp.net/) Core Web API

- Request Pipeline overview
- **Middleware** — built-in and custom, order of execution, `Use`/`Run`/`Next`
- Routing (attribute routing, conventional routing)
- Filters: Action, Exception, Authorization, Resource, Result filters
- Model Binding & Validation
- Dependency Injection lifetimes: Transient, Scoped, Singleton
- Controllers vs Minimal APIs
- API Versioning
- Content Negotiation
- Exception Handling (global exception middleware, `ProblemDetails`)
- CORS
- Rate Limiting (built-in .NET 7+ feature)
- Output Caching / Response Caching
- Health Checks

## 9. Authentication & Authorization

- JWT Authentication (how tokens work, claims, refresh tokens)
- Authorization: Role-based vs Policy-based vs Claims-based
- Identity Framework basics
- OAuth2 / OpenID Connect concepts

## 10. Configuration & Hosting

- `appsettings.json`, environment-specific configs
- Options Pattern (`IOptions`, `IOptionsSnapshot`, `IOptionsMonitor`)
- Dependency Injection container internals
- Hosted Services / Background Services (`IHostedService`, `BackgroundService`)
- Logging (`ILogger`, Serilog, structured logging)
- `appsettings` secrets management, Azure Key Vault basics

## 11. Testing

- Unit Testing (xUnit/NUnit/MSTest)
- Mocking (Moq, NSubstitute)
- Integration Testing in Web API (`WebApplicationFactory`)
- Test-Driven Development basics

## 12. Architecture & System Design

- Layered Architecture (Presentation, Business, Data)
- Clean Architecture / Onion Architecture
- Microservices basics (communication, API Gateway, service discovery)
- CQRS pattern
- Repository & Unit of Work pattern
- Domain-Driven Design (basic concepts: entities, value objects, aggregates)
- REST API design best practices
- Caching strategies (in-memory, distributed/Redis)
- Message Queues (RabbitMQ/Azure Service Bus — conceptual)

## 13. What's New: .NET 7 / 8 / 10

- **.NET 7:** Minimal APIs enhancements, Rate limiting middleware, `IEndpointFilter`
- **.NET 8:** Native AOT, Blazor United (render modes), Keyed DI services, `TimeProvider`, primary constructors adoption
- **.NET 9/10 (if applicable):** check release notes closer to interview — new performance features, params collections, field-backed properties

## 14. Database & SQL (often paired with .NET interviews)

- Indexes, Joins, Normalization
- Stored Procedures vs ORM
- Transactions & Isolation Levels
- Query optimization basics

## 15. Miscellaneous / Frequently Asked

- Difference between .NET Framework, .NET Core, .NET 5+
- Garbage Collection (Generations, `IDisposable`, `using` statement, finalizers)
- Reflection basics
- Dependency Injection vs Service Locator
- Git basics (often asked alongside technical rounds)

---

[OOP Fundamentals](https://app.notion.com/p/OOP-Fundamentals-3bfdc3422e8580138f39f856ebfc154a?pvs=21)

[C# Language Deep Dive](https://app.notion.com/p/C-Language-Deep-Dive-3c4dc3422e858056a392f490746da53f?pvs=21)