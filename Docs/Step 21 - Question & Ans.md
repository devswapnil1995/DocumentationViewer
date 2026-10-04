## GBIT Interview
* Show example of SOLID Principle, also explain open close
---------
We'll build a simple **Order Processing system**.

```text
OrderController
      ↓
IOrderService
      ↓
OrderService
   ┌──┴──────────────┐
   ↓                 ↓
IPaymentService   IOrderRepository
   ↓                 ↓
StripePayment      SqlOrderRepository
```

This design lets us demonstrate **SRP, OCP, LSP, ISP and DIP**.

---

## Complete Example

```csharp
// ==========================================
// DOMAIN
// ==========================================

public class Order
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
}
```

---

## 1. SRP — Single Responsibility Principle

> A class should have only one reason to change.

Instead of putting everything inside `OrderService`:

```csharp
// ❌ BAD
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        // Validate order
        // Save to database
        // Process payment
        // Send email
        // Generate invoice
    }
}
```

We separate responsibilities.

```csharp
public interface IOrderValidator
{
    bool Validate(Order order);
}

public class OrderValidator : IOrderValidator
{
    public bool Validate(Order order)
    {
        return order.Amount > 0 &&
               !string.IsNullOrEmpty(order.CustomerEmail);
    }
}
```

Payment has its own responsibility:

```csharp
public interface IPaymentService
{
    bool ProcessPayment(decimal amount);
}
```

Repository has its own responsibility:

```csharp
public interface IOrderRepository
{
    void Save(Order order);
}
```

Email has its own responsibility:

```csharp
public interface IEmailService
{
    void SendConfirmation(string email);
}
```

Now each class has one main responsibility.

```text
OrderValidator
    → Validation

PaymentService
    → Payment

OrderRepository
    → Database

EmailService
    → Email
```

That's **SRP**.

---

## 2. OCP — Open/Closed Principle

> Open for extension, closed for modification.

Suppose today we support Stripe:

```csharp
public class StripePaymentService : IPaymentService
{
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Stripe payment: {amount}");
        return true;
    }
}
```

Tomorrow business says:

> "We also want PayPal."

We don't modify `OrderService`.

We simply add:

```csharp
public class PayPalPaymentService : IPaymentService
{
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"PayPal payment: {amount}");
        return true;
    }
}
```

Our existing service remains unchanged.

```text
IPaymentService
       │
       ├── StripePaymentService
       │
       └── PayPalPaymentService
```

That's **OCP**.

---

## 3. LSP — Liskov Substitution Principle

> A derived implementation should be usable wherever its abstraction is expected without breaking the application.

Our `OrderService` expects:

```csharp
IPaymentService
```

It doesn't care whether it's Stripe or PayPal.

```csharp
public class OrderService : IOrderService
{
    private readonly IPaymentService _paymentService;

    public OrderService(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public void Process(Order order)
    {
        _paymentService.ProcessPayment(order.Amount);
    }
}
```

Now either implementation works:

```csharp
IPaymentService payment = new StripePaymentService();

payment.ProcessPayment(100);
```

or:

```csharp
IPaymentService payment = new PayPalPaymentService();

payment.ProcessPayment(100);
```

`OrderService` doesn't break.

That's **LSP**.

**Bad LSP example**

Suppose:

```csharp
public class CashPaymentService : IPaymentService
{
    public bool ProcessPayment(decimal amount)
    {
        throw new NotSupportedException();
    }
}
```

If the abstraction says:

```csharp
IPaymentService.ProcessPayment()
```

but one implementation cannot actually support that behavior, that's a sign the abstraction may be wrong.

This leads naturally to **ISP**.

---

## 4. ISP — Interface Segregation Principle

> Don't force a class to implement methods it doesn't need.

Bad interface:

```csharp
// ❌ BAD
public interface IOrderOperations
{
    void CreateOrder(Order order);
    void CancelOrder(int id);
    void GenerateInvoice(int id);
    void SendEmail(string email);
    void ProcessPayment(decimal amount);
}
```

Imagine a class only responsible for payment:

```csharp
public class StripePaymentService : IOrderOperations
{
    // Now it is forced to implement
    // CreateOrder()
    // CancelOrder()
    // GenerateInvoice()
    // SendEmail()
    
    // ❌ Not its responsibility
}
```

Instead, create smaller interfaces:

```csharp
public interface IOrderRepository
{
    void Save(Order order);
}

public interface IPaymentService
{
    bool ProcessPayment(decimal amount);
}

public interface IEmailService
{
    void SendConfirmation(string email);
}

public interface IInvoiceService
{
    void GenerateInvoice(Order order);
}
```

Now a payment service only needs:

```csharp
public class StripePaymentService : IPaymentService
{
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Stripe payment: {amount}");
        return true;
    }
}
```

That's **ISP**.

---

## 5. DIP — Dependency Inversion Principle

> High-level modules should depend on abstractions, not concrete implementations.

Bad:

```csharp
// ❌ BAD
public class OrderService
{
    private readonly StripePaymentService _payment;

    public OrderService()
    {
        _payment = new StripePaymentService();
    }
}
```

Now `OrderService` is tightly coupled to Stripe.

Instead:

```csharp
public class OrderService
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderRepository _repository;
    private readonly IOrderValidator _validator;
    private readonly IEmailService _emailService;

    public OrderService(
        IPaymentService paymentService,
        IOrderRepository repository,
        IOrderValidator validator,
        IEmailService emailService)
    {
        _paymentService = paymentService;
        _repository = repository;
        _validator = validator;
        _emailService = emailService;
    }

    public void ProcessOrder(Order order)
    {
        if (!_validator.Validate(order))
        {
            throw new Exception("Invalid order");
        }

        _paymentService.ProcessPayment(order.Amount);

        _repository.Save(order);

        _emailService.SendConfirmation(order.CustomerEmail);
    }
}
```

Now `OrderService` depends on:

```text
IPaymentService
IOrderRepository
IOrderValidator
IEmailService
```

not:

```text
StripePaymentService
SqlOrderRepository
OrderValidator
EmailService
```

That's **DIP**.

-----------------
-----------------

* How we can prevent any method to get override without using sealed keyword?
> If I don't want a method to be overridden, I simply don't mark it as virtual or abstract. A normal instance method cannot be overridden. If the method is already virtual and I want to stop further overriding at a particular derived level, then sealed override is the appropriate mechanism

----------------
----------------
* What is wait, yeild, ref, out?

> `Task.Wait()` synchronously blocks the current thread until a Task completes, whereas `yield` is an iterator feature that allows a method to produce values one at a time using lazy evaluation. In asynchronous .NET code, I generally prefer `await` over `Task.Wait()` because `await` doesn't block the thread while waiting.


## `Task.Wait()`

`Wait()` **blocks the current thread** until the task completes.

```csharp
Task task = Task.Run(() =>
{
    Thread.Sleep(3000);
    Console.WriteLine("Task completed");
});

task.Wait();

Console.WriteLine("Main completed");
```

Execution:

```text
Task starts
   ↓
Wait 3 seconds
   ↓
Task completes
   ↓
Main continues
```

The important point:

> `Wait()` is **blocking**.

In ASP.NET Core, you generally prefer:

```csharp
await task;
```

instead of:

```csharp
task.Wait();
```

Because `await` doesn't block the current thread while the asynchronous operation is waiting.

### `Wait()` vs `await`

```csharp
task.Wait();   // blocks thread ❌
```

```csharp
await task;   // asynchronously waits ✅
```

---

## `yield`

`yield` is used with **iterators**.

It allows a method to return values **one at a time** instead of creating the entire collection first.

Example:

```csharp
public IEnumerable<int> GetNumbers()
{
    yield return 1;
    yield return 2;
    yield return 3;
}
```

Usage:

```csharp
foreach (var number in GetNumbers())
{
    Console.WriteLine(number);
}
```

Output:

```text
1
2
3
```

---

**Why use `yield`?**

Without `yield`:

```csharp
public IEnumerable<int> GetNumbers()
{
    var numbers = new List<int>();

    numbers.Add(1);
    numbers.Add(2);
    numbers.Add(3);

    return numbers;
}
```

The complete collection is created first.

With:

```csharp
yield return 1;
yield return 2;
yield return 3;
```

values are generated **lazily**, as the consumer asks for them.

Think:

```text
Without yield:

Generate 1
Generate 2
Generate 3
      ↓
Return entire collection
      ↓
foreach


With yield:

foreach asks → Generate 1
foreach asks → Generate 2
foreach asks → Generate 3
```

---

**Very important: `yield return` doesn't mean async**

This is a common interview trap.

```csharp
yield return 1;
```

does **not** mean:

> Run asynchronously.

It means:

> Return this value to the iterator and pause the iterator's execution until the next value is requested.

---

`yield break`

You can stop an iterator early:

```csharp
public IEnumerable<int> GetNumbers()
{
    yield return 1;
    yield return 2;

    yield break;

    yield return 3;
}
```

Output:

```text
1
2
```

---

**Interview comparison**

| `Wait()`                            | `yield`                          |
| ----------------------------------- | -------------------------------- |
| Used with Tasks/synchronization     | Used with iterators              |
| Blocks thread                       | Doesn't mean blocking            |
| Waits for operation                 | Produces values                  |
| `task.Wait()`                       | `yield return`                   |
| Can hurt scalability in ASP.NET     | Enables lazy iteration           |
| Prefer `await` for async operations | Useful for large/sequential data |

-------------
-------------
* What is Polly?

> Polly is used to implement resilience policies around operations that can experience transient failures.

For example, your .NET API calls an external payment API:

```text
Your API
   ↓
Payment API
   ↓
Temporary failure
```

Instead of immediately returning an error, Polly can retry the request.

---

## 1. Why do we need Polly?

Suppose:

```csharp
var response = await httpClient.GetAsync(
    "https://payment-api.com/payment");
```

The external API might temporarily fail because of:

* Network issue
* Temporary timeout
* Server overload
* HTTP 503
* Connection failure
* Transient database/network failure

You don't necessarily want to fail the user request immediately.

Polly allows you to define:

```text
Failure
  ↓
Retry?
  ↓
Still failing?
  ↓
Circuit Breaker?
  ↓
Fallback?
```

---

`Retry`

Example:

```text
Attempt 1 → Failed
Attempt 2 → Failed
Attempt 3 → Success
```

Conceptually:

```csharp
await policy.ExecuteAsync(async () =>
{
    return await httpClient.GetAsync(url);
});
```

You can configure retries with delays.

For example:

```text
Retry 1 → after 1 second
Retry 2 → after 2 seconds
Retry 3 → after 4 seconds
```

This is called **exponential backoff**.

**Important**

Don't retry every exception.

For example:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
```

Usually retrying these doesn't fix anything.

A temporary:

```text
408
429
500
502
503
504
```

may be appropriate depending on the API and operation.

---

`Circuit Breaker`

This is one of the most important Polly concepts.

Imagine your payment service is completely down.

Without circuit breaker:

```text
Request 1 → Payment API → Failed
Request 2 → Payment API → Failed
Request 3 → Payment API → Failed
Request 4 → Payment API → Failed
...
1000 requests → Payment API
```

You're continuously hitting an unhealthy service.

Circuit breaker:

```text
Request
   ↓
Failures reach threshold
   ↓
Circuit OPEN
   ↓
Stop calling external API temporarily
   ↓
Wait
   ↓
Try again
   ↓
Circuit HALF-OPEN
   ↓
Success → CLOSED
```

So:

```text
CLOSED
   ↓ failures
OPEN
   ↓ after delay
HALF-OPEN
   ↓
Success → CLOSED
Failure → OPEN
```

This protects both your application and the failing downstream service.

---

`Timeout`

You can also define a timeout.

For example:

```text
Call external API
      ↓
Wait 5 seconds
      ↓
Still no response
      ↓
Timeout
```

Instead of allowing a request to hang indefinitely.

---

`Fallback`

Suppose your recommendation service is unavailable.

Instead of:

```text
Recommendation API
       ↓
Failure
       ↓
500 Internal Server Error
```

you could return a fallback:

```text
Recommendation API
       ↓
Failure
       ↓
Fallback
       ↓
"Popular products"
```

---

`Modern .NET usage`

For a modern .NET application, you may see Polly integrated through Microsoft's HTTP resilience support.

For example, conceptually:

```csharp
builder.Services
    .AddHttpClient<IPaymentClient, PaymentClient>()
    .AddStandardResilienceHandler();
```

This is preferable to manually creating a resilience policy around every `HttpClient` call.

The modern resilience pipeline can combine things such as:

```text
Rate limiter
     ↓
Total timeout
     ↓
Retry
     ↓
Circuit breaker
     ↓
Attempt timeout
```

---

## Polly vs `try-catch`

This is a very important interview distinction.

`try-catch` handles an error:

```csharp
try
{
    await paymentClient.ProcessAsync();
}
catch (Exception ex)
{
    // Handle error
}
```

Polly is about **resilience behavior**:

```text
Temporary failure
       ↓
Retry
       ↓
Timeout
       ↓
Circuit breaker
       ↓
Fallback
```

You can still use `try-catch` around a Polly-protected operation when you need application-specific error handling.

---

**Real-world example for your interview**

Suppose your application has:

```text
Angular
   ↓
.NET API
   ↓
Payment Service
```

Payment Service occasionally returns `503`.

You configure:

```text
.NET API
   ↓
HttpClient
   ↓
Polly resilience pipeline
   │
   ├── Timeout
   ├── Retry
   └── Circuit Breaker
   ↓
Payment Service
```

If the payment service has a temporary failure:

```text
.NET API
   ↓
Payment API
   ↓
503
   ↓
Retry
   ↓
Payment API
   ↓
Success
```

If the payment service continues failing:

```text
503
 ↓
503
 ↓
503
 ↓
Circuit opens
 ↓
Don't keep hammering Payment API
```

```text
Retry       → Try again
Timeout     → Stop waiting
Circuit     → Stop calling unhealthy service
Fallback    → Provide alternative response
Rate limit  → Control request volume
```
-------------
-------------
* What tool we use for retry pattern?

> It depends on the dependency. For HTTP calls in modern .NET, I can use `Microsoft.Extensions.Http.Resilience`. Azure SDKs also provide built-in retry mechanisms for Azure services. For EF Core, I can use `EnableRetryOnFailure` for transient database failures. For asynchronous message processing, Service Bus retry/redelivery or a messaging framework such as MassTransit can be appropriate. For background jobs, Hangfire provides job retries. Polly is still an option when I need custom resilience policies.

### Common alternatives

| Tool / approach                          | Where you'd use it                       | Example                                      |
| ---------------------------------------- | ---------------------------------------- | -------------------------------------------- |
| **Microsoft.Extensions.Http.Resilience** | `HttpClient` calls                       | Modern .NET resilience pipeline              |
| **Azure SDK built-in retry**             | Azure Blob, Key Vault, Service Bus, etc. | SDK automatically retries transient failures |
| **EF Core execution strategy**           | Database operations                      | Retries transient DB failures                |
| **Azure Service Bus retry / redelivery** | Message processing                       | Failed message is delivered again            |
| **Hangfire**                             | Background jobs                          | Retry failed jobs                            |
| **MassTransit**                          | Messaging/microservices                  | Message retry policies                       |
| **Custom retry logic**                   | Simple/special cases                     | `for` loop + delay                           |

---
**Easy way to remember**

```text
HTTP API        → Http.Resilience
Azure service   → Azure SDK retry
SQL / EF Core   → EnableRetryOnFailure
Service Bus     → Redelivery / retry
Background job  → Hangfire
Custom policy   → Polly
```

------
------

* How LLM model/AI tool works and generate response?
----
A simplified architecture is:

```text
User
 ↓
Chat UI / Your .NET API
 ↓
Authentication + Request validation
 ↓
AI/LLM API
 ↓
Tokenization
 ↓
Transformer model
 ↓
Probability calculation
 ↓
Token selection
 ↓
Repeat until response is complete
 ↓
Detokenization
 ↓
Response
 ↓
Your application
 ↓
User
```

***1. User sends a prompt***

Suppose the user asks:

```text
Explain dependency injection in .NET
```

Your application might send something like:

```json
{
  "model": "some-llm-model",
  "messages": [
    {
      "role": "system",
      "content": "You are a helpful .NET assistant."
    },
    {
      "role": "user",
      "content": "Explain dependency injection in .NET"
    }
  ]
}
```

The model doesn't simply receive the English sentence and "understand" it like a human.

---

***2. Tokenization***

The text is converted into **tokens**.

For example, conceptually:

```text
"Explain dependency injection in .NET"
```

might become something like:

```text
["Explain", " dependency", " injection", " in", " .", "NET"]
```

The exact tokens depend on the tokenizer/model.

Each token is represented internally by a numerical ID:

```text
"Explain"       → 1234
"dependency"    → 5678
"injection"     → 9123
...
```

So the model receives numbers rather than raw text.

---

***3. Tokens become vectors***

The token IDs are mapped into numerical representations called **embeddings**.

Conceptually:

```text
Token
 ↓
Token ID
 ↓
Embedding vector
```

For example, conceptually:

```text
"dog"
 ↓
[0.21, -0.73, 0.45, ...]
```

The actual vectors have many dimensions.

These numerical representations allow the neural network to process relationships between tokens.

---

***4. Transformer processes the tokens***

Modern LLMs are generally based on the **Transformer architecture**.

The important concept here is:

**Attention**

The model looks at relationships between tokens.

For example:

```text
"The developer used dependency injection because it makes testing easier."
```

The model can determine relationships between:

```text
developer
   ↕
dependency injection
   ↕
testing
```

The key mechanism is **self-attention**.

---

***5. What does attention do?***

> Attention helps the model determine which other tokens are relevant when processing a particular token.

Suppose:

```text
"The server returned an error because it was unavailable."
```

When processing:

```text
"it"
```

the model needs to determine what "it" refers to.

Attention helps establish relationships between the tokens.

Technically, attention uses:

```text
Query (Q)
Key   (K)
Value (V)
```

and computes attention weights.

A simplified formula is:

```text
Attention(Q,K,V)
=
softmax(QKᵀ / √dₖ)V
```

You don't normally need to derive this in a .NET interview, but knowing the concept is useful.

---

***6. The model predicts the next token***

This is the most important concept.

Suppose the prompt is:

```text
"The capital of France is"
```

The model calculates probabilities for possible next tokens.

Conceptually:

```text
Paris      → 0.92
London     → 0.01
Berlin     → 0.01
Madrid     → 0.01
...
```

The model selects a token according to its decoding strategy.

So it produces:

```text
Paris
```

But it doesn't stop there.

---

***7. It predicts the next token again***

Now the context becomes:

```text
"The capital of France is Paris"
```

The model predicts the next token.

Conceptually:

```text
"."       → 0.70
"and"     → 0.10
"which"   → 0.05
...
```

It selects:

```text
.
```

Then continues.

This happens repeatedly:

```text
Prompt
 ↓
Predict token
 ↓
Add token to context
 ↓
Predict next token
 ↓
Add token
 ↓
Predict next token
 ↓
...
```

Eventually it reaches a stopping condition.

---

***8. Why does it look like the AI is writing a whole answer?***

Because this happens **many times extremely quickly**.

For example:

```text
Explain
 ↓
dependency
 ↓
injection
 ↓
is
 ↓
a
 ↓
design
 ↓
pattern
 ↓
...
```

The model generates a sequence of tokens.

That's why people often say:

> An autoregressive LLM generates the response token by token.

---

***9. What is the model actually doing?***

At a high level:

```text
Input tokens
     ↓
Transformer layers
     ↓
Probability distribution
     ↓
Choose next token
     ↓
Feed token back into context
     ↓
Repeat
```

The model has learned statistical patterns from its training data.

It isn't performing a traditional database search for every answer.

---

***10. Where does the model's knowledge come from?***

During **training**, the model processes huge amounts of training data.

Very simplified:

```text
Training data
     ↓
Tokenization
     ↓
Neural network
     ↓
Predict missing/next tokens
     ↓
Calculate error
     ↓
Adjust weights
     ↓
Repeat billions/trillions of times
```

The model learns numerical **weights/parameters** representing patterns learned during training.

When you ask a question later, it uses those learned parameters to generate a response.

---

***11. Training vs inference***

This distinction is very important for interviews.

**Training**

```text
Huge dataset
    ↓
Model
    ↓
Prediction
    ↓
Calculate error
    ↓
Update weights
    ↓
Repeat
```

The model's parameters are changed.

**Inference**

When you send a prompt:

```text
Prompt
 ↓
Already-trained model
 ↓
Generate response
```

The model's weights generally **aren't being retrained from your individual request**.

This process is called **inference**.

---

***12. Where does temperature come in?***

During generation, the model has probabilities.

For example:

```text
Option A → 70%
Option B → 20%
Option C → 10%
```

A decoding parameter such as **temperature** can affect how concentrated or diverse the sampling is.

Conceptually:

```text
Low temperature
→ more deterministic

Higher temperature
→ more variation
```

It doesn't mean:

> "Make the AI smarter."

It controls aspects of the token-selection distribution.

---

***13. What is context window?***

The model can't necessarily process unlimited conversation history.

It has a **context window**.

For example:

```text
System instructions
+
Conversation history
+
Current user message
+
Tool results
+
Generated response
```

all consume context.

Conceptually:

```text
┌──────────────────────────┐
│ Context Window           │
│                          │
│ System instructions      │
│ Previous messages        │
│ Current prompt           │
│ Retrieved documents      │
│ Tool results             │
└──────────────────────────┘
```

This is extremely important for applications using LLMs.

---

***14. How does an AI application use your database?***

The LLM itself doesn't automatically know your company's database.

Suppose your application has:

```text
Customer DB
   ↓
Orders
   ↓
Invoices
   ↓
Employees
```

You might build:

```text
User
 ↓
.NET API
 ↓
Retrieve relevant data
 ↓
Build context
 ↓
LLM
 ↓
Response
```

This is commonly associated with **RAG — Retrieval-Augmented Generation**.

---
***15. RAG example***

User asks:

> "What is our company's leave policy?"

Your company documents contain:

```text
LeavePolicy.pdf
EmployeeHandbook.pdf
HRPolicy.docx
```

Your system can do:

```text
User question
     ↓
Create embedding
     ↓
Vector search
     ↓
Find relevant documents
     ↓
Retrieve relevant chunks
     ↓
Build prompt
     ↓
LLM
     ↓
Answer
```

For example:

```text
Question:
"What is our maternity leave policy?"

Retrieved context:
"Maternity leave is 26 weeks..."
```

Then:

```text
Prompt to LLM:

Answer the question using the following
company policy:

[Maternity leave content]

Question:
What is our maternity leave policy?
```

The model generates the final answer based on the supplied context.

---

***16. Where does your .NET application fit?***

For your background, think about this architecture:

```text
                    ┌─────────────────┐
                    │ Angular / Client │
                    └────────┬────────┘
                             │
                             ↓
                    ┌─────────────────┐
                    │ .NET Web API    │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ↓              ↓              ↓
        Authentication    Business       RAG/Data
                          Logic           Retrieval
              │              │              │
              │              │              ↓
              │              │        Vector Database
              │              │              │
              │              └──────────────┘
              │
              ↓
         LLM API
              ↓
       Transformer Model
              ↓
       Token generation
              ↓
          Response
              ↓
          .NET API
              ↓
           Angular
```

---

***17. What happens when you call an LLM API?***

For example, conceptually:

```csharp
var response = await aiClient.GenerateAsync(
    "Explain dependency injection in .NET");
```

Behind the scenes:

```text
Your .NET application
        ↓
HTTPS request
        ↓
AI provider API
        ↓
Authentication/API key
        ↓
Request validation
        ↓
Tokenization
        ↓
Model inference
        ↓
Transformer computation
        ↓
Token probabilities
        ↓
Token selection
        ↓
Repeat
        ↓
Generated tokens
        ↓
HTTP response
        ↓
Your .NET application
```

---

***18. Streaming***

You may notice ChatGPT appears to generate text gradually.

That's **streaming**.

Instead of waiting for:

```text
Complete response
      ↓
Return everything
```

the server can send chunks:

```text
Token/chunk 1
     ↓
Token/chunk 2
     ↓
Token/chunk 3
     ↓
Token/chunk 4
     ↓
...
```

Your UI displays them progressively.

For a .NET application this could involve:

```text
LLM
 ↓
Streaming HTTP response
 ↓
.NET API
 ↓
SignalR / SSE / streaming response
 ↓
Angular
```
-----
-----
* Services provided by LLM which we can use in our AI application?

| Capability                  | What you use it for                   | Example                                |
| --------------------------- | ------------------------------------- | -------------------------------------- |
| **Text generation**         | Generate answers/content/code         | Chatbot, AI assistant                  |
| **Embeddings**              | Convert text into vectors             | RAG, semantic search                   |
| **Vision**                  | Understand images                     | OCR-like analysis, image understanding |
| **Audio / Speech-to-text**  | Convert voice → text                  | Voice assistant                        |
| **Text-to-speech**          | Text → voice                          | Voice assistant                        |
| **Structured output**       | Get JSON matching a schema            | Extract invoice/customer data          |
| **Tool / Function calling** | Let AI invoke your APIs/functions     | Check order status                     |
| **Moderation / Safety**     | Detect unsafe content                 | Content filtering                      |
| **Image generation**        | Generate images                       | Marketing/product images               |
| **Fine-tuning**             | Adapt a model to specialized examples | Domain-specific behavior               |

------------
----------

* Output for
```
for(int i=0;i<5;i++)
{
   Task.Run(() =>
   {
       Console.WriteLine(i);
   });
}
```
The output is nondeterministic.

In modern C#, the for loop variable i is captured by the lambda, and the tasks may execute after the loop has already changed i.

---------
---------