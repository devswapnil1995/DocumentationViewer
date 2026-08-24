# C# Language Deep Dive

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

`ref`, `out`, and `in` are all about **passing arguments by reference**, but they have different contracts.

The easiest interview mental model is:

> **`ref` = read + write**
> 
> 
> **`out` = write only / must be assigned**
> 
> **`in` = read only**
> 

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

`ref` is for an existing value that the method needs to read and modify. `out` is for a value that the method is responsible for producing, commonly used in Try-pattern APIs such as `TryParse`. `in` is for read-only reference access, mainly when working with larger value types where avoiding a copy is beneficial. In normal cases, I prefer regular parameters and return values because they're simpler and clearer.

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/RefOutInExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/RefOutInExample.cs)

## Nullable Reference Types

- In C#, value types (like int, float, bool) normally cannot store null. To solve this, C# 2.0 introduced nullable types using System.Nullable<T>, which let value types hold either their normal range of values or null
- The main use of nullable type is in database applications. Suppose, in a table a column required null values, then you can use nullable type to enter null values.
- Nullable type is also useful to represent undefined value.
- You can also use Nullable type instead of a reference type to store a null value.
- You cannot directly access the value of the Nullable type. You have to use GetValueOrDefault() method to get an original assigned value if it is not null. You will get the default value if it is null. The default value for null will be zero.

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/NullableReferenceTypesExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/NullableReferenceTypesExample.cs)

## record vs class vs struct

```
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

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/TypeKindsExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/TypeKindsExample.cs)

## **Switch Expression**

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

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/SwitchExpressionExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/SwitchExpressionExample.cs)

## **Generic Constraints**

- In C#, Generics allow classes, methods, interfaces and delegates to work with any data type. However, sometimes you may want to restrict the type of arguments that can be passed to a generic type. This is where constraints come in.
- Generic constraints specify requirements for the type parameters in generics, ensuring only suitable types are used.

```csharp
*class ClassName<T> where T : constraint{
// members using T
}*
```

**Why Use Constraints**
Without constraints, a generic type can accept any type. Constraints help in:

1. **Restricting types:** Limit type parameters to reference types, value types or specific classes/interfaces.
2. **Enabling functionality:** Allow use of certain members or constructors of the type parameter.
3. **Improving type safety:** Prevent invalid type substitutions at compile-time.

### **Types of Constraints in C#**

#### **1. where T : struct**

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
> 

#### **2. where T : class**

- Restricts T to reference types (class, interface, delegate, array).
- Allows null values.

```csharp
class ReferenceContainer<T> where T : class
{
    public T Data { get; set; }
}
```

> *Useful when designing classes that should work only with objects and not primitive/value types.*
> 

#### **3. where T : new()**

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
> 

#### **4. where T : BaseClassName**

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

#### **5. where T : InterfaceName**

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

#### **6. Multiple Constraints**

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

#### **7. Constraints on Multiple Type Parameters**

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

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/GenericConstraintsExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/GenericConstraintsExample.cs)

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
*[modifier] delegate [return_type] [delegate_name]([parameter_list]);*
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

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/DelegateExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/DelegateExample.cs)

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

[https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/EventExample.cs](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/EventExample.cs)

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
Func<Employee,decimal> calculateSalary=employee=> employee.BasicSalary+employee.Bonus;
```

Use it:

```csharp
decimal salary=calculateSalary(employee);
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

> **"Give me something back."**
> 

Think **`Func`**.

---

#### 2. Use `Predicate` when you're asking a YES/NO question

#### Scenario: Is employee active?

```csharp
Predicate<Employee> isActive=employee=>employee.IsActive;
```

Result:

```csharp
bool result=isActive(employee);
```

You get:

```
true / false
```

Think:

```
Employee
   ↓
Predicate
   ↓
Yes / No
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
> 

Think **`Predicate`**.

---

#### 3. Use `Action` when you want to DO something

#### Scenario: Log a message

```csharp
Action<string> log= message => Console.WriteLine(message);
```

Usage:

```csharp
log("Order completed");
```

There is no return value.

Think:

```
Message
   ↓
Action
   ↓
DO SOMETHING
```

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
> 

Think **`Action`**.

---

#### 4. Scenario: Filtering → `Func<T, bool>`

#### This is where you need to know something important.

You might think:

```csharp
Predicate<Employee>
```

is always used for filtering.

But LINQ's `Where()` actually expects a:

```csharp
Func<T,bool>
```

Example:

```csharp
var activeEmployees=employees.Where(e=>e.IsActive);
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
varemployees= ...
```

You want only names:

```csharp
var names=employees.Select(e=>e.Name);
```

`Select` needs something like:

```csharp
Func<Employee,string>
```

Meaning:

```
Employee → string
```

So:

```
e=>e.Name
```

is a `Func`.

### Think:

```
Select
   ↓
Transform
   ↓
Func
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
> 

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
ProcessPayment(payment,p=>SendEmail(p));
```

or:

```csharp
ProcessPayment(payment,p=>SendSms(p));
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
Predicate<Employee>canGetBonus=employee=>employee.PerformanceScore>=80;
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
Predicate<Product> isAvailable= product => product.Stock>0;
```

---

#### 9. Scenario: Calculation with multiple inputs → `Func`

```csharp
Func<decimal,decimal,decimal> calculateTotal=(price,tax)=>price+tax;
```

Usage:

```csharp
vartotal=calculateTotal(1000,180);
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
Predicate<int> p= x => x >10;
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
Predicate<User> isAdmin =user => user.Role =="Admin";
```

#### Use `Func<T,bool>` when working with APIs that expect `Func`, especially LINQ:

```csharp
users.Where(user=>user.IsActive);
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