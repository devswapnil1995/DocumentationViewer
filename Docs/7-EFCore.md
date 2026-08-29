- Code First vs Database First
- DbContext lifecycle & scope
- Migrations — Add-Migration, Update-Database
- Change Tracking — Tracking vs AsNoTracking()
- Relationships — One-to-Many, Many-to-Many, One-to-One
- Fluent API vs Data Annotations
- Lazy vs Eager (Include) vs Explicit Loading
- Transactions in EF Core
- Raw SQL — FromSqlRaw, ExecuteSqlRaw
- Concurrency handling — Optimistic concurrency
- Performance — AsNoTracking, compiled queries, avoiding N+1
---------------------

## Code First vs Database First

The basic difference is:

> **Code First → C# code is the source of truth.**

> **Database First → Existing database is the source of truth.**

### Code First

You create your C# entity classes first.

For example:

```csharp
public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Salary { get; set; }
}
```

Then create your `DbContext`:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
}
```

Then create a migration:

```bash
dotnet ef migrations add InitialCreate
```

And apply it:

```bash
dotnet ef database update
```

Conceptually:

```text
C# Classes
     ↓
DbContext
     ↓
Migration
     ↓
Database Schema
```

So **your code drives the database schema**.

You add a property:

```csharp
public string Email { get; set; }
```

Then:

```bash
dotnet ef migrations add AddEmployeeEmail
dotnet ef database update
```

EF Core generates the required database change.

Conceptually:

```text
Old Model
   ↓
Add Email
   ↓
Migration
   ↓
ALTER TABLE Employees
ADD Email ...
```

***When to use Code First?***

Code First is commonly preferred when:

* You're creating a new application
* Development is code-driven
* Developers own the database schema
* You want schema changes tracked through migrations
* You're working with CI/CD

For a new ASP.NET Core application, **Code First is a very common approach**.

---

### Database First

Here, the database already exists.

For example:

```text
Existing Database
      ↓
Employees
Departments
Orders
Customers
```

You generate your C# entity classes and `DbContext` from that database.

Conceptually:

```text
Existing Database
       ↓
Scaffolding
       ↓
C# Entities
       ↓
DbContext
```

You can use EF Core scaffolding, for example:

```bash
dotnet ef dbcontext scaffold "connection-string" Microsoft.EntityFrameworkCore.SqlServer
```

This generates things such as:

```csharp
public partial class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

and:

```csharp
public partial class AppDbContext : DbContext
{
    public virtual DbSet<Employee> Employees { get; set; }
}
```

***When to use Database First?***

Common when:

* Database already exists
* You're integrating with a legacy system
* DBAs control the schema
* You cannot freely change the database
* You need to generate models from an existing schema

### Code First vs Database First

|                               | Code First       | Database First            |
| ----------------------------- | ---------------- | ------------------------- |
| Starting point                | C# code          | Existing database         |
| Source of truth               | Code/model       | Database                  |
| Schema creation               | Migrations       | Existing DB               |
| Common for                    | New applications | Existing/legacy DB        |
| Database generated from code? | ✅                | ❌                         |
| Models generated from DB?     | ❌ Usually        | ✅                         |
| Migrations commonly used?     | ✅                | Depends                   |
| Developer-driven              | Usually          | Often DBA/database-driven |


### Interview Question

**"What is the difference between Code First and Database First?"**

> **"In Code First, we define the entity model in C# and use EF Core migrations to create and evolve the database schema. In Database First, the database schema already exists and we scaffold the entity classes and DbContext from the database. Code First is commonly used for new applications, while Database First is useful when working with existing or legacy databases."**

-------------------------------
-------------------------------

## `DbContext` Lifecycle & Scope

> `DbContext` represents a session/unit of work with the database.

It is responsible for things like:

* Querying the database
* Tracking entities
* Detecting changes
* Saving changes
* Managing database operations

Example:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
}
```

You use it:

```csharp
public class EmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetEmployee(int id)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

Here `_context` provides access to the database.

**How is `DbContext` registered?**

Normally in ASP.NET Core:

```csharp
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
```

By default, `AddDbContext<T>()` registers the `DbContext` as:

> **Scoped**

This is extremely important.


**Why NOT Singleton?**

You should generally **not register `DbContext` as Singleton**.

Don't do:

```csharp
builder.Services.AddSingleton<AppDbContext>();
```

Why?

***`Problem 1 — DbContext is not thread-safe`***

Multiple requests could try to use the same context simultaneously.

```text
Request A ──┐
            │
            ↓
        DbContext
            ↑
            │
Request B ──┘
```

This can lead to concurrency problems.

***`Problem 2 — Change tracking grows`***

A long-lived context can accumulate tracked entities.

```text
Request 1 → tracked entities
Request 2 → more tracked entities
Request 3 → more tracked entities
...
```

This can increase memory usage and cause stale entity/state issues.

***`Problem 3 — Wrong unit of work`***

A `DbContext` is designed around a relatively short-lived unit of work.

You generally don't want:

```text
Application lifetime
        ↓
One DbContext
        ↓
Everything
```

Instead:

```text
Request
   ↓
DbContext
   ↓
SaveChanges()
   ↓
Request ends
   ↓
Context disposed
```

**Why NOT Transient?**

You technically can register it as transient:

```csharp
builder.Services.AddTransient<AppDbContext>();
```

But this is generally **not the preferred lifetime for Web APIs**.

You could end up with different context instances within the same request if multiple components request it.

For example:

```text
Request
  │
  ├── Service
  │     ↓
  │   DbContext #1
  │
  └── Repository
        ↓
      DbContext #2
```

Now they don't share the same change tracker/unit of work.

With Scoped:

```text
Request
  │
  ├── Service ─────┐
  │                │
  └── Repository ──┤
                   ↓
              DbContext #1
```

Much more appropriate.

---

### `IDbContextFactory`

What if you need a DbContext outside the normal HTTP request scope?

For example:

```text
Background Service
Worker
Long-running process
Parallel independent operations
```

You can use:

```csharp
IDbContextFactory<AppDbContext>
```

Example registration:

```csharp
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Then:

```csharp
public class MyWorker
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public MyWorker(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task Process()
    {
        await using var context =
            await _factory.CreateDbContextAsync();

        var employees = await context.Employees.ToListAsync();
    }
}
```

This creates a context for the operation and disposes it afterward.

-------------------
-------------------

## EF Core Migrations 

> Migrations are used to **keep your database schema synchronized with your C# entity model**.

The easiest way to remember:

> **Migration = a version-controlled record of database schema changes.**


**Why do we need migrations?**

Suppose initially you have:

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Database:

```text
Employees
----------------
Id
Name
```

Later you add:

```csharp
public string Email { get; set; }
```

Now your C# model says:

```text
Employee
 ├── Id
 ├── Name
 └── Email
```

But the database still has:

```text
Employees
 ├── Id
 └── Name
```

There's a mismatch.

Migrations solve this.

```text
C# Model
   ↓
Detect change
   ↓
Migration
   ↓
Database schema updated
```

**Create a Migration**

After changing your model:

```bash
dotnet ef migrations add AddEmployeeEmail
```

EF Core creates a migration class.

Conceptually:

```text
Migrations/
    ├── 20260828090000_InitialCreate.cs
    └── 20260828100000_AddEmployeeEmail.cs
```

The migration contains instructions describing the schema change.

For example, conceptually:

```csharp
migrationBuilder.AddColumn<string>(
    name: "Email",
    table: "Employees",
    nullable: true);
```

**Update the Database**

Creating a migration **doesn't necessarily apply it to the database**.

You then run:

```bash
dotnet ef database update
```

Flow:

```text
C# Model
   ↓
dotnet ef migrations add
   ↓
Migration generated
   ↓
dotnet ef database update
   ↓
Database changed
```

**`migrations add`**

> **Creates a migration describing the changes between the current model and the previous migration.**

**`database update`**

> **Applies pending migrations to the database.**

Remember:

```text
Add Migration
    =
Create migration instructions

Database Update
    =
Execute/apply those instructions
```

**Migration ≠ Database Backup**

Don't confuse these.

Migration:

> Describes **schema changes**.

Backup:

> Protects **database data**.

For example:

```text
Migration:
Add Salary column

Backup:
Save complete database/data state
```

**Migration vs Model Snapshot**

EF Core migrations also use a **model snapshot**.

You'll commonly see:

```text
Migrations/
   ├── InitialCreate.cs
   ├── AddEmployeeSalary.cs
   ├── AppDbContextModelSnapshot.cs
```

The snapshot represents the current EF Core model state used to help EF determine future changes.

Conceptually:

```text
Current C# Model
       ↓
Compare with
       ↓
Model Snapshot
       ↓
Determine changes
       ↓
Generate Migration
```

---

### Development vs Production

In development, it's common to run:

```bash
dotnet ef database update
```

manually.

But production deployment needs more care.

Typical production approach:

```text
Developer
   ↓
Create migration
   ↓
Commit to Git
   ↓
CI/CD
   ↓
Review/test
   ↓
Apply migration
   ↓
Production DB
```

For production, teams often generate and review SQL migration scripts rather than blindly running development-style commands.

For example:

```bash
dotnet ef migrations script
```

This generates SQL that can be reviewed/deployed through the organization's database deployment process.


> **"In development it's common to use `dotnet ef database update`. In production, I would follow the team's deployment process, often generating a migration SQL script or using a controlled migration mechanism through CI/CD, so that database changes can be reviewed, tested, and deployed safely."**


> "EF Core maintains a migration history table, typically `__EFMigrationsHistory`, in the database."

> "Migrations are part of the application's database schema evolution and should normally be version controlled."

--------------------------------
--------------------------------

## EF Core Change Tracking

> **Change Tracking is how EF Core keeps track of entities it has loaded so it knows what changed and what needs to be saved to the database.**


**Simple Example**

Suppose the database has:

```text
Employee
----------------
Id       = 1
Name     = Swapnil
Salary   = 50000
```

You retrieve it:

```csharp
var employee = await db.Employees.FirstAsync(e => e.Id == 1);
```

EF Core starts tracking this entity.

Conceptually:

```text
Database
   ↓
Employee
   ↓
DbContext
   ↓
Tracking
```

Now change:

```csharp
employee.Salary = 60000;
```

You don't have to explicitly say:

```csharp
Update(employee);
```

if the entity is already tracked.

Then:

```csharp
await db.SaveChangesAsync();
```

EF Core detects the change and generates an appropriate SQL `UPDATE`.

Conceptually:

```text
Salary: 50000 → 60000
              ↓
        Change detected
              ↓
       SaveChanges()
              ↓
       UPDATE Employee
              ↓
       Salary = 60000
```

**How does EF Core know what changed?**

When EF Core starts tracking an entity, it maintains information about its original/current state.

Example:

```text
Original Value
----------------
Salary = 50000
```

Later:

```csharp
employee.Salary = 60000;
```

EF Core can detect:

```text
Original: 50000
Current:  60000
           ↓
        Modified
```

Then `SaveChanges()` persists the change.

---

### Entity States

Every tracked entity has a state.

The main states are:

```text
Detached
Unchanged
Added
Modified
Deleted
```

**`Detached`**

EF Core is **not tracking** the entity.

```text
Entity
  ↓
Not associated with DbContext tracking
```

Example:

```csharp
var employee = new Employee
{
    Id = 1,
    Name = "Swapnil"
};
```

Simply creating an object doesn't make EF Core track it.

**`Unchanged`**

Entity is being tracked, but nothing has changed.

```text
Database
   ↓
Employee
   ↓
Tracked
   ↓
Unchanged
```

For example:

```csharp
var employee = await db.Employees.FirstAsync();
```

Immediately after loading:

```text
State = Unchanged
```

**`Added`**

You tell EF Core that this is a new entity:

```csharp
var employee = new Employee
{
    Name = "Swapnil",
    Salary = 60000
};

db.Employees.Add(employee);
```

State:

```text
Added
```

Then:

```csharp
await db.SaveChangesAsync();
```

generates an `INSERT`.

```text
Added
  ↓
SaveChanges()
  ↓
INSERT
```

**`Modified`**

Example:

```csharp
employee.Salary = 60000;
```

EF Core detects the modification.

```text
Unchanged
    ↓
Property changed
    ↓
Modified
```

Then:

```csharp
SaveChanges()
```

generates an `UPDATE`.

**`Deleted`**

Example:

```csharp
db.Employees.Remove(employee);
```

State:

```text
Deleted
```

Then:

```csharp
SaveChanges()
```

generates:

```sql
DELETE ...
```

------------

### Tracking Query

By default, queries returning entities from a normal EF Core `DbContext` are generally **tracking queries**.

Example:

```csharp
var employee = await db.Employees.FirstAsync(e => e.Id == 1);
```

EF Core tracks `employee`.

You can check:

```csharp
var state = db.Entry(employee).State;
```

Initially:

```text
Unchanged
```

After:

```csharp
employee.Salary = 70000;
```

typically:

```text
Modified
```

### `AsNoTracking()`

Now suppose you're building a read-only API.

```csharp
var employees = await db.Employees
    .AsNoTracking()
    .ToListAsync();
```

This tells EF Core:

> "I only want to read these entities; don't track them."

Conceptually:

```text
Database
   ↓
Employee
   ↓
AsNoTracking()
   ↓
No change tracking
   ↓
Application
```

**Why use `AsNoTracking()`?**

Change tracking has a cost.

EF Core needs to maintain tracking information for entities.

If you're doing:

```text
Reports
Search
Dashboard
Read-only API
Large result sets
```

you often don't need that tracking.

So:

```csharp
var employees = await db.Employees
    .AsNoTracking()
    .Where(e => e.IsActive)
    .ToListAsync();
```

can reduce tracking overhead.

**What happens if I modify an AsNoTracking entity?**

Example:

```csharp
var employee = await db.Employees
    .AsNoTracking()
    .FirstAsync(e => e.Id == 1);

employee.Salary = 80000;

await db.SaveChangesAsync();
```

Will EF Core automatically know that Salary changed?

**No.**

Because:

```text
AsNoTracking()
      ↓
Entity not tracked
      ↓
Salary changed
      ↓
DbContext doesn't automatically track that change
```

You need to explicitly tell EF Core how to handle it.

For example:

```csharp
db.Employees.Update(employee);

await db.SaveChangesAsync();
```

But be careful: `Update()` can mark the entity graph/properties as modified in ways that aren't always what you want.

For APIs, a more controlled approach can be to load the tracked entity and modify only the properties that should change.


### Real API Example

Suppose:

```http
GET /api/employees
```

***You're just returning data.***

Use:

```csharp
var employees = await db.Employees
    .AsNoTracking()
    .Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name,
        Salary = e.Salary
    })
    .ToListAsync();
```

Excellent combination:

```text
AsNoTracking
     +
Projection
     +
Database filtering
```


***Update Scenario***

Suppose:

```http
PUT /api/employees/1
```

You need to update an employee.

A common approach is:

```csharp
var employee = await db.Employees
    .FirstOrDefaultAsync(e => e.Id == id);

if (employee == null)
    return NotFound();

employee.Name = request.Name;
employee.Salary = request.Salary;

await db.SaveChangesAsync();
```

Because the employee is tracked:

```text
Database
   ↓
Tracked Employee
   ↓
Modify properties
   ↓
Change Detection
   ↓
SaveChanges()
   ↓
UPDATE
```

You don't necessarily need to call `Update()`.

---

### `SaveChanges()` and Change Tracking

This is an important sequence to remember:

```text
1. Query entity
       ↓
2. EF Core tracks it
       ↓
3. Modify entity
       ↓
4. Change Detection
       ↓
5. SaveChanges()
       ↓
6. SQL generated
       ↓
7. Database updated
```

Example:

```csharp
var employee = await db.Employees
    .FirstAsync(e => e.Id == 1);

employee.Salary = 75000;

await db.SaveChangesAsync();
```

---

### `DetectChanges()`

EF Core has change detection mechanisms that identify modifications to tracked entities.

You can explicitly call:

```csharp
db.ChangeTracker.DetectChanges();
```

but normally you don't need to do this manually.

`SaveChanges()` handles the required change detection process as part of saving.

---

### `ChangeTracker`

You can inspect tracked entities:

```csharp
var entries = db.ChangeTracker
    .Entries();
```

You can inspect states:

```csharp
foreach (var entry in db.ChangeTracker.Entries())
{
    Console.WriteLine(
        $"{entry.Entity.GetType().Name}: {entry.State}");
}
```

Useful for debugging and understanding what EF Core is tracking.

----------------------------
----------------------------

## EF Core Relationships

> A relationship defines how two entities are connected.

For example:

```text
Department
    │
    │ has many
    ↓
Employees
```

or:

```text
Student ←→ Course
```

or:

```text
Person ←→ Passport
```

The three relationships you should know are:

1. **One-to-Many**
2. **Many-to-Many**
3. **One-to-One**

**`1. One-to-Many`**

> **One record in Entity A can have many records in Entity B.**

Example:

```text
Department
    │
    ├── Employee 1
    ├── Employee 2
    └── Employee 3
```

One department → many employees.

But each employee belongs to one department.

**Department**

```csharp
public class Department
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
```

**Employee**

```csharp
public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; }
}
```

Here:

```csharp
public int DepartmentId { get; set; }
```

is the **Foreign Key**.

And:

```csharp
public Department Department { get; set; }
```

is the **Navigation Property**.

> Navigation properties allow you to navigate between entities.

From Employee:

```csharp
employee.Department
```

From Department:

```csharp
department.Employees
```

So:

```text
Employee
    ↓
Department
```

and:

```text
Department
    ↓
Employees
```

**Configure One-to-Many with Fluent API**

You can explicitly configure it:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasOne(e => e.Department)
        .WithMany(d => d.Employees)
        .HasForeignKey(e => e.DepartmentId);
}
```

Read this almost like English:

```text
Employee
   HAS ONE
Department

Department
   HAS MANY
Employees

Employee.DepartmentId
   IS FOREIGN KEY
```

**`2. Many-to-Many`**

Example:

```text
Student ←→ Course
```

A student can take many courses.

A course can have many students.

```text
Student
 ├── Course A
 ├── Course B
 └── Course C

Course A
 ├── Student 1
 ├── Student 2
 └── Student 3
```

That's:

> **Many Students ↔ Many Courses**

**Database Representation**

Relational databases normally represent many-to-many relationships using a **join table**.

```text
Students
---------
Id
Name


Courses
---------
Id
Name


StudentCourses
--------------
StudentId
CourseId
```

Example:

```text
StudentCourses

StudentId    CourseId
---------------------
1            10
1            20
2            10
3            30
```

Meaning:

```text
Student 1 → Course 10
Student 1 → Course 20
Student 2 → Course 10
Student 3 → Course 30
```

**`3. EF Core Many-to-Many`**

Modern EF Core can configure many-to-many without explicitly creating a join entity when the join table needs no additional data.

```csharp
public class Student
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

```csharp
public class Course
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
```

**Many-to-Many with Explicit Join Entity**

Suppose the relationship itself has additional information:

```text
Student
   ↓
Course

Enrollment
 ├── EnrollmentDate
 ├── Grade
 └── Status
```

Now you should create a join entity.

```csharp
public class Enrollment
{
    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public string Grade { get; set; }

    public Student Student { get; set; }

    public Course Course { get; set; }
}
```

Now:

```text
Student
   ↓
Enrollment
   ↓
Course
```

This is very common in real-world applications.

---

**One-to-One

One record corresponds to exactly one record.

Example:

```text
Employee ←→ EmployeeProfile
```

One employee has one profile.

One profile belongs to one employee.

---

### C# Example

```csharp
public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; }

    public EmployeeProfile Profile { get; set; }
}
```

```csharp
public class EmployeeProfile
{
    public int Id { get; set; }

    public string Address { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; }
}
```

Configure:

```csharp
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Profile)
    .WithOne(p => p.Employee)
    .HasForeignKey<EmployeeProfile>(p => p.EmployeeId);
```

Meaning:

```text
Employee
   HAS ONE
EmployeeProfile

EmployeeProfile
   BELONGS TO ONE
Employee
```

---

### Cascade Delete

Suppose:

```text
Department
    ↓
Employees
```

If the Department is deleted, what happens to Employees?

EF Core/database relationship configuration can define delete behavior.

Common options include:

```text
Cascade
Restrict
NoAction
SetNull
```

For example, cascade:

```text
Delete Department
       ↓
Delete related Employees
```

Be careful with cascade deletes in large relationship graphs because one delete can trigger many dependent deletes.

> The principal is the entity whose key is referenced by the dependent. The dependent contains the foreign key.


And remember:

> **Relationship = how entities are connected.**

> **Loading = when related data is fetched.**

> **Change Tracking = whether EF Core tracks entity changes.**

These are three separate concepts, but they work together in EF Core.

-----------------------------------
-----------------------------------

## Fluent API vs Data Annotations

Both are used to **configure how your C# classes map to database tables**.

For example, you may want to define:

* Primary key
* Required fields
* Maximum length
* Foreign keys
* Relationships
* Table/column names
* Indexes
* Delete behavior

There are two main approaches:

```text
Data Annotations
        OR
Fluent API
```

### Data Annotations

You put attributes directly on your entity class.

Example:

```csharp
public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Column("employee_salary")]
    public decimal Salary { get; set; }
}
```

Common attributes:

```text
[Key]
[Required]
[MaxLength]
[MinLength]
[StringLength]
[Column]
[Table]
[ForeignKey]
[NotMapped]
[DatabaseGenerated]
```

### Example: 

**`1. [Required]`**

```csharp
public class Employee
{
    [Required]
    public string Name { get; set; }
}
```

This tells EF Core:

> `Name` is required.


**`[MaxLength]`**

```csharp
[MaxLength(100)]
public string Name { get; set; }
```

Conceptually:

```text
Name → maximum 100 characters
```

**`[Table]`**

```csharp
[Table("Employees")]
public class Employee
{
    public int Id { get; set; }
}
```

This tells EF Core to map the entity to:

```text
Employees
```

instead of relying on the default table naming convention.

------

### Fluent API

> Instead of putting configuration on the entity, you configure it inside `OnModelCreating()`.

Example:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasKey(e => e.Id);

    modelBuilder.Entity<Employee>()
        .Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);
}
```

Now the entity itself remains clean:

```csharp
public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; }
}
```


***Fluent API for Relationships***

This is where Fluent API becomes especially useful.

Suppose:

```text
Department
    ↓
Employees
```

You can configure:

```csharp
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Department)
    .WithMany(d => d.Employees)
    .HasForeignKey(e => e.DepartmentId);
```

This is much more expressive for complex relationships.

***Fluent API for Indexes***

You can configure an index:

```csharp
modelBuilder.Entity<Employee>()
    .HasIndex(e => e.Email)
    .IsUnique();
```

Meaning:

```text
Employees
    ↓
Email
    ↓
UNIQUE INDEX
```

This is one example where Fluent API is particularly useful.

***Fluent API for Delete Behavior***

For example:

```csharp
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Department)
    .WithMany(d => d.Employees)
    .HasForeignKey(e => e.DepartmentId)
    .OnDelete(DeleteBehavior.Restrict);
```

Now deleting a Department won't automatically delete its Employees through a cascade relationship.


**Where should Fluent API go?**

Normally:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // configuration
}
```

Example:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .Property(e => e.Name)
            .HasMaxLength(100);
    }
}
```
    
**Why use `IEntityTypeConfiguration<T>`?**

Instead of putting 200 lines inside:

```csharp
OnModelCreating()
```

you can organize:

```text
Data/
 └── Configurations/
       ├── EmployeeConfiguration.cs
       ├── DepartmentConfiguration.cs
       ├── OrderConfiguration.cs
       └── CustomerConfiguration.cs
```
---

### When to use what?

```text
Simple entity rules
       ↓
Data Annotations ✅


Complex relationship/Indexes / constraints
       ↓
Fluent API ✅


Large application
       ↓
IEntityTypeConfiguration<T>
```

### One-line interview memory:

> **Data Annotations = simple configuration close to the model.**

> **Fluent API = powerful, explicit, centralized configuration.**

> **Large project = Fluent API + `IEntityTypeConfiguration<T>`.**

------------------------
------------------------

## EF Core Loading Strategies 

We already introduced **Eager, Lazy, and Explicit Loading**, but let's make this interview-ready and connect it with performance.

Assume:

```csharp
public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; }
}
```

### Eager Loading — `Include()`

> **Load related data as part of the initial query.**

```csharp
var orders = await db.Orders
    .Include(o => o.Customer)
    .ToListAsync();
```

You are explicitly telling EF Core:

> "I need the Customer along with the Orders."

**For nested relationships**

```csharp
var orders = await db.Orders
    .Include(o => o.Customer)
        .ThenInclude(c => c.Address)
    .ToListAsync();
```

Meaning:

```text
Order
  ↓
Customer
  ↓
Address
```

### Lazy Loading

> **Related data is loaded automatically when you access the navigation property.**

For example:

```csharp
var order = await db.Orders
    .FirstAsync();

var customer = order.Customer;
```

If lazy loading is configured, accessing:

```csharp
order.Customer
```

can trigger another database query.

**Main advantage**

Very convenient.

**Main disadvantage**

Hidden database calls.

For example:

```csharp
var orders = await db.Orders.ToListAsync();

foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name);
}
```

Potentially:

```text
1 query → Orders

N queries → Customers

Total = N + 1 queries
```

This is why lazy loading can cause serious performance problems.

### Explicit Loading

> **You manually decide when to load related data.**

```csharp
var order = await db.Orders
    .FirstAsync();

await db.Entry(order)
    .Reference(o => o.Customer)
    .LoadAsync();
```

For a collection:

```csharp
await db.Entry(order)
    .Collection(o => o.OrderItems)
    .LoadAsync();
```

You have complete control over when the query happens.


### Eager vs Lazy vs Explicit

|                            | Eager         | Lazy                | Explicit                |
| -------------------------- | ------------- | ------------------- | ----------------------- |
| Syntax                     | `Include()`   | Navigation property | `Load()`                |
| When loaded                | Initial query | When accessed       | When manually requested |
| Automatic                  | ❌             | ✅                   | ❌                       |
| DB queries predictable     | ✅             | ⚠️                  | ✅                       |
| N+1 risk                   | Low           | 🔴 High             | Low                     |
| Control                    | Good          | Low                 | Excellent               |
| Convenience                | ⭐⭐            | ⭐⭐⭐                 | ⭐⭐                      |
| Performance predictability | ⭐⭐⭐           | ⭐                   | ⭐⭐⭐                     |


### Which one should you choose?

**`Scenario 1`**

> "I always need Customer when retrieving Orders."

Use:

```csharp
.Include(o => o.Customer)
```

or projection.

**Eager Loading**

**`Scenario 2`**

> "I only need Customer in some situations."

You could use:

```csharp
.Entry(order)
.Reference(o => o.Customer)
.LoadAsync();
```

**Explicit Loading**

**`Scenario 3`**

> "I want related data to load automatically whenever I access it."

Lazy loading can do this.

**Lazy Loading ⚠️**

But be careful with performance.

---

### Projection — Often the Best Choice for APIs

This is the important part.

Suppose your API only needs:

```json
{
    "orderId": 10,
    "customerName": "Swapnil"
}
```

Don't necessarily load the entire Customer entity.

Instead:

```csharp
var orders = await db.Orders
    .Select(o => new OrderDto
    {
        OrderId = o.Id,
        CustomerName = o.Customer.Name
    })
    .ToListAsync();
```

EF Core can translate this into a SQL query that retrieves the required data.

Conceptually:

```text
Database
   ↓
Only required columns
   ↓
DTO
   ↓
API response
```

This can be more efficient than:

```csharp
.Include(o => o.Customer)
```

when you only need a few fields.

---

### `Include()` Does NOT Mean "Always One SQL Query"

This is a subtle interview point.

When you use:

```csharp
.Include(...)
```

EF Core determines how to load the relationship.

For collection includes, a single SQL query can produce a **cartesian explosion** when multiple collection relationships are joined.

Example:

```csharp
var orders = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .ToListAsync();
```

If an order has:

```text
10 Items
5 Payments
```

a single joined query can potentially produce:

```text
10 × 5 = 50 rows
```

for that order before EF reconstructs the object graph.

---

### `AsSplitQuery()`

For certain complex `Include()` scenarios, you can use:

```csharp
var orders = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .AsSplitQuery()
    .ToListAsync();
```

Instead of one large joined query, EF Core can execute multiple queries.

Conceptually:

```text
Query 1 → Orders
Query 2 → Items
Query 3 → Payments
```

This can reduce the data duplication caused by large joins.

But remember:

> **Split query isn't automatically faster either.**

It trades a potentially huge joined result for multiple database round trips.

---

### Important Performance Rule

Don't blindly think:

```text
Eager = Fast
Lazy = Slow
Explicit = Fast
```

That's too simplistic.

Instead think:

```text
How much data do I need?
        +
How many queries are generated?
        +
How much data is transferred?
        +
How complex is the SQL?
```

That's what determines performance.

---------------------
---------------------

## Transactions in EF Core

> A **transaction** ensures that multiple database operations are treated as **one unit**.

> **Either all operations succeed, or all of them are rolled back.**

This is especially important when one business operation modifies multiple records/tables.

### Simple Example

Imagine transferring ₹10,000:

```text
Account A
   ↓
- ₹10,000

Account B
   ↓
+ ₹10,000
```

You don't want this situation:

```text
A → ₹10,000 deducted ✅
B → ₹10,000 NOT added ❌
```

The operation should be:

```text
Deduct A
   +
Add B
   ↓
Both succeed → COMMIT ✅

Anything fails → ROLLBACK ❌
```

That's a transaction.

---

### ACID Properties

Transactions are commonly explained using **ACID**:

**`A — Atomicity`**

> All operations succeed or none do.

```text
A + B + C
   ↓
All succeed → Commit
Any failure → Rollback
```

**`C — Consistency`**

> Database moves from one valid state to another valid state.

Example:

```text
Balance cannot become invalid
Foreign key rules remain valid
Constraints are maintained
```

**`I — Isolation`**

> Concurrent transactions should not improperly interfere with each other.

For example:

```text
Transaction A
       ↕
Transaction B
```

The database controls how their changes interact.

**`D — Durability`**

> Once a transaction is committed, the changes should persist even if the application subsequently crashes.


**Does `SaveChanges()` use a transaction?**

This is an important interview question.

For a single `SaveChanges()` operation, EF Core generally uses a transaction when the database provider supports transactions.

Example:

```csharp
_context.Employees.Add(employee);
_context.Departments.Add(department);

await _context.SaveChangesAsync();
```

EF Core can execute the resulting database changes transactionally.

So:

```text
SaveChanges()
     ↓
Transaction
     ↓
INSERT
INSERT
     ↓
Commit
```

If something fails during the operation, the transaction can be rolled back.

**When do we need an explicit transaction?**

Suppose you have **multiple `SaveChanges()` calls** or need to coordinate multiple database operations and want them to succeed/fail together.

Example:

```csharp
await CreateOrder();
await UpdateInventory();
await CreatePayment();
```

If these are separate operations and one fails, you may need an explicit transaction to ensure the entire business operation is atomic.

**Explicit Transaction in EF Core**

You can use:

```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync();

try
{
    // Operation 1
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    // Operation 2
    inventory.Quantity -= order.Quantity;
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

Conceptually:

```text
Begin Transaction
       ↓
Create Order
       ↓
Save
       ↓
Update Inventory
       ↓
Save
       ↓
Commit
```

If something fails:

```text
Begin Transaction
       ↓
Create Order
       ↓
Update Inventory ❌
       ↓
Rollback
       ↓
Nothing committed
```

**Why do we sometimes call `SaveChanges()` multiple times?**

You don't necessarily need multiple `SaveChanges()` calls.

Often you can simply do:

```csharp
_context.Orders.Add(order);

inventory.Quantity -= order.Quantity;

await _context.SaveChangesAsync();
```

One `SaveChanges()` can persist the changes together.

But sometimes you need the result of an earlier operation before continuing.

For example:

```text
Create Order
     ↓
Need generated OrderId
     ↓
Create dependent record
     ↓
Save
```

A transaction can be useful when multiple database operations must still behave as one atomic business operation.

### Transaction Example: Order + Payment

Imagine:

```text
Create Order
     ↓
Create Payment
     ↓
Reduce Inventory
```

You want:

```text
Order       ✅
Payment     ✅
Inventory   ✅
```

or:

```text
Order       ❌
Payment     ❌
Inventory   ❌
```

Not:

```text
Order       ✅
Payment     ✅
Inventory   ❌
```

So:

```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync();

try
{
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    _context.Payments.Add(payment);
    await _context.SaveChangesAsync();

    inventory.Quantity -= order.Quantity;
    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```
---------

### Transaction with `BeginTransactionAsync()`

The important methods are:

```csharp
BeginTransactionAsync()
CommitAsync()
RollbackAsync()
```

Basic pattern:

```csharp
await using var transaction =
    await db.Database.BeginTransactionAsync();

try
{
    // database operations

    await db.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```
---------
### TransactionScope

You may also hear about:

```csharp
TransactionScope
```

Example:

```csharp
using var scope = new TransactionScope(
    TransactionScopeAsyncFlowOption.Enabled);

await Operation1();
await Operation2();

scope.Complete();
```

If `Complete()` isn't called, the transaction is rolled back when the scope is disposed.

However, don't automatically choose `TransactionScope` for every EF Core transaction. `DbContext.Database.BeginTransactionAsync()` is often clearer when you're controlling a transaction for a specific EF Core database connection/context.

---

### Multiple DbContexts

This is where things get more complicated.

Suppose:

```text
DbContext A
     +
DbContext B
```

and you need both to participate in the **same transaction**.

You can't simply assume that two independent contexts automatically share a transaction.

You may need to coordinate the connection/transaction appropriately, or use an ambient transaction such as `TransactionScope`, depending on the architecture and database provider.

This is more advanced, but good to know for senior interviews.

---

### Transaction vs `SaveChanges()`

This is a common interview question.

`SaveChanges()`

Persists tracked changes.

```csharp
await db.SaveChangesAsync();
```

For one `SaveChanges()` call, EF Core generally wraps the database commands in a transaction where supported.

### Explicit transaction

Useful when you need to group **multiple operations / multiple `SaveChanges()` calls** into one atomic unit.

```text
One SaveChanges()
    ↓
EF Core transaction generally sufficient


Multiple operations that must succeed together
    ↓
Explicit transaction
```

---

### Transaction Isolation Levels

You may hear:

```text
Read Uncommitted
Read Committed
Repeatable Read
Serializable
Snapshot
```

They determine how much one transaction can see/interact with changes from other concurrent transactions.

For example:

```text
Transaction A
       ↕
Transaction B
```

Isolation determines what A can see from B and vice versa.

You don't need to memorize every detail initially. For interviews, understand the concept:

> **Higher isolation generally provides stronger consistency guarantees but can reduce concurrency and increase locking/contention.**

------------------
------------------

## Raw SQL in EF Core 

> Sometimes LINQ is not the best option. EF Core allows you to execute **raw SQL** when you need more control over the query.

> Use LINQ by default; use raw SQL when you have a specific reason.

**Why use Raw SQL?**

Normally:

```csharp
var employees = await db.Employees
    .Where(e => e.Salary > 50000)
    .ToListAsync();
```

EF Core converts LINQ into SQL.

But sometimes you may have:

* A complex SQL query
* Existing stored procedures
* Database-specific SQL features
* A performance-critical query where SQL gives better control
* Legacy database code

Then raw SQL can be useful.

---

### `FromSql` — Query Entities

Suppose you want to execute:

```sql
SELECT *
FROM Employees
WHERE Salary > 50000
```

You can use:

```csharp
var employees = await db.Employees
    .FromSql($"SELECT * FROM Employees WHERE Salary > 50000")
    .ToListAsync();
```

For parameterized values:

```csharp
var salary = 50000;

var employees = await db.Employees
    .FromSql($"SELECT * FROM Employees WHERE Salary > {salary}")
    .ToListAsync();
```

The important point is that this form supports parameterization.

---

### SQL Injection

Never do this with user input:

```csharp
var name = request.Name;

var employees = await db.Employees
    .FromSqlRaw(
        $"SELECT * FROM Employees WHERE Name = '{name}'")
    .ToListAsync();
```

This can create a **SQL injection vulnerability**.

The problem is directly concatenating untrusted input into SQL.

---

### Safe Parameterization
Prefer parameterized SQL.

For example:

```csharp
var name = request.Name;

var employees = await db.Employees
    .FromSql($"SELECT * FROM Employees WHERE Name = {name}")
    .ToListAsync();
```

Or:

```csharp
var employees = await db.Employees
    .FromSqlRaw(
        "SELECT * FROM Employees WHERE Name = {0}",
        name)
    .ToListAsync();
```

The value is sent as a SQL parameter rather than being directly concatenated into the SQL command.

---

### `FromSqlInterpolated` vs `FromSqlRaw`

You may see older code like:

```csharp
.FromSqlInterpolated(...)
```

and:

```csharp
.FromSqlRaw(...)
```

The important distinction:

---
### `FromSqlRaw`

You are responsible for making sure values are parameterized correctly.

```csharp
var sql = "SELECT * FROM Employees WHERE Name = {0}";

db.Employees.FromSqlRaw(sql, name);
```
---
### `FromSql`

Modern EF Core provides:

```csharp
db.Employees.FromSql($"...");
```

which is designed to parameterize interpolated values.

---

### `ExecuteSql` — INSERT / UPDATE / DELETE

`FromSql` is primarily for queries that return entity data.

For commands such as:

```sql
UPDATE
DELETE
INSERT
```

you can use:

```csharp
await db.Database.ExecuteSqlAsync(...);
```

Example:

```csharp
var salary = 60000;

await db.Database.ExecuteSqlAsync(
    $"UPDATE Employees SET Salary = {salary} WHERE Id = {id}");
```

This executes the command directly against the database.

---

### `ExecuteSqlRaw`

You can also use:

```csharp
await db.Database.ExecuteSqlRawAsync(
    "UPDATE Employees SET Salary = {0} WHERE Id = {1}",
    salary,
    id);
```

Again, parameterization is important.

---

### `FromSql` vs `ExecuteSql`

Very important interview distinction:

| Method          | Purpose                                        |
| --------------- | ---------------------------------------------- |
| `FromSql`       | Query data/entities                            |
| `ExecuteSql`    | Execute INSERT/UPDATE/DELETE or other commands |
| LINQ            | Normal querying and manipulation               |
| `SaveChanges()` | Persist tracked entity changes                 |

Think:

```text
Need data?
   ↓
FromSql


Need to execute command?
   ↓
ExecuteSql
```

---

### Stored Procedures

Suppose your database already contains:

```sql
GetEmployeesByDepartment
```

You may need to execute it from EF Core.

For example:

```csharp
var departmentId = 10;

var employees = await db.Employees
    .FromSql(
        $"EXEC GetEmployeesByDepartment {departmentId}")
    .ToListAsync();
```

The exact syntax depends on the database provider.

---

### Raw SQL Doesn't Mean EF Core Stops Existing

This is important.

If you do:

```csharp
var employees = await db.Employees
    .FromSql(...)
    .ToListAsync();
```

EF Core still materializes the results as `Employee` entities.

If it's a tracking query, the returned entities can also participate in EF Core's normal tracking behavior.

So:

```text
Raw SQL
   ↓
EF Core
   ↓
Entity
   ↓
Change Tracking
```

can still apply.

---
### Raw SQL vs LINQ 

**LINQ**

```csharp
var employees = await db.Employees
    .Where(e => e.Salary > 50000)
    .OrderBy(e => e.Name)
    .ToListAsync();
```

**Raw SQL**

```csharp
var employees = await db.Employees
    .FromSql($"SELECT * FROM Employees WHERE Salary > 50000")
    .ToListAsync();
```

For most normal application queries:

```text
LINQ ✅
```

is preferable because it is:

* Type-safe
* Easier to maintain
* Easier to refactor
* Integrated with EF Core
* Database-provider independent to a greater extent

---

### When Would You Actually Use Raw SQL?

A good interview answer is:

> **"I prefer LINQ for most queries because it provides type safety, maintainability, and database abstraction. I would use raw SQL when I have a specific requirement such as an existing stored procedure, database-specific functionality, or a query that is difficult to express efficiently with LINQ."**

That's a strong answer.

---

### Don't Use Raw SQL Just Because You Think It's Faster 🚨

A common misconception:

> ❌ "Raw SQL is always faster than LINQ."

Not necessarily.

EF Core translates LINQ into SQL.

For:

```csharp
db.Employees
    .Where(e => e.Salary > 50000)
```

EF Core generates SQL.

So the important thing is the **SQL that ultimately reaches the database**, not simply whether you started with LINQ or raw SQL.

Performance should be measured using:

* Generated SQL
* Execution plan
* Indexes
* Number of rows
* Network transfer
* Query execution time

------------------------
------------------------

## EF Core Performance Optimization

> **"Your EF Core query is slow. How will you improve it?"**

Don't answer only:

> "Use `AsNoTracking()`."

There are several things you should check.

---

**`First Rule: Don't Load More Data Than Needed`**

❌ Bad:

```csharp
var employees = await db.Employees
    .ToListAsync();
```

Suppose the table has:

```text
Id
Name
Email
Address
Salary
DateOfBirth
Department
...
```

but your API only needs:

```text
Id
Name
Email
```

You're loading unnecessary data.

**Better: Projection**

```csharp
var employees = await db.Employees
    .Select(e => new EmployeeDto
    {
        Id = e.Id,
        Name = e.Name,
        Email = e.Email
    })
    .ToListAsync();
```

Now the database can return only the required columns.

### Interview line:

> **"I prefer projection with `Select()` to retrieve only the columns required by the API."**

---

**`Use AsNoTracking() for Read-Only Queries`**

If you're only reading:

```csharp
var employees = await db.Employees
    .AsNoTracking()
    .ToListAsync();
```

Why?

Because EF Core doesn't need to maintain normal tracking information for those entities.

Use it for:

```text
GET APIs
Reports
Dashboards
Search results
Read-only queries
```

---

**`Filtering Should Happen in the Database`**

❌ Avoid:

```csharp
var employees = await db.Employees
    .ToListAsync();

var result = employees
    .Where(e => e.Salary > 50000)
    .ToList();
```

This potentially loads everything first.

Then filtering happens in memory.

**Better:**

```csharp
var result = await db.Employees
    .Where(e => e.Salary > 50000)
    .ToListAsync();
```

Now:

```text
Database
   ↓
WHERE Salary > 50000
   ↓
Only matching rows
   ↓
Application
```

> **Filter as early as possible at the database level.**

---

**`Avoid ToList() Too Early`**

This is a common mistake.

❌:

```csharp
var employees = await db.Employees
    .ToListAsync();

var result = employees
    .Where(e => e.Salary > 50000)
    .Select(e => e.Name)
    .ToList();
```

The query executes at:

```csharp
.ToListAsync()
```

Everything before that is translated to SQL.

So instead:

```csharp
var result = await db.Employees
    .Where(e => e.Salary > 50000)
    .Select(e => e.Name)
    .ToListAsync();
```

Now the database performs the filtering and projection.

---

**`Understand IQueryable vs IEnumerable`**

This is a popular interview question.

**`IQueryable`**

```csharp
IQueryable<Employee>
```

The query can be translated into SQL and executed by the database.

Example:

```csharp
var query = db.Employees
    .Where(e => e.Salary > 50000);
```

The query hasn't necessarily executed yet.

**`IEnumerable`**

```csharp
IEnumerable<Employee>
```

Usually means you're working with data in memory.

Example:

```csharp
var employees = await db.Employees
    .ToListAsync();

IEnumerable<Employee> result =
    employees.Where(e => e.Salary > 50000);
```

The database query has already happened.

**`Deferred Execution`**

LINQ queries against `IQueryable` are generally **deferred**.

Example:

```csharp
var query = db.Employees
    .Where(e => e.Salary > 50000);
```

At this point:

```text
SQL hasn't necessarily executed yet.
```

Execution happens when you materialize it:

```csharp
await query.ToListAsync();
```

Other terminal operations include:

```csharp
FirstAsync()
SingleAsync()
CountAsync()
AnyAsync()
ToListAsync()
```

**`N+1 Query Problem`**

We discussed this with lazy loading, but it's important enough to remember separately.

Suppose:

```csharp
var orders = await db.Orders
    .ToListAsync();

foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name);
}
```

If lazy loading is enabled, you might get:

```text
1 query → Orders

1 query → Customer for Order 1
1 query → Customer for Order 2
1 query → Customer for Order 3
...
```

For 1,000 orders:

```text
1 + 1000 = 1001 queries
```

🚨 Huge performance problem.

**Solutions:**

Use projection:

```csharp
var orders = await db.Orders
    .Select(o => new OrderDto
    {
        Id = o.Id,
        CustomerName = o.Customer.Name
    })
    .ToListAsync();
```

or eager loading when you actually need the entity:

```csharp
var orders = await db.Orders
    .Include(o => o.Customer)
    .ToListAsync();
```

---

**`Pagination`**

Never blindly return millions of records.

❌:

```csharp
var employees = await db.Employees
    .ToListAsync();
```

Suppose:

```text
5 million employees
```

That's obviously problematic.

Instead:

```csharp
var employees = await db.Employees
    .OrderBy(e => e.Id)
    .Skip(0)
    .Take(20)
    .ToListAsync();
```

For page 2:

```csharp
.Skip(20)
.Take(20)
```

---

**`Skip() / Take() vs Keyset Pagination`**

For basic pagination:

```csharp
.Skip(page * pageSize)
.Take(pageSize)
```

works.

But with very large datasets, high `OFFSET` values can become expensive.

Example:

```text
Skip(5,000,000)
Take(20)
```

The database may still need to process/skip a huge number of rows.

For very large/high-performance systems, **keyset (seek) pagination** can be better.

Example:

```csharp
var employees = await db.Employees
    .Where(e => e.Id > lastSeenId)
    .OrderBy(e => e.Id)
    .Take(20)
    .ToListAsync();
```

Conceptually:

```text
Last ID = 5000

Give me next 20 where:

Id > 5000
```

This can work very efficiently with an appropriate index.

---

**`Database Indexes`**

Suppose you're frequently querying:

```csharp
.Where(e => e.Email == email)
```

If `Email` isn't indexed, the database may need to scan many rows.

You can create an index:

```csharp
modelBuilder.Entity<Employee>()
    .HasIndex(e => e.Email);
```

Or unique:

```csharp
modelBuilder.Entity<Employee>()
    .HasIndex(e => e.Email)
    .IsUnique();
```

Conceptually:

```text
No Index
   ↓
Scan many rows


Index
   ↓
Find matching data faster
```

But indexes aren't free.

They also increase:

* Storage
* INSERT cost
* UPDATE cost
* DELETE cost

So don't create indexes on every column.

---

**`Composite Indexes`**

Suppose you frequently query:

```csharp
.Where(e =>
    e.DepartmentId == departmentId &&
    e.IsActive)
```

A composite index may help:

```csharp
modelBuilder.Entity<Employee>()
    .HasIndex(e => new
    {
        e.DepartmentId,
        e.IsActive
    });
```

But index design should be based on actual query patterns and database behavior.

---

**`Don't Call Count() and Then ToList() Unnecessarily`**

❌:

```csharp
var employees = await query.ToListAsync();

var count = employees.Count;
```

If you only need the count:

```csharp
var count = await query.CountAsync();
```

The database can perform:

```sql
COUNT(...)
```

instead of returning all records.

---

**`Any() vs Count() > 0`**

Suppose you want to know whether something exists.

❌:

```csharp
if (await db.Employees.CountAsync() > 0)
{
}
```

Better:

```csharp
if (await db.Employees.AnyAsync())
{
}
```

You're asking:

> **"Does at least one record exist?"**

rather than:

> **"How many records are there?"**

---

**`First() vs Single()`**

Suppose you need one employee.

```csharp
var employee = await db.Employees
    .FirstOrDefaultAsync(e => e.Id == id);
```

`FirstOrDefault()` means:

> Give me the first matching record, or null if none exists.

`SingleOrDefault()` means:

> There should be zero or one matching record; throw if multiple records exist.

So don't use `Single()` just because you think it is "more precise."

Choose based on the business/data constraint.

---

**`Include() Can Cause Too Much Data`**

Example:

```csharp
var departments = await db.Departments
    .Include(d => d.Employees)
    .Include(d => d.Manager)
    .ToListAsync();
```

If you only need:

```text
Department Name
Employee Name
```

projection may be better:

```csharp
var result = await db.Departments
    .Select(d => new DepartmentDto
    {
        Name = d.Name,
        Employees = d.Employees
            .Select(e => e.Name)
            .ToList()
    })
    .ToListAsync();
```

Again:

> **Don't load entities just to throw most of the data away.**

---

**`AsSplitQuery()`**

We discussed this earlier.

If you have multiple collection includes:

```csharp
var result = await db.Orders
    .Include(o => o.Items)
    .Include(o => o.Payments)
    .AsSplitQuery()
    .ToListAsync();
```

This can avoid certain **cartesian explosion** problems from large joins.

But it means multiple queries.

So:

```text
Single Query
    ↓
Potentially huge joined result


Split Query
    ↓
Multiple SQL queries
    ↓
Less duplicated join data
```

Choose based on the actual query and workload.

---

**`Bulk Operations`**

Suppose you need to deactivate 100,000 employees.

Naively:

```csharp
var employees = await db.Employees.ToListAsync();

foreach (var employee in employees)
{
    employee.IsActive = false;
}

await db.SaveChangesAsync();
```

This loads and tracks a huge number of entities.

Modern EF Core provides:

```csharp
await db.Employees
    .Where(e => e.LastLogin < cutoffDate)
    .ExecuteUpdateAsync(setters =>
        setters.SetProperty(
            e => e.IsActive,
            false));
```

This allows the database to perform the update directly without loading all entities into memory.

Similarly:

```csharp
await db.Employees
    .Where(e => !e.IsActive)
    .ExecuteDeleteAsync();
```

This is extremely useful for bulk operations.

---

**`Avoid Unnecessary Entity Loading`**

Instead of:

```csharp
var employee = await db.Employees
    .FirstAsync(e => e.Id == id);

employee.IsActive = false;

await db.SaveChangesAsync();
```

if your operation is simply a direct bulk-style update and you don't need the entity:

```csharp
await db.Employees
    .Where(e => e.Id == id)
    .ExecuteUpdateAsync(setters =>
        setters.SetProperty(e => e.IsActive, false));
```

This avoids loading the entity first.

---

**`Use Database Execution Plans`**

Suppose your query takes:

```text
5 seconds
```

Don't immediately start changing C#.

Look at the database execution plan.

You may discover:

```text
Table Scan
```

instead of:

```text
Index Seek
```

Maybe the problem is simply a missing or inappropriate index.

This is why:

> **EF Core performance isn't only an EF Core problem.**

The database matters enormously.

---------------------
---------------------

## EF Core Concurrency

> **What happens when two users try to update the same record at the same time?**

**The Problem**

Suppose we have:

```text
Product
----------------
Id       = 1
Name     = Laptop
Stock    = 10
```

Two users open the same product.

```text
User A → Stock = 10
User B → Stock = 10
```

Now:

```text
User A changes Stock → 8
User B changes Stock → 5
```

Both are working with the **same original version**.

Without concurrency protection, the last update may overwrite the first one.

```text
Initial
Stock = 10
   │
   ├── User A → 8
   │
   └── User B → 5
             ↓
        Final = 5
```

User A's update has effectively been lost.

This is called:

> **Lost Update**

---

### Optimistic Concurrency

EF Core commonly uses **optimistic concurrency**.

The idea is:

> **Assume conflicts are uncommon, but detect them when they happen.**

Instead of locking the record while the user is editing it, we store a **concurrency token/version**.

Conceptually:

```text
Product

Id       = 1
Stock    = 10
Version  = 5
```

User A reads:

```text
Stock = 10
Version = 5
```

User B also reads:

```text
Stock = 10
Version = 5
```

User A updates:

```text
Stock = 8
Version = 6
```

Now User B tries to update using the old version:

```text
Expected Version = 5
Actual Version   = 6
```

EF Core detects:

> "Someone changed this record since you read it."

And the update fails.

---

### RowVersion / Timestamp 

With SQL Server, a common approach is a `rowversion` column.

Example:

```csharp
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public byte[] RowVersion { get; set; }
}
```

Configure it:

```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.RowVersion)
    .IsRowVersion();
```

Now the database maintains the row version.

Conceptually:

```text
Product
-------------------------
Id
Name
Price
RowVersion
```

When the row changes, the database generates a new version value.

---

### What SQL Does EF Core Generate?

Suppose:

```text
Original RowVersion = ABC
```

When EF Core updates the entity, conceptually the SQL contains a condition like:

```sql
UPDATE Products
SET Price = 50000
WHERE Id = 1
  AND RowVersion = ABC
```

If nobody changed the record:

```text
Id = 1
RowVersion = ABC
       ↓
1 row updated
       ↓
Success ✅
```

If somebody already changed it:

```text
Id = 1
RowVersion = XYZ
       ↓
RowVersion != ABC
       ↓
0 rows updated
       ↓
Concurrency conflict 🚨
```

EF Core can then throw:

```csharp
DbUpdateConcurrencyException
```

---

### Handling `DbUpdateConcurrencyException`

Example:

```csharp
try
{
    await db.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    // Handle concurrency conflict
}
```

In an API, you might return:

```http
409 Conflict
```

because the client's update conflicts with a newer version of the resource.

---

### Real API Example

Suppose:

```http
GET /api/products/1
```

Response:

```json
{
    "id": 1,
    "name": "Laptop",
    "price": 50000,
    "version": "..."
}
```

The client edits the price.

Later:

```http
PUT /api/products/1
```

and sends the version it originally received.

If another user has already modified the product:

```text
Client Version
      ↓
Old ❌
      ↓
Database Version
      ↓
New
```

Your API can respond:

```http
409 Conflict
```

with something like:

```json
{
    "message": "The product was modified by another user. Please refresh and try again."
}
```

---

### Concurrency Token

`RowVersion` isn't the only way.

EF Core supports **concurrency tokens**.

For example:

```csharp
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public Guid Version { get; set; }
}
```

Configure:

```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.Version)
    .IsConcurrencyToken();
```

Now EF Core uses `Version` when checking whether the entity has changed.

Conceptually:

```text
Original Version
       ↓
UPDATE ... WHERE Version = OriginalVersion
       ↓
If 0 rows affected
       ↓
Concurrency conflict
```

---

### RowVersion vs Concurrency Token

|                          | RowVersion                | Concurrency Token            |
| ------------------------ | ------------------------- | ---------------------------- |
| Common with              | SQL Server                | Various approaches/providers |
| Value generated by DB    | ✅                         | Depends                      |
| Typical type             | `byte[]`                  | Various types                |
| Purpose                  | Detect concurrent updates | Detect concurrent updates    |
| Automatic version change | DB-managed                | Depends on configuration     |

Important:

> **`rowversion` is SQL Server-specific terminology.**

Don't say that every database has a `rowversion` column.

---

### Optimistic vs Pessimistic Concurrency ⭐⭐

### Optimistic

```text
Read
 ↓
Work
 ↓
Update
 ↓
Check whether someone changed it
```

No lock is held for the entire editing period.

Good when:

```text
Conflicts are relatively uncommon
```

---

### Pessimistic

The idea is:

> Lock the resource while working with it so another transaction cannot modify it in the same way.

Conceptually:

```text
User A
  ↓
Lock record
  ↓
Work
  ↓
Update
  ↓
Unlock
```

User B has to wait or cannot acquire the required lock.

This can be useful in specific high-contention scenarios, but it can also introduce:

* Blocking
* Lock contention
* Deadlocks
* Reduced concurrency

---

### Optimistic vs Pessimistic

|                                  | Optimistic                                         | Pessimistic                   |
| -------------------------------- | -------------------------------------------------- | ----------------------------- |
| Lock while editing               | ❌                                                  | Usually                       |
| Assumes conflicts                | Rare                                               | Possible/frequent             |
| Performance under low contention | Good                                               | Can be unnecessary overhead   |
| Conflict handling                | Detect after/before update                         | Prevent/block through locking |
| Deadlock risk                    | Lower from application-level concurrency mechanism | Higher                        |
| Common EF Core approach          | ⭐⭐⭐                                                | More specialized              |

For most normal web applications:

> **Optimistic concurrency is the common approach.**

---

### 🔥 Remember these 5 points:

```text
1. Concurrent users can overwrite each other's changes.

2. Optimistic concurrency detects conflicts using a
   concurrency token.

3. SQL Server commonly uses rowversion.

4. EF Core can throw DbUpdateConcurrencyException.

5. API commonly returns 409 Conflict.
```

> **Transaction = "all operations together."**

> **Concurrency = "what if someone else changes the same data?"**
---

