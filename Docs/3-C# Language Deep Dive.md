# C# Language Deep Dive

- Value Types vs Reference Types
- `ref`, `out`, `in` parameters
- Nullable Reference Types
- Records (`record` vs `class` vs `struct`)
- Pattern Matching (switch expressions, `is`, `when`)
- Generics and Constraints
- Delegates, Events, Func/Action/Predicate
- Extension Methods
- `Span<T>`, `Memory<T>` (performance-focused topics)
- Init-only properties, required members (C# 11+)

## Value Types vs Reference Types

- The key difference is how they behave when they're assigned or passed around. A value type contains its actual value, so assigning it creates an independent copy. Examples are `int`, `bool`, `struct`, and `enum`.
- A reference type variable contains a reference to an object, so assigning it copies the reference and both variables can point to the same object. Classes, arrays, and strings are examples of reference types.
- I wouldn't define the distinction simply as stack versus heap. That's an implementation detail. A value type can exist inside a heap-allocated object, for example. The more important distinction is value semantics versus reference semantics.
- This also explains boxing and unboxing: when a value type needs to be represented as an `object` or another compatible reference representation, it can be boxed. In modern C#, generics help avoid unnecessary boxing.
- If the type represents a small value where copying should create an independent value, a struct may be appropriate. If it represents an entity with identity, complex state, mutability, or needs inheritance, I would generally use a class.

| Concept | Value Type | Reference Type |
| --- | --- | --- |
| Examples | `int`, `bool`, `struct`, `enum` | `class`, `string`, array |
| Variable contains | Actual value | Reference to object |
| Assignment | Copies value | Copies reference |
| Independent copy | Yes | No, unless explicitly cloned |
| Can be `null`? | Normally no | Yes |
| Heap? | Not necessarily | Objects generally allocated on managed heap |
| Inheritance | Cannot inherit from another struct/class | Supports class inheritance |
| Boxing | Can be boxed | Already reference type |
| Typical use | Values | Entities/objects |

## ref, out, in parameters

> `ref`, `out`, and `in` are all about **passing arguments by reference**, but they have different contracts.

The easiest interview mental model is:

**`ref` = read + write**
**`out` = write only / must be assigned**
**`in` = read only** 

### ref:

- The **ref** is a keyword in C# which is used for the passing the arguments by a reference. Or we can say that if any changes made in this argument in the method will reflect in that variable when the control return to the calling method.
- The *ref* parameter does not pass the [**property**](https://www.geeksforgeeks.org/c-sharp/c-sharp-properties/).
- It is necessary the parameters should initialize before it pass to ref.
- It is not necessary to initialize the value of a parameter before returning to the calling method.
- The passing of value through ref parameter is useful when the called method also need to change the value of passed parameter.
- When ref keyword is used the data may pass in bi-directional.

### out:

- The **out** is a keyword in C# which is used for the passing the arguments to methods as a reference type. It is generally used when a method returns multiple values. The out parameter does not pass the property.
- It is not necessary to initialize parameters before it pass to out.
- It is necessary to initialize the value of a parameter before returning to the calling method.
- The declaring of parameter through out parameter is useful when a method return multiple values.
- When out keyword is used the data only passed in unidirectional.

### in:

- Pass the argument by reference, but the method cannot modify it.
- I only need to read this value. Don't give me a writable copy; give me read-only reference access

```
             Do I need special parameter semantics?
                          │
                ┌─────────┴─────────┐
                │                   │
               NO                  YES
                │                   │
          Normal parameter          ↓
                            What do I need?
                                  │
            ┌─────────────────────┼────────────────────┐
            ↓                     ↓                    ↓
   Modify existing         Produce a value       Read only,
   caller variable?        for the caller?       avoid copying?
            │                     │                    │
           ref                   out                  in
```

> `ref` is for an existing value that the method needs to read and modify. `out` is for a value that the method is responsible for producing, commonly used in Try-pattern APIs such as `TryParse`. `in` is for read-only reference access, mainly when working with larger value types where avoiding a copy is beneficial. In normal cases, I prefer regular parameters and return values because they're simpler and clearer.

[Take me to github for Ref-Out-In Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/RefOutInExample.cs)

---

## Nullable Reference Types

- In C#, value types (like int, float, bool) normally cannot store null. To solve this, C# 2.0 introduced nullable types using System.Nullable<T>, which let value types hold either their normal range of values or null
- The main use of nullable type is in database applications. Suppose, in a table a column required null values, then you can use nullable type to enter null values.
- Nullable type is also useful to represent undefined value.
- You can also use Nullable type instead of a reference type to store a null value.
- You cannot directly access the value of the Nullable type. You have to use GetValueOrDefault() method to get an original assigned value if it is not null. You will get the default value if it is null. The default value for null will be zero.

[Take me to github for Nullable Reference Types Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/NullableReferenceTypesExample.cs)

---

## record vs class vs struct

```text
                     C# Types
                        │
          ┌─────────────┴─────────────┐
          │                           │
    Reference types              Value types
          │                           │
    ┌─────┴─────┐                     │
    │           │                     │
  class     record class         struct
              │
          record struct
```

### **Class (`class`)**

- Classes are the foundational building blocks of Object-Oriented Programming (OOP) in C#. They track identity and maintain an evolving state over time.
- **Memory**: Allocated on the managed heap. Variables store a pointer to the memory location.
- **Equality**: Two variables are only equal if they point to the exact same object in memory, even if their internal data matches perfectly.
- **Best Used For**: Complex business logic, entities with state that changes frequently (e.g., a `BankAccount` with `Deposit()` methods), and architectures relying heavily on inheritance.

### **Struct (`struct`)**

- Structs are lightweight data containers designed to minimize memory overhead for short-lived data.
- **Memory**: Allocated on the stack or inline inside containing types.
- **Performance Trap**: Traditional structs use reflection to determine value equality, making operations like `Equals()` notably slow.
- **Best Used For**: Small, lightweight, primitive-like data structures with minimal to no behavior (e.g., a 2D `Point(x, y)`, `Color(r, g, b)`, or vectors).

### **Record (`record` or `record class`)**

- Introduced in C# 9, records are specialized classes designed to act as transparent, immutable data containers.
- **Value Semantics**: They are reference types but behave like value types when compared. If two separate record instances hold identical properties, they are considered equal.
- **Nondestructive Mutation**: Because they are immutable by default, you modify them using the `with` keyword, which safely clones the record with specified modifications.
- **Best Used For**: Data Transfer Objects (DTOs), API request/response payloads, and configuration settings where data remains constant.

### **Record Struct (`record struct`)**

- Introduced in C# 10, these combine the stack allocation benefits of a struct with the compiler-generated enhancements of a record.
- **Optimization**: Unlike traditional structs, `record struct` generates strongly typed equality operators at compile-time, completely bypassing slow reflection.
- **Best Used For**: Scenarios where you need maximum performance, zero heap allocations, and fast value-based equality checking.

| Feature | `class` | `record class` | `struct` | `record struct` |
| --- | --- | --- | --- | --- |
| Reference/value | Reference | Reference | Value | Value |
| Identity-oriented | ✅ | Usually ❌ | ❌ | ❌ |
| Value equality by default | ❌ | ✅ | Value equality semantics | ✅ |
| Assignment copies | Reference | Reference | Value | Value |
| `with` expression | ❌ | ✅ | ❌ | ✅ |
| Inheritance | ✅ | ✅ | ❌ class inheritance | ❌ class inheritance |
| Good for entities | ✅ | Sometimes | Usually ❌ | Usually ❌ |
| Good for DTO/data | Sometimes | ✅ | Sometimes | ✅ |
| Typical mutability | Mutable | Often immutable | Often immutable/small | Often immutable-ish |

use a **class for identity**, a **record for data/value-oriented reference types**, and a **struct or record struct for small value types where copying should produce an independent value.**

| Type | Use when | Example |
| --- | --- | --- |
| **`class`** | Entity has **identity**, lifecycle, mutable/complex state, inheritance | `Customer`, `Order`, `BankAccount` |
| **`struct`** | Small value where **copying should create an independent value** | `Point`, `Coordinate`, `Temperature` |
| **`record`** | Data/value-oriented object where **value equality** is useful; often immutable | DTOs, API responses, commands, events |
| **`record struct`** | Small value type where you also want **record-style value equality / `with`** | `Money`, `Point`, `Coordinate` |

[Take me to github for Type Kinds Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/TypeKindsExample.cs)

---

## Switch Expression

- Switch expression in c# introduced after .NET 10
- They are a more concise, readable and expressive way to handle multiple conditional branches compared to the traditional switch statement.
- Switch expressions can return values directly, making them suitable for assignments, return statements or inline logic.

It differs from the traditional switch statement in the following ways:

- Returns a value directly.
- Uses expression syntax instead of statements.
- Eliminates the need for break or return in each branch.
- Supports pattern matching, including type patterns, property patterns and relational patterns.

**When to Use Switch Expressions**

- When you need a concise value-based mapping from an input.
- When using pattern matching for cleaner and more readable code.
- When you want to avoid fall-through bugs common in switch statements.

[Take me to github for Switch Expression Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/SwitchExpressionExample.cs)

## Generic Constraints

- In C#, Generics allow classes, methods, interfaces and delegates to work with any data type. 
- However, sometimes you may want to restrict the type of arguments that can be passed to a generic type. This is where constraints come in.
- Generic constraints specify requirements for the type parameters in generics, ensuring only suitable types are used.

```csharp
class ClassName<T> where T : constraint{
    // members using T
}
```

**Why Use Constraints**
Without constraints, a generic type can accept any type. Constraints help in:

1. **Restricting types:** Limit type parameters to reference types, value types or specific classes/interfaces.
2. **Enabling functionality:** Allow use of certain members or constructors of the type parameter.
3. **Improving type safety:** Prevent invalid type substitutions at compile-time.

### Types of Constraints in C#

#### 1. where T : struct

- Restricts T to value types (like int, double, bool, or user-defined struct).
- Cannot be null.
- Cannot be used with nullable value types (int?, double?).

```csharp
class ValueContainer<T> where T : struct
{
    public T Data { get; set; }
}
```

> *Use this when you want to ensure the generic type is always a non-nullable value type.*


#### 2. where T : class

- Restricts T to reference types (class, interface, delegate, array).
- Allows null values.

```csharp
class ReferenceContainer<T> where T : class
{
    public T Data { get; set; }
}
```

> *Useful when designing classes that should work only with objects and not primitive/value types.*
 

#### 3. where T : new()

- Restricts T to types that have a public parameterless constructor.
- Must appear last when combining multiple constraints.

```csharp
class Factory<T> where T : new()
{
    public T CreateInstance()
    {
        return new T(); // Safe to create instance
    }
}
```

> *Helpful when you want to instantiate objects of type T inside the generic class.*
 

#### 4. where T : BaseClassName

- Restricts T to types that inherit from a specific base class.
- Allows access to members of the base class inside the generic class.

```csharp
class Animal { }
class Dog : Animal { }

class AnimalContainer<T> where T : Animal
{
    public T Data { get; set; }
}
```

Ensures only classes derived from Animal (like Dog) can be used as type arguments.

#### 5. where T : InterfaceName

- Restricts T to types that implement a specific interface.
- Enables calling interface methods on the generic parameter.

```csharp
interface IShape
{
    void Draw();
}

class ShapeContainer<T> where T : IShape
{
    public void Render(T shape)
    {
        shape.Draw();
    }
}
```

Ensures all type arguments follow a specific contract.

#### 6. Multiple Constraints

- More than one constraint can be applied using a comma.
- **Example:** Restrict T to be a reference type and must have a parameterless constructor.

```csharp
class Sample<T> where T : class, new()
{
    public T Create()
    {
        return new T();
    }
}
```

Common in factory patterns where you want reference types that can be instantiated.

#### 7. Constraints on Multiple Type Parameters

Each type parameter can have its own constraint.

```csharp
class Sample<T1, T2>
    where T1 : class
    where T2 : struct
{
    public T1 RefData { get; set; }
    public T2 ValData { get; set; }
}
```

Ensures fine-grained control when working with multiple generics.

[Take me to github for Generic Constraints Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/GenericConstraintsExample.cs)

---

## Delegates, Events, Func/Action/Predicate

### Delegate:

- A delegate defines the signature of methods it can point to.
- It can reference both static and instance methods.
- Delegates are type-safe, meaning the method signature must match the delegate declaration.
- They are the foundation of events and anonymous functions in C#.

**When to Use Delegates**

- For implementing callbacks.
- For handling events.
- For writing flexible, reusable code where behavior can be passed as parameters.
- For functional-style programming with LINQ and lambdas.

```csharp
  [modifier] delegate [return_type] [delegate_name]([parameter_list]);
```

- **modifier:** Defines the accessibility of the delegate (public, private, internal, etc.). It is optional.
- **delegate:** The keyword used to declare a delegate.
- **return_type:** The type of value returned by the methods referenced by the delegate (can also be void).
- **delegate_name:** The identifier you assign to the delegate.
- **parameter_list:** Defines the parameters the delegate requires. The methods assigned must match this list exactly.

**Multicasting of a Delegate:**

Delegates can reference multiple methods at once using the + or += operator. Such delegates are called multicast delegates.

**Properties:**

- Delegates are combined and when you call a delegate then a complete list of methods is called.
- All methods are invoked in the order they were added to the delegate (invocation order).
- '+' or '+=' Operator is used to add the methods to delegates.
- '–' or '-=' Operator is used to remove the methods from the delegates list.

[Take me to github for Delegate Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/DelegateExample.cs)

### Events:

- An event is a mechanism for sending notifications from one object to another. It is declared in a class and is triggered when a specific action happens.
- Events rely on delegates to point to the method that will handle the occurrence.

**Key points**

- Events are declared using delegates.
- The event keyword restricts delegate access, allowing only subscription (+=) and unsubscription (-=) from outside the class, while only the declaring class can raise (invoke) the event.
- Events support loose coupling between components.

**Event Handler**

An event handler is a method that responds to an event. It contains the code to be executed when the event is raised. Event handlers are usually void methods with two parameters:

- The sender (object that raised the event).
- Event data (information about the event, derived from EventArgs).

**Important Notes**

- Use EventHandler and EventHandler<TEventArgs> whenever possible instead of custom delegates.
- Always check for null (?.Invoke) before invoking events to avoid exceptions.
- Events promote encapsulation because they can only be raised by the declaring class or its derived classes (typically through protected methods).
- Multiple subscribers can listen to the same event.

[Take me to github for Event Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/EventExample.cs)

In order to get rid of all the first steps, we can directly use Func, Action, or Predicate delegates.

1. The Func delegate takes zero, one or more input parameters, and returns a value (with its out parameter).
2. The action takes zero, one or more input parameters, but does not return anything.
3. Predicate is a special kind of Func. It represents a method that contains a set of criteria mostly defined inside an if condition and checks whether the passed parameter meets those criteria or not.

| Delegate | Return type | Typical use |
| --- | --- | --- |
| `Func<T, TResult>` | `TResult` | Calculate / transform / get a result |
| `Predicate<T>` | `bool` | Test/filter something |
| `Action<T>` | `void` | Perform an operation |

#### 1. Use `Func` when you need a RESULT

#### Scenario: Calculate salary

```csharp
Func<Employee,decimal> calculateSalary = employee => employee.BasicSalary + employee.Bonus;
```

Use it:

```csharp
decimal salary = calculateSalary(employee);
```

Think:

```
Employee
   ↓
 Func
   ↓
Salary
```

### Other scenarios

```
Price → discounted price
Employee → salary
Order → total
String → string length
Product → product name
```

Whenever your question is:

> **"Give me something back."** Think **`Func`**.

---

#### 2. Use `Predicate` when you're asking a YES/NO question

#### Scenario: Is employee active?

```csharp
Predicate<Employee> isActive = employee => employee.IsActive;
```

Result:

```csharp
bool result = isActive(employee);
```

You get:
```
true / false
```

### Other scenarios
```
Is user logged in?
Is order valid?
Is product in stock?
Is age greater than 18?
Is number even?
```

Whenever your question is:

> **"Is this true?"**

Think **`Predicate`**.

---

#### 3. Use `Action` when you want to DO something

#### Scenario: Log a message

```csharp
Action<string> log = message => Console.WriteLine(message);
```

Usage:

```csharp
 log("Order completed");
```

There is no return value.

### Other scenarios

```
Send email
Write log
Print something
Update UI
Publish event
Save something
Notify user
```

Whenever your question is:

> **"Perform this operation."**

Think **`Action`**.

---

#### 4. Scenario: Filtering → `Func<T, bool>`

#### This is where you need to know something important.

You might think:

```csharp
Predicate<Employee>
```

is always used for filtering. But LINQ's `Where()` actually expects a:

```csharp
    Func<T,bool>
```

Example:

```csharp
var activeEmployees = employees.Where(e => e.IsActive);
```

Conceptually:
```
Employee
   ↓
Func<Employee, bool>
   ↓
true / false
   ↓
Where keeps it
```

So in **LINQ**, you'll commonly use:

```csharp
Func<T,bool>
```

rather than explicitly declaring `Predicate<T>`.

---

#### 5. Scenario: Transforming data → `Func`

Suppose:

```csharp
var employees= ...
```

You want only names:

```csharp
var names = employees.Select(e=>e.Name);
```

`Select` needs something like:

```csharp
Func<Employee,string>
```

Meaning:

```text
Employee → string
```

So:

```csharp
e=>e.Name
```
---

#### 6. Scenario: Logging → `Action`

Suppose you want to make your method accept a logging function:

```csharp
void ProcessOrder(Orderorder,Action<string>logger)
{
	logger("Processing order...");
}
```

Call it:

```csharp
ProcessOrder(order,message => Console.WriteLine(message));
```

Why `Action`?

Because the method doesn't need a result from the logger.

It just says:

> "Do something with this message."

---

#### 7. Scenario: Notification → `Action`

```csharp
voidProcessPayment(Paymentpayment,Action<Payment>notify)
{
	// process paymentnotify(payment);
}
```

The caller decides what notification means:

```csharp
ProcessPayment(payment,p => SendEmail(p));
```

or:

```csharp
ProcessPayment(payment,p => SendSms(p));
```

or:

```csharp
ProcessPayment(payment,p=>Console.WriteLine("Payment completed"));
```

This is a very practical use of `Action`.

---

#### 8. Scenario: Business rule → `Predicate` / `Func<T,bool>`

Suppose your system needs a reusable rule:

```csharp
Predicate<Employee> canGetBonus = employee => employee.PerformanceScore >= 80;
```

Then:

```csharp
if (canGetBonus(employee))
{
	GiveBonus(employee);
}
```

You can change the rule without changing the method that uses it.

Another example:

```csharp
Predicate<Product> isAvailable = product => product.Stock>0;
```

---

#### 9. Scenario: Calculation with multiple inputs → `Func`

```csharp
Func<decimal,decimal,decimal> calculateTotal=(price,tax) => price+tax;
```

Usage:

```csharp
var total=calculateTotal(1000,180);
```

Think:

```
price + tax
     ↓
   Func
     ↓
  total
```

---

#### 10. Scenario: Operation with multiple inputs → `Action`

Suppose you want to log a user action:

```csharp
Action<string,DateTime> logAction=
    (message,time)=>
    {
	    Console.WriteLine($"{time}: {message}");
    };
```

No result.

Therefore:

**`Action`**.

---

#### 11. Very important: `Predicate<T>` vs `Func<T,bool>`

These can look almost identical:

```csharp
Predicate<int> p = x => x >10;
```

and:

```csharp
Func<int,bool> f = x= > x > 10;
```

Both mean:

```
int → bool
```

So which should you use?

#### Use `Predicate<T>` when you're explicitly modeling a predicate/condition:

```csharp
Predicate<User> isAdmin = user => user.Role =="Admin";
```

#### Use `Func<T,bool>` when working with APIs that expect `Func`, especially LINQ:

```csharp
users.Where(user => user.IsActive);
```

In practice, you'll encounter **`Func<T,bool>` much more frequently in LINQ**.

---

#### 12. Decision tree

When you're writing a lambda, ask:

```
             What does my lambda do?
                       │
          ┌────────────┼────────────┐
          ↓            ↓            ↓
      Returns       true/false    returns
      a value?       question?    nothing?
          │            │            │
          ↓            ↓            ↓
        Func       Predicate      Action
```

Examples:

```
Calculate total       → Func
Get employee name     → Func
Convert object        → Func

Is employee active?   → Predicate
Is order valid?       → Predicate
Is number even?       → Predicate

Send email            → Action
Log message           → Action
Publish event         → Action
```

---

## Extension Methods
- In C#, an extension method is a special kind of static method that allows you to add new methods to an existing type (class, struct or interface) 
without modifying its source code or creating a derived type. 
- Extension methods are defined in a static class and are marked with the this keyword in their first parameter.

```csharp
public static class ClassName {
    public static ReturnType MethodName(this TargetType obj, parameters){
        // method body
    }
}
```

#### Key Points
- Must be defined in a static class.
- Must be declared as a static method.
- The first parameter must use the this keyword to indicate the type being extended.
- They provide additional functionality without altering the original type.
- They are called like instance methods on the target type.

#### Example 1: Extension Method on Built-in Type
This example shows how to extend a built-in .NET type (string) with a custom method. 
We will add a WordCount method to count the number of words in a string.

#### Example 2: Extension Method on User-defined Class
This example extends a user-defined class Student by adding a method PrintResult. 
The method evaluates whether the student has passed or failed, without modifying the original Student class.

#### Example 3: Extension Method on Interface
In this example, we extend an interface ILogger. 
By defining the LogError extension method, every class implementing ILogger automatically gains this additional functionality without needing to redefine it.

#### Benefits
- Add functionality without modifying original code.
- Can extend sealed classes (e.g., string, DateTime).
- Keep code clean and readable.
- Useful in scenarios like LINQ queries and utility functions.

#### Limitations
- Cannot override existing methods.
- Can lead to confusion if overused, especially with similar method names.
- Extension methods are static methods defined inside a static class, with the first parameter marked using the this keyword.

["Take me to github for Extension Methods Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/ExtensionMethodsExample.cs)

---

## `Span<T>`, `Memory<T>` (performance-focused topics)
- Span<T> and Memory<T> are both zero/low-allocation ways to work with contiguous memory in .NET
- They are especially useful when performance matters—parsing, serialization, networking, file processing, etc.
- Span<T> is a lightweight, stack-only view over contiguous memory. I use it when I need high-performance synchronous processing while avoiding allocations and copying
- Memory<T> provides a similar memory abstraction but isn't stack-only, so it can be stored on the heap and safely used across asynchronous operations. 
- I can obtain a Span<T> from it for synchronous processing.

###### contiguous
- Data is stored next to each other in memory, without gaps.
- Suppose an int takes 4 bytes.
```csharpe
    int[] numbers = { 10, 20, 30, 40 };
```
Conceptually, memory looks like:
```csharpe
Memory Address

1000 → 10
1004 → 20
1008 → 30
1012 → 40
```
The elements are next to each other: That's contiguous memory.
┌──────┬──────┬──────┬──────┐
│  10  │  20  │  30  │  40  │
└──────┴──────┴──────┴──────┘
 1000   1004   1008   1012

### The problem with normal arrays

Suppose you have:

```csharp
byte[] data = GetData();

byte[] part1 = data[0..100];
byte[] part2 = data[100..200];
```

Those slices create **new arrays and copy data**.

If you're processing millions of messages, this can cause:

* More memory allocations
* More GC pressure
* More CPU spent copying data

Instead:

```csharp
Span<byte> part1 = data.AsSpan(0, 100);
Span<byte> part2 = data.AsSpan(100, 100);
```

Now `part1` and `part2` are just **views over the original array**.

No new byte array is created.

---

### What exactly is `Span<T>`?

> ** `Span<T>` is a window/view over a continuous region of memory.**

For example:

```text
Original array

Index:   0   1   2   3   4   5   6   7   8   9
         └─────────────── byte[] ─────────────────┘

Span<int>
             └───────┘
             2       6
```

Example:

```csharp
int[] numbers = { 10, 20, 30, 40, 50 };

Span<int> span = numbers.AsSpan(1, 3);

Console.WriteLine(span[0]); // 20
Console.WriteLine(span[1]); // 30
Console.WriteLine(span[2]); // 40
```

The span doesn't own the data.

It simply points to:

```text
numbers
   ↓
[10][20][30][40][50]
     └──────────┘
        Span
```

---

### The really important part: `Span<T>` is a `ref struct`

This is where interviews often become interesting.

```csharp
Span<int>
```

is a `ref struct`.

That means it has **stack-only lifetime restrictions**.

For example, you generally cannot:

```csharp
class MyClass
{
    Span<int> span; // ❌
}
```

And you cannot use `Span<T>` across an `await`:

```csharp
async Task Process()
{
    Span<byte> buffer = ...;

    await SomethingAsync();

    // ❌ span cannot survive across await
}
```

Why?

Because `Span<T>` can refer to memory whose lifetime is tied to the current stack frame. So .NET prevents it from escaping that safe lifetime.

---

### Then what is `Memory<T>`?

This is where `Memory<T>` becomes useful.

Think:

> `Span<T>` = synchronous/high-performance view
> `Memory<T>` = heap-friendly view that can survive asynchronous operations

Example:

```csharp
Memory<byte> memory = new byte[1000];
```

You can store it in a class:

```csharp
class BufferManager
{
    private Memory<byte> buffer;

    public BufferManager()
    {
        buffer = new byte[1000];
    }
}
```

And you can use it with async code:

```csharp
async Task ProcessAsync(Memory<byte> memory)
{
    await SomeAsyncOperation();
    Span<byte> span = memory.Span;
    // Process span
}
```

This is one of the biggest differences to remember.

---

### `Span<T>` vs `Memory<T>`

|                         | `Span<T>`            | `Memory<T>`          |
| ----------------------- | -------------------- | -------------------- |
| Type                    | `ref struct`         | `struct`             |
| Stack-only restrictions | Yes                  | No                   |
| Can be stored in class  | ❌                    | ✅                    |
| Can cross `await`       | ❌                    | ✅                    |
| Synchronous processing  | Excellent            | Excellent            |
| Async processing        | Not directly         | Excellent            |
| Allocation              | No allocation itself | No allocation itself |
| Can access array        | Yes                  | Yes                  |
| `.Span` property        | N/A                  | Yes                  |

A very useful mental model:

```text
Memory<T>
   │
   │ .Span
   ↓
Span<T>
```

You can convert:

```csharp
Memory<byte> memory = new byte[1000];

Span<byte> span = memory.Span;
```

---

### Example: parsing data

Suppose you receive:

```text
"Swapnil,30,India"
```

You could do:

```csharp
string[] parts = input.Split(',');
```

But `Split()` creates strings/arrays. For performance-sensitive code, you can work with spans.

Conceptually:

```csharp
ReadOnlySpan<char> span = input.AsSpan();
int comma = span.IndexOf(',');
ReadOnlySpan<char> name = span[..comma];
```

Now `name` is simply a view into the original string. No new string needs to be created just to identify that portion.

---

### `ReadOnlySpan<T>`

There is also:

```csharp
ReadOnlySpan<T>
```

Use it when you want to read memory but **not modify it**.

```csharp
ReadOnlySpan<char> text = "Hello World".AsSpan();
Console.WriteLine(text[0]);
```

You cannot do:

```csharp
text[0] = 'X'; // ❌
```

This is very useful for APIs:

```csharp
void Parse(ReadOnlySpan<char> input)
{
    // Read only
}
```

You are telling the caller:

> "Give me access to this memory, but I promise not to modify it."

---

### A very important performance example

Consider:

```csharp
string input = "123456789";
```

You want to process the first 3 characters.

### Approach 1 — substring

```csharp
string value = input.Substring(0, 3);
```

Creates:

```text
new string
   ↓
"123"
```

### Approach 2 — span

```csharp
ReadOnlySpan<char> value = input.AsSpan(0, 3);
```

Conceptually:

```text
Original string

"123456789"
 └───┘
 Span
```

No new string allocation just for the slice.

This is why spans are popular in:

* JSON parsing
* HTTP processing
* serialization
* protocol parsing
* database drivers
* high-performance libraries

---

### Where `Memory<T>` shines

Imagine a network operation.

```csharp
Memory<byte> buffer = new byte[4096];
await stream.ReadAsync(buffer);
```

After the async operation:

```csharp
Span<byte> data = buffer.Span;
Process(data);
```

This gives you a nice pattern:

```text
              Async world
                  │
                  ▼
             Memory<byte>
                  │
                await
                  │
                  ▼
            Memory<byte>
                  │
                .Span
                  ▼
             Span<byte>
                  │
                  ▼
        High-performance processing
```

This is the pattern I would remember.

---

### Important: Span doesn't automatically make code faster

This is a common misconception.

Using:

```csharp
Span<T>
```

doesn't magically make every operation faster. The performance advantage usually comes from:

**avoiding unnecessary allocations and copies.**

For example:

```csharp
var result = input.Substring(10, 20);
```

versus:

```csharp
var result = input.AsSpan(10, 20);
```

The second avoids creating a new string. But if your application doesn't have allocation/copying as a bottleneck, using spans everywhere can make the code unnecessarily complicated.

---

### The easiest rule to remember

Think of these four types like this:

```text
T[]
 │
 │ owns actual data
 ▼
Array


Span<T>
 │
 │ temporary view
 ▼
Synchronous processing


ReadOnlySpan<T>
 │
 │ temporary read-only view
 ▼
Synchronous read-only processing


Memory<T>
 │
 │ longer-lived / async-friendly view
 ▼
Async processing
```

---

## Init-only properties
- Init-only properties introduced in C# 9 are class or struct properties that can only be assigned during object initialization. 
- Unlike standard set accessors, an init accessor restricts assignments to object initializers or constructors. 
- This enforces immutability without using read-only fields.
- These are features for controlling how objects are initialized, especially useful when you want objects to be immutable or require certain values when created.
These are **C# features for controlling how objects are initialized**, especially useful when you want objects to be immutable or require certain values when created.

They are related, but solve **different problems**.

---

### `init`-only properties

Normally, a property with `set` can be changed anytime:

```csharp
public class Employee
{
    public string Name { get; set; }
}
```

So:

```csharp
var employee = new Employee
{
    Name = "Swapnil"
};

employee.Name = "Rahul"; // ✅ Allowed
```

With `init`:

```csharp
public class Employee
{
    public string Name { get; init; }
}
```

Now:

```csharp
var employee = new Employee
{
    Name = "Swapnil"
};
```

is allowed.

But:

```csharp
employee.Name = "Rahul"; // ❌ Compilation error
```

### Think of `init` as:

> **"You can set this property only while creating the object."**

```text
new Employee
{
    Name = "Swapnil"   ← ✅
}

employee.Name = ...   ← ❌
```

---

### Why was `init` introduced?

Before `init`, you had a few options.

### Option 1 — `set`

```csharp
public string Name { get; set; }
```

Easy, but mutable.

### Option 2 — constructor

```csharp
public Employee(string name)
{
    Name = name;
}

public string Name { get; }
```

Then:

```csharp
var employee = new Employee("Swapnil");
```

This gives immutability, but constructors can become cumbersome when there are many properties.

### `init` gives you both:

```csharp
var employee = new Employee
{
    Name = "Swapnil",
    Department = "IT",
    City = "Mumbai"
};
```

while keeping them immutable afterward.

---

#### `required` is a different concept

Suppose:

```csharp
public class Employee
{
    public string Name { get; init; }
    public string Department { get; init; }
}
```

You can do:

```csharp
var employee = new Employee();
```

The compiler doesn't complain. But logically, an employee **must have a name**.
That's where `required` comes in:

```csharp
public class Employee
{
    public required string Name { get; init; }

    public string Department { get; init; }
}
```

Now:

```csharp
var employee = new Employee
{
    Name = "Swapnil"
};
```

✅ Good.

But:

```csharp
var employee = new Employee();
```

❌ Compiler error.

Because `Name` is required.

---

#### `required` does NOT mean `init`

This is extremely important.

You can have:

```csharp
public required string Name { get; set; }
```

This means:

> Name **must be provided during initialization**, but it can still be changed later.

Example:

```csharp
var employee = new Employee
{
    Name = "Swapnil"
};

employee.Name = "Rahul"; // ✅
```

Because it has `set`.

---

You can combine them:

```csharp
public required string Name { get; init; }
```

Now:

> Name **must be provided during initialization AND cannot be changed afterward.**

```text
required
   ↓
Must provide value

init
   ↓
Cannot change after initialization
```

---

#### Compare all combinations

| Declaration                           | Must provide? | Can change later? |
| ------------------------------------- | ------------: | ----------------: |
| `string Name { get; set; }`           |             ❌ |                 ✅ |
| `string Name { get; init; }`          |             ❌ |                 ❌ |
| `required string Name { get; set; }`  |             ✅ |                 ✅ |
| `required string Name { get; init; }` |             ✅ |                 ❌ |

This table is worth remembering for interviews.

---

#### Real-world example

Imagine an API request model:

```csharp
public class CreateEmployeeRequest
{
    public required string Name { get; init; }

    public required string Email { get; init; }

    public string? Department { get; init; }
}
```

You can create:

```csharp
var request = new CreateEmployeeRequest
{
    Name = "Swapnil",
    Email = "swapnil@example.com",
    Department = "IT"
};
```

But this:

```csharp
var request = new CreateEmployeeRequest
{
    Name = "Swapnil"
};
```

fails because:

```text
Email
  ↓
required
  ↓
Missing
  ↓
❌ Compiler error
```

And after creation:

```csharp
request.Name = "Rahul";
```

also fails because:

```text
init
 ↓
Cannot modify after initialization
```

---
#### The easiest way to remember

Think about creating an employee:

#### `set`

> "You can change it whenever you want."

```csharp
Name { get; set; }
```

#### `init`

> "Set it when creating the object, then it's locked."

```csharp
Name { get; init; }
```

#### `required`

> "You are not allowed to create this object without providing it."

```csharp
required string Name
```

### Together

```csharp
public required string Name { get; init; }
```

means:

> **"You MUST provide Name when creating the object, and once created, you CANNOT change it."**

That's the core distinction you should know for a C#/.NET interview.