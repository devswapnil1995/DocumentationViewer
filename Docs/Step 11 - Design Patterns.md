Design patterns are **reusable solutions to commonly occurring software design problems**.

The classic **GoF (Gang of Four)** patterns are divided into 3 categories:

```text
Design Patterns
│
├── 1. Creational
│      ↓
│   How objects are created
│
├── 2. Structural
│      ↓
│   How objects/classes are composed
│
└── 3. Behavioral
       ↓
    How objects communicate/behave
```

---

### 1. Creational Patterns

> **Concerned with object creation.**

| Pattern              | Purpose                                        | Example                |
| -------------------- | ---------------------------------------------- | ---------------------- |
| **Singleton**        | Only one instance                              | Configuration/cache    |
| **Factory Method**   | Create objects without exposing creation logic | Payment service        |
| **Abstract Factory** | Create related families of objects             | UI components          |
| **Builder**          | Build complex objects step-by-step             | Complex request/config |
| **Prototype**        | Create object by cloning existing object       | Object templates       |

**Most important for .NET**

**Singleton, Factory, Builder**

Example Factory:

```csharp
public interface IPayment
{
    void Pay();
}

public class CreditCardPayment : IPayment
{
    public void Pay() { }
}

public class UPIPayment : IPayment
{
    public void Pay() { }
}
```

Factory:

```csharp
public class PaymentFactory
{
    public IPayment Create(string type)
    {
        return type switch
        {
            "UPI" => new UPIPayment(),
            "Card" => new CreditCardPayment(),
            _ => throw new ArgumentException()
        };
    }
}
```

Instead of:

```csharp
if (...)
    new UPIPayment();
else
    new CreditCardPayment();
```

everywhere, creation is centralized.

---

### 2. Structural Patterns

> **Concerned with how classes and objects are combined to form larger structures.**

| Pattern       | Purpose                                          |
| ------------- | ------------------------------------------------ |
| **Adapter**   | Make incompatible interfaces work together       |
| **Bridge**    | Separate abstraction from implementation         |
| **Composite** | Treat individual and groups uniformly            |
| **Decorator** | Add behavior without modifying original class    |
| **Facade**    | Provide a simple interface over a complex system |
| **Flyweight** | Share objects to reduce memory                   |
| **Proxy**     | Control access to another object                 |

### Most important for .NET

**Adapter, Decorator, Facade, Proxy**

**Adapter**

Suppose your application expects:

```csharp
public interface IPaymentService
{
    void Pay(decimal amount);
}
```

But third-party library gives:

```csharp
public class StripeClient
{
    public void MakePayment(decimal amount)
    {
    }
}
```

You can't directly use it as `IPaymentService`.

Create an adapter:

```csharp
public class StripeAdapter : IPaymentService
{
    private readonly StripeClient _client;

    public StripeAdapter(StripeClient client)
    {
        _client = client;
    }

    public void Pay(decimal amount)
    {
        _client.MakePayment(amount);
    }
}
```

Now:

```text
Your Application
       ↓
IPaymentService
       ↓
StripeAdapter
       ↓
StripeClient
```

---

### 3. Behavioral Patterns

> **Concerned with communication, responsibilities, and behavior between objects.**

| Pattern                     | Purpose                                                         |
| --------------------------- | --------------------------------------------------------------- |
| **Chain of Responsibility** | Pass request through a chain of handlers                        |
| **Command**                 | Encapsulate a request/action as an object                       |
| **Interpreter**             | Interpret a language/grammar                                    |
| **Iterator**                | Traverse a collection                                           |
| **Mediator**                | Centralize communication between objects                        |
| **Memento**                 | Capture/restore object state                                    |
| **Observer**                | Notify objects when state changes                               |
| **State**                   | Change behavior based on current state                          |
| **Strategy**                | Select algorithm/behavior dynamically                           |
| **Template Method**         | Define algorithm structure, allow subclasses to customize steps |
| **Visitor**                 | Add operations to object structures without modifying them      |

**Most important for .NET/backend**

I'd focus heavily on:

```text
Strategy
Factory
Decorator
Chain of Responsibility
Observer
Mediator
Command
State
```

**Strategy Pattern**

This one is extremely useful.

Suppose you have payment methods:

```text
Payment
 ├── Credit Card
 ├── UPI
 └── PayPal
```

Instead of:

```csharp
if (type == "UPI")
{
}
else if (type == "Card")
{
}
else if (type == "PayPal")
{
}
```

Create a strategy:

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}
```

Implementations:

```csharp
public class UpiPayment : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine("UPI payment");
    }
}
```

```csharp
public class CardPayment : IPaymentStrategy
{
    public void Pay(decimal amount)
    {
        Console.WriteLine("Card payment");
    }
}
```

Then:

```csharp
public class PaymentService
{
    private readonly IPaymentStrategy _strategy;

    public PaymentService(IPaymentStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Pay(decimal amount)
    {
        _strategy.Pay(amount);
    }
}
```

The behavior can be changed without changing `PaymentService`.

---

**Decorator Pattern**

> Add behavior to an existing object **without modifying the original class**.

For example:

```text
OrderService
     ↓
Logging Decorator
     ↓
Caching Decorator
     ↓
Actual OrderService
```

Conceptually:

```text
Request
   ↓
Logging
   ↓
Caching
   ↓
Business Logic
```

This is very relevant to .NET middleware and cross-cutting concerns.

---

**Chain of Responsibility**

You have a sequence of handlers:

```text
Request
   ↓
Authentication
   ↓
Authorization
   ↓
Validation
   ↓
Business Logic
```

Each handler can:

* Handle the request
* Reject it
* Pass it to the next handler

ASP.NET Core **middleware pipeline** is a great practical example of this concept.

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ValidationMiddleware>();
```

Conceptually:

```text
Request
  ↓
Middleware 1
  ↓
Middleware 2
  ↓
Middleware 3
  ↓
Endpoint
```

---

**Observer Pattern**

> One object changes → notify interested objects.

Example:

```text
Order Created
     ↓
   Event
   / | \
  ↓  ↓  ↓
Email
SMS
Analytics
```

In .NET, concepts such as events/delegates and messaging/event-driven architectures can implement similar ideas.

---

**Mediator Pattern**

> Objects communicate through a mediator instead of directly communicating with each other.

Without mediator:

```text
A → B
A → C
A → D
B → C
B → D
```

Lots of coupling.

With mediator:

```text
A ─┐
B ─┼→ Mediator
C ─┤
D ─┘
```

In .NET applications, **MediatR** is a commonly encountered implementation/library for the mediator pattern.

---

**Factory vs Strategy — VERY IMPORTANT**

These are often confused.

`Factory`

> **Which object should I create?**

```text
Factory
   ↓
UPIPayment
```

`Strategy`

> **Which behavior/algorithm should I use?**

```text
PaymentService
      ↓
Strategy
   ↙     ↘
UPI      Card
```

Easy memory:

```text
Factory  → CREATE
Strategy → BEHAVIOR
```

---
---

> **Don't use patterns just because they exist. Use a pattern when it solves an actual design problem.**

**Example: E-commerce .NET API**

Imagine an Order API:

```text
                         Order API
                            |
                     ┌──────┴──────┐
                     ↓             ↓
                Controller      Middleware
                                    |
                              Chain of Responsibility
                                    |
                                    ↓
                              Order Service
                                    |
                   ┌────────────────┼────────────────┐
                   ↓                ↓                ↓
              Repository         Payment         Notification
                   |              Strategy            |
                   ↓                ↓                  ↓
               Database       ┌─────┼─────┐       Observer
                              ↓     ↓     ↓
                             UPI   Card  PayPal
```

Here we're already using multiple patterns.

---

**`1. Strategy + Factory`**

Suppose payment can be:

```text
UPI
Credit Card
PayPal
```

We can use **Strategy** for the payment behavior:

```csharp
public interface IPaymentStrategy
{
    Task PayAsync(decimal amount);
}
```

```csharp
public class UpiPayment : IPaymentStrategy
{
    public Task PayAsync(decimal amount)
    {
        // UPI payment
        return Task.CompletedTask;
    }
}
```

Then a **Factory** can decide which strategy to create:

```csharp
public class PaymentFactory
{
    public IPaymentStrategy Create(string type)
    {
        return type switch
        {
            "UPI" => new UpiPayment(),
            "CARD" => new CardPayment(),
            _ => throw new ArgumentException()
        };
    }
}
```

So:

```text
Factory
   ↓
decides WHICH object
   ↓
Strategy
   ↓
defines HOW it behaves
```

These two patterns work very naturally together.

---

**`2. Repository + Strategy`**

You could have:

```text
OrderService
     |
     +---- IOrderRepository
     |
     +---- IPaymentStrategy
```

Repository handles:

> **How do I access data?**

Strategy handles:

> **Which business behavior should I use?**

Different problems → different patterns.

---

**`3. Decorator + Service`**

Suppose you have:

```text
OrderService
```

You want to add:

* Logging
* Caching
* Performance measurement

without modifying `OrderService`.

You can use **Decorator**:

```text
Request
   ↓
Logging Decorator
   ↓
Caching Decorator
   ↓
OrderService
```

Conceptually:

```csharp
public class LoggingOrderService : IOrderService
{
    private readonly IOrderService _inner;

    public LoggingOrderService(IOrderService inner)
    {
        _inner = inner;
    }

    public async Task CreateOrder()
    {
        Console.WriteLine("Starting");

        await _inner.CreateOrder();

        Console.WriteLine("Finished");
    }
}
```

Now you've combined:

```text
Decorator
+
Dependency Injection
+
Interface
```

---

**`4. Chain of Responsibility + API`**

ASP.NET Core middleware is a good example of a **pipeline/chain concept**:

```text
HTTP Request
     ↓
Exception Middleware
     ↓
Logging Middleware
     ↓
Authentication
     ↓
Authorization
     ↓
Routing
     ↓
Controller
```

Each stage can process the request and pass it forward.

---

**`5. Observer + Background Processing`**

Suppose:

```text
Order Created
      ↓
   Event
      ↓
 ┌────┼─────┐
 ↓    ↓     ↓
Email SMS Analytics
```

This is an Observer/event-driven style.

You could then have:

```text
Event
  ↓
Message Queue
  ↓
BackgroundService
  ↓
Process notification
```

So multiple patterns/concepts work together.

---
A mature application could look something like:

```text
┌─────────────────────────────────────────┐
│              ASP.NET Core               │
│                                         │
│  Middleware → Chain of Responsibility   │
│       ↓                                 │
│  Controllers                            │
│       ↓                                 │
│  Application Services                   │
│       ↓                                 │
│  Strategy ←→ Factory                    │
│       ↓                                 │
│  Repository                             │
│       ↓                                 │
│  EF Core / Database                     │
│                                         │
│  Events → Observer                      │
│       ↓                                 │
│  Queue                                  │
│       ↓                                 │
│  BackgroundService                      │
│                                         │
│  Decorators → Logging / Caching         │
└─────────────────────────────────────────┘
```

**"Can you use multiple design patterns in one application?"**

> **"Yes. Design patterns solve different types of problems, so it's common to combine them. For example, an application might use Factory and Strategy for selecting business behavior, Repository for data access, Decorator for cross-cutting concerns, and Chain of Responsibility for request processing. The patterns should be introduced based on actual design problems rather than forcing patterns into the application."**
----
----