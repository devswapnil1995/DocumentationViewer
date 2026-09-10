> SOLID = 5 principles that help us write maintainable, flexible, testable, and loosely coupled code.

> Keep classes focused (S), extend instead of modifying (O), ensure implementations can safely replace abstractions (L), keep interfaces small (I), and depend on abstractions rather than concrete implementations (D).

```text
S → Single Responsibility Principle
O → Open/Closed Principle
L → Liskov Substitution Principle
I → Interface Segregation Principle
D → Dependency Inversion Principle
```

**`1. S — Single Responsibility Principle (SRP)`**

> A class should have one responsibility and one reason to change.

> SRP means a class should have one reason to change.

**❌ Bad**

```csharp
public class OrderService
{
    public void CreateOrder()
    {
        // Create order
    }

    public void SaveToDatabase()
    {
        // Save to DB
    }

    public void SendEmail()
    {
        // Send email
    }

    public void GenerateInvoice()
    {
        // Generate invoice
    }
}
```

This class has multiple responsibilities:

```text
OrderService
 ├── Order logic
 ├── Database logic
 ├── Email logic
 └── Invoice logic
```

If email implementation changes, `OrderService` changes.

**✅ Better**

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailService _emailService;

    public OrderService(IOrderRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task CreateOrder()
    {
        // Order business logic
        await _repository.Save();
        await _emailService.Send();
    }
}
```

Separate responsibilities:

```text
OrderService       → Business logic
OrderRepository    → Database
EmailService       → Email
InvoiceService     → Invoice
```

---

**`2. O — Open/Closed Principle`**

> Software should be open for extension but closed for modification.

> OCP means extend behavior without modifying existing, stable code.

Suppose you calculate discounts.

**❌ Bad**

```csharp
public decimal CalculateDiscount(string customerType)
{
    if (customerType == "Regular")
        return 10;

    if (customerType == "Premium")
        return 20;

    if (customerType == "VIP")
        return 30;

    return 0;
}
```

Now you add:

```text
Gold
Corporate
Employee
Student
```

You keep modifying this method.

**✅ Better**

Use abstraction:

```csharp
public interface IDiscountStrategy
{
    decimal Calculate(decimal amount);
}
```

Implement different strategies:

```csharp
public class RegularDiscount : IDiscountStrategy
{
    public decimal Calculate(decimal amount)
        => amount * 0.10m;
}
```

```csharp
public class PremiumDiscount : IDiscountStrategy
{
    public decimal Calculate(decimal amount)
        => amount * 0.20m;
}
```

Now adding a new discount means **adding a new class**, rather than modifying existing business logic.

```text
IDiscountStrategy
       ↑
 ┌─────┼──────┐
 ↓     ↓      ↓
Regular Premium VIP
```

---

**`3. L — Liskov Substitution Principle`**

> A derived class should be usable wherever its base class is expected without breaking the correctness of the program.

> LSP means derived types must honor the contract/behavior expected from the base abstraction and should be safely substitutable for it.

Classic example:

```text
Bird
 ├── Sparrow
 └── Penguin
```

Suppose:

```csharp
public class Bird
{
    public virtual void Fly()
    {
        Console.WriteLine("Flying");
    }
}
```

Then:

```csharp
public class Penguin : Bird
{
    public override void Fly()
    {
        throw new NotSupportedException();
    }
}
```

Now:

```csharp
Bird bird = new Penguin();

bird.Fly(); // 💥
```

The derived class breaks the expectation of the base class.

**Better design**

Don't put `Fly()` into the base class if every bird can't fly.

```csharp
public abstract class Bird
{
    public abstract void Eat();
}
```

Then:

```csharp
public interface IFlyingBird
{
    void Fly();
}
```

Sparrow:

```csharp
public class Sparrow : Bird, IFlyingBird
{
    public void Fly()
    {
        Console.WriteLine("Flying");
    }

    public override void Eat()
    {
        Console.WriteLine("Eating");
    }
}
```

Penguin:

```csharp
public class Penguin : Bird
{
    public override void Eat()
    {
        Console.WriteLine("Eating");
    }
}
```


---

**`4. I — Interface Segregation Principle`**

> Clients should not be forced to depend on methods they don't need.

> ISP means interfaces should be small and focused so implementations aren't forced to depend on methods they don't use.

**❌ Bad**

```csharp
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}
```

Now suppose a robot implements it:

```csharp
public class Robot : IWorker
{
    public void Work()
    {
    }

    public void Eat()
    {
        // Robot doesn't eat!
    }

    public void Sleep()
    {
        // Robot doesn't sleep!
    }
}
```

We're forcing `Robot` to implement methods it doesn't need.

**✅ Better**

Split the interface:

```csharp
public interface IWorkable
{
    void Work();
}

public interface IEatable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}
```

Human:

```csharp
public class Human : IWorkable, IEatable, ISleepable
{
    public void Work() { }
    public void Eat() { }
    public void Sleep() { }
}
```

Robot:

```csharp
public class Robot : IWorkable
{
    public void Work() { }
}
```
---

**`5. D — Dependency Inversion Principle`**

> High-level modules should depend on abstractions, not concrete low-level implementations.

**❌ Bad**

```csharp
public class OrderService
{
    private readonly EmailService _emailService;

    public OrderService()
    {
        _emailService = new EmailService();
    }
}
```

`OrderService` directly depends on:

```text
OrderService
      ↓
EmailService
```

If you want to change to SMS:

```text
EmailService → SmsService
```

you need to modify `OrderService`.

**✅ Better**

Create abstraction:

```csharp
public interface INotificationService
{
    void Send(string message);
}
```

Implementation:

```csharp
public class EmailService : INotificationService
{
    public void Send(string message)
    {
        // Send email
    }
}
```

Then:

```csharp
public class OrderService
{
    private readonly INotificationService _notification;

    public OrderService(INotificationService notification)
    {
        _notification = notification;
    }
}
```

DI:

```csharp
builder.Services.AddScoped<
    INotificationService,
    EmailService>();
```

Now:

```text
              INotificationService
                     ↑
                     |
              EmailService
                     |
                 DI Container
                     |
                     ↓
                OrderService
```

Tomorrow you can use:

```csharp
builder.Services.AddScoped<
    INotificationService,
    SmsService>();
```

without changing `OrderService`.

---

**SOLID in one example**

Imagine an e-commerce application:

```text
                    OrderService
                         |
        +----------------+----------------+
        |                |                |
        ↓                ↓                ↓
   IOrderRepository  IPaymentService  INotification
        |                |                |
        ↓                ↓                ↓
    PostgreSQL      Stripe/UPI/etc.   Email/SMS
```

This gives you:

**`S`**

Each component has a focused responsibility.

**`O`**

Add a new payment method without rewriting order logic.

**`L`**

Implementations must correctly honor their abstractions.

**`I`**

Use focused interfaces:

```text
IPaymentService
IEmailService
IOrderRepository
```

instead of one giant:

```text
IEverythingService
```

**`D`**

`OrderService` depends on interfaces, not concrete classes.

---

**The most important distinction: DIP vs DI**

Dependency Inversion Principle (DIP) is a **design principle**:

> High-level code should depend on abstractions.

Dependency Injection (DI) is a **technique** used to implement that principle.

```text
DIP
 ↓
"Depend on abstraction"
 ↓
DI
 ↓
"Give the implementation from outside"
```

Example:

```csharp
public OrderService(IOrderRepository repository)
```

DIP:

```text
OrderService → IOrderRepository
```

DI:

```text
DI Container → OrderRepository
                     ↓
               IOrderRepository
                     ↓
                OrderService
```

---

### Interview Cheat Sheet

| Principle   | Simple meaning                   | Keyword                  |
| ----------- | -------------------------------- | ------------------------ |
| **S — SRP** | One responsibility               | **One reason to change** |
| **O — OCP** | Extend without modifying         | **Extension**            |
| **L — LSP** | Child must honor parent contract | **Substitution**         |
| **I — ISP** | Small, focused interfaces        | **Don't force methods**  |
| **D — DIP** | Depend on abstractions           | **Loose coupling**       |

---------
---------