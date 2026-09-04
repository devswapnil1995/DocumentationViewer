- Four Pillars: Abstraction, Encapsulation, Inheritance, Polymorphism
- Method Overloading vs Overriding
- Abstract Class vs Interface
- Sealed Classes and Methods
- Static vs Instance members
- Constructors (default, parameterized, static, private)
- Composition vs Inheritance ("has-a" vs "is-a")
- Boxing and Unboxing

---

## What is OOP?

- OPP is standing for object oriented programming.
- In this programming, we follow the principle “DO NOT REPEAT”.
- It is ideal for building scalable, reusable and maintainable code.
- In this programming everything is representing by object.

### Four Pillars

#### 1. Abstraction

- It is used to display only necessary and essentials feature of object to other classes.
- Abstraction is process of hiding the implementation details from user, only functionality provided to user.
- In other word, user will have information **what** object does instead of **how** it does.
- It allow us to hide unnecessary details and expose only what’s needed.
E.g.
```csharp
    Console.WriteLine(); // We just call this method we never care about how it is working internally
```

> Abstraction is “To represent the essential feature without representing the background details.”

1. While driving car, we don’t care how engine works
2. While using TV remote, we don’t care how it is working internally
3. While processing payment we can make payment any type like UPI, Credit Card, Debit Card but we don’t care how it is working in background

**To achieve abstraction we can use:**
1. Abstract Class
2. Interface

[Click here for Abstraction Encapsulation BasicExample](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/AbstractionEncapsulationBasicExample.cs)

#### 2. Encapsulation

- It is like putting all important stuff into locker and give key only to authorized/correct person.
- In coding terms, hiding internal details of class and exposing only what’s necessary.
- Encapsulation is like capsule, where we put all class members like data, method, variable inside a class and expose only required specific thing by handling access modifiers.
- It is also known as data hiding

For example, think about a bank account system. You don’t want anyone to change your account balance, bank provides separate methods like deposit or withdraw money.

1. It protect your data - no one can change variable 
2. It make your code clean - you control how data can accessed
3. It prevents accidental modification - Only allowed methods can change values

[Click here for Abstraction Encapsulation BasicExample](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/AbstractionEncapsulationBasicExample.cs)

We use access specifiers to achieve encapsulation

| **Aspect** | **Abstraction** | **Encapsulation** |
| --- | --- | --- |
| **Definition** | Hiding implementation details and showing only essential features. | Hiding internal state (data) and providing controlled access. |
| **Focus** | *What* the object does. | *How* the data is protected/controlled. |
| **Achieved By** | Abstract classes, Interfaces | Access modifiers, Properties, Methods |
| **Example** | ILogger.Log() – you know logging exists, not how it’s done. | BankAccount._balance hidden, exposed via methods. |

#### 3. Polymorphism

- Meaning of this is “One name, multiple form”.
- It allows method to behave differently based on the object that is calling them.
- This makes your code flexible, reusable and easier to maintain.
- In polymorphism, we have same method name but different behaviors.

> We can initiate base class with derived class but we cannot create object of derived class using base class. When required such scenario
   
   **1. Method accepts Base, but caller can provide any Derived**
    
    ```csharp
        Base obj = new Derived(); 
        void Process(Base obj)
        {
          obj.DoSomething();
        }
        Process(new Derived1());
        Process(new Derived2());
    ```
    
   **2. Collections of different derived types**
    
    ```csharp
        List<Base> items = new List<Base>();
        items.Add(new Derived1());
        items.Add(new Derived2());
        items.Add(new Derived3());
    ```
    
   **3. Dependency Injection**
    
    ```csharp
        class Service
        {
    	    private readonly IRepository repository;
    	    public Service(IRepository repository)
    	    {
    	        this.repository = repository;
    	    }
        }
    ```
    

—> For example, We have multiple payment method, but internally it is calling make payment method which execute it functionality based on method type.

#### Two Types of polymorphism

##### —> Compile time polymorphism (Method overloading)
> Same method name, different parameter. Compiler decide based on parameters which method should get execute. No need of inheritance in this.

[Click here for Method overloading Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/MethodOverloadingExample.cs)

##### —> Runtime polymorphism (Method overriding)
> The child class redefines method from parent class, basically we required inheritance here. Method name and signature should be same but implementation will be differ. We need to defined method with virtual keyword in parent class and use override in child class.

[Click here for Method overriding Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/MethodOverridingExample.cs)

#### 4. Inheritance

- With the help of inheritance we can inherit properties, methods from parent class in child class.
- Basically child class will get behavior of parent class.
- Parent class should hold all common method/functionality, child/derived class can inherits it. This feature is called extensibility.

##### Types of inheritance:

1. Single Inheritance -  One parent, one child
 The `Dog` class **reuses** the `MakeSound()` method from `Animal`. This is **single inheritance** in action!
2. Multilevel Inheritance - Parent, Child & Grand child (child class will be parent of another child class)
 In **multilevel inheritance**, a child class inherits from another child class.
3. Hierarchical Inheritance  - One parent, multiple child
 One parent (`Animal`), multiple children (`Dog` and `Bird`).
4. Multiple inheritance - we need interfaces to achieve this

[Click here for Inheritance Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/InheritanceExample.cs)

---------
---------

## Abstract Class vs Interface

### Abstract class:

- Abstract class serves as blueprint for other classes, which contains abstract methods(without implementation) and regular methods(with implementation).
- Abstract class cannot be instantiated, we cannot create object of abstract class.
- An abstract class is declared using the `abstract` keyword.
- Abstract method must be implemented in derived classes, with abstract methods, we enforce mandatory implementation in child classes.
- We can create constructor of abstract class
- We cannot use multiple abstract classes at same time
- We can created object of derived class using abstract class reference, this is called polymorphism. Even method call defined in abstract class it will execute code from override method.

**Why abstract methods are important?**

- Force consistency -  Ensures all subclasses implement the method
- Improve code organization - Define a clear structure for derived classes
- Enhances maintainability - Change in abstract methods apply to all child classes
- Encourage polymorphism -  Method behave differently, based on object type
- Abstract methods are rules for derived classes, they ensures that every subclasses implement a method in its own way.

[Click here for Abstract Class Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/AbstractClassExample.cs)

### Interface:

- It is a contract, if any class implements interface they need to follow all contract methods.
- It contains only methods without body, that is pure abstract methods. It cannot contains concreate method
- It help us to achieve multiple inheritance.
- Interfaces help define a clear contract for your classes.
- They allow flexibility, scalability, and better code organization.
- You can implement multiple interfaces in a class, unlike abstract classes.
- They promote loose coupling, making code easier to maintain and test.
- No constructor
- We use interfaces in depedency injection as well, as there is no constructor there will be no dependency of initialization

> C# 8 introduced default interface methods, allowing interfaces to contain method implementations primarily so interfaces can evolve without breaking existing implementations.

#### Type of interface:

1. Basic Interface - Defining contract
    - An **interface in C#** is simply a contract. Let’s create a **basic interface** and implement it in a class.
2. Interface with multiple implementation
    - A single interface can be implemented by **multiple classes**, each providing its own version of behavior!
3. Multiple Interface (**Multiple Inheritance with Interfaces!**)
    - Unlike classes, a class in C# **can implement multiple interfaces**!
4. Explicit Interface Implementation (Avoiding Naming Conflicts)
    - When a class implements **multiple interfaces with the same method name**, explicit interface implementation helps!

[Click here for Interface Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/InterfaceExample.cs)

##### Use Abstract Classes When:

- You have common behavior to share across multiple classes.
- You need to provide default implementations for some methods.
- You want to enforce certain behaviors but also allow overrides.

##### Use Interfaces When:

- You only need to define a contract without implementation.
- You want to support multiple inheritance (C# doesn’t support multiple base classes).
- You need to ensure different classes follow the same method structure.

------------------------
------------------------

## Sealed Class & Method

- When you seal a class, you prevent other classes from inheriting it. This ensures that no one can modify or extend its functionality, keeping it secure and stable.
- A sealed class is a class that cannot be inherited
- We can create object of sealed class
- The sealed keyword is like a lock on a bank vault. 
- It prevents unauthorized modifications and inheritance, keeping your class safe and secure.

### Sealed method
- A sealed method is a method in a derived class that prevents further overriding in classes that inherit from it.
- It acts as a final override point in an inheritance hierarchy.
- If this class becomes someone's parent then won't able to override but it does not mean that main parent class will not allow overriding.

**One interview trap:** A `sealed` method does not mean the class cannot be inherited. It only prevents that particular virtual method from being overridden further.

| `sealed class` | `sealed method` |
| --- | --- |
| Prevents inheritance | Prevents further overriding |
| Applied to class | Applied to an overridden method |
| `sealed class A` | `sealed override void Show()` |
| No class can derive from it | Derived classes can still inherit the class |
| Stops the inheritance chain | Stops the override chain |


[Click here for Sealed Class/Method Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/SealedClassMethodExample.cs)

---
---

## Static & Instance Method

- Static Method:
> A method that belongs to the class itself, not to any specific instance. Called on the class, not on objects.

- Instance Method:
> A method that belongs to a specific object instance. Each instance can have different behavior and access instance data.

- In C#, “static” means “relating to the type itself, rather than an instance of the type”. You access a static member using the type name instead of a reference or a value e.g  [`Guid().New`]
- In addition to methods and variables, you can also declare class to be static. A static class cannot be instantiated and can only contains static members.
- Static classes always derive from object, you can’t specify a different base type
- Static class connot implement an interface
- Static class connot have any instance members
- Static classes are implicity abstract, you can’t add abstract modifier yourself
- Static class may be generic
- Static constructor runs once before the first access to any static member
- Static methods are normally faster to invoke on call stack than instance method
- We cannot access instance member's from static class, as it don't `this` reference.

**When to Use:**
- Static Class: Only static members, pure utilities (Math, Console, File)
- Regular Class with Static: Mix of static and instance (Repository with static factory)


| **Static Method** | **Non Static Method** |
| --- | --- |
| 1. Static method is defined with the static keyword. | 1. Non Static Method is defined without static keyword. |
| 2. Static method is called by its class name | 2. Non Static Method is called by making the object of a class. |
| 3. We can’t use this keyword inside the Static Method | 3. We can use thiskeyword inside Non Static Method |
| 4. Static Method uses the memory of Class | 4. Non Static Method uses a memory of an object |

-> When interviewer asks: "Should this be static or normal?"
            
1. Ask clarifying questions:  
   - "Do we need multiple instances?"
   - "Does each instance have unique state?"
   - "Is this a utility or business entity?"
  
2. Provide examples:  
   - Static: Logger, Math helper, Validator
   - Normal: User, Order, Account, Repository
   
3. Mention SOLID principles:
   - Static makes DI harder
   - Normal classes are more testable\
   - Normal classes are more flexible
   
4. Consider the domain:
   - Real-world entity → Normal class
   - Pure utility → Static class

[Click here for Static vs Instance Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/StaticVsInstanceExample.cs)

> No. Static members belong to the type rather than an instance, so I access them using the class name. The class itself doesn't have to be declared static. An instance is required only for non-static members.
---
---

## Constructor

> A constructor is a special method, which gets invoked when object of class created. It should be a same name as class name. It cannot have return type.

### Types of constructor:
- Default constructor - Takes no parameter
- Parameterized constructor - Takes one or more parameter
- Copy constructor - Creates copy of existing object
- Static constructor - If we have static members, to instantiate static member class

[Click here for Constructor Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/ConstructorExample.cs)

---
---

## Access Modifiers

> Access modfiers defines scope and visiblity of classes, methods, fields, contructor and other members. They defined where and how member can accessed in program.

- Access modifiers help for data hiding.
- Access Modifiers and Accessibility
    - public:
        - Accessible in anywhere in project as well as in referenced assemblies.
        - It helps to keep member available globally.
    - private:
        - When we define member as private, it only accessible in same class.
        - This help us in data hiding
    - protected:
        - It only allowed to access members in same class as well as in derived classes
        - It does not allowed outside from base and derived class
    - internal:
        - Accesible in same assembly(project), not in referenced project/assemblies
    - protected internal:
        - It is combination of protected and internal.
        - Members are assesible within same assembly and also by derived classes(even they are in different assembly)
    - private protected:
        - This is valid after .NET 7.2
        - Private protected access is only granted to containing class, any other class inside or outside of assembly is not access to these members.
- Namespaces doesn’t allow the access modifiers as they have no access restrictions.
- The user is allowed to use only one accessibility at a time except the private protected and protected internal.
- The default accessibility for the top-level types (that are not nested in other types, can only have public or internal accessibility) is internal.
- If no access modifier is specified for a member declaration, then the default accessibility is used based on the context.

[Click here for Access Modifiers Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/AccessModifiersExample.cs)

---
---

## Composition

> Composition is design principle where one class contain an instance of another class as private member creating **“has-a”** relationship. (Inheritance creates “Is-a”)

- It helps in encapsulation
- Composition in C# creates a strong ownership relationship where the contained object cannot exist independently of its container. 
- This design principle promotes encapsulation and provides better control over object lifecycles, making it ideal for scenarios where components are integral parts of a whole system.

> I will generally prefer composition over inheritance because composition gives lower coupling, better flexibility, and allows behaviors to be changed independently. I use inheritance when there's a genuine is-a relationship and I need polymorphism—for example, Developer and Manager being Employees. I avoid inheritance just for code reuse because it creates a strong dependency on the base class and can lead to fragile hierarchies. So my default is composition, but I choose based on the domain relationship and whether polymorphism is actually needed

| Question | Inheritance | Composition |
| --- | --- | --- |
| Relationship | **Is-a** | **Has-a / Uses-a** |
| Coupling | High | Lower |
| Flexibility | Lower | Higher |
| Runtime behavior change | Difficult | Easy |
| Code reuse | Through parent | Through components |
| Polymorphism | Excellent | Excellent through interfaces |
| Multiple behaviors | Can become complex | Very good |
| Maintenance | Can become difficult | Usually easier |
| Typical example | `Dog : Animal` | `Car has Engine` |
| Best for | Stable hierarchy | Changeable behavior |


[Click here for Composition Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/CompositionExample.cs)

---
---

## Boxing And Unboxing

### Boxing

- The process of converting a [**Value Type**](https://www.geeksforgeeks.org/c-sharp/c-sharp-data-types/) variable (char, int etc.) to a [**Reference Type**](https://www.geeksforgeeks.org/c-sharp/c-sharp-data-types/) variable (object) is called Boxing.
- Boxing is an implicit conversion process where a value type is wrapped inside an object instance and stored on the heap.
- Value-type variables are generally stored on the stack when they are local variables. When they are fields within a reference type, they are part of the heap-allocated memory for the reference type.
- Reference type variables store memory addresses (references) on the stack, while their actual data is stored on the heap (except in some optimizations like String Interning).

### UnBoxing

- Unboxing is the process of explicitly converting a boxed object back into its original value type. It is an explicit conversion process. We can also say that the reverse process of boxing.

```csharp
    int num = 23;         // value type is int and assigned value 23
    Object Obj = num;    // Boxing
    int i = (int)Obj;    // Unboxing*
```
- In unboxing first we create a Value Type integer i to unbox the value from obj. Unboxing requires explicit casting. The object must contain a valid boxed value of the correct type, or an InvalidCastException will be thrown.

**Why we required boxing and unboxing:**

- Sometimes an API needs an `object`, but you have a value type like `int`.
- Boxing is needed when a value type needs to be treated as an object or another reference-type representation, because `object` can represent any C# type. Boxing copies the value into an object on the heap.
- Unboxing retrieves the value type from that boxed object using an explicit cast. We see this mainly with APIs that accept `object`, non-generic collections, interfaces, and some reflection scenarios. In modern C#, we generally avoid unnecessary boxing because it can create allocations and impact performance, which is one reason generics such as `List<int>` are preferred.

[Click here for Boxing and Unboxing Example](https://github.com/devswapnil1995/TopicDemoApp/blob/main/Modules/BoxingUnboxingExample.cs)

---
---

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

---
---