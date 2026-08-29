- async/await internals (Task, ValueTask)
- Task vs Thread vs Thread Pool
- ConfigureAwait(false) — when and why
- Deadlocks in async code
- CancellationToken
- Parallel.For, Parallel.ForEach
- lock, Monitor, Mutex, Semaphore, SemaphoreSlim
- ConcurrentDictionary and other thread-safe collections
- Channels (System.Threading.Channels)


## async/await internals (Task, ValueTask)

### Why do we need `async` / `await`?

Imagine an ASP.NET Core API:

```csharp
public async Task<IActionResult> GetEmployees()
{
    var employees = await db.Employees.ToListAsync();

    return Ok(employees);
}
```

The database might take:

```text
100 ms
```

During those 100 ms, the application doesn't need to continuously occupy a thread just waiting for the database.

Conceptually:

```text
Request
   ↓
Start DB operation
   ↓
Thread is free ──────────────┐
                             │
Database working             │
                             ↓
                       DB operation complete
                             ↓
                       Continue execution
```

That's the major benefit of async I/O.

---

### What does `async` actually mean?

When you write:

```csharp
public async Task<int> GetData()
{
    ...
}
```

`async` tells the compiler that the method contains asynchronous operations and may use `await`.

It does **not** mean:

> "Run this method on another thread."

🚨 This is a very important interview point.

### Wrong understanding:

```text
async = new thread ❌
```

### Better understanding:

```text
async/await
    ↓
non-blocking asynchronous operation
    ↓
especially useful for I/O
```

---

### What does `await` do?

Example:

```csharp
var employees = await db.Employees.ToListAsync();
```

Think of `await` as:

> **"If the operation isn't complete, pause this method and resume it when the operation completes."**

It doesn't mean:

> "Block the current thread until it finishes."

Conceptually:

```text
Method starts
    ↓
Start DB operation
    ↓
Is DB operation complete?
    │
    ├── YES → Continue
    │
    └── NO
         ↓
     Return control
         ↓
     DB completes
         ↓
     Resume method
```

---

### What happens internally? 
Consider:

```csharp
public async Task<string> GetDataAsync()
{
    var result = await GetFromDatabaseAsync();

    return result;
}
```

The compiler transforms the async method into a structure commonly described as a **state machine**.

Conceptually:

```text
GetDataAsync()
      ↓
State Machine
      ↓
Start operation
      ↓
Await
      ↓
Save current state
      ↓
Return incomplete Task
      ↓
Operation completes
      ↓
Resume state machine
      ↓
Continue after await
      ↓
Return result
```

You don't manually implement this state machine. The C# compiler generates the machinery for you.

---

### What is a `Task`?

A `Task` represents an **asynchronous operation**.

For example:

```csharp
Task<int>
```

means:

> "An asynchronous operation that will eventually produce an `int`."

Example:

```csharp
Task<int> task = GetEmployeeCountAsync();
```

You can think:

```text
Task<int>
   ↓
Operation in progress
   ↓
Eventually:
   ↓
int result
```

---

### `Task` vs Result

This is important.

```csharp
int result = GetEmployeeCount();
```

means:

```text
Call method
   ↓
Wait
   ↓
Get int
```

Whereas:

```csharp
Task<int> task = GetEmployeeCountAsync();
```

means:

```text
Start asynchronous operation
   ↓
Get Task representing the operation
   ↓
Operation completes later
   ↓
Result becomes available
```

Then:

```csharp
int result = await task;
```

---

### `Task` vs `Task<T>`

#### `Task`

For an operation that doesn't return a value:

```csharp
public async Task SaveAsync()
{
    await db.SaveChangesAsync();
}
```

#### `Task<T>`

For an operation that returns a value:

```csharp
public async Task<Employee> GetEmployeeAsync()
{
    return await db.Employees.FirstAsync();
}
```

So:

```text
Task
   ↓
No result


Task<T>
   ↓
Result of type T
```

---

### Does `await` Create a New Thread?

### No. ❌

This:

```csharp
await db.Employees.ToListAsync();
```

doesn't mean:

```text
Thread 1
   ↓
Create Thread 2
   ↓
Thread 2 → Database
```

For I/O operations, the OS/runtime can perform the asynchronous operation without keeping a .NET thread blocked waiting for the result.

This is why async is especially valuable in:

```text
ASP.NET Core
Database calls
HTTP calls
File I/O
Network I/O
```

---

### Async vs Parallelism

These are **not the same thing**.

### Async

Usually about:

> **Not blocking while waiting for an asynchronous operation.**

Example:

```csharp
var employee = await GetEmployeeAsync();
```

### Parallelism

About:

> **Doing multiple pieces of work at the same time.**

Example:

```csharp
Parallel.For(0, 100, i =>
{
    Process(i);
});
```

So:

```text
Async
→ Efficient waiting


Parallelism
→ Concurrent execution
```

You can have async without parallelism.

---

### Why is Async Important in ASP.NET Core?

Suppose 100 requests arrive.

Each request calls a database that takes 1 second.

#### Synchronous blocking:

```text
Request 1 → Thread blocked → DB
Request 2 → Thread blocked → DB
Request 3 → Thread blocked → DB
...
```

Threads are occupied waiting.

#### Async:

```text
Request 1 → DB async → thread available
Request 2 → DB async → thread available
Request 3 → DB async → thread available
...
```

When the database operations finish, execution resumes.

This allows the server to handle I/O-bound workloads more efficiently.

---

### 11. `async` Method Without `await`

You might see:

```csharp
public async Task<int> GetNumberAsync()
{
    return 10;
}
```

This produces a compiler warning because there is no asynchronous operation being awaited.

If the method doesn't need asynchronous work, don't unnecessarily mark it `async`.

You could simply return:

```csharp
public Task<int> GetNumberAsync()
{
    return Task.FromResult(10);
}
```

Though even that may be unnecessary if the method isn't part of an async abstraction.

---

### Why Should We Avoid `.Result` and `.Wait()`? 🚨

Bad:

```csharp
var employees = db.Employees
    .ToListAsync()
    .Result;
```

or:

```csharp
db.Employees
    .ToListAsync()
    .Wait();
```

These synchronously block the calling thread.

Better:

```csharp
var employees = await db.Employees
    .ToListAsync();
```

Think:

```text
.Result / .Wait()
       ↓
Block thread ❌


await
       ↓
Asynchronous waiting ✅
```

They can also contribute to deadlock problems in environments with synchronization contexts, particularly older UI/ASP.NET environments.

---

### `async` All the Way

Suppose:

```csharp
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

If the database operation is asynchronous, prefer propagating async through the call chain.

```csharp
public async Task<IActionResult> Get()
{
    var result = await service.GetAsync();

    return Ok(result);
}
```

Service:

```csharp
public async Task<List<Employee>> GetAsync()
{
    return await repository.GetAsync();
}
```

Repository:

```csharp
public async Task<List<Employee>> GetAsync()
{
    return await db.Employees.ToListAsync();
}
```

Conceptually:

```text
Controller
    ↓ await
Service
    ↓ await
Repository
    ↓ await
Database
```

This is often called:

> **Async all the way.**

---

### What is `ValueTask`? 
Now the more advanced part.

Normally:

```csharp
Task<int>
```

represents the asynchronous result.

`ValueTask<T>` is a value-type abstraction that can sometimes avoid allocating a new `Task<T>` when the result is **already available synchronously**.

Example:

```csharp
public ValueTask<int> GetValueAsync()
{
    return new ValueTask<int>(10);
}
```

The important idea:

```text
Task<T>
    ↓
Reference type
    ↓
May involve allocation


ValueTask<T>
    ↓
Value type
    ↓
Can represent an already-available result without
allocating a Task in some cases
```

---

### Why Does `ValueTask<T>` Exist?

Consider a method that frequently completes synchronously.

For example:

```csharp
public Task<int> GetCachedValueAsync()
{
    if (_cache.TryGetValue("count", out int value))
        return Task.FromResult(value);

    return LoadFromDatabaseAsync();
}
```

If the cache frequently contains the value, you may be repeatedly creating completed `Task<int>` instances.

`ValueTask<int>` can be useful for such APIs:

```csharp
public ValueTask<int> GetCachedValueAsync()
{
    if (_cache.TryGetValue("count", out int value))
        return new ValueTask<int>(value);

    return new ValueTask<int>(LoadFromDatabaseAsync());
}
```

The exact benefit depends on the workload.

---
### Interview answer:

> **"`Task` should generally be the default. `ValueTask` is useful in performance-sensitive scenarios where an operation frequently completes synchronously and avoiding task allocations provides a measurable benefit."**

---

### Final Mental Model

```text
                 async / await
                       │
             ┌─────────┴─────────┐
             ↓                   ↓
          I/O-bound           CPU-bound
             │                   │
             ↓                   ↓
        async/await        Parallelism
             │              / Task.Run
             ↓
       Don't block thread
             │
             ↓
          Task<T>
             │
             └──────→ ValueTask<T>
                       when a
                  performance case exists
```

### Remember these 7 points:

```text
1. async ≠ new thread

2. await ≠ blocking

3. Task represents an async operation

4. async/await is especially useful for I/O

5. Don't use Task.Run() just to make I/O async

6. Task is the default; ValueTask is specialized

7. Async and parallelism are different concepts
```

> **"`async/await` provides a non-blocking programming model, especially for I/O-bound operations. It doesn't inherently create a new thread; the compiler uses a state machine to suspend and later resume the method around incomplete awaits."**

---

## Task vs Thread vs ThreadPool

The easiest way to understand it:

> **Thread = worker**

> **ThreadPool = collection of reusable workers**

> **Task = work you want to get done**

### Thread

A `Thread` represents an actual OS/runtime execution thread.

Example:

```csharp
var thread = new Thread(() =>
{
    Console.WriteLine("Running...");
});

thread.Start();
```

Conceptually:

```text
Your Application
      ↓
   Thread
      ↓
 Executes code
```

A thread has its own:

* Stack
* Execution state
* Scheduling overhead
* Resources

Creating lots of dedicated threads can be expensive.

---

### ThreadPool 

.NET maintains a pool of reusable worker threads.

Instead of creating a new thread every time:

```text
Request 1 → Thread
Request 2 → Thread
Request 3 → Thread
```

.NET can reuse existing ThreadPool threads:

```text
             ThreadPool
        ┌──────┬──────┬──────┐
        ↓      ↓      ↓      ↓
      T1       T2     T3     T4
        ↑      ↑
      Work   Work
```

When work finishes:

```text
Thread
  ↓
Work complete
  ↓
Return to pool
  ↓
Available for another work item
```

This avoids repeatedly creating and destroying threads.

---

### Task

A `Task` represents an operation/work item.

Example:

```csharp
Task task = Task.Run(() =>
{
    DoSomeWork();
});
```

Think:

```text
Task
 ↓
"I need this work executed"
 ↓
Scheduler
 ↓
ThreadPool thread
 ↓
Execute work
```

Important:

> **A Task is not a Thread.**

A Task is an abstraction representing work or an asynchronous operation.

---

### Task for CPU-Bound Work

Consider:

```csharp
Task.Run(() =>
{
    CalculateHugeNumber();
});
```

This is CPU-bound work.

Typically:

```text
Task
 ↓
ThreadPool
 ↓
Worker thread
 ↓
CPU executes calculation
```

So:

```text
CPU-bound
   ↓
Task.Run()
   ↓
ThreadPool
```

can be appropriate in the right scenario.

---

### Task for I/O-Bound Work

Suppose:

```csharp
var response =
    await httpClient.GetAsync(url);
```

or:

```csharp
var employees =
    await db.Employees.ToListAsync();
```

Don't normally do:

```csharp
await Task.Run(async () =>
{
    return await db.Employees.ToListAsync();
});
```

That's unnecessary.

Prefer:

```csharp
var employees =
    await db.Employees.ToListAsync();
```

because EF Core already provides asynchronous database operations.

---

### Thread vs ThreadPool

|              | Thread                        | ThreadPool                   |
| ------------ | ----------------------------- | ---------------------------- |
| Worker       | Dedicated thread              | Reusable thread              |
| Creation     | You create/manage it          | .NET manages pool            |
| Overhead     | Higher                        | Lower for typical short work |
| Reuse        | ❌                             | ✅                            |
| Suitable for | Specialized long-running work | General background/CPU work  |
| Management   | Manual                        | Runtime managed              |


### Thread vs Task

|                   | Thread                    | Task                         |
| ----------------- | ------------------------- | ---------------------------- |
| Represents        | Execution thread          | Unit of work/async operation |
| Abstraction       | Lower-level               | Higher-level                 |
| Resource          | Actual execution resource | Abstraction                  |
| Return value      | Manual handling           | `Task<T>`                    |
| Composition       | Harder                    | Easy                         |
| Cancellation      | More manual               | `CancellationToken`          |
| Usually preferred | ❌                         | ✅                            |

For normal application development:

> **Prefer Tasks over manually creating Threads.**

---

## `ConfigureAwait(false)` 

The simplest explanation:

> **`ConfigureAwait(false)` tells the awaiter: "After this await completes, I don't need to resume on the captured synchronization context."**

But there's an important modern .NET/ASP.NET Core nuance:

> **In ASP.NET Core, there is normally no custom `SynchronizationContext`, so `ConfigureAwait(false)` is usually not necessary for application code.**


### First: What is a SynchronizationContext?

A `SynchronizationContext` provides a way for code to say:

> "When my asynchronous operation finishes, resume me on this particular context."

This was especially important in older application models such as:

```text
UI applications
├── WPF
└── WinForms

Older ASP.NET
```

For example, imagine a UI thread:

```text
UI Thread
   ↓
Button click
   ↓
await network call
   ↓
UI thread becomes available
   ↓
network call completes
   ↓
Continue on UI thread
```

Why?

Because UI controls generally need to be accessed from the UI thread.

---

### Example Without `ConfigureAwait(false)`

```csharp
public async Task LoadDataAsync()
{
    var data = await GetDataAsync();

    UpdateUI(data);
}
```

In an environment with a synchronization context:

```text
Before await
     ↓
UI Context
     ↓
await
     ↓
Operation completes
     ↓
Resume on UI Context
     ↓
UpdateUI()
```

This is useful because:

```csharp
UpdateUI(data);
```

needs to execute on the UI thread.

---

### What Does `ConfigureAwait(false)` Do?

```csharp
var data = await GetDataAsync()
    .ConfigureAwait(false);
```

You're saying:

> **"Don't capture the current synchronization context for the continuation."**

Conceptually:

```text
Before await
     ↓
UI Context
     ↓
await
     ↓
Operation completes
     ↓
Don't require UI Context
     ↓
Continue wherever appropriate
```

So:

```csharp
await something;
```

vs:

```csharp
await something.ConfigureAwait(false);
```

The important difference is **context capture**, not whether the operation itself becomes asynchronous.

---

### Does `ConfigureAwait(false)` Create a New Thread?

### No. ❌

This is a common interview trap.

It does **not** mean:

```text
ConfigureAwait(false)
       ↓
Create new thread ❌
```

It means:

```text
ConfigureAwait(false)
       ↓
Don't capture SynchronizationContext
```

---

### Why Was It Important in Older ASP.NET?

Older ASP.NET applications had a synchronization context associated with requests.

Imagine:

```text
Request Thread
     ↓
await
     ↓
Database call
     ↓
Try to resume on captured context
```

In certain situations, blocking code such as:

```csharp
var result = GetDataAsync().Result;
```

could create a deadlock.

Conceptually:

```text
Request Context
      ↓
Waiting for async operation
      ↓
Blocks
      ↓
Async operation tries to resume
      ↓
Needs blocked context
      ↓
DEADLOCK 🚨
```

Using:

```csharp
.ConfigureAwait(false)
```

could prevent that particular context-capture issue.

---

### Classic Deadlock Example 

Consider an environment with a synchronization context:

```csharp
public string GetData()
{
    return GetDataAsync().Result;
}

public async Task<string> GetDataAsync()
{
    var result = await GetFromDatabaseAsync();

    return result;
}
```

Potential sequence:

```text
1. UI/request thread calls GetDataAsync()

2. GetDataAsync() starts DB operation

3. await pauses the method

4. Caller uses .Result

5. Caller thread is BLOCKED

6. DB operation completes

7. Continuation wants to resume on
   captured context

8. Context/thread is blocked by .Result

9. DEADLOCK 🚨
```

This is one of the classic reasons `ConfigureAwait(false)` became important.

---

###   How `ConfigureAwait(false)` Changes It

```csharp
public async Task<string> GetDataAsync()
{
    var result = await GetFromDatabaseAsync()
        .ConfigureAwait(false);

    return result;
}
```

Now:

```text
await
 ↓
Don't capture SynchronizationContext
 ↓
Operation completes
 ↓
Continuation doesn't need that context
```

This can avoid that specific synchronization-context deadlock.

### But the better solution is:

Don't block in the first place.

Instead of:

```csharp
GetDataAsync().Result;
```

use:

```csharp
await GetDataAsync();
```

---

### What About ASP.NET Core? 

This is where interviews often become tricky.

In modern ASP.NET Core:

```text
ASP.NET Core
      ↓
No custom SynchronizationContext
      ↓
Normally no request SynchronizationContext
      ↓
ConfigureAwait(false) usually doesn't change
      ↓
Continuation doesn't need to return to a request context
```

Therefore, this:

```csharp
var result = await service.GetAsync();
```

is generally fine.

You don't normally need:

```csharp
var result = await service.GetAsync()
    .ConfigureAwait(false);
```

in every ASP.NET Core method.

---

### Should I Use `ConfigureAwait(false)` in ASP.NET Core?

### Application code

Usually:

```csharp
await service.GetAsync();
```

is perfectly fine.

You generally don't need to write:

```csharp
await service.GetAsync()
    .ConfigureAwait(false);
```

everywhere.

---

### Where Is `ConfigureAwait(false)` More Useful?

It's particularly relevant when writing:

### Reusable libraries

For example:

```text
NuGet package
Shared class library
Infrastructure library
SDK
Reusable component
```

A library shouldn't generally assume:

> "I need to resume on whatever application's synchronization context happens to exist."

So library code often uses:

```csharp
await operation.ConfigureAwait(false);
```

when it doesn't need the caller's context.

---

### Library Example

Suppose you're writing:

```csharp
public class PaymentClient
{
    public async Task<PaymentResult> ProcessAsync()
    {
        var response = await _httpClient
            .PostAsync(...);

        return ParseResponse(response);
    }
}
```

If the library doesn't need to resume on a particular context, you can write:

```csharp
public async Task<PaymentResult> ProcessAsync()
{
    var response = await _httpClient
        .PostAsync(...)
        .ConfigureAwait(false);

    return ParseResponse(response);
}
```

This communicates:

> **"The continuation doesn't depend on the caller's context."**

---

### What Does "Continuation" Mean?

This is an important word for interviews.

Consider:

```csharp
var result = await GetDataAsync();

Console.WriteLine(result);
```

The code after `await`:

```csharp
Console.WriteLine(result);
```

is effectively the **continuation**.

Conceptually:

```text
GetDataAsync()
      ↓
await
      ↓
[operation completes]
      ↓
Continuation
      ↓
Console.WriteLine()
```

`ConfigureAwait(false)` affects where/how that continuation is scheduled with respect to synchronization-context capture.

---

### `ConfigureAwait(false)` Doesn't Make I/O Faster

Another common misconception:

```text
ConfigureAwait(false)
      ↓
Database query becomes faster ❌
```

No.

It mainly avoids synchronization-context capture.

Any performance benefit is generally from reducing unnecessary context-related overhead, not from making the database/network operation itself faster.

---

### Does `ConfigureAwait(false)` Mean "Run on ThreadPool"?

Not exactly.

Another interview trap:

> "`ConfigureAwait(false)` means resume on ThreadPool."

That's too simplistic.

Better:

> **"`ConfigureAwait(false)` prevents the continuation from requiring the captured synchronization context. The continuation can then run according to the task scheduler/awaiter without being forced back to that context."**

In many practical cases this may involve a ThreadPool thread, but that's an implementation detail rather than the definition of `ConfigureAwait(false)`.

---

### `ConfigureAwait(false)` vs `Task.Run()`

Don't confuse these.

### `ConfigureAwait(false)`

Controls:

```text
Synchronization-context capture
```

Example:

```csharp
await operation.ConfigureAwait(false);
```

### `Task.Run()`

Schedules work to the ThreadPool:

```csharp
await Task.Run(() => CpuHeavyWork());
```

So:

```text
ConfigureAwait(false)
→ Context behavior


Task.Run()
→ ThreadPool scheduling
```

Very different purposes.

---

## Deadlocks in Async Code

> **A deadlock happens when two pieces of code are waiting for each other to make progress, so neither can continue.**

### The Simplest Deadlock Example

Imagine:

```text
Person A → waiting for Person B
Person B → waiting for Person A
```

Neither can proceed.

```text
A ──────waits for──────→ B
↑                       │
└──────waits for────────┘

          DEADLOCK
```

---

### Classic Async Deadlock 

Consider:

```csharp
public string GetData()
{
    return GetDataAsync().Result;
}

public async Task<string> GetDataAsync()
{
    var data = await GetFromDatabaseAsync();

    return data;
}
```

The dangerous part is:

```csharp
.Result
```

---

### How Does the Deadlock Happen?

This is easiest to understand in an environment with a `SynchronizationContext`, such as older ASP.NET or UI applications.

Suppose the request/UI thread starts:

```csharp
GetDataAsync().Result
```

### Step 1

`GetDataAsync()` starts.

```text
Request/UI Thread
       ↓
GetDataAsync()
```

---

### Step 2

It reaches:

```csharp
await GetFromDatabaseAsync();
```

The database operation isn't finished yet.

So the method pauses.

```text
GetDataAsync()
       ↓
await
       ↓
DB operation running
```

---

### Step 3

But the caller is doing:

```csharp
.Result
```

So the original thread is now blocked:

```text
Request/UI Thread
       ↓
BLOCKED waiting for Task
```

---

### Step 4

The database operation finishes.

The async method wants to continue after:

```csharp
await
```

Because the environment captured the synchronization context, the continuation tries to return to that context.

```text
DB complete
    ↓
Continuation
    ↓
Needs captured context
```

But...

```text
Captured context
       ↓
Thread is BLOCKED by .Result
```

So:

```text
Thread waits for Task
Task waits for Thread
```
---

### Does This Deadlock Happen in ASP.NET Core?

In **modern ASP.NET Core**, there normally isn't the classic request `SynchronizationContext` that existed in older ASP.NET.

Therefore, the classic:

```csharp
.Result
```

deadlock scenario is **less likely to occur in ASP.NET Core** in exactly the same way.

But that does **not** mean `.Result` is good.

It can still:

* Block ThreadPool threads
* Reduce scalability
* Cause ThreadPool starvation
* Create other synchronization problems
* Make async code harder to reason about

So the recommendation remains:

> **Don't synchronously block on asynchronous operations.**

---

### Deadlock vs ThreadPool Starvation 

These are frequently confused.

#### Deadlock

Two or more operations are waiting on each other.

```text
A → waiting for B
B → waiting for A
```

Nobody progresses.

---

#### ThreadPool Starvation

Too many ThreadPool threads are blocked.

```text
Thread 1 → blocked
Thread 2 → blocked
Thread 3 → blocked
Thread 4 → blocked
...
```

New work can't get an available worker quickly enough.

```text
Requests
   ↓
Blocking code
   ↓
ThreadPool threads occupied
   ↓
New requests wait
   ↓
Poor performance
```

So:

> **Deadlock = circular waiting**

> **Starvation = workers are exhausted/occupied**

---

### Classic Multithreading Deadlock

Deadlocks aren't limited to async code.

Consider:

```csharp
lock (lockA)
{
    lock (lockB)
    {
        // Work
    }
}
```

Another thread:

```csharp
lock (lockB)
{
    lock (lockA)
    {
        // Work
    }
}
```

Now:

```text
Thread 1:
holds A → waits for B

Thread 2:
holds B → waits for A
```

```text
       A                    B
       ↑                    ↑
       │                    │
Thread 1 ──holds──→ A       │
       │                    │
       └──waits────────────→ B

Thread 2 ──holds──→ B
       │
       └──waits────────────→ A
```

💥 Deadlock.

---

### Four Conditions for a Classic Deadlock 

A deadlock traditionally requires four conditions:

### 1. Mutual Exclusion

A resource can only be used by one thread at a time.

```text
Thread A → Resource
Thread B → Waiting
```

---

### 2. Hold and Wait

A thread holds one resource while waiting for another.

```text
Thread A
  ↓
Holds Lock A
  ↓
Waiting for Lock B
```

---

### 3. No Preemption

The resource can't simply be forcibly taken away.

---

### 4. Circular Wait

There is a cycle:

```text
A waits for B
B waits for C
C waits for A
```

Together:

```text
Mutual exclusion
       +
Hold and wait
       +
No preemption
       +
Circular wait
       ↓
Potential DEADLOCK
```

This is a good theory question for interviews.

---

## `CancellationToken`

> **`CancellationToken` allows you to request that an ongoing operation stop gracefully.**

The key word is **request**.

It does **not forcibly kill** the operation.

---

### Why do we need Cancellation?

Imagine an API:

```text
Client
  ↓
GET /api/employees
  ↓
Database query
  ↓
100,000 records...
```

But the user closes the browser after 1 second.

Without cancellation:

```text
User gone ❌
     ↓
API still running
     ↓
Database still working
     ↓
CPU / DB resources wasted
```

With cancellation:

```text
User closes browser
       ↓
Cancellation requested
       ↓
API notices cancellation
       ↓
DB query cancelled
       ↓
Resources released
```

This is particularly useful for long-running operations.

---

### The Three Main Components

You mainly work with:

```text
CancellationTokenSource
          ↓
   creates/manages
          ↓
CancellationToken
          ↓
 passed to operation
```

Think of it like this:

### `CancellationTokenSource`

The **controller** of cancellation.

### `CancellationToken`

The **signal** that tells an operation:

> "Cancellation has been requested."

---

### Basic Example

```csharp
var cts = new CancellationTokenSource();

CancellationToken token = cts.Token;
```

Then:

```csharp
await DoWorkAsync(token);
```

Somewhere else:

```csharp
cts.Cancel();
```

Flow:

```text
CancellationTokenSource
        │
        │ Cancel()
        ↓
CancellationToken
        │
        ↓
Operation notices cancellation
        │
        ↓
Stops gracefully
```

---

### Important: Cancellation Is Cooperative

This is the most important concept.

Calling:

```csharp
cts.Cancel();
```

doesn't automatically kill your method.

It simply says:

> **"Please cancel."**

The operation must cooperate.

Example:

```csharp
public async Task DoWorkAsync(CancellationToken token)
{
    while (true)
    {
        token.ThrowIfCancellationRequested();

        await DoSomethingAsync(token);
    }
}
```

The operation checks the token and stops when cancellation is requested.

---

### `ThrowIfCancellationRequested()`

This is commonly used:

```csharp
token.ThrowIfCancellationRequested();
```

If cancellation hasn't been requested:

```text
Continue
```

If cancellation has been requested:

```text
OperationCanceledException
```

Conceptually:

```text
Token
 │
 ├── Not cancelled → continue
 │
 └── Cancelled
       ↓
OperationCanceledException
```

---

### `IsCancellationRequested`

You can also check:

```csharp
if (token.IsCancellationRequested)
{
    return;
}
```

Example:

```csharp
public async Task ProcessAsync(CancellationToken token)
{
    for (int i = 0; i < 1000; i++)
    {
        if (token.IsCancellationRequested)
            return;

        await ProcessItemAsync(i);
    }
}
```

Difference:

```csharp
token.IsCancellationRequested
```

checks the status.

```csharp
token.ThrowIfCancellationRequested()
```

checks and throws `OperationCanceledException`.

---

### Passing CancellationToken to Async APIs

The most useful pattern is:

```csharp
await SomeOperationAsync(token);
```

For example, with EF Core:

```csharp
var employees = await db.Employees
    .ToListAsync(token);
```

Now if cancellation is requested, EF Core can attempt to cancel the database operation.

Similarly with HTTP:

```csharp
var response = await httpClient
    .GetAsync(url, token);
```

This is much better than simply checking cancellation yourself after the operation has already completed.

---

### ASP.NET Core Example 

This is extremely useful for Web API interviews.

ASP.NET Core can provide a request cancellation token:

```csharp
[HttpGet]
public async Task<IActionResult> GetEmployees(
    CancellationToken cancellationToken)
{
    var employees = await _db.Employees
        .ToListAsync(cancellationToken);

    return Ok(employees);
}
```

You don't have to manually create the token.

ASP.NET Core can bind the request's cancellation signal to the `CancellationToken` parameter.

Conceptually:

```text
Client
  ↓
HTTP Request
  ↓
Controller
  ↓
CancellationToken
  ↓
Service
  ↓
EF Core
  ↓
Database
```

If the request is aborted, cancellation can propagate down the call chain.

---

### Cancellation Should Flow Through Your Layers

Suppose you have:

```text
Controller
   ↓
Service
   ↓
Repository
   ↓
EF Core
```

Don't stop the token at the controller.

### Controller

```csharp
public async Task<IActionResult> Get(
    CancellationToken cancellationToken)
{
    var result = await _service.GetAsync(
        cancellationToken);

    return Ok(result);
}
```

### Service

```csharp
public async Task<List<Employee>> GetAsync(
    CancellationToken cancellationToken)
{
    return await _repository.GetAsync(
        cancellationToken);
}
```

### Repository

```csharp
public async Task<List<Employee>> GetAsync(
    CancellationToken cancellationToken)
{
    return await _db.Employees
        .ToListAsync(cancellationToken);
}
```

This is:

> **Cancellation propagation.**

---

## `Parallel.For` / `Parallel.ForEach`

The easiest way to remember:

> **`Task.WhenAll` → run multiple async operations concurrently**

> **`Parallel.ForEach` → process multiple items in parallel using multiple threads**

---

### What is Parallel Processing?

Suppose you have:

```text
1, 2, 3, 4, 5, 6
```

and each item requires expensive CPU processing.

#### Sequential

```text
Thread
  ↓
1 → 2 → 3 → 4 → 5 → 6
```

#### Parallel

```text
Thread 1 → 1 → 3
Thread 2 → 2 → 5
Thread 3 → 4
Thread 4 → 6
```

Multiple items can be processed concurrently.

---

### `Parallel.For`

Example:

```csharp
Parallel.For(0, 10, i =>
{
    Console.WriteLine(i);
});
```

This processes iterations in parallel where beneficial.

Conceptually:

```text
0 ──┐
1 ──┤
2 ──┤
3 ──┼──→ ThreadPool → CPU
4 ──┤
5 ──┤
...
```

The runtime decides how to partition and schedule the work.

---

### `Parallel.ForEach`

If you already have a collection:

```csharp
var numbers = Enumerable.Range(1, 100);

Parallel.ForEach(numbers, number =>
{
    Process(number);
});
```

This is generally useful when each iteration is independent and CPU-intensive.

---

### `Parallel.For` vs `Parallel.ForEach`

#### `Parallel.For`

Works with a numeric range:

```csharp
Parallel.For(0, 100, i =>
{
    Process(i);
});
```

#### `Parallel.ForEach`

Works with a collection:

```csharp
Parallel.ForEach(employees, employee =>
{
    Process(employee);
});
```

Simple:

```text
For
→ numbers/range

ForEach
→ collection
```

---

### Why Use Parallel Instead of Normal `foreach`?

Normal:

```csharp
foreach (var item in items)
{
    Process(item);
}
```

is sequential.

```text
1 → 2 → 3 → 4 → 5
```

Parallel:

```csharp
Parallel.ForEach(items, item =>
{
    Process(item);
});
```

can execute multiple iterations concurrently:

```text
1 ─┐
2 ─┤
3 ─┼→ Concurrent processing
4 ─┤
5 ─┘
```

Potential benefit:

> **Reduced elapsed time for CPU-bound independent work.**

---

### CPU-Bound vs I/O-Bound

This is the most important thing to understand.

### CPU-bound

Examples:

```text
Image processing
Encryption
Compression
Large calculations
Data transformation
Mathematical calculations
```

Parallel processing can help:

```csharp
Parallel.ForEach(items, item =>
{
    PerformCpuHeavyOperation(item);
});
```

---

### I/O-bound

Examples:

```text
Database
HTTP API
File/network I/O
```

Don't automatically use:

```csharp
Parallel.ForEach(...)
```

for these.

Prefer asynchronous APIs:

```csharp
await ...
```

or controlled async concurrency.

---

### `Parallel.ForEach` vs `Task.WhenAll`

This is a **very common interview question**.

Suppose you need to call 100 APIs.

### `Task.WhenAll`

```csharp
var tasks = urls.Select(url =>
    httpClient.GetAsync(url));

var responses = await Task.WhenAll(tasks);
```

Good fit:

```text
HTTP calls
   ↓
I/O-bound
   ↓
Task.WhenAll
```

---

### `Parallel.ForEach`

```csharp
Parallel.ForEach(urls, url =>
{
    httpClient.GetAsync(url).Wait();
});
```

🚨 Don't do this.

You're mixing synchronous parallelism with asynchronous I/O.

---

### Modern Async Parallelism

For async operations, .NET provides:

```csharp
Parallel.ForEachAsync
```

Example:

```csharp
await Parallel.ForEachAsync(
    urls,
    async (url, cancellationToken) =>
    {
        var response = await httpClient.GetAsync(
            url,
            cancellationToken);

        await ProcessResponseAsync(response);
    });
```

Now:

```text
Multiple items
      ↓
Async operations
      ↓
Controlled concurrency
```

---

### Why Limit Parallelism?

Suppose:

```text
10,000 HTTP requests
```

and you launch all simultaneously.

You could overwhelm:

```text
Your application
      ↓
Network
      ↓
External API
      ↓
Database
```

Possible problems:

* Rate limits
* Connection exhaustion
* High memory usage
* Increased latency
* Service overload
* Throttling

So controlled concurrency is often better.

---

### Thread Safety

This is one of the biggest mistakes with parallel code.

Suppose:

```csharp
int total = 0;

Parallel.ForEach(numbers, number =>
{
    total += number;
});
```

🚨 This is unsafe.

Multiple threads can modify:

```text
total
```

at the same time.

You have a race condition.

---

### Use `Interlocked`

For simple atomic operations:

```csharp
int total = 0;

Parallel.ForEach(numbers, number =>
{
    Interlocked.Add(ref total, number);
});
```

Now the update is atomic.

---

### Or Use Thread-Safe Collections

Instead of:

```csharp
var results = new List<int>();
```

inside parallel code:

```csharp
Parallel.ForEach(items, item =>
{
    results.Add(Process(item));
});
```

🚨 `List<T>` isn't thread-safe for concurrent writes.

Consider:

```csharp
ConcurrentBag<int> results = new();
```

Then:

```csharp
Parallel.ForEach(items, item =>
{
    results.Add(Process(item));
});
```

---
## Thread Synchronization 

When multiple threads/tasks access the **same shared data**, we can get problems such as **race conditions**.

.NET provides several synchronization mechanisms:

```text
lock
Monitor
Mutex
SemaphoreSlim
Interlocked
Concurrent Collections
```

The key is knowing **when to use which one**.

---

### First: What is a Race Condition?

Suppose:

```csharp
int counter = 0;
```

Two threads do:

```csharp
counter++;
```

It looks like one operation, but internally it is roughly:

```text
Read counter
   ↓
Add 1
   ↓
Write counter
```

Imagine:

```text
Thread 1              Thread 2

Read 0                Read 0
  ↓                     ↓
Add 1                 Add 1
  ↓                     ↓
Write 1               Write 1
```

Expected:

```text
2
```

Actual:

```text
1
```

That's a:

> **Race condition**

Why Does This Happen?

Because multiple threads are accessing **shared mutable state** concurrently.

```text
             Shared Data
                  ↑
             ┌────┴────┐
             │         │
          Thread 1  Thread 2
             │         │
             └──access─┘
```

We need synchronization when operations aren't safely concurrent.

---

### `lock` 

The simplest synchronization mechanism in C# is:

```csharp
private readonly object _lock = new();

lock (_lock)
{
    counter++;
}
```

It means:

> **Only one thread at a time can execute this critical section for that lock object.**

Conceptually:

```text
Thread 1 → 🔒 → executes
Thread 2 → waits
Thread 3 → waits

Thread 1 → 🔓

Thread 2 → 🔒 → executes
```

--> Critical Section

The code protected by a lock is called the **critical section**.

```csharp
lock (_lock)
{
    // Critical section
    counter++;
}
```

The goal is to protect access to shared state.

--> Important: What Should You Lock On?

Prefer a private object:

```csharp
private readonly object _lock = new();
```

Then:

```csharp
lock (_lock)
{
    // ...
}
```

Avoid:

```csharp
lock (this)
```

and especially:

```csharp
lock (typeof(MyClass))
```

because other code could also lock the same object, causing unexpected contention or deadlocks.

--> How `lock` Works Internally

Conceptually, `lock` is based on `Monitor`.

This:

```csharp
lock (_lock)
{
    DoWork();
}
```

is conceptually similar to:

```csharp
Monitor.Enter(_lock);

try
{
    DoWork();
}
finally
{
    Monitor.Exit(_lock);
}
```

The important part is:

```csharp
finally
{
    Monitor.Exit(_lock);
}
```

The lock must be released even if an exception occurs.

---

### `Monitor`

`Monitor` provides more control than `lock`.

Example:

```csharp
Monitor.Enter(_lock);

try
{
    DoWork();
}
finally
{
    Monitor.Exit(_lock);
}
```

You can also use:

```csharp
Monitor.TryEnter(...)
```

This allows you to attempt to acquire a lock without waiting indefinitely.

Example:

```csharp
if (Monitor.TryEnter(_lock, TimeSpan.FromSeconds(1)))
{
    try
    {
        DoWork();
    }
    finally
    {
        Monitor.Exit(_lock);
    }
}
else
{
    Console.WriteLine("Could not acquire lock");
}
```

### Interview answer:

> **"`lock` is a convenient C# syntax around Monitor. Monitor provides additional capabilities such as TryEnter and explicit control over entering and exiting the critical section."**

---

# 8. `lock` vs `Monitor`

|                          | `lock`     | `Monitor`   |
| ------------------------ | ---------- | ----------- |
| Easy to use              | ⭐⭐⭐        | ⭐⭐          |
| Based on                 | Monitor    | —           |
| Automatic release        | ✅          | Manual      |
| `TryEnter`               | ❌ directly | ✅           |
| Typical application code | ✅          | Less common |

Usually:

> **Use `lock` unless you specifically need Monitor functionality.**

---

### `Mutex`

A `Mutex` is another synchronization primitive.

Example:

```csharp
using var mutex = new Mutex();

mutex.WaitOne();

try
{
    DoWork();
}
finally
{
    mutex.ReleaseMutex();
}
```

The important distinction:

> **A Mutex can provide synchronization across processes.**

For example:

```text
Application A
     ↓
     Mutex
     ↑
Application B
```

Whereas `lock` is generally used for synchronization within a process.

---

### `lock` vs `Mutex`

|               | `lock`                 | `Mutex`                       |
| ------------- | ---------------------- | ----------------------------- |
| Same process  | ✅                      | ✅                             |
| Cross-process | ❌                      | ✅                             |
| Performance   | Generally faster       | More overhead                 |
| Typical use   | Thread synchronization | Cross-process synchronization |

So if you simply need:

> "Only one thread in my application can access this."

Use:

```csharp
lock
```

If you need:

> "Only one process/application instance can access this resource."

A named `Mutex` may be appropriate.

---

### `SemaphoreSlim` 

`SemaphoreSlim` is extremely important in modern async programming.

Suppose you want:

> **Maximum 3 operations at the same time.**

```csharp
private readonly SemaphoreSlim _semaphore = new(3, 3);
```

Then:

```csharp
await _semaphore.WaitAsync();

try
{
    await DoWorkAsync();
}
finally
{
    _semaphore.Release();
}
```

Conceptually:

```text
Semaphore capacity = 3

Operation 1 → allowed
Operation 2 → allowed
Operation 3 → allowed
Operation 4 → waits
Operation 5 → waits
```

When one finishes:

```text
Operation 1 → Release()
               ↓
Operation 4 → allowed
```

---

# 12. Why `SemaphoreSlim` Is Important with Async

Remember:

```text
lock
 ↓
Synchronous
```

You cannot normally do:

```csharp
lock (_lock)
{
    await DoWorkAsync(); // ❌
}
```

Instead:

```csharp
await _semaphore.WaitAsync();

try
{
    await DoWorkAsync();
}
finally
{
    _semaphore.Release();
}
```

This allows asynchronous waiting.

---

### `SemaphoreSlim(1, 1)` = Async Lock

This is a very useful mental model.

```csharp
private readonly SemaphoreSlim _semaphore = new(1, 1);
```

Only one operation can enter:

```text
Operation A → 🔒
Operation B → waits
Operation C → waits
```

So:

```text
SemaphoreSlim(1,1)
        ↓
One-at-a-time access
        ↓
Async-compatible
```

---

### Semaphore vs Mutex

A semaphore can allow **multiple** callers at the same time.

Example:

```csharp
new SemaphoreSlim(5, 5);
```

Up to 5 operations can enter.

A mutex generally allows one owner at a time.

```text
Mutex
→ 1

Semaphore
→ N
```

---

### `Interlocked` 
For very simple atomic operations, `Interlocked` is often better than a lock.

Example:

```csharp
int counter = 0;

Interlocked.Increment(ref counter);
```

Instead of:

```csharp
lock (_lock)
{
    counter++;
}
```

`Interlocked` provides atomic operations such as:

```csharp
Interlocked.Increment(ref counter);
Interlocked.Decrement(ref counter);
Interlocked.Add(ref counter, 10);
Interlocked.Exchange(ref counter, 100);
Interlocked.CompareExchange(...);
```

---

### Why Use `Interlocked`?

If your operation is simply:

```text
increment
decrement
add
exchange
compare-and-swap
```

you often don't need a full lock.

Example:

```csharp
Interlocked.Increment(ref _requestCount);
```

This is designed for atomic operations.

---

### `Interlocked` vs `lock`

### `Interlocked`

```csharp
Interlocked.Increment(ref counter);
```

Good for:

```text
Simple atomic state changes
```

### `lock`

```csharp
lock (_lock)
{
    counter++;
    UpdateSomethingElse();
}
```

Good when you need to protect:

```text
Multiple operations
Multiple fields
A larger critical section
```

Think:

```text
Simple atomic operation
        ↓
Interlocked

Complex critical section
        ↓
lock
```

---

### Concurrent Collections

.NET provides thread-safe collections such as:

```csharp
ConcurrentDictionary<TKey,TValue>
ConcurrentQueue<T>
ConcurrentStack<T>
ConcurrentBag<T>
```

For example:

```csharp
var dictionary =
    new ConcurrentDictionary<int, string>();
```

Multiple threads can safely access it without you manually putting a `lock` around every operation.

Example:

```csharp
dictionary.TryAdd(1, "Swapnil");
```

---

### Why Use Concurrent Collections?

Instead of:

```csharp
lock (_lock)
{
    dictionary.Add(key, value);
}
```

you can often use:

```csharp
_concurrentDictionary.TryAdd(key, value);
```

The collection handles the necessary synchronization internally.

This can make concurrent code simpler and safer.

---


### Real-World Example

Suppose you have an API that maintains:

```csharp
private int _requestCount;
```

Multiple requests can arrive simultaneously.

Use:

```csharp
Interlocked.Increment(ref _requestCount);
```

Simple and efficient.

---

Now suppose you have:

```csharp
private readonly Dictionary<int, Employee> _employees;
```

and multiple threads modify it.

A normal `Dictionary` isn't safe for concurrent writes.

You could use:

```csharp
ConcurrentDictionary<int, Employee>
```

or protect access with:

```csharp
lock
```

---

Now suppose:

> Only 10 external API calls should execute concurrently.

Use:

```csharp
SemaphoreSlim(10, 10)
```

---

Now suppose:

> Only one application instance should perform a system-wide operation.

A named:

```csharp
Mutex
```

may be appropriate.

---

### Don't Hold Locks During I/O 🚨

Avoid:

```csharp
lock (_lock)
{
    CallDatabase();
}
```

or:

```csharp
lock (_lock)
{
    CallExternalApi();
}
```

The lock remains held while waiting for a slow external operation.

That can cause:

```text
Lock held
   ↓
DB/API takes time
   ↓
Other threads wait
   ↓
Contention
```

Instead, keep the critical section small.

---

### Deadlock Risk ⭐⭐⭐

Synchronization can introduce deadlocks.

Bad:

```text
Thread 1:
Lock A → Lock B

Thread 2:
Lock B → Lock A
```

We discussed this in the previous topic.

Best practice:

> **Always acquire multiple locks in a consistent order.**

---

### `lock` vs `SemaphoreSlim` — Interview Question

### Question:

> "I need to protect an async method. Should I use lock?"

Answer:

> **"A normal `lock` can't be held across an `await`. For asynchronous mutual exclusion, I'd typically use `SemaphoreSlim` and `WaitAsync()`."**

Example:

```csharp
await _semaphore.WaitAsync();

try
{
    await DoSomethingAsync();
}
finally
{
    _semaphore.Release();
}
```

---

### One More Important Concept: Atomicity

An operation is **atomic** when it appears to happen as one indivisible operation from the perspective of other threads.

For example:

```csharp
Interlocked.Increment(ref counter);
```

is atomic.

But:

```csharp
counter++;
```

is not guaranteed to be atomic as a compound read-modify-write operation.

That's why:

```csharp
Interlocked.Increment(...)
```

is useful.

---

### Interview Questions

### Q1. What is a race condition?

> **"A race condition occurs when multiple threads access shared mutable state concurrently and the result depends on the timing or ordering of their execution."**

---

### Q2. What is `lock`?

> **"`lock` provides mutual exclusion, ensuring that only one thread at a time can execute a protected critical section for a particular lock object."**

---

### Q3. `lock` vs `Monitor`?

> **"`lock` is syntactic sugar around Monitor's enter/exit behavior. Monitor provides additional functionality such as TryEnter."**

---

### Q4. `lock` vs `Mutex`?

> **"`lock` is generally used for synchronization within a process, while Mutex can also provide cross-process synchronization. Mutex has more overhead."**

---

### Q5. When would you use `SemaphoreSlim`?

> **"I'd use SemaphoreSlim when I need asynchronous synchronization or need to limit concurrency, such as allowing only 10 concurrent API calls. `WaitAsync()` allows callers to wait without synchronously blocking a thread."**

---

### Q6. When would you use `Interlocked`?

> **"For simple atomic operations such as incrementing, decrementing, exchanging, or compare-and-swap on shared values. It's preferable to a lock when the operation is simple enough."**

---

### Q7. What are Concurrent Collections?

> **"They are thread-safe collection implementations such as ConcurrentDictionary and ConcurrentQueue that support concurrent access without requiring the caller to manually synchronize every collection operation."**

---

